namespace IngameScript
{
    using Sandbox.Game.EntityComponents;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Ingame.Utilities;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRageMath;

    /**
     * TODO:
     * Convert message handling to coroutine
     * Check for duplicate / conflicting messages
     * Add ability to work for a single airlock
     * Test if this works for multiple airlocks cycling
     * Send stdout to external source
     * Rename?
     **/ 

    /// <summary>
    /// Airlock Control program.
    /// </summary>
    partial class Program : MyGridProgram
    {
        /// <summary>
        /// Ini instance.
        /// </summary>
        private readonly MyIni ini = new MyIni();

        /// <summary>
        /// Command Line instance.
        /// </summary>
        private readonly MyCommandLine CommandLine = new MyCommandLine();

        /// <summary>
        /// Diciontary of Airlocks by group name.
        /// </summary>
        private readonly Dictionary<string, Airlock> Airlocks = new Dictionary<string, Airlock>();

        /// <summary>
        /// List of Block Groups.
        /// </summary>
        private readonly List<IMyBlockGroup> BlockGroups = new List<IMyBlockGroup>();

        /// <summary>
        /// Dictionary of commands.
        /// </summary>
        private readonly Dictionary<string, Action> Commands = new Dictionary<string, Action>();

        /// <summary>
        /// Id of output controller
        /// </summary>
        private readonly long? outputControllerId;

        /// <summary>
        /// Tag for the message
        /// </summary>
        private readonly string outputControllerMessageTag;

        /// <summary>
        /// Whether the controller should broadcast its status message
        /// </summary>
        private readonly bool sendBroadcastMessage;

        /// <summary>
        /// The transmission distance for broadcast transmissions
        /// </summary>
        private readonly TransmissionDistance transmissionDistance;

        /// <summary>
        /// Buffer of airlock status messages
        /// </summary>
        private readonly List<AirlockStatusMessage> airlockStatusMessages = new List<AirlockStatusMessage>();

        /// <summary>
        /// Whether external messages should trigger the airlock
        /// </summary>
        private readonly bool AllowExternalCommands;

        /// <summary>
        /// External Command Tag to filter for
        /// </summary>
        private readonly string ExternalCommandTag;

        /// <summary>
        /// Broadcast Listener
        /// </summary>
        private readonly IMyBroadcastListener broadcastListener;

        /// <summary>
        /// Unicast Listener
        /// </summary>
        private readonly IMyUnicastListener unicastListener;

        /// <summary>
        /// Creates a new instance of the Airlock Control Program.
        /// </summary>
        public Program()
        {
            this.Commands["cycle"] = this.Cycle;
            this.Commands["cancel"] = this.Cancel;
            this.Commands["release"] = this.Release;

            if (this.ini.TryParse(this.Me.CustomData))
            {
                this.Runtime.UpdateFrequency = UpdateFrequency.Update10;

                this.GridTerminalSystem.GetBlockGroups(this.BlockGroups, b => b.Name.ToLower().Contains("airlock"));

                foreach(IMyBlockGroup blockGroup in this.BlockGroups)
                {
                    try
                    {
                        this.Airlocks.Add(blockGroup.Name, new Airlock(blockGroup, this.Me, message => this.Stdout(message)));
                    }
                    catch(Airlock.InvalidAirlockException ex)
                    {
                        this.Stdout(ex.Message);
                    }
                }

                string statusControllerName = this.ini.Get("output", "sReceiverName").ToString("");

                if (!string.IsNullOrEmpty(statusControllerName))
                {
                    IMyTerminalBlock controller = this.GridTerminalSystem.GetBlockWithName(statusControllerName);

                    if (controller != null)
                    {
                        this.outputControllerId = controller.EntityId;
                    }
                    else
                    {
                        this.Echo($"No Controller named {statusControllerName}. Ignoring...");
                    }
                }

                this.outputControllerMessageTag = this.ini.Get("output", "sTag").ToString(AirlockStatusMessage.MessageTag);
                this.transmissionDistance = (TransmissionDistance)Enum.Parse(typeof(TransmissionDistance), this.ini.Get("output", "sTransmissionDistance").ToString("CurrentConstruct"));

                this.sendBroadcastMessage = !this.outputControllerId.HasValue || this.ini.Get("output", "bForceBroadcast").ToBoolean(false);

                this.AllowExternalCommands = this.ini.Get("input", "bAllowExternalCommands").ToBoolean(true);
                this.ExternalCommandTag = this.ini.Get("input", "sExternalCommandTag").ToString(AirlockCommandMessage.CommandTag);

                this.broadcastListener = this.IGC.RegisterBroadcastListener(this.ExternalCommandTag);
                this.unicastListener = this.IGC.UnicastListener;

                this.Echo("Initialization Success.");
                this.Echo($"Connected Airlocks: {this.Airlocks.Count}");
                
                this.Stdout("Initialization Success.");
                this.Stdout($"Connected Airlocks: {this.Airlocks.Count}");
            }
            else
            {
                this.Echo("Unable to parse INI");
            }
        }

        /// <summary>
        /// On Update100 checks all airlocks to run updates if necessary. On a command line call, attemps to Cycle or Cancel the airlock.
        /// </summary>
        /// <remarks>
        /// When cycling or canceling, the first argument should be the command "cycle" or "cancel", and the second argument should be the group name of the block group for the airlock.
        /// </remarks>
        /// <param name="argument">Argument string.</param>
        /// <param name="updateSource">Update Source.</param>
        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update10) > 0)
            {
                foreach(Airlock airlock in this.Airlocks.Values)
                {
                    airlock.Check();
                }
            }
            else if ((updateSource & UpdateType.Update100) > 0)
            {
                this.SendStatusUpdate();
            }
            else if ((updateSource & UpdateType.IGC) > 0)
            {
                if (this.AllowExternalCommands)
                {
                    // TODO: Convert into a Coroutine, check for duplicate or conflicting messages etc
                    while (this.broadcastListener.HasPendingMessage)
                    {
                        MyIGCMessage message = this.broadcastListener.AcceptMessage();
                        AirlockCommandMessage commandMessage = AirlockCommandMessage.FromMessage(message);
                        this.Main($"{commandMessage.Command} \"{commandMessage.Name}\" {commandMessage.Arguments}", UpdateType.None);
                    }

                    while (this.unicastListener.HasPendingMessage)
                    {
                        MyIGCMessage message = this.unicastListener.AcceptMessage();
                        if (message.Tag == this.ExternalCommandTag)
                        {
                            AirlockCommandMessage commandMessage = AirlockCommandMessage.FromMessage(message);
                            this.Main($"{commandMessage.Command} \"{commandMessage.Name}\" {commandMessage.Arguments}", UpdateType.None);
                        }
                    }
                }
            }
            else if (this.CommandLine.TryParse(argument))
            {
                Action action;
                if (this.Commands.TryGetValue(this.CommandLine.Argument(0), out action))
                {
                    action?.Invoke();
                }
                else
                {
                    this.Stdout($"Unrecognized command: {argument}");
                }
            }
        }

        /// <summary>
        /// Cycles the airlock with the group name in the second argument.
        /// </summary>
        private void Cycle()
        {
            string airlockName = this.CommandLine.Argument(1);
            Airlock airlock;
            if (this.Airlocks.TryGetValue(airlockName, out airlock))
            {
                if (this.CommandLine.Switch("interior"))
                {
                    airlock.OpenToInterior();
                }
                else if (this.CommandLine.Switch("exterior"))
                {
                    airlock.OpenToExterior();
                }
                else
                {
                    airlock.Cycle();
                }
            }
        }

        /// <summary>
        /// Triggers the force open of the airlock
        /// </summary>
        private void Release()
        {
            string airlockName = this.CommandLine.Argument(1);
            Airlock airlock;
            if (this.Airlocks.TryGetValue(airlockName, out airlock))
            {
                if (this.CommandLine.Switch("interior"))
                {
                    airlock.ForceOpenInterior();
                }
                else if (this.CommandLine.Switch("exterior"))
                {
                    airlock.ForceOpenExterior();
                }
                else
                {
                    airlock.ForceOpenExterior();
                    airlock.ForceOpenInterior();
                }
            }
        }

        /// <summary>
        /// Cancels cycling the airlock with the group name in the second argument.
        /// </summary>
        private void Cancel()
        {
            string airlockName = this.CommandLine.Argument(1);
            Airlock airlock;
            if (this.Airlocks.TryGetValue(airlockName, out airlock))
            {
                airlock.Cancel();
            }
        }

        /// <summary>
        /// Sends the status update for the airlocks.
        /// </summary>
        private void SendStatusUpdate()
        {
            this.airlockStatusMessages.Clear();
            foreach (Airlock airlock in this.Airlocks.Values)
            {
                this.airlockStatusMessages.Add(airlock.GetStatusMessage());
            }

            if (this.outputControllerId.HasValue)
            {
                this.IGC.SendUnicastMessage(this.outputControllerId.Value, this.outputControllerMessageTag, ImmutableArray.CreateRange(this.airlockStatusMessages.Select(x => x.ToData())));
            }
            
            if (this.sendBroadcastMessage)
            {
                this.IGC.SendBroadcastMessage(this.outputControllerMessageTag, ImmutableArray.CreateRange(this.airlockStatusMessages.Select(x => x.ToData())), this.transmissionDistance);
            }
        }
    }
}

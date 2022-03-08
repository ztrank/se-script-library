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
        /// Creates a new instance of the Airlock Control Program.
        /// </summary>
        public Program()
        {
            this.Commands["cycle"] = this.Cycle;
            this.Commands["cancel"] = this.Cancel;

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
    }
}

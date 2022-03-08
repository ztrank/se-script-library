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
    /// Industry Program
    /// </summary>
    partial class Program : MyGridProgram
    {
        /// <summary>
        /// Ini reader instance.
        /// </summary>
        private readonly MyIni ini = new MyIni();

        /// <summary>
        /// Command line reader instance.
        /// </summary>
        private readonly MyCommandLine commandLine = new MyCommandLine();

        /// <summary>
        /// Dictionary of available commands.
        /// </summary>
        private readonly Dictionary<string, Action<UpdateType>> Commands = new Dictionary<string, Action<UpdateType>>();

        /// <summary>
        /// Initialized list of terminal blocks.
        /// </summary>
        private readonly List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();

        /// <summary>
        /// List of assemblers.
        /// </summary>
        private readonly List<IMyAssembler> Assemblers = new List<IMyAssembler>();

        /// <summary>
        /// List of cargo containers.
        /// </summary>
        private readonly List<IMyCargoContainer> CargoContainers = new List<IMyCargoContainer>();

        /// <summary>
        /// Assembler network.
        /// </summary>
        private readonly AssemblerNetwork assemblerNetwork;

        /// <summary>
        /// Program Constructor. Runs at compile, and game session startup.
        /// </summary>
        public Program()
        {
            MyIniParseResult result;
            this.Commands["clear"] = this.ClearQueue;
            this.Commands["empty"] = this.Empty;

            if (this.ini.TryParse(this.Me.CustomData, out result))
            {
                string mainAssemblerName = this.ini.Get("assemblers", "main").ToString("Assembler");
                IMyAssembler assembler = this.GridTerminalSystem.GetBlockWithName(mainAssemblerName) as IMyAssembler;
                if (assembler == null)
                {
                    throw new Exception($"Unable to find the main assembler with name: {mainAssemblerName}");
                }
                
                this.GridTerminalSystem.GetBlocksOfType(this.Assemblers, b => b.IsSameConstructAs(this.Me));
                
                this.assemblerNetwork = new AssemblerNetwork(assembler, this.Assemblers, this.Stdout);

                this.GridTerminalSystem.GetBlocksOfType(this.CargoContainers, b => b.IsSameConstructAs(this.Me));
                this.assemblerNetwork.ConnectContainers(this.CargoContainers);
            }
            else
            {
                this.Stdout($"Unable to read INI: {result}");
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                if (this.commandLine.TryParse(argument))
                {
                    if (this.commandLine.ArgumentCount > 0 && this.Commands.ContainsKey(this.commandLine.Argument(0)))
                    {
                        this.Commands[this.commandLine.Argument(0)].Invoke(updateSource);
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                this.Me.GetSurface(0).WriteText(ex.Message);
            }
            
        }

        private void ClearQueue(UpdateType updateSource)
        {
            this.assemblerNetwork.ClearQueue();
        }

        private void Empty(UpdateType updateSource)
        {
            if (this.commandLine.Switch("input") || this.commandLine.Switch("all"))
            {
                this.Stdout("Emptying input inventories...");
                this.assemblerNetwork.EmptyInput();
            }
            
            if (this.commandLine.Switch("output") || this.commandLine.Switch("all"))
            {
                this.Stdout("Emptying output inventories...");
                this.assemblerNetwork.EmptyOutput();
            }
        }
    }
}

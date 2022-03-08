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

    partial class Program : MyGridProgram
    {
        /// <summary>
        /// Inventory Controller.
        /// </summary>
        private readonly Inventory controller;

        /// <summary>
        /// My command line.
        /// </summary>
        private readonly MyCommandLine CommandLine = new MyCommandLine();

        /// <summary>
        /// Ship class.
        /// </summary>
        private readonly Ship ship;

        /// <summary>
        /// Commands to execute.
        /// </summary>
        private readonly Dictionary<string, Action<string, UpdateType>> Commands = new Dictionary<string, Action<string, UpdateType>>();

        /// <summary>
        /// Tick Counter.
        /// </summary>
        private int TickCounter = 0;

        /// <summary>
        /// Creates and initializes the Inventory Program.
        /// </summary>
        public Program()
        {
            this.ship = new Ship(this);

            this.ProgramName = "Inventory";
            this.Commands["empty"] = this.Empty;
            this.controller = new Inventory(this.GridTerminalSystem, this.Me, this.Stdout, this.Stdout)
                .Initialize();
            this.Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        /// <summary>
        /// Executes the controller actions.
        /// </summary>
        /// <param name="argument">Argument string.</param>
        /// <param name="updateSource">Update Source.</param>
        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update100) > 0)
            {
                this.TickCounter++;
                this.controller.Paint();

                // Every 1000 ticks lets reinitialize the Controller to find any new blocks or
                // to handle game load scenarios.
                if (this.TickCounter >= 10)
                {
                    this.TickCounter = 0;
                    this.controller.Initialize();
                }
            }
            else if (this.CommandLine.TryParse(argument) && this.Commands.ContainsKey(this.CommandLine.Argument(0)))
            {
                this.Commands[this.CommandLine.Argument(0)]?.Invoke(argument, updateSource);
            }
        }

        private void Empty(string argument, UpdateType updateSource)
        {
            IMyShipConnector connector;
            if (this.ship.TryGetOtherConnector(out connector))
            {
                this.controller.TransferGrids(connector);
            }
        }
    }
}

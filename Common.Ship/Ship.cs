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

    public partial class Program
    {
        public partial class Ship
        {
            /// <summary>
            /// INI For the program.
            /// </summary>
            private readonly MyIni ini;

            /// <summary>
            /// Gets the terminal blocks.
            /// </summary>
            public List<IMyTerminalBlock> TerminalBlocks { get; } = new List<IMyTerminalBlock>();

            /// <summary>
            /// Gets the subsystem list.
            /// </summary>
            public List<ISubSystem> SubSystems { get; } = new List<ISubSystem>();

            /// <summary>
            /// Gets or sets the grid terminal system.
            /// </summary>
            public IMyGridTerminalSystem GridTerminalSystem { get; }

            /// <summary>
            /// Gets or sets the intergrid communication system.
            /// </summary>
            public IMyIntergridCommunicationSystem IGC { get; }

            /// <summary>
            /// Gets or sets the programmable block for this grid.
            /// </summary>
            public IMyProgrammableBlock Me { get; }

            /// <summary>
            /// Gets the runtime info.
            /// </summary>
            public IMyGridProgramRuntimeInfo Runtime { get; }

            /// <summary>
            /// Gets the ship's cube size.
            /// </summary>
            public MyCubeSize ShipType => this.Me.CubeGrid.GridSizeEnum;

            /// <summary>
            /// Log Action.
            /// </summary>
            public Action<string> Log { get; set; }

            /// <summary>
            /// Error Action.
            /// </summary>
            public Action<string> Error { get; set; }

            /// <summary>
            /// Gets a value indicating whether or not the log messages should be logged.
            /// </summary>
            public bool Debug { get; }

            /// <summary>
            /// Initializes the ship.
            /// </summary>
            /// <param name="program">Running Program.</param>
            /// <returns>Initialized Ship.</returns>
            public Ship(MyGridProgram program, MyIni ini, bool debug = false)
            {
                this.GridTerminalSystem = program.GridTerminalSystem;
                this.Me = program.Me;
                this.IGC = program.IGC;
                this.Runtime = program.Runtime;
                this.Debug = debug;
                this.ini = ini;
            }

            /// <summary>
            /// Initializes the ship by attaching and initializing all sub systems.
            /// </summary>
            /// <returns>Initialized ship.</returns>
            public Ship Initialize()
            {
                foreach(ISubSystem system in this.SubSystems)
                {
                    system.Attach(this.GridTerminalSystem, this.Me, this.Runtime, this.IGC, this.ini, this.Log, this.Error, this.Debug);
                    system.Initialize();
                }

                return this;
            }

            /// <summary>
            /// Reinitializes all subsystems.
            /// </summary>
            public void Reinitialize()
            {
                foreach(ISubSystem system in this.SubSystems)
                {
                    system.Initialize();
                }
            }

            /// <summary>
            /// Builder function for adding subsystems.
            /// </summary>
            /// <param name="subSystem">Subsytem to add.</param>
            /// <returns>Ship with subsystem.</returns>
            public Ship With(ISubSystem subSystem)
            {
                this.SubSystems.Add(subSystem);
                return this;
            }
        }
    }
}

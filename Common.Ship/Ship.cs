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
            /// Gets the terminal blocks.
            /// </summary>
            public List<IMyTerminalBlock> TerminalBlocks { get; } = new List<IMyTerminalBlock>();

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
            /// Initializes the ship.
            /// </summary>
            /// <param name="program">Running Program.</param>
            /// <returns>Initialized Ship.</returns>
            public Ship(MyGridProgram program)
            {
                this.GridTerminalSystem = program.GridTerminalSystem;
                this.Me = program.Me;
                this.IGC = program.IGC;
            }
        }
    }
}

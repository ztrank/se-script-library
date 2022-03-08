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

    partial class Program
    {
        /// <summary>
        /// Controller Abstract Class
        /// </summary>
        public abstract class Controller
        {
            /// <summary>
            /// Gets or sets the Grid Terminal System.
            /// </summary>
            public IMyGridTerminalSystem GridTerminalSystem { get; set; }

            /// <summary>
            /// Gets or sets the programmable block for this grid.
            /// </summary>
            public IMyProgrammableBlock Me { get; set; }

            /// <summary>
            /// Gets or sets the Log action.
            /// </summary>
            public Action<string> Stdout { get; set; }

            /// <summary>
            /// Gets or sets the error action.
            /// </summary>
            public Action<string> Stderr { get; set; }
        }
    }
}

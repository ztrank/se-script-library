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
        public partial class UserInterface
        {
            /// <summary>
            /// Selection Mode Enum.
            /// </summary>
            public enum SelectionMode
            {
                /// <summary>
                /// Only one row is selected at a time, changing selection deselects any currently selected.
                /// </summary>
                Single,

                /// <summary>
                /// Multiple rows can be selected at a time. Rows are deselected explicitly, rather than implicitly.
                /// </summary>
                Multiple
            }
        }
    }
}

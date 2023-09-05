﻿namespace IngameScript
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
        public enum AirlockState
        {
            /// <summary>
            /// Airlock is pressurized.
            /// </summary>
            Pressurized,

            /// <summary>
            /// Airlock is in the process of pressurizing.
            /// </summary>
            Pressurizing,

            /// <summary>
            /// Airlock is in the process of depressurizing.
            /// </summary>
            Depressurizing,

            /// <summary>
            /// Airlock is depressurized.
            /// </summary>
            Depressurized
        }
    }
}

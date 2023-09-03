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
        public partial interface IControllerSubSystem
        {
            Vector3D CenterOfMass { get; }
            MyShipMass CalculateShipMass();
        }

        /// <summary>
        /// Ship properties and methods for Mass calculations.
        /// </summary>
        public partial class ControllerSubSystem
        {
            /// <summary>
            /// Gets the center of mass of the ship.
            /// </summary>
            public Vector3D CenterOfMass => this.Main.CenterOfMass;

            /// <summary>
            /// Calculates the ship mass.
            /// </summary>
            /// <returns>My Ship Mass.</returns>
            public MyShipMass CalculateShipMass() => this.Main.CalculateShipMass();
        }
    }
}

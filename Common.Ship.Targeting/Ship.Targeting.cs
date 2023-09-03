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
        /// <summary>
        /// Ship properties and methods for Mass calculations.
        /// </summary>
        public partial class Ship
        {
            /// <summary>
            /// Internal reference to targetting subsystem.
            /// </summary>
            private ITargettingSubSystem targetting;

            /// <summary>
            /// Gets the Targetting Sub System.
            /// </summary>
            public ITargettingSubSystem Targetting
            {
                get
                {
                    if (this.targetting == null)
                    {
                        this.targetting = (ITargettingSubSystem)this.SubSystems.Find(s => s is ITargettingSubSystem);
                    }

                    return this.targetting;
                }
            }
        }
    }
}

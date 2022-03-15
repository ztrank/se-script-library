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
            /// Internal cargo hold member. Use CargoHold instead.
            /// </summary>
            private ICargoHold _cargoHold;

            /// <summary>
            /// Gets the CargoHold subsystem.
            /// </summary>
            public ICargoHold CargoHold
            {
                get
                {
                    if (this._cargoHold == null)
                    {
                        this._cargoHold = (ICargoHold)this.SubSystems.Find(s => s is ICargoHold);
                    }

                    return _cargoHold;
                }
            }
        }
    }
}

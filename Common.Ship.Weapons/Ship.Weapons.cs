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
        /// Ships with weapons.
        /// </summary>
        public partial class Ship
        {
            /// <summary>
            /// Internal Weapon sub system.
            /// </summary>
            private IWeaponSubSystem weaponSubSystem;

            /// <summary>
            /// Gets the weapon sub system.
            /// </summary>
            public IWeaponSubSystem WeaponSubSystem
            {
                get
                {
                    if (this.weaponSubSystem == null)
                    {
                        this.weaponSubSystem = (IWeaponSubSystem)this.SubSystems.Find(s => s is IWeaponSubSystem);
                    }

                    return this.weaponSubSystem;
                }
            }
        }
    }
}

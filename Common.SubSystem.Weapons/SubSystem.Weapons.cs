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
        /// Weapon sub system.
        /// </summary>
        public interface IWeaponSubSystem : ISubSystem
        {
            /// <summary>
            /// Gets the weapon systems.
            /// </summary>
            List<WeaponSystem> WeaponSystems { get; }
        }

        /// <summary>
        /// Weapon sub system.
        /// </summary>
        public class WeaponSubSystem : SubSystem, IWeaponSubSystem
        {
            /// <summary>
            /// Gets the Weapon system list.
            /// </summary>
            public List<WeaponSystem> WeaponSystems { get; } = new List<WeaponSystem>();

            /// <summary>
            /// Initializes the weapon systems.
            /// </summary>
            protected override void OnInitialize()
            {
                this.WeaponSystems.Clear();
                List<IMyUserControllableGun> AllWeapons = new List<IMyUserControllableGun>();
                this.GridTerminalSystem.GetBlocksOfType(AllWeapons, b => b.IsSameConstructAs(this.CPU));
                foreach (IMyUserControllableGun gun in AllWeapons)
                {
                    this.WeaponSystems.Add(new WeaponSystem(gun));
                }
            }
        }


        /// <summary>
        /// Weapon system wrapper class.
        /// </summary>
        public class WeaponSystem
        {
            /// <summary>
            /// Constructs the new weapon system.
            /// </summary>
            /// <param name="weapon">Actual weapon.</param>
            /// <param name="ship">Ship the weapon is part of.</param>
            public WeaponSystem(IMyUserControllableGun weapon)
            {
                List<MyItemType> Items = new List<MyItemType>();
                this.MyGun = weapon;
                this.MyGun.GetInventory().GetAcceptedItems(Items, item => item.GetItemInfo().IsAmmo);

                if (Items.Any())
                {
                    this.AmmoItemType = Items[0];
                }
            }

            /// <summary>
            /// Gets the ammunition type.
            /// </summary>
            public MyItemType AmmoItemType { get; private set; }

            /// <summary>
            /// Gets the weapon's inventory.
            /// </summary>
            public IMyInventory Inventory
            {
                get
                {
                    return this.MyGun.GetInventory();
                }
            }

            /// <summary>
            /// Gets the weapon itself.
            /// </summary>
            public IMyUserControllableGun MyGun { get; }

            /// <summary>
            /// Gets the magazine fill ratio.
            /// </summary>
            public float MagazineRatio
            {
                get
                {
                    return (float)this.Inventory.MaxVolume / (float)this.Inventory.CurrentVolume;
                }
            }

            /// <summary>
            /// Fires the weapon once.
            /// </summary>
            public void FireOnce()
            {
                this.MyGun.ShootOnce();
            }

            /// <summary>
            /// Toggles the fire on or off.
            /// </summary>
            public void ToggleFire()
            {
                this.MyGun.Shoot = !this.MyGun.Shoot;
            }
        }
    }
}

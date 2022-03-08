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
            /// Gets the Weapon system list.
            /// </summary>
            public List<WeaponSystem> WeaponSystems { get; } = new List<WeaponSystem>();

            /// <summary>
            /// initializes the ship with the given weapon systems.
            /// </summary>
            /// <returns>The ship with Weapon Systems initialized.</returns>
            public Ship WithWeaponSystems()
            {
                this.WeaponSystems.Clear();
                List<IMyUserControllableGun> AllWeapons = new List<IMyUserControllableGun>();
                this.GridTerminalSystem.GetBlocksOfType(AllWeapons, b => b.IsSameConstructAs(this.Me));
                foreach(IMyUserControllableGun gun in AllWeapons)
                {
                    this.WeaponSystems.Add(new WeaponSystem(gun, this));
                }
                return this;
            }
        }

        /// <summary>
        /// Weapon system wrapper class.
        /// </summary>
        public class WeaponSystem
        {
            /// <summary>
            /// All inventories connected to this weapon system that can transfer the ammunition.
            /// </summary>
            public List<IMyInventory> ConnectedInventories { get; } = new List<IMyInventory>();

            /// <summary>
            /// Constructs the new weapon system.
            /// </summary>
            /// <param name="weapon">Actual weapon.</param>
            /// <param name="ship">Ship the weapon is part of.</param>
            public WeaponSystem(IMyUserControllableGun weapon, Ship ship)
            {
                List<MyItemType> Items = new List<MyItemType>();
                this.MyGun = weapon;
                this.MyGun.GetInventory().GetAcceptedItems(Items, item => item.GetItemInfo().IsAmmo);

                if (Items.Any())
                {
                    this.AmmoItemType = Items[0];
                }

                foreach(IMyCargoContainer cargo in ship.CargoContainers)
                {
                    if (cargo.GetInventory().CanTransferItemTo(this.Inventory, this.AmmoItemType))
                    {
                        this.ConnectedInventories.Add(cargo.GetInventory());
                    }
                }

                this.ConnectInventory(ship.CargoContainers.Select(b => b.GetInventory()));
                this.ConnectInventory(ship.Connectors.Select(b => b.GetInventory()));
                this.ConnectInventory(ship.Controllers.Select(b => b.GetInventory()));
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
            /// Pulls ammunition from the cargo into the weapon system until the system is full or the connected inventories are empty.
            /// </summary>
            public void PullAmmo()
            {
                MyFixedPoint volume = (MyFixedPoint)this.AmmoItemType.GetItemInfo().Volume;
                MyFixedPoint available = this.Inventory.MaxVolume - this.Inventory.CurrentVolume;
                List<MyInventoryItem> items = new List<MyInventoryItem>();

                if (available > volume)
                {
                    int count = (int)((float)available / (float)volume);
                    foreach (IMyInventory inventory in this.ConnectedInventories)
                    {
                        items.Clear();
                        int inventoryCount = (int)inventory.GetItemAmount(this.AmmoItemType);
                        inventory.GetItems(items, i => i.Type.Equals(this.AmmoItemType));
                        foreach (MyInventoryItem item in items)
                        {
                            int transferAmount = (int)(item.Amount < count ? item.Amount : count);
                            if (inventory.TransferItemTo(this.Inventory, item, transferAmount))
                            {
                                count -= transferAmount;
                            }

                            if (count <= 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Toggles the fire on or off.
            /// </summary>
            public void ToggleFire()
            {
                this.MyGun.Shoot = !this.MyGun.Shoot;
            }

            /// <summary>
            /// Checks if the inventory can be used, and then adds it to the list of connected inventories.
            /// </summary>
            /// <param name="inventories">List of inventories.</param>
            private void ConnectInventory(IEnumerable<IMyInventory> inventories)
            {
                this.ConnectedInventories.AddRange(inventories.Where(i => i != null && i.CanTransferItemTo(this.Inventory, this.AmmoItemType)));
            }
        }
    }
}

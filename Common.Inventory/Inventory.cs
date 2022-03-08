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
        public partial class Inventory
        {
            /// <summary>
            /// Transfers all items from one inventory to the destination.
            /// </summary>
            /// <param name="source"></param>
            /// <param name="destination"></param>
            public static void EmptyInventory(IEnumerable<IMyInventory> sources, IEnumerable<IMyInventory> destinations)
            {
                
                List<MyInventoryItem> Items = new List<MyInventoryItem>();
                foreach(IMyInventory source in sources)
                {
                    Items.Clear();
                    source.GetItems(Items);
                    foreach (MyInventoryItem item in Items)
                    {
                        foreach(IMyInventory destination in destinations)
                        {
                            source.TransferItemTo(destination, item, null);
                        }
                    }
                }
            }

            public static float GetFilledRatio(IEnumerable<IMyInventory> inventories)
            {
                MyFixedPoint volume = 0;
                MyFixedPoint maxVolume = 0;
                foreach (IMyInventory inventory in inventories)
                {
                    maxVolume += inventory.MaxVolume;
                    volume += inventory.CurrentVolume;
                }

                return (float)volume / (float)maxVolume;
            }

            public static float GetFilledRatio(IEnumerable<IMyGasTank> gasTanks)
            {
                double volume = 0;
                float max = 0;
                foreach (IMyGasTank tank in gasTanks)
                {
                    max += tank.Capacity;
                    volume += tank.Capacity * tank.FilledRatio;
                }

                return (float)volume / max;
            }
        }
    }
}

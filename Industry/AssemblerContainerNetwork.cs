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
        public class AssemblerContainerNetwork
        {
            /// <summary>
            /// List of connected Containers.
            /// </summary>
            private readonly List<IMyCargoContainer> Containers = new List<IMyCargoContainer>();

            /// <summary>
            /// Assembler.
            /// </summary>
            private readonly IMyAssembler assembler;

            /// <summary>
            /// Standard out.
            /// </summary>
            private readonly Action<string> stdout;

            /// <summary>
            /// Initializes a new instance of the assembler network.
            /// </summary>
            /// <param name="assembler">Assembler for this network.</param>
            public AssemblerContainerNetwork(IMyAssembler assembler, Action<string> stdout)
            {
                this.stdout = stdout;
                this.assembler = assembler;
            }

            /// <summary>
            /// Connects the network by finding containers on this grid that the assembler is connected to.
            /// </summary>
            /// <param name="containers">List of Containers to connect to.</param>
            public void ConnectNetwork(List<IMyCargoContainer> containers)
            {
                this.Containers.Clear();
                foreach(IMyCargoContainer container in containers)
                {
                    if (this.assembler.IsSameConstructAs(container) && this.assembler.OutputInventory.IsConnectedTo(container.GetInventory()))
                    {
                        this.Containers.Add(container);
                    }
                }

                this.stdout($"{this.assembler.CustomName} connected to {this.Containers.Count} containers!");
            }

            /// <summary>
            /// Clears the queue.
            /// </summary>
            public void ClearQueue()
            {
                this.assembler.ClearQueue();
            }

            /// <summary>
            /// Emptys the input inventory.
            /// </summary>
            public void EmptyInput()
            {
                this.EmptyInventory(this.assembler.InputInventory);
            }

            /// <summary>
            /// Emptys the output inventory.
            /// </summary>
            public void EmptyOutput()
            {
                this.EmptyInventory(this.assembler.OutputInventory);
            }

            /// <summary>
            /// Empties an inventory into the cargo containers.
            /// </summary>
            /// <param name="source">Source inventory.</param>
            private void EmptyInventory(IMyInventory source)
            {
                this.stdout($"{this.assembler.CustomName} emptying {source.ItemCount} items into {this.Containers.Count} containers");
                while(source.ItemCount > 0)
                {
                    for (int i = 0; i < source.ItemCount; i++)
                    {
                        MyInventoryItem? nullableItem = source.GetItemAt(i);

                        if (nullableItem != null)
                        {
                            MyInventoryItem item = (MyInventoryItem)nullableItem;
                            float itemVolume = item.Type.GetItemInfo().Volume;
                            MyFixedPoint volume = itemVolume * item.Amount;
                            MyFixedPoint itemAmount = item.Amount;

                            foreach (IMyCargoContainer container in this.Containers)
                            {
                                MyFixedPoint available = container.GetInventory().MaxVolume - container.GetInventory().CurrentVolume;
                                MyFixedPoint count = 0;
                                if (available - 1 > volume)
                                {
                                    count = itemAmount;
                                    itemAmount = 0;
                                }
                                else
                                {
                                    count = (available - 1) * (1 / itemVolume);
                                    itemAmount -= count;
                                }

                                if (source.TransferItemTo(container.GetInventory(), i, null, true, count))
                                {
                                    this.stdout.Invoke("Transfer successful");
                                }
                                else
                                {
                                    this.stdout.Invoke("Unable to empty inventory");
                                }

                                if (itemAmount <= 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

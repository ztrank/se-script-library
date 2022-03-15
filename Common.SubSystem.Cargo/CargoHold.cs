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
        public interface ICargoHold : ISubSystem
        {
            /// <summary>
            /// Gets a list of all inventories on the grid.
            /// </summary>
            List<IMyInventory> Inventories { get; }

            /// <summary>
            /// Gets a list of all cargo containers on the grid.
            /// </summary>
            List<IMyCargoContainer> CargoContainers { get; }
        }

        public class CargoHold : SubSystem, ICargoHold
        {
            /// <summary>
            /// Gets a list of all inventories on the grid.
            /// </summary>
            public List<IMyInventory> Inventories { get; } = new List<IMyInventory>();

            /// <summary>
            /// Gets a list of all cargo containers on the grid.
            /// </summary>
            public List<IMyCargoContainer> CargoContainers { get; } = new List<IMyCargoContainer>();
            
            /// <summary>
            /// Requeries the grid for inventories and cargo containers.
            /// </summary>
            protected override void OnInitialize()
            {
                this.Inventories.Clear();
                this.CargoContainers.Clear();
                this.TerminalBlocks.Clear();

                this.GridTerminalSystem.GetBlocks(this.TerminalBlocks);

                foreach(IMyTerminalBlock block in this.TerminalBlocks)
                {
                    if (!block.IsSameConstructAs(this.CPU))
                    {
                        continue;
                    }

                    if (block.InventoryCount > 0)
                    {
                        for(int i = 0; i < block.InventoryCount; i++)
                        {
                            this.Inventories.Add(block.GetInventory(i));
                        }
                    }

                    if (block is IMyCargoContainer)
                    {
                        this.CargoContainers.Add((IMyCargoContainer)block);
                    }
                }
            }

            /// <summary>
            /// Requeries the grid for inventories and cargo containers.
            /// </summary>
            protected override void OnReinitialize()
            {
                this.OnInitialize();
            }
        }
    }
}

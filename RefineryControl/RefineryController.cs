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
        /// <summary>
        /// Refinery Controller
        /// </summary>
        public class RefineryController : Controller
        {
            /// <summary>
            /// Refinery block list.
            /// </summary>
            private readonly List<IMyTerminalBlock> refineryBlocks = new List<IMyTerminalBlock>();

            /// <summary>
            /// Sorter block list.
            /// </summary>
            private readonly List<IMyConveyorSorter> sorterBlocks = new List<IMyConveyorSorter>();

            /// <summary>
            /// Dictionary of Refineries.
            /// </summary>
            private readonly Dictionary<string, Refinery> Refineries = new Dictionary<string, Refinery>();

            /// <summary>
            /// Gets or sets a value indicating whether to silently ignore an invalid refinery.
            /// </summary>
            public bool IgnoreInvalidRefineries { get; set; }

            /// <summary>
            /// Gets the refinery with the given name.
            /// </summary>
            /// <param name="name">Refinery name.</param>
            /// <returns>Refinery.</returns>
            public Refinery this[string name]
            {
                get
                {
                    return this.Refineries[name];
                }
            }

            public List<Refinery> RefineryList
            {
                get
                {
                    return this.Refineries.Values.ToList();
                }
            }

            /// <summary>
            /// Gets the refinery keys.
            /// </summary>
            public IEnumerable<string> Names
            {
                get
                {
                    return this.Refineries.Keys;
                }
            }

            /// <summary>
            /// Initializes the controller by querying the grid for refineries and matching input/output sorters.
            /// </summary>
            /// <returns>This Refinery Controller</returns>
            public RefineryController Initialize()
            {
                this.refineryBlocks.Clear();
                this.sorterBlocks.Clear();
                this.Refineries.Clear();
                this.GridTerminalSystem.SearchBlocksOfName("Refinery", this.refineryBlocks, b => b.IsSameConstructAs(this.Me) && (b is IMyRefinery));
                this.GridTerminalSystem.GetBlocksOfType(this.sorterBlocks, b => b.IsSameConstructAs(this.Me));

                foreach(IMyTerminalBlock refineryBlock in this.refineryBlocks)
                {
                    Refinery refinery = new Refinery()
                    {
                        RefineryBlock = (IMyRefinery)refineryBlock
                    };

                    foreach (IMyConveyorSorter sorter in this.sorterBlocks)
                    {
                        if (sorter.CustomName.Contains(refineryBlock.CustomName))
                        {
                            if (sorter.CustomName.ToLower().Contains("input"))
                            {
                                refinery.InputSorter = sorter;
                            }
                            else if (sorter.CustomName.ToLower().Contains("output"))
                            {
                                refinery.OutputSorter = sorter;
                            }
                        }
                    } 

                    if (refinery.IsValid)
                    {
                        if (this.Refineries.ContainsKey(refineryBlock.CustomName))
                        {
                            this.Stderr($"Multiple Refineries with the same name: {refineryBlock.CustomName}. Refinery added arbitrarily.");
                        }
                        else
                        {
                            this.Refineries.Add(refineryBlock.CustomName, refinery);
                        }
                    }
                    else if (!this.IgnoreInvalidRefineries)
                    {
                        this.Stderr($"Invalid Refinery: {refineryBlock.CustomName}");
                    }
                }

                this.refineryBlocks.Clear();
                this.sorterBlocks.Clear();
                return this;
            }
        }
    }
}

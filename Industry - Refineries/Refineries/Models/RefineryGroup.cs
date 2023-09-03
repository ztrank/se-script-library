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
        public class RefineryGroup : UserInterface.ISelectable
        {
            public static void GetRefineryOutputs(List<MyInventoryItemFilter> filterList)
            {
                filterList.Add(new MyInventoryItemFilter(MyDefinitionId.Parse("MyObjectBuilder_Ore/Iron"), true));
                filterList.Add(new MyInventoryItemFilter(MyDefinitionId.Parse("MyObjectBuilder_Ingot/Iron"), true));
            }

            /// <summary>
            /// Gets the refinery group name.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets or sets a value indicating whether the refinery group is selected or not.
            /// </summary>
            public bool IsSelected { get; set; }

            /// <summary>
            /// Gets the refinery count.
            /// </summary>
            public int Count => this.RefineryBlocks.Count;

            /// <summary>
            /// Gets the list of refinery blocks.
            /// </summary>
            public List<IMyRefinery> RefineryBlocks { get; } = new List<IMyRefinery>();

            /// <summary>
            /// Gets the list of input filters.
            /// </summary>
            public List<IMyConveyorSorter> InputSorters { get; } = new List<IMyConveyorSorter>();

            /// <summary>
            /// Gets the list of output filters.
            /// </summary>
            public List<IMyConveyorSorter> OutputSorters { get; } = new List<IMyConveyorSorter>();

            /// <summary>
            /// Gets the list of all filters.
            /// </summary>
            public List<IMyConveyorSorter> Sorters { get; } = new List<IMyConveyorSorter>();

            /// <summary>
            /// Gets the first input sorter's mode, it should equal all other sorters modes.
            /// </summary>
            public MyConveyorSorterMode Mode
            {
                get
                {
                    if (this.InputSorters.Any())
                    {
                        return this.InputSorters[0].Mode;
                    }

                    return MyConveyorSorterMode.Blacklist;
                }
            }

            /// <summary>
            /// Populates the group with the blocks from the given block group.
            /// </summary>
            /// <param name="group">Block group.</param>
            /// <returns>Refinery group.</returns>
            public RefineryGroup Populate(IMyBlockGroup group)
            {
                this.Name = group.Name;
                this.RefineryBlocks.Clear();
                this.InputSorters.Clear();
                this.OutputSorters.Clear();
                this.Sorters.Clear();
                group.GetBlocksOfType(this.RefineryBlocks);
                group.GetBlocksOfType(this.Sorters);

                this.InputSorters.AddRange(this.Sorters.Where(f => f.CustomName.ToLower().Contains("input")));
                this.OutputSorters.AddRange(this.Sorters.Where(f => f.CustomName.ToLower().Contains("output")));

                List<MyInventoryItemFilter> filters = new List<MyInventoryItemFilter>();
                GetRefineryOutputs(filters);

                this.OutputSorters.ForEach(output => output.SetFilter(MyConveyorSorterMode.Whitelist, filters));
                

                filters.Clear();

                // If we have any input filters, set them to the first one so they are all the same.
                if (this.InputSorters.Any())
                {
                    this.InputSorters[0].GetFilterList(filters);
                    MyConveyorSorterMode mode = MyConveyorSorterMode.Whitelist;

                    this.SetFilters(mode, filters);
                }

                return this;
            }

            /// <summary>
            /// Gets the filters from the first sorter, which should equal all the others.
            /// </summary>
            /// <param name="itemFilters">List of item filters to populate.</param>
            public void GetFilters(List<MyInventoryItemFilter> itemFilters)
            {
                if (this.InputSorters.Any())
                {
                    this.InputSorters[0].GetFilterList(itemFilters);
                }
            }

            /// <summary>
            /// Sets the filters and mode on all the sorters.
            /// </summary>
            /// <param name="mode">Mode to use.</param>
            /// <param name="itemFilters">Filters to use.</param>
            public void SetFilters(MyConveyorSorterMode mode, List<MyInventoryItemFilter> itemFilters)
            {
                foreach(IMyConveyorSorter sorter in this.InputSorters)
                {
                    sorter.SetFilter(mode, itemFilters);
                }
            }
        }
    }
}

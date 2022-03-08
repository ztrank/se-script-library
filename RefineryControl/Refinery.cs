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
        /// Refinery class.
        /// </summary>
        public class Refinery : IComparable<Refinery>
        {
            /// <summary>
            /// Internal list of filter items.
            /// </summary>
            private readonly List<MyInventoryItemFilter> FilterList = new List<MyInventoryItemFilter>();

            /// <summary>
            /// Gets or sets the refinery block.
            /// </summary>
            public IMyRefinery RefineryBlock { get; set; }

            /// <summary>
            /// Gets or sets the input sorter.
            /// </summary>
            public IMyConveyorSorter InputSorter { get; set; }

            /// <summary>
            /// Gets or sets the output sorter.
            /// </summary>
            public IMyConveyorSorter OutputSorter { get; set; }

            /// <summary>
            /// Gets the list of filter items.
            /// </summary>
            public List<MyInventoryItemFilter> Filter
            {
                get
                {
                    this.FilterList.Clear();
                    this.InputSorter.GetFilterList(this.FilterList);
                    return this.FilterList;
                }
            }

            /// <summary>
            /// Gets the sort mode.
            /// </summary>
            public MyConveyorSorterMode Mode
            {
                get
                {
                    return this.InputSorter.Mode;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this class is valid or not, i.e. has a refinery and, input and output sorters.
            /// </summary>
            public bool IsValid
            {
                get
                {
                    return this.RefineryBlock != null && this.InputSorter != null && this.OutputSorter != null;
                }
            }

            public string Status
            {
                get
                {
                    if (!this.RefineryBlock.Enabled)
                    {
                        return "OFF";
                    }

                    if (!this.RefineryBlock.IsWorking)
                    {
                        return "ERR";
                    }

                    if (this.RefineryBlock.IsProducing)
                    {
                        return "RUNNING";
                    }

                    return "IDLE";
                }
            }

            /// <summary>
            /// Sets the refinery to allow it to process all ores.
            /// </summary>
            public void AllowAll()
            {
                this.InputSorter.SetFilter(MyConveyorSorterMode.Blacklist, null);
                this.OutputSorter.SetFilter(MyConveyorSorterMode.Blacklist, null);
            }

            /// <summary>
            /// Sets the refinery to allow only items in the list.
            /// </summary>
            /// <param name="items">Items to allow.</param>
            public void Allow(List<MyInventoryItemFilter> items)
            {
                this.InputSorter.SetFilter(MyConveyorSorterMode.Whitelist, items);
                this.OutputSorter.SetFilter(MyConveyorSorterMode.Blacklist, null);
                this.OutputSorter.DrainAll = true;
            }

            /// <summary>
            /// Sets the refinery to not allow items in the list.
            /// </summary>
            /// <param name="items">Items to disallow.</param>
            public void Disallow(List<MyInventoryItemFilter> items)
            {
                this.InputSorter.SetFilter(MyConveyorSorterMode.Blacklist, items);
                this.OutputSorter.SetFilter(MyConveyorSorterMode.Blacklist, null);
                this.OutputSorter.DrainAll = true;
            }

            public bool IsSameRefinery(Refinery that)
            {
                return this.RefineryBlock.EntityId == that.RefineryBlock.EntityId;
            }

            public bool IsSame(Refinery that)
            {
                if (!this.IsSameRefinery(that))
                {
                    return false;
                }

                if (this.InputSorter.EntityId != that.InputSorter.EntityId)
                {
                    return false;
                }

                if (this.OutputSorter.EntityId != that.OutputSorter.EntityId)
                {
                    return false;
                }

                return true;
            }

            public int CompareTo(Refinery other)
            {
                if (other == null)
                {
                    return 1;
                }

                if (this == other || this.IsSame(other))
                {
                    return 0;
                }

                return this.RefineryBlock.CustomName.CompareTo(other.RefineryBlock.CustomName);
            }
        }
    }
}

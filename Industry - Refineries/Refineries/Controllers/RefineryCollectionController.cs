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
        public class RefineryCollectionController : UserInterface.SelectionController<RefineryGroup, RefineryCollection, RefineryCollectionModel, RefineryCollectionView>
        {
            /// <summary>
            /// Create the Refinery Collection Controller.
            /// </summary>
            /// <param name="surface">Text Surface reference.</param>
            /// <param name="gridTerminalSystem">Grid Terminal System.</param>
            /// <param name="cpu">Programmable block.</param>
            public RefineryCollectionController(IMyTextSurface surface, IMyGridTerminalSystem gridTerminalSystem, IMyProgrammableBlock cpu)
                : base(surface, gridTerminalSystem, cpu)
            {
                this.View = new RefineryCollectionView(surface);
            }

            /// <summary>
            /// Gets the refinery collection view.
            /// </summary>
            protected override RefineryCollectionView View { get; }

            /// <summary>
            /// Gets the refinery collection model.
            /// </summary>
            protected override RefineryCollectionModel Model { get; } = new RefineryCollectionModel();

            /// <summary>
            /// Queries the grid for refinery groups.
            /// </summary>
            public void QueryGrid()
            {
                this.groups.Clear();
                this.Model.Collection.Values.Clear();
                this.GridTerminalSystem.GetBlockGroups(groups, this.IsValidBlockGroup);
                this.Model.Collection.Values.AddRange(groups.Select(g => new RefineryGroup().Populate(g)));
                this.Rerender();
            }

            public void GetSelected(List<RefineryGroup> groups)
            {
                groups.AddRange(this.Model.Collection.Values.Where(g => g.IsSelected));
            }

            /// <summary>
            /// Validates the block group is a valid refinery group.
            /// </summary>
            /// <param name="group">Group to check.</param>
            /// <returns>True if the group is on this grid, and has a refinery, input, and output sorters.</returns>
            private bool IsValidBlockGroup(IMyBlockGroup group)
            {
                this.blocks.Clear();
                group.GetBlocks(this.blocks, b => b.IsSameConstructAs(this.CPU) && (b is IMyRefinery || b is IMyConveyorSorter));

                bool hasRefinery = false;
                bool hasInput = false;
                bool hasOutput = false;

                this.blocks.ForEach(block =>
                {
                    if (block is IMyRefinery)
                    {
                        hasRefinery = true;
                    }
                    else if (block is IMyConveyorSorter)
                    {
                        if (block.CustomName.ToLower().Contains("input"))
                        {
                            hasInput = true;
                        }
                        else if (block.CustomName.ToLower().Contains("output"))
                        {
                            hasOutput = true;
                        }
                    }
                });

                return hasRefinery && hasInput && hasOutput;
            }
        }
    }
}

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
        public class OreFilterCollectionController : UserInterface.SelectionController<OreFilter, OreFilterCollection, OreFilterCollectionModel, OreFilterCollectionView>
        {
            /// <summary>
            /// Creates a new instance of the OreFilterCollectionController.
            /// </summary>
            /// <param name="surface">Text Surface.</param>
            /// <param name="gridTerminalSystem">Grid Terminal System.</param>
            /// <param name="cpu">Programmable Block.</param>
            public OreFilterCollectionController(IMyTextSurface surface, IMyGridTerminalSystem gridTerminalSystem, IMyProgrammableBlock cpu) : base(surface, gridTerminalSystem, cpu)
            {
                this.View = new OreFilterCollectionView(surface);
            }

            /// <summary>
            /// Gets the controller's view.
            /// </summary>
            protected override OreFilterCollectionView View { get; }

            /// <summary>
            /// Gets the controller's model.
            /// </summary>
            protected override OreFilterCollectionModel Model { get; } = new OreFilterCollectionModel();

            /// <summary>
            /// Sets the selected ore filters.
            /// </summary>
            /// <param name="filters">List of ore filters selected.</param>
            public void SetSelected(List<MyInventoryItemFilter> filters)
            {
                foreach(OreFilter oreFilter in this.Model.Collection.Values)
                {
                    oreFilter.IsSelected = false;
                }

                foreach(MyInventoryItemFilter filter in filters)
                {
                    OreFilter oreFilter = this.Model.Collection[filter.ItemId];
                    if (oreFilter != null)
                    {
                        oreFilter.IsSelected = true;
                    }
                }

                this.Rerender();
            }
        }
    }
}

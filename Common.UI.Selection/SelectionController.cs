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
        public partial class UserInterface
        {
            /// <summary>
            /// Selection Controller.
            /// </summary>
            /// <typeparam name="T">Type of the ISelectable.</typeparam>
            /// <typeparam name="C">Type of the ISelectableCollection</typeparam>
            /// <typeparam name="M">Type of the ISelectableCollectionModel</typeparam>
            /// <typeparam name="V">Type of the SelectionView</typeparam>
            public abstract class SelectionController<T, C, M, V> : Controller<V, M>
                where T : ISelectable
                where C : ISelectableCollection<T>
                where M : ISelectableCollectionModel<T, C>
                where V : SelectionView<T, C, M>
            {
                /// <summary>
                /// Creates a new instance of the selection controller.
                /// </summary>
                /// <param name="surface">Text Surface.</param>
                /// <param name="gridTerminalSystem">Grid Terminal System.</param>
                /// <param name="cpu">Programmable block.</param>
                protected SelectionController(IMyTextSurface surface, IMyGridTerminalSystem gridTerminalSystem, IMyProgrammableBlock cpu)
                    : base(surface, gridTerminalSystem, cpu)
                {
                }

                /// <summary>
                /// Moves the cursor up (negative index) and clamps it at or above 0. Recalculates the visible indexes.
                /// </summary>
                public void Up()
                {
                    this.Model.CursorIndex--;

                    if (this.Model.CursorIndex < 0)
                    {
                        this.Model.CursorIndex = 0;
                    }

                    if (this.Model.CursorIndex < this.Model.TopIndex)
                    {
                        this.Model.TopIndex = this.Model.CursorIndex;
                        int maxRows = this.View.MaxVisibleRows(this.Model);
                        this.Model.BottomIndex = this.Model.TopIndex + maxRows;
                    }

                    this.Rerender();
                }

                /// <summary>
                /// Moves the cursor down (positive index) and clamps it to below the collection count. Recalculates the visible indexes.
                /// </summary>
                public void Down()
                {
                    this.Model.CursorIndex++;

                    if (this.Model.CursorIndex >= this.Model.Collection.Values.Count)
                    {
                        this.Model.CursorIndex = this.Model.Collection.Values.Count - 1;
                    }

                    if (this.Model.CursorIndex > this.Model.BottomIndex)
                    {
                        this.Model.BottomIndex = this.Model.CursorIndex;
                        int maxRows = this.View.MaxVisibleRows(this.Model);
                        this.Model.TopIndex = this.Model.BottomIndex - maxRows;
                    }

                    this.Rerender();
                }

                /// <summary>
                /// Changes the selection mode.
                /// </summary>
                public void ChangeSelectionMode()
                {
                    this.Model.SelectionMode = this.Model.SelectionMode == SelectionMode.Multiple ? SelectionMode.Single : SelectionMode.Multiple;
                    this.Rerender();
                }

                /// <summary>
                /// Toggles the selection value of the selected item. On single select mode, deselects all others if selection is positive.
                /// </summary>
                public void ToggleSelect()
                {
                    this.Model.Collection.Values[this.Model.CursorIndex].IsSelected = !this.Model.Collection.Values[this.Model.CursorIndex].IsSelected;

                    if (this.Model.SelectionMode == SelectionMode.Single && this.Model.Collection.Values[this.Model.CursorIndex].IsSelected)
                    {
                        for (int i = 0; i < this.Model.Collection.Values.Count; i++)
                        {
                            if (i == this.Model.CursorIndex)
                            {
                                continue;
                            }

                            this.Model.Collection.Values[this.Model.CursorIndex].IsSelected = false;
                        }
                    }

                    this.Rerender();
                    this.Emit("selection", this.Model);
                }

                /// <summary>
                /// Selects all if the selection mode is multiple.
                /// </summary>
                public void SelectAll()
                {
                    if (this.Model.SelectionMode == SelectionMode.Multiple)
                    {
                        this.Model.Collection.Values.ForEach(m => m.IsSelected = true);
                        this.Rerender();
                        this.Emit("selection", this.Model);
                    }
                }

                /// <summary>
                /// Deselects all.
                /// </summary>
                public void DeselectAll()
                {
                    this.Model.Collection.Values.ForEach(m => m.IsSelected = false);
                    this.Rerender();
                    this.Emit("selection", this.Model);
                }
            }
        }
    }
}

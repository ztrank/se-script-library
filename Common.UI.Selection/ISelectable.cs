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
            /// Selectable interface.
            /// </summary>
            public interface ISelectable
            {
                /// <summary>
                /// Gets or sets a value indicating whether the row is selected or not.
                /// </summary>
                bool IsSelected { get; set; }
            }

            /// <summary>
            /// Selectable collection.
            /// </summary>
            /// <typeparam name="T">Type of selectable value.</typeparam>
            public interface ISelectableCollection<T>
                where T : ISelectable
            {
                /// <summary>
                /// Gets the list of selectable values.
                /// </summary>
                List<T> Values { get; }
            }

            /// <summary>
            /// Selectable Collection Model.
            /// </summary>
            /// <typeparam name="T">Type of the selectable.</typeparam>
            /// <typeparam name="V">Type of the selectable collection.</typeparam>
            public interface ISelectableCollectionModel<T, V>
                where T : ISelectable
                where V : ISelectableCollection<T>
            {
                /// <summary>
                /// Gets the collection.
                /// </summary>
                V Collection { get; set; }

                /// <summary>
                /// Gets or sets the top index.
                /// </summary>
                int TopIndex { get; set; }

                /// <summary>
                /// Gets or sets the bottom index.
                /// </summary>
                int BottomIndex { get; set; }

                /// <summary>
                /// Gets or sets the cursor index.
                /// </summary>
                int CursorIndex { get; set; }

                /// <summary>
                /// Gets or sets the selection mode.
                /// </summary>
                SelectionMode SelectionMode { get; set; }

                /// <summary>
                /// Gets or sets the font id.
                /// </summary>
                string FontId { get; set; }

                /// <summary>
                /// Gets or sets the scale.
                /// </summary>
                float Scale { get; set; }
            }
        }
    }
}

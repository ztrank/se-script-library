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
            /// Selection model for selection views.
            /// </summary>
            /// <typeparam name="T">Type of collection value.</typeparam>
            /// <typeparam name="V">Type of collection.</typeparam>
            public partial class SelectionModel<T, V> : ISelectableCollectionModel<T, V>
                where T : ISelectable
                where V : ISelectableCollection<T>
            {
                /// <summary>
                /// Gets the collection.
                /// </summary>
                public V Collection { get; set; }

                /// <summary>
                /// Gets or sets the top index value.
                /// </summary>
                public int TopIndex { get; set; } = 0;

                /// <summary>
                /// Gets or sets the bottom index value.
                /// </summary>
                public int BottomIndex { get; set; } = 0;

                /// <summary>
                /// Gets or sets the cursor index value.
                /// </summary>
                public int CursorIndex { get; set; } = 0;

                /// <summary>
                /// Gets or sets the selection mode.
                /// </summary>
                public SelectionMode SelectionMode { get; set; } = SelectionMode.Single;

                /// <summary>
                /// Gets or sets the font id.
                /// </summary>
                public string FontId { get; set; } = "White";

                /// <summary>
                /// Gets or sets the text color.
                /// </summary>
                public Color TextColor { get; set; } = Color.White;

                /// <summary>
                /// Gets or sets the scale.
                /// </summary>
                public float Scale { get; set; } = 1f;
            }
        }
    }
}

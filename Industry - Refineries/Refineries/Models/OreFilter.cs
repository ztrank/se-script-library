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
        public class OreFilter : UserInterface.ISelectable
        {
            /// <summary>
            /// Creates a new instance of an ore filter with the given name.
            /// </summary>
            /// <param name="name">Ore name.</param>
            public OreFilter(string name)
            {
                this.Name = name;
                this.DefinitionId = MyDefinitionId.Parse($"MyObjectBuilder_Ore/{this.Name}");
            }

            /// <summary>
            /// Gets or sets a value indicating whether the ore filter is selected or not.
            /// </summary>
            public bool IsSelected { get; set; } = false;

            /// <summary>
            /// Gets the ore name.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets or sets the type name for the sorter filter.
            /// </summary>
            public MyDefinitionId DefinitionId { get; }
        }
    }
}

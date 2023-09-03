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
        public class RefineryCollection : UserInterface.ISelectableCollection<RefineryGroup>
        {
            /// <summary>
            /// Gets refineries with the same name.
            /// </summary>
            /// <param name="name">Name of refinery group.</param>
            /// <returns>Refinery Group.</returns>
            public RefineryGroup this[string name] => this.Values.Find(r => r.Name == name);

            /// <summary>
            /// Gets the list of refineries.
            /// </summary>
            public List<RefineryGroup> Values { get; } = new List<RefineryGroup>();
        }
    }
}

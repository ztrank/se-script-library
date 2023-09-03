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
        public class OreFilterCollection : UserInterface.ISelectableCollection<OreFilter>
        {
            /// <summary>
            /// Gets the ore filter with the given name.
            /// </summary>
            /// <param name="name">Ore name.</param>
            /// <returns>Ore Filter.</returns>
            public OreFilter this[string name] => this.Values.Find(f => f.Name == name);

            /// <summary>
            /// Gets the ore filter with matching definition id.
            /// </summary>
            /// <param name="definitionId">Definition Id.</param>
            /// <returns>Ore filter.</returns>
            public OreFilter this[MyDefinitionId definitionId] => this.Values.Find(f => f.DefinitionId == definitionId);

            /// <summary>
            /// Gets the list of ore filters.
            /// </summary>
            public List<OreFilter> Values { get; } = new List<OreFilter>()
            {
                new OreFilter("Cobalt"),
                new OreFilter("Gold"),
                new OreFilter("Ice"),
                new OreFilter("Iron"),
                new OreFilter("Magnesium"),
                new OreFilter("Nickel"),
                new OreFilter("Organic"),
                new OreFilter("Platinum"),
                new OreFilter("Scrap"),
                new OreFilter("Silicon"),
                new OreFilter("Silver"),
                new OreFilter("Stone"),
                new OreFilter("Uranium")
            };
        }
    }
}

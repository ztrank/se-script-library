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
        /// Inventory Settings.
        /// </summary>
        public class InventorySettings
        {
            /// <summary>
            /// Invalid Inventory Settings.
            /// </summary>
            public class InvalidInventorySettingsException : Exception
            {
            }

            /// <summary>
            /// Display Settings for the inventory controller.
            /// </summary>
            public class InventorySettingsDisplay
            {
                /// <summary>
                /// Creates a new instance.
                /// </summary>
                /// <param name="name">Name of the block.</param>
                /// <param name="index">Surface index.</param>
                public InventorySettingsDisplay(string name, int index)
                {
                    this.BlockName = name;
                    this.SurfaceIndex = index;
                }

                /// <summary>
                /// Gets the block name.
                /// </summary>
                public string BlockName { get; private set; }

                /// <summary>
                /// Gets the surface index.
                /// </summary>
                public int SurfaceIndex { get; private set; }

                /// <summary>
                /// Gets the section name.
                /// </summary>
                public string Section
                {
                    get
                    {
                        return $"display-{this.SurfaceIndex}";
                    }
                }
            }

            /// <summary>
            /// Creates a new instance of the inventory settings.
            /// </summary>
            /// <param name="block">Terminal Block.</param>
            public InventorySettings(IMyTerminalBlock block, Action<string> log)
            {
                MyIni ini = new MyIni();
                if (ini.TryParse(block.CustomData))
                {

                    string name = ini.Get("inventory", "sReferenceName").ToString();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        throw new InvalidInventorySettingsException();
                    }

                    name = name.StartsWith("\"") ? name.Substring(1) : name;
                    this.ReferenceName = name.EndsWith("\"") ? name.Substring(0, name.Length - 1) : name;
                    this.ReferenceInventoryIndex = ini.Get("inventory", "iReferenceInventoryIndex").ToInt32(0);
                    this.ShowDisconnectedCargo = ini.Get("inventory", "bDisconnectedCargo").ToBoolean(false);
                    this.ShowDisconnectedOxygen = ini.Get("inventory", "bDisconnectedOxygen").ToBoolean(false);
                    this.ShowDisconnectedHydrogen = ini.Get("inventory", "bDisconnectedHydrogen").ToBoolean(false);
                    this.ShowDisconnectedReactor = ini.Get("inventory", "bDisconnectedReactor").ToBoolean(false);
                    this.IndicatorThresholds = new Dictionary<string, ResourceLevelIndicator.DisplayColorThresholds>()
                    {
                        { "cargo", new ResourceLevelIndicator.DisplayColorThresholds(ini, "inventory", "Cargo", -1, -1, .9f, .95f) },
                        { "oxygen", new ResourceLevelIndicator.DisplayColorThresholds(ini, "inventory", "Oxygen", .1f, .2f) },
                        { "hydrogen", new ResourceLevelIndicator.DisplayColorThresholds(ini, "inventory", "Hydrogen", .1f, .2f) },
                        { "reactor", new ResourceLevelIndicator.DisplayColorThresholds(ini, "inventory", "Reactor", .1f, .2f) }
                    };

                    this.Displays = new List<InventorySettingsDisplay>();
                    ArrayConverter arrayConverter = new ArrayConverter();
                    List<string> displayItems = new List<string>();
                    displayItems.AddRange(arrayConverter.Deserialize(ini.Get("inventory", "rgDisplays").ToString("[]")));
                    List<string> innerParts = new List<string>();
                    foreach (string item in displayItems)
                    {
                        innerParts.Clear();
                        string panelName;
                        int panelIndex = 0;

                        if (item.StartsWith("["))
                        {
                            log("Parsing inner item: " + item);
                            innerParts.AddRange(arrayConverter.Deserialize(item));
                            log("Parsed inner itme: " + string.Join(",", innerParts));
                            panelName = innerParts[0];
                            log("Panel Name: " + panelName);
                            if (innerParts.Count > 1)
                            {
                                int.TryParse(innerParts[1], out panelIndex);
                            }
                            log("Panel Index: " + panelIndex);
                        }
                        else
                        {
                            panelName = item;
                        }
                        log("Adding Panel");
                        panelName = panelName.Replace("\"", "");
                        this.Displays.Add(new InventorySettingsDisplay(panelName, panelIndex));
                        log("Added Panel");
                    }
                }

                log("InventorySettings Initialized");
            }

            /// <summary>
            /// Gets the reference block name.
            /// </summary>
            public string ReferenceName { get; private set; }

            /// <summary>
            /// Gets the reference inventory index.
            /// </summary>
            public int ReferenceInventoryIndex { get; private set; }

            /// <summary>
            /// Gets a value indicating whether to include cargo containers not connected to the reference block.
            /// </summary>
            public bool ShowDisconnectedCargo { get; private set; }

            /// <summary>
            /// Gets a value indicating whether to include oxygen tanks not connected to the reference block.
            /// </summary>
            public bool ShowDisconnectedOxygen { get; private set; }

            /// <summary>
            /// Gets a value indicating whether to include hydrogen tanks not connected to the reference block.
            /// </summary>
            public bool ShowDisconnectedHydrogen { get; private set; }

            /// <summary>
            /// Gets a value indicating whether to include reactors not connected to the reference block.
            /// </summary>
            public bool ShowDisconnectedReactor { get; private set; }

            /// <summary>
            /// Gets a dictionary of inventory types to their color display thresholds.
            /// </summary>
            public Dictionary<string, ResourceLevelIndicator.DisplayColorThresholds> IndicatorThresholds { get; private set; }
            
            /// <summary>
            /// Gets a list of inventory displays.
            /// </summary>
            public List<InventorySettingsDisplay> Displays { get; private set; }
        }
    }
}

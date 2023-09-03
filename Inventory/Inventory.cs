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
        public partial class Inventory 
        {
            /// <summary>
            /// Grid terminal system.
            /// </summary>
            private readonly IMyGridTerminalSystem GridTerminalSystem;

            /// <summary>
            /// Programmable block.
            /// </summary>
            private readonly IMyProgrammableBlock Me;

            /// <summary>
            /// Log Action.
            /// </summary>
            private readonly Action<string> Log;

            /// <summary>
            /// Error action.
            /// </summary>
            private readonly Action<string> Error;

            /// <summary>
            /// List of all inventories.
            /// </summary>
            private readonly List<IMyInventory> Inventories = new List<IMyInventory>();

            /// <summary>
            /// List of all hydrogen tanks.
            /// </summary>
            private readonly List<IMyGasTank> HydrogenTanks = new List<IMyGasTank>();

            /// <summary>
            /// List of all oxygen tanks.
            /// </summary>
            private readonly List<IMyGasTank> OxygenTanks = new List<IMyGasTank>();

            /// <summary>
            /// List of all reactors.
            /// </summary>
            private readonly List<IMyReactor> Reactors = new List<IMyReactor>();

            /// <summary>
            /// List of inventory displays.
            /// </summary>
            private readonly List<InventoryDisplay> Displays = new List<InventoryDisplay>();

            /// <summary>
            /// Inventory Settings.
            /// </summary>
            private InventorySettings Settings;

            /// <summary>
            /// Reference inventory.
            /// </summary>
            private IMyInventory ReferenceInventory;

            /// <summary>
            /// Creates a new instance of the inventory class.
            /// </summary>
            /// <param name="gridTerminalSystem">Grid Terminal System.</param>
            /// <param name="me">Programmable Block.</param>
            /// <param name="log">Log Action.</param>
            /// <param name="error">Error Action.</param>
            public Inventory(IMyGridTerminalSystem gridTerminalSystem, IMyProgrammableBlock me, Action<string> log, Action<string> error)
            {
                this.GridTerminalSystem = gridTerminalSystem;
                this.Me = me;
                this.Log = log;
                this.Error = error;
            }

            /// <summary>
            /// Gets the cargo fill ratio.
            /// </summary>
            private float CargoRatio
            {
                get
                {
                    return GetFilledRatio(this.Inventories);
                }
            }

            /// <summary>
            /// Gets the hydrogen fill ratio.
            /// </summary>
            private float HydrogenRatio
            {
                get
                {
                    return GetFilledRatio(this.HydrogenTanks);
                }
            }

            /// <summary>
            /// Gets the oxygen fill ratio.
            /// </summary>
            private float OxygenRatio
            {
                get
                {
                    return GetFilledRatio(this.OxygenTanks);
                }
            }

            /// <summary>
            /// Gets the reactor fill ratio.
            /// </summary>
            private float ReactorRatio
            {
                get
                {
                    return GetFilledRatio(this.Reactors.Select(r => r.GetInventory()));
                }
            }

            /// <summary>
            /// Directs the displays to paint their sprites.
            /// </summary>
            public void Paint()
            {
                float cargoRatio = this.CargoRatio;
                float hydrogenRatio = this.HydrogenRatio;
                float oxygenRatio = this.OxygenRatio;
                float reactorRatio = this.ReactorRatio;

                foreach (InventoryDisplay display in this.Displays)
                {
                    display.Paint(cargoRatio, hydrogenRatio, oxygenRatio, reactorRatio);
                }
            }

            /// <summary>
            /// Initializes the Inventory Controller.
            /// </summary>
            /// <returns>Inventory Controller.</returns>
            public Inventory Initialize()
            {
                this.Inventories.Clear();
                this.OxygenTanks.Clear();
                this.HydrogenTanks.Clear();
                this.Reactors.Clear();
                this.Displays.Clear();
                this.Settings = new InventorySettings(this.Me, this.Log);
                this.ReferenceInventory = this.QueryReferenceInventory();
                this.QueryBlocks();
                this.QueryDisplays();

                this.Log("Reference Block: " + this.Settings.ReferenceName);
                this.Log("Displays: " + this.Displays.Count);
                this.Log("Inventories: " + this.Inventories.Count);
                this.Log("Hydrogen Tanks: " + this.HydrogenTanks.Count);
                this.Log("Oxygen Tanks: " + this.OxygenTanks.Count);
                this.Log("Reactors: " + this.Reactors.Count);

                return this;
            }

            /// <summary>
            /// Tries to empty the containers in this inventory into cargo containers or connectors on the connected ship.
            /// </summary>
            public void TransferGrids(IMyShipConnector connector)
            {
                List<IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();
                this.GridTerminalSystem.GetBlocks(terminalBlocks);
                terminalBlocks.RemoveAll(block => !block.IsSameConstructAs(connector) || 
                    !(block is IMyCargoContainer || block is IMyShipConnector) ||
                    !block.HasInventory || 
                    !block.GetInventory().IsConnectedTo(connector.GetInventory()));

                terminalBlocks.OrderBy(b => b.GetInventory().MaxVolume).Reverse();

                IEnumerable<IMyInventory> destinations = terminalBlocks.Select(b => b.GetInventory());

                EmptyInventory(this.Inventories, destinations);
            }

            /// <summary>
            /// Queries the reference block and inventory.
            /// </summary>
            /// <returns>Reference Inventory.</returns>
            private IMyInventory QueryReferenceInventory()
            {
                IMyTerminalBlock referenceBlock = this.GridTerminalSystem.GetBlockWithName(this.Settings.ReferenceName);
                if (referenceBlock == null)
                {
                    throw new Exception($"No Main Inventory block with name {this.Settings.ReferenceName} found.");
                }

                IMyInventory referenceInventory = referenceBlock.GetInventory(this.Settings.ReferenceInventoryIndex);
                if (referenceInventory == null)
                {
                    throw new Exception($"No Inventory on main block {this.Settings.ReferenceName} at index {this.Settings.ReferenceInventoryIndex}.");
                }

                return referenceInventory;
            }

            /// <summary>
            /// Queries and sets the various inventory blocks.
            /// </summary>
            private void QueryBlocks()
            {
                List<IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();
                this.GridTerminalSystem.GetBlocks(terminalBlocks);
                foreach (IMyTerminalBlock block in terminalBlocks)
                {
                    if (!block.IsSameConstructAs(this.Me))
                    {
                        continue;
                    }

                    // Add hydrogen and oxygen tanks.
                    if (block is IMyGasTank)
                    {
                        if (block.DefinitionDisplayNameText.Contains("Oxygen") && (this.Settings.ShowDisconnectedOxygen || block.GetInventory().IsConnectedTo(this.ReferenceInventory)))
                        {
                            this.OxygenTanks.Add((IMyGasTank)block);
                        }
                        else if (this.Settings.ShowDisconnectedHydrogen || block.GetInventory().IsConnectedTo(this.ReferenceInventory))
                        {
                            this.HydrogenTanks.Add((IMyGasTank)block);
                        }
                        continue;
                    }

                    // Add reactors
                    if (block is IMyReactor && (this.Settings.ShowDisconnectedReactor || block.GetInventory().IsConnectedTo(this.ReferenceInventory)))
                    {
                        this.Reactors.Add((IMyReactor)block);
                        continue;
                    }

                    // Remove ejectors and sorters
                    // TODO Make this optional
                    if (block is IMyConveyorSorter || block.CustomName.ToLower().Contains("ejector"))
                    {
                        continue;
                    }

                    // Add all connected inventories.
                    if (block.InventoryCount > 0)
                    {
                        for (int i = 0; i < block.InventoryCount; i++)
                        {
                            IMyInventory inventory = block.GetInventory(i);
                            if (this.Settings.ShowDisconnectedCargo || inventory.IsConnectedTo(this.ReferenceInventory))
                            {
                                this.Inventories.Add(inventory);
                            }
                        }
                    }
                    else if (block.GetInventory() != null && (this.Settings.ShowDisconnectedCargo || block.GetInventory().IsConnectedTo(this.ReferenceInventory)))
                    {
                        this.Inventories.Add(block.GetInventory());
                    }
                }
            }
            
            /// <summary>
            /// Queries the display blocks.
            /// </summary>
            private void QueryDisplays()
            {
                InventoryDisplay.InventoryDisplaySettings displaySettings = new InventoryDisplay.InventoryDisplaySettings()
                {
                    CargoThresholds = this.Settings.IndicatorThresholds["cargo"],
                    HydrogenThresholds = this.Settings.IndicatorThresholds["hydrogen"],
                    OxygenThresholds = this.Settings.IndicatorThresholds["oxygen"],
                    ReactorThresholds = this.Settings.IndicatorThresholds["reactor"]
                };
                foreach (InventorySettings.InventorySettingsDisplay display in this.Settings.Displays)
                {
                    IMyTerminalBlock displayBlock = this.GridTerminalSystem.GetBlockWithName(display.BlockName);

                    if (displayBlock != null && displayBlock.IsSameConstructAs(this.Me))
                    {
                        this.Log("Found Block with Display Name: " + display.BlockName);
                        if (displayBlock is IMyTextSurface)
                        {
                            this.Displays.Add(new InventoryDisplay((IMyTextSurface)displayBlock, displayBlock.CustomData, displaySettings, display.Section));
                        }
                        else if (displayBlock is IMyTextSurfaceProvider)
                        {
                            this.Displays.Add(new InventoryDisplay(((IMyTextSurfaceProvider)displayBlock).GetSurface(display.SurfaceIndex), displayBlock.CustomData, displaySettings, display.Section));
                        }
                        else
                        {
                            this.Error("Block is not a surface provider");
                        }
                    }
                    else
                    {
                        this.Error("No Block found with name: " + display.BlockName);
                    }
                }
            }
        }
    }
}

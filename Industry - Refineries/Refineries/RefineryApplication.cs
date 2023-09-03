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
        public class RefineryApplication : Application
        {
            /// <summary>
            /// Refinery string.
            /// </summary>
            private const string RefineryCmd = "refinery";

            /// <summary>
            /// Ore string.
            /// </summary>
            private const string OreCmd = "ore";

            /// <summary>
            /// Refinery Collection Controller.
            /// </summary>
            private RefineryCollectionController refineryCollectionController;

            /// <summary>
            /// Ore Filter Collection Controller.
            /// </summary>
            private OreFilterCollectionController oreFilterCollectionController;

            /// <summary>
            /// Refinery Button Controls.
            /// </summary>
            private readonly UserInterface.Input refineryButtons;

            /// <summary>
            /// Ore button controls.
            /// </summary>
            private readonly UserInterface.Input oreButtons;

            /// <summary>
            /// Selected refinery groups.
            /// </summary>
            private readonly List<RefineryGroup> selectedRefineryGroups = new List<RefineryGroup>();

            /// <summary>
            /// Selected ore filters.
            /// </summary>
            private readonly List<MyInventoryItemFilter> selectedOreFilters = new List<MyInventoryItemFilter>();

            /// <summary>
            /// If the refinery application is initialized.
            /// </summary>
            private bool IsInitiallyRendered = false;

            /// <summary>
            /// Creates a new instance of the Refinery Application
            /// </summary>
            /// <param name="gridTerminalSystem">Grid Terminal System.</param>
            /// <param name="me">Programmable Block.</param>
            /// <param name="runtime">Runtime Info.</param>
            /// <param name="echo">Echo action.</param>
            /// <param name="refinerySurfaceGrid">Refinery Surface Grid.</param>
            /// <param name="oreSurfaceGrid">Ore Surface Grid.</param>
            /// <param name="debug">Debug flag.</param>
            public RefineryApplication(
                IMyGridTerminalSystem gridTerminalSystem, 
                IMyProgrammableBlock me, 
                IMyGridProgramRuntimeInfo runtime, 
                IMyBlockGroup refinerySurfaceGrid,
                IMyBlockGroup oreSurfaceGrid,
                IMyButtonPanel refineryButtons,
                IMyButtonPanel oreButtons,
                Action<string> echo, bool debug = false)
                : base(gridTerminalSystem, me, runtime, echo, debug)
            {
                this.refineryButtons = new UserInterface.Input(refineryButtons);
                this.oreButtons = new UserInterface.Input(oreButtons);
                this.RefinerySurfaceGrid = new UserInterface.Surface.Grid(refinerySurfaceGrid, this.Me);
                this.OreSurfaceGrid = new UserInterface.Surface.Grid(oreSurfaceGrid, this.Me);
            }

            /// <summary>
            /// Creates a new instance of the Refinery Application
            /// </summary>
            /// <param name="gridTerminalSystem">Grid Terminal System.</param>
            /// <param name="me">Programmable Block.</param>
            /// <param name="runtime">Runtime Info.</param>
            /// <param name="echo">Echo action.</param>
            /// <param name="refinerySurface">Refinery Surface.</param>
            /// <param name="oreSurface">Ore Surface.</param>
            /// <param name="debug">Debug flag.</param>
            public RefineryApplication(
                IMyGridTerminalSystem gridTerminalSystem,
                IMyProgrammableBlock me,
                IMyGridProgramRuntimeInfo runtime,
                IMyTextPanel refinerySurface,
                IMyTextPanel oreSurface,
                IMyButtonPanel refineryButtons,
                IMyButtonPanel oreButtons,
                Action<string> echo, bool debug = false)
                : base(gridTerminalSystem, me, runtime, echo, debug)
            {
                this.refineryButtons = new UserInterface.Input(refineryButtons);
                this.oreButtons = new UserInterface.Input(oreButtons);
                this.RefinerySurface = refinerySurface;
                this.OreSurface = oreSurface;
            }

            /// <summary>
            /// Creates a new instance of the Refinery Application
            /// </summary>
            /// <param name="gridTerminalSystem">Grid Terminal System.</param>
            /// <param name="me">Programmable Block.</param>
            /// <param name="runtime">Runtime Info.</param>
            /// <param name="echo">Echo action.</param>
            /// <param name="refineryGrid">Refinery Grid.</param>
            /// <param name="oreSurface">Ore Surface.</param>
            /// <param name="debug">Debug flag.</param>
            public RefineryApplication(
                IMyGridTerminalSystem gridTerminalSystem,
                IMyProgrammableBlock me,
                IMyGridProgramRuntimeInfo runtime,
                IMyBlockGroup refineryGrid,
                IMyTextPanel oreSurface,
                IMyButtonPanel refineryButtons,
                IMyButtonPanel oreButtons,
                Action<string> echo, bool debug = false)
                : base(gridTerminalSystem, me, runtime, echo, debug)
            {
                this.refineryButtons = new UserInterface.Input(refineryButtons);
                this.oreButtons = new UserInterface.Input(oreButtons);
                this.RefinerySurfaceGrid = new UserInterface.Surface.Grid(refineryGrid, this.Me);
                this.OreSurface = oreSurface;
            }

            /// <summary>
            /// Creates a new instance of the Refinery Application
            /// </summary>
            /// <param name="gridTerminalSystem">Grid Terminal System.</param>
            /// <param name="me">Programmable Block.</param>
            /// <param name="runtime">Runtime Info.</param>
            /// <param name="echo">Echo action.</param>
            /// <param name="refineryGrid">Refinery Grid.</param>
            /// <param name="oreSurface">Ore Surface.</param>
            /// <param name="debug">Debug flag.</param>
            public RefineryApplication(
                IMyGridTerminalSystem gridTerminalSystem,
                IMyProgrammableBlock me,
                IMyGridProgramRuntimeInfo runtime,
                IMyTextPanel refinerySurface,
                IMyBlockGroup oreSurfaceGrid,
                IMyButtonPanel refineryButtons,
                IMyButtonPanel oreButtons,
                Action<string> echo, bool debug = false)
                : base(gridTerminalSystem, me, runtime, echo, debug)
            {
                this.refineryButtons = new UserInterface.Input(refineryButtons);
                this.oreButtons = new UserInterface.Input(oreButtons);
                this.RefinerySurface = refinerySurface;
                this.OreSurfaceGrid = new UserInterface.Surface.Grid(oreSurfaceGrid, this.Me);
            }

            /// <summary>
            /// Initializes the Refinery Application.
            /// </summary>
            protected override void OnInitialize()
            {
                this.AddTrigger(UpdateType.Update100, this.Draw);

                this.InitializeUserInterfaceInput(this.refineryButtons, RefineryCmd);
                this.InitializeUserInterfaceInput(this.oreButtons, OreCmd);
                

                IMyTextSurface refinerySurface = this.RefinerySurface ?? this.RefinerySurfaceGrid.Primary.TextSurface;
                IMyTextSurface oreSurface = this.OreSurface ?? this.OreSurfaceGrid.Primary.TextSurface;

                this.refineryCollectionController = new RefineryCollectionController(refinerySurface, this.GridTerminalSystem, this.Me);
                this.oreFilterCollectionController = new OreFilterCollectionController(oreSurface, this.GridTerminalSystem, this.Me);

                // Add refinery list actions
                this.AddControllerAction(RefineryCmd, "up", this.refineryCollectionController.Up);
                this.AddControllerAction(RefineryCmd, "down", this.refineryCollectionController.Down);
                this.AddControllerAction(RefineryCmd, "enter", this.refineryCollectionController.ToggleSelect);
                this.AddControllerAction(RefineryCmd, "mode", this.refineryCollectionController.ChangeSelectionMode);
                this.AddControllerAction(RefineryCmd, "all", this.refineryCollectionController.SelectAll);
                this.AddControllerAction(RefineryCmd, "none", this.refineryCollectionController.DeselectAll);
                this.AddControllerAction(RefineryCmd, "query", this.refineryCollectionController.QueryGrid);
                this.AddControllerAction(RefineryCmd, "ui-next", this.refineryButtons.NextPage);

                // Add Ore Filter list actions
                this.AddControllerAction(OreCmd, "up", this.oreFilterCollectionController.Up);
                this.AddControllerAction(OreCmd, "down", this.oreFilterCollectionController.Down);
                this.AddControllerAction(OreCmd, "enter", this.oreFilterCollectionController.ToggleSelect);
                this.AddControllerAction(OreCmd, "mode", this.oreFilterCollectionController.ChangeSelectionMode);
                this.AddControllerAction(OreCmd, "all", this.oreFilterCollectionController.SelectAll);
                this.AddControllerAction(OreCmd, "none", this.oreFilterCollectionController.DeselectAll);
                this.AddControllerAction(OreCmd, "ui-next", this.oreButtons.NextPage);

                // Add UI Control Actions
                this.AddAction("ui", this.HandleUiPress);

                // Add event handlers
                this.refineryCollectionController.On(UserInterface.RerenderEvt, this.Draw);
                this.oreFilterCollectionController.On(UserInterface.RerenderEvt, this.Draw);
                this.refineryCollectionController.On(UserInterface.SelectionEvt, this.OnRefinerySelect);
                this.oreFilterCollectionController.On(UserInterface.SelectionEvt, this.OnOreSelect);
                this.refineryButtons.On(UserInterface.ChangeEvt, this.Draw);
                this.oreButtons.On(UserInterface.ChangeEvt, this.Draw);
            }

            /// <summary>
            /// Gets or sets the refinery surface grid.
            /// </summary>
            public UserInterface.Surface.Grid RefinerySurfaceGrid { get; set; }

            /// <summary>
            /// Gets or sets the ore surface grid.
            /// </summary>
            public UserInterface.Surface.Grid OreSurfaceGrid { get; set; }

            /// <summary>
            /// Gets or sets the refinery surface.
            /// </summary>
            public IMyTextSurface RefinerySurface { get; set; }

            /// <summary>
            /// Gets or sets the ore surface.
            /// </summary>
            public IMyTextSurface OreSurface { get; set; }

            /// <summary>
            /// Handles UI button commands by re-executing the application's execute method with the new argument.
            /// </summary>
            /// <param name="command">Command should follow 'ui refinery|ore #'</param>
            private void HandleUiPress(MyCommandLine command)
            {
                int buttonIndex;
                if (int.TryParse(command.Argument(2), out buttonIndex))
                {
                    string argument = null;
                    if (command.Argument(1) == RefineryCmd)
                    {
                        argument = this.refineryButtons.OnPress(buttonIndex);
                    }
                    else if (command.Argument(1) == OreCmd)
                    {
                        argument = this.oreButtons.OnPress(buttonIndex);
                    }

                    if (argument != null)
                    {
                        this.Execute(argument, UpdateType.Trigger);
                    }
                }
            }

            /// <summary>
            /// Initializes the button panels' pages.
            /// </summary>
            /// <param name="input">Button panel input.</param>
            /// <param name="command">Command string.</param>
            private void InitializeUserInterfaceInput(UserInterface.Input input, string command)
            {
                input.Add(
                    new UserInterface.Input.Page()
                        .With(new UserInterface.Input.Button()
                        {
                            Name = "Up",
                            Argument = $"{command} up",
                            Index = 0,
                            RotationOrScale = 6f
                        })
                        .With(new UserInterface.Input.Button()
                        {
                            Name = "Down",
                            Argument = $"{command} down",
                            Index = 1,
                            RotationOrScale = 6f
                        })
                        .With(new UserInterface.Input.Button()
                        {
                            Name = "Enter",
                            Argument = $"{command} enter",
                            Index = 2,
                            RotationOrScale = 6f
                        })
                        .With(new UserInterface.Input.Button()
                        {
                            Name = "Next\nPage",
                            Argument = $"{command} ui-next",
                            Index = 3,
                            RotationOrScale = 6f
                        })
                );

                input.Add(
                    new UserInterface.Input.Page()
                    .With(new UserInterface.Input.Button()
                    {
                        Name = command == RefineryCmd ? "Mode" : null,
                        Argument = command == RefineryCmd ? $"{command} mode" : null,
                        Index = 0,
                        RotationOrScale = 6f
                    })
                    .With(new UserInterface.Input.Button()
                    {
                        Name = "Select\nAll",
                        Argument = $"{command} all",
                        Index = 1,
                        RotationOrScale = 6f
                    })
                    .With(new UserInterface.Input.Button()
                    {
                        Name = "Deselect\nAll",
                        Argument = $"{command} none",
                        Index = 2,
                        RotationOrScale = 5f
                    })
                    .With(new UserInterface.Input.Button()
                    {
                        Name = "Next\nPage",
                        Argument = $"{command} ui-next",
                        Index = 3,
                        RotationOrScale = 6f
                    })
                );

                if (command == RefineryCmd)
                {
                    input.Add(
                        new UserInterface.Input.Page()
                        .With(new UserInterface.Input.Button()
                        {
                            Name = "Query",
                            Argument = $"{command} query",
                            Index = 0,
                            RotationOrScale = 6f
                        })
                        .With(new UserInterface.Input.Button()
                        {
                            Name = null,
                            Index = 1
                        })
                        .With(new UserInterface.Input.Button()
                        {
                            Name = null,
                            Index = 2
                        })
                        .With(new UserInterface.Input.Button()
                        {
                            Name = "Next\nPage",
                            Argument = $"{command} ui-next",
                            Index = 3,
                            RotationOrScale = 6f
                        })
                    );
                }
            }

            /// <summary>
            /// Draws the views.
            /// </summary>
            private void Draw()
            {
                if (!this.IsInitiallyRendered)
                {
                    this.IsInitiallyRendered = true;
                    this.refineryCollectionController.Rerender();
                    this.oreFilterCollectionController.Rerender();
                }

                if (this.RefinerySurfaceGrid != null)
                {
                    foreach(UserInterface.Surface surface in this.RefinerySurfaceGrid.Surfaces)
                    {
                        this.refineryCollectionController.Draw(surface.TextSurface, surface.Offset);
                    }
                }
                
                if (this.OreSurfaceGrid != null)
                {
                    foreach(UserInterface.Surface surface in this.OreSurfaceGrid.Surfaces)
                    {
                        this.oreFilterCollectionController.Draw(surface.TextSurface, surface.Offset);
                    }
                }

                if (this.RefinerySurface != null)
                {
                    this.refineryCollectionController.Draw(Vector2.Zero);
                }

                if (this.OreSurface != null)
                {
                    this.oreFilterCollectionController.Draw(Vector2.Zero);
                }

                this.refineryButtons.Draw();
                this.oreButtons.Draw();
            }

            private void OnRefinerySelect()
            {
                // If Single, get the refinery and update the selected ore
                // If multiple, get the selected, if 1 then set the selected ore, otherwise do nothing
                this.selectedRefineryGroups.Clear();
                this.selectedOreFilters.Clear();
                
                this.refineryCollectionController.GetSelected(this.selectedRefineryGroups);
                
                if (this.selectedRefineryGroups.Any())
                {
                    this.oreFilterCollectionController.SetScreenSaver(false);

                    if (this.selectedRefineryGroups.Count > 1)
                    {
                        this.oreFilterCollectionController.SetSelected(this.selectedOreFilters, true);
                    }
                    else
                    {
                        this.selectedRefineryGroups[0].GetFilters(this.selectedOreFilters);
                        this.oreFilterCollectionController.SetSelected(this.selectedOreFilters, true);
                    }
                }
                else
                {
                    this.oreFilterCollectionController.SetScreenSaver(true);
                    this.oreFilterCollectionController.SetSelected(this.selectedOreFilters, true);
                }

                this.Draw();
            }

            private void OnOreSelect()
            {
                // Get the selected ore
                // Get the selected refineries
                // Set the filters on the selected refineries.
                this.selectedOreFilters.Clear();
                this.selectedRefineryGroups.Clear();
                this.oreFilterCollectionController.GetSelected(this.selectedOreFilters);
                this.refineryCollectionController.GetSelected(this.selectedRefineryGroups);

                foreach(RefineryGroup group in this.selectedRefineryGroups)
                {
                    group.SetFilters(group.Mode, this.selectedOreFilters);
                }
                this.Draw();
            }
        }
    }
}

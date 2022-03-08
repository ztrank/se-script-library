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

    partial class Program : MyGridProgram
    {
        private readonly RefineryController controller;
        private readonly RefineryViewController refineryViewController;
        private readonly RefineryControlInterface refineryControlInterface;
        private readonly MyCommandLine commandLine = new MyCommandLine();
        private readonly MyIni ini = new MyIni();
        private string RefineryDisplayGroupName;
        private string OreDisplayGroupName;

        private int TickCount = 0;
        public Program()
        {
            this.ProgramName = "Refinery Control";
            this.controller = new RefineryController()
            {
                GridTerminalSystem = this.GridTerminalSystem,
                Me = this.Me,
                Stdout = this.Stdout,
                Stderr = this.Stderr
            }.Initialize();

            this.refineryViewController = new RefineryViewController()
            {
                GridTerminalSystem = this.GridTerminalSystem,
                Me = this.Me,
                Stderr = this.Stderr,
                Stdout = this.Stdout
            };

            this.refineryControlInterface = new RefineryControlInterface()
            {
                GridTerminalSystem = this.GridTerminalSystem,
                Me = this.Me,
                Stdout = this.Stdout,
                Stderr = this.Stderr
            };

            if (this.ini.TryParse(this.Me.CustomData))
            {
                this.RefineryDisplayGroupName = this.ini.Get("refinery", "sRefineryDisplayGroupName").ToString();
                this.OreDisplayGroupName = this.ini.Get("refinery", "sOreDisplayGroupName").ToString();

                IMyBlockGroup refineryGroup = this.GridTerminalSystem.GetBlockGroupWithName(this.RefineryDisplayGroupName);
                this.refineryViewController.SetDisplayGrid(refineryGroup, true);
                IMyBlockGroup oreGroup = this.GridTerminalSystem.GetBlockGroupWithName(this.OreDisplayGroupName);
                this.refineryControlInterface.SetDisplayGrid(new DisplayGrid(refineryGroup, this.Stdout));
                this.refineryViewController.SetRefineries(this.controller.RefineryList);
            }

            this.Runtime.UpdateFrequency = UpdateFrequency.Update100;
            // TODO Get and set the display groups.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update100) > 0)
            {
                this.TickCount++;
                this.refineryControlInterface.Paint();
                this.refineryViewController.Paint();

                if (this.TickCount >= 10)
                {
                    this.TickCount = 0;
                    // TODO Reinitialize
                    //this.controller.Initialize();
                }
            }
            else if(this.commandLine.TryParse(argument))
            {
                if (this.commandLine.Argument(0) == "refineryView")
                {
                    if (this.commandLine.Argument(1) == "up")
                    {
                        this.refineryViewController.Up();
                    }
                    else if (this.commandLine.Argument(1) == "down")
                    {
                        this.refineryViewController.Down();
                    }
                    else if (this.commandLine.Argument(1) == "enter")
                    {
                        this.refineryViewController.Select();
                        this.refineryControlInterface.SetRefinery(this.refineryViewController.Selected);
                    }
                }
                else if (this.commandLine.Argument(0) == "controlView")
                {
                    if (this.commandLine.Argument(1) == "up")
                    {
                        this.refineryControlInterface.Up();
                    }
                    else if (this.commandLine.Argument(1) == "down")
                    {
                        this.refineryControlInterface.Down();
                    }
                    else if (this.commandLine.Argument(1) == "enter")
                    {
                        this.refineryControlInterface.Select();
                        if (this.refineryControlInterface.GetMode() == "Allowed")
                        {
                            this.refineryViewController.Selected?.Allow(this.refineryControlInterface.GetFilters());
                        }
                        else
                        {
                            this.refineryViewController.Selected?.Disallow(this.refineryControlInterface.GetFilters());
                        }
                    }
                    else if(this.commandLine.Argument(1) == "switch")
                    {
                        this.refineryControlInterface.ToggleMode();
                    }
                }
            }
        }
    }
}

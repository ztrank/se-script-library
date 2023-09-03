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

    /// <summary>
    /// Industry Program
    /// </summary>
    partial class Program : MyGridProgram
    {
        /// <summary>
        /// Ini reader instance.
        /// </summary>
        private readonly MyIni ini = new MyIni();

        /// <summary>
        /// Command line reader instance.
        /// </summary>
        private readonly MyCommandLine commandLine = new MyCommandLine();

        /// <summary>
        /// Dictionary of available commands.
        /// </summary>
        private readonly Dictionary<string, Action<UpdateType>> Commands = new Dictionary<string, Action<UpdateType>>();

        /// <summary>
        /// Refinery Application.
        /// </summary>
        private readonly RefineryApplication refineryApplication;

        /// <summary>
        /// Program Constructor. Runs at compile, and game session startup.
        /// </summary>
        public Program()
        {
            MyIniParseResult result;

            if (this.ini.TryParse(this.Me.CustomData, out result))
            {
                string refineries = "refineries";
                string invalidRefinery = "Invalid Refinery Configuration.";



                string refineryUIGrid = this.ini.Get(refineries, "refineryUIGrid").ToString();
                string oreUIGrid = this.ini.Get(refineries, "oreUIGrid").ToString();
                string refineryUIPanel = this.ini.Get(refineries, "refineryUIPanel").ToString();
                string oreUIPanel = this.ini.Get(refineries, "oreUIPanel").ToString();
                string refineryButtonName = this.ini.Get(refineries, "refineryButtons").ToString();
                string oreButtonName = this.ini.Get(refineries, "oreButtons").ToString();

                IMyBlockGroup refinerySurfaceGrid = string.IsNullOrWhiteSpace(refineryUIGrid) ? null : this.GridTerminalSystem.GetBlockGroupWithName(refineryUIGrid);
                IMyBlockGroup oreSurfaceGrid = string.IsNullOrWhiteSpace(oreUIGrid) ? null : this.GridTerminalSystem.GetBlockGroupWithName(oreUIGrid);
                IMyTextPanel refinerySurface = string.IsNullOrWhiteSpace(refineryUIPanel) ? null : (IMyTextPanel)this.GridTerminalSystem.GetBlockWithName(refineryUIPanel);
                IMyTextPanel oreSurface = string.IsNullOrWhiteSpace(oreUIPanel) ? null : (IMyTextPanel)this.GridTerminalSystem.GetBlockWithName(oreUIPanel);

                IMyButtonPanel refineryButtons = (IMyButtonPanel)this.GridTerminalSystem.GetBlockWithName(refineryButtonName);
                IMyButtonPanel oreButtons = (IMyButtonPanel)this.GridTerminalSystem.GetBlockWithName(oreButtonName);

                if (oreSurfaceGrid == null && oreSurface == null)
                {
                    throw new Exception($"{invalidRefinery} No Ore text surface or grid.");
                }

                if (refinerySurfaceGrid == null && refinerySurface == null)
                {
                    throw new Exception($"{invalidRefinery} No Refinery text surface or grid.");
                }

                if (refineryButtons == null)
                {
                    throw new Exception($"{invalidRefinery} No Refinery Buttons.");
                }

                if (oreButtons == null)
                {
                    throw new Exception($"{invalidRefinery} No Ore buttons.");
                }

                if (refinerySurfaceGrid != null && oreSurfaceGrid != null)
                {
                    this.refineryApplication = new RefineryApplication(this.GridTerminalSystem, this.Me, this.Runtime, refinerySurfaceGrid, oreSurfaceGrid, refineryButtons, oreButtons, this.Echo);
                }
                else if (refinerySurfaceGrid != null && oreSurfaceGrid == null)
                {
                    this.refineryApplication = new RefineryApplication(this.GridTerminalSystem, this.Me, this.Runtime, refinerySurfaceGrid, oreSurface, refineryButtons, oreButtons, this.Echo);
                }
                else if (refinerySurfaceGrid == null && oreSurfaceGrid != null)
                {
                    this.refineryApplication = new RefineryApplication(this.GridTerminalSystem, this.Me, this.Runtime, refinerySurface, oreSurfaceGrid, refineryButtons, oreButtons, this.Echo);
                }
                else
                {
                    this.refineryApplication = new RefineryApplication(this.GridTerminalSystem, this.Me, this.Runtime, refinerySurface, oreSurface, refineryButtons, oreButtons, this.Echo);
                }

                this.refineryApplication.Initialize();
            }
            else
            {
                this.Stdout($"Unable to read INI: {result}");
                this.Me.GetSurface(0).WriteText($"Unable to read INI: {result}");
            }
            this.Me.GetSurface(0).ContentType = ContentType.NONE;
            this.Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                if (this.commandLine.TryParse(argument))
                {
                    if (this.commandLine.ArgumentCount > 0 && this.Commands.ContainsKey(this.commandLine.Argument(0)))
                    {
                        this.Commands[this.commandLine.Argument(0)].Invoke(updateSource);
                    }
                }

                this.refineryApplication.Execute(argument, updateSource);
            }
            catch (Exception ex)
            {
                this.Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
                this.Me.GetSurface(0).WriteText(ex.Message);
                throw;
            }
        }
    }
}

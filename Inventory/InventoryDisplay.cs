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
        /// Class to hold the inventory display logic.
        /// </summary>
        public class InventoryDisplay
        {
            /// <summary>
            /// Inventory Display Settings.
            /// </summary>
            public class InventoryDisplaySettings
            {
                /// <summary>
                /// Gets or sets the hydrogen thresholds.
                /// </summary>
                public ResourceLevelIndicator.DisplayColorThresholds HydrogenThresholds { get; set; }

                /// <summary>
                /// Gets or sets the oxygen thresholds.
                /// </summary>
                public ResourceLevelIndicator.DisplayColorThresholds OxygenThresholds { get; set; }

                /// <summary>
                /// Gets or sets the cargo thresholds.
                /// </summary>
                public ResourceLevelIndicator.DisplayColorThresholds CargoThresholds { get; set; }

                /// <summary>
                /// Gets or sets the reactor thresholds.
                /// </summary>
                public ResourceLevelIndicator.DisplayColorThresholds ReactorThresholds { get; set; }
            }

            /// <summary>
            /// Text Surface.
            /// </summary>
            private readonly IMyTextSurface Surface;

            /// <summary>
            /// My Ini.
            /// </summary>
            private readonly MyIni ini = new MyIni();

            /// <summary>
            /// Writing Viewport.
            /// </summary>
            private readonly RectangleF ViewPort;

            /// <summary>
            /// Show Cargo.
            /// </summary>
            private readonly bool ShowCargo;

            /// <summary>
            /// Show Hydrogen.
            /// </summary>
            private readonly bool ShowHydrogen;

            /// <summary>
            /// Show Oxygen.
            /// </summary>
            private readonly bool ShowOxygen;

            /// <summary>
            /// Show reactors.
            /// </summary>
            private readonly bool ShowReactors;

            /// <summary>
            /// Cargo Indicator.
            /// </summary>
            private readonly ResourceLevelIndicator Cargo;

            /// <summary>
            /// Hydrogen Indicator.
            /// </summary>
            private readonly ResourceLevelIndicator Hydrogen;

            /// <summary>
            /// Oxygen Indicator.
            /// </summary>
            private readonly ResourceLevelIndicator Oxygen;

            /// <summary>
            /// Reactors indicator.
            /// </summary>
            private readonly ResourceLevelIndicator Reactors;

            /// <summary>
            /// Inventory Display Settings.
            /// </summary>
            private readonly InventoryDisplaySettings settings;

            /// <summary>
            /// Count of indicators.
            /// </summary>
            private readonly int indicators = 0;

            /// <summary>
            /// Warn color.
            /// </summary>
            private Color warn = Color.Yellow;

            /// <summary>
            /// Error color.
            /// </summary>
            private Color error = Color.Red;

            /// <summary>
            /// Texture color.
            /// </summary>
            private Color texture = Color.White;

            /// <summary>
            /// Border color.
            /// </summary>
            private Color border = Color.Teal;

            /// <summary>
            /// Bar color.
            /// </summary>
            private Color bar = Color.Green;

            /// <summary>
            /// Background Color
            /// </summary>
            private Color background = Color.Aquamarine;

            /// <summary>
            /// Cargo Icon Path
            /// </summary>
            private string CargoIcon = @"Textures\FactionLogo\Builders\BuilderIcon_1.dds";

            /// <summary>
            /// Oxygen Icon Path
            /// </summary>
            private string OxygenIcon = "IconOxygen";

            /// <summary>
            /// Reactors Icon Path
            /// </summary>
            private string ReactorsIcon = @"Textures\FactionLogo\Others\OtherIcon_19.dds";

            /// <summary>
            /// Hydrogen Icon Path
            /// </summary>
            private string HydrogenIcon = "IconHydrogen";


            /// <summary>
            /// Creates a new instance of the inventory display.
            /// </summary>
            /// <param name="surface">Surface to draw on.</param>
            /// <param name="customData">Custom data from the block.</param>
            /// <param name="settings">Inventory settings.</param>
            public InventoryDisplay(IMyTextSurface surface, string customData, InventoryDisplaySettings settings, string section)
            {
                this.settings = settings;

                if (this.ini.TryParse(customData))
                {
                    this.ShowCargo = this.ini.Get(section, "cargo").ToBoolean(true);
                    this.ShowHydrogen = this.ini.Get(section, "hydrogen").ToBoolean(true);
                    this.ShowOxygen = this.ini.Get(section, "oxygen").ToBoolean(true);
                    this.ShowReactors = this.ini.Get(section, "reactors").ToBoolean(false);

                    this.background = this.ParseColor(this.ini.Get($"{section}-colors", "background").ToString(), this.background);
                    this.warn = this.ParseColor(this.ini.Get($"{section}-colors", "warn").ToString(), this.warn);
                    this.error = this.ParseColor(this.ini.Get($"{section}-colors", "error").ToString(), this.error);
                    this.texture = this.ParseColor(this.ini.Get($"{section}-colors", "texture").ToString(), this.texture);
                    this.border = this.ParseColor(this.ini.Get($"{section}-colors", "border").ToString(), this.border);
                    this.bar = this.ParseColor(this.ini.Get($"{section}-colors", "bar").ToString(), this.bar);

                    this.OxygenIcon = this.ini.Get($"{section}-icons", "oxygen").ToString(this.OxygenIcon);
                    this.HydrogenIcon = this.ini.Get($"{section}-icons", "hydrogen").ToString(this.HydrogenIcon);
                    this.ReactorsIcon = this.ini.Get($"{section}-icons", "reactors").ToString(this.ReactorsIcon);
                    this.CargoIcon = this.ini.Get($"{section}-icons", "cargo").ToString(this.CargoIcon);
                }
                else
                {
                    this.ShowCargo = true;
                    this.ShowHydrogen = true;
                    this.ShowOxygen = true;
                    this.ShowReactors = false;
                }

                this.Surface = surface;
                this.Surface.ContentType = ContentType.SCRIPT;
                this.Surface.Script = "";
                this.Surface.ScriptBackgroundColor = this.background;
                this.ViewPort = new RectangleF((this.Surface.TextureSize - this.Surface.SurfaceSize) / 2f, this.Surface.SurfaceSize);

                if (this.ShowCargo)
                {
                    this.Cargo = this.CreateIndicator(this.CargoIcon, this.settings.CargoThresholds);
                    this.indicators++;

                }

                if (this.ShowHydrogen)
                {
                    this.Hydrogen = this.CreateIndicator(this.HydrogenIcon, this.settings.HydrogenThresholds);
                    this.indicators++;
                }

                if (this.ShowOxygen)
                {
                    this.Oxygen = this.CreateIndicator(this.OxygenIcon, this.settings.OxygenThresholds);
                    this.indicators++;
                }

                if (this.ShowReactors)
                {
                    this.Reactors = this.CreateIndicator(this.ReactorsIcon, this.settings.ReactorThresholds);
                    this.indicators++;
                }
            }

            /// <summary>
            /// Gets the gutter size.
            /// </summary>
            /// <remarks>
            /// let x = indicator size (64)
            /// let y = gutter size ???
            /// let width = viewport width
            /// width = (# indicators)x + (# indicators + 1)y
            /// y = (width - (#)x) / (#+1)
            /// </remarks>
            public Vector2 Gutter
            {
                get
                {
                    return new Vector2((this.ViewPort.Width - (this.indicators * 64)) / (this.indicators + 1), 0);
                }
            }

            /// <summary>
            /// Paints the indicators into the center of the screen.
            /// </summary>
            /// <param name="cargoRatio">Cargo fill ratio.</param>
            /// <param name="hydrogenRatio">Hydrogen fill ratio.</param>
            /// <param name="oxygenRatio">Oxygen fill ratio.</param>
            public void Paint(float cargoRatio, float hydrogenRatio = 0, float oxygenRatio = 0, float reactorRatio = 0)
            {
                using (MySpriteDrawFrame frame = this.Surface.DrawFrame())
                {
                    Vector2 position = this.ViewPort.Position + new Vector2(0, this.ViewPort.Height / 2f - 25) + this.Gutter;
                    if (this.Cargo != null)
                    {
                        this.Cargo.Ratio = cargoRatio;
                        frame.AddRange(this.Cargo.GenerateSprite(position));
                        position += this.Gutter + new Vector2(64, 0);
                    }

                    if (this.Hydrogen != null)
                    {
                        this.Hydrogen.Ratio = hydrogenRatio;
                        frame.AddRange(this.Hydrogen.GenerateSprite(position));
                        position += this.Gutter + new Vector2(64, 0);
                    }

                    if (this.Oxygen != null)
                    {
                        this.Oxygen.Ratio = oxygenRatio;
                        frame.AddRange(this.Oxygen.GenerateSprite(position));
                        position += this.Gutter + new Vector2(64, 0);
                    }

                    if (this.Reactors != null)
                    {
                        this.Reactors.Ratio = reactorRatio;
                        frame.AddRange(this.Reactors.GenerateSprite(position));
                    }
                }
            }

            /// <summary>
            /// Creates an indicator object.
            /// </summary>
            /// <param name="symbol">Symbol for indicator.</param>
            /// <param name="thresholds">Threshold values.</param>
            /// <returns>Resource Level Indicator.</returns>
            private ResourceLevelIndicator CreateIndicator(string symbol, ResourceLevelIndicator.DisplayColorThresholds thresholds)
            {
                return new ResourceLevelIndicator()
                {
                    BarColor = this.bar,
                    BorderColor = this.border,
                    TextureColor = this.texture,
                    Warning = this.warn,
                    Error = this.error,
                    Symbol = symbol,
                    MinError = thresholds.MinError,
                    MinWarning = thresholds.MinWarn,
                    MaxError = thresholds.MaxError,
                    MaxWarning = thresholds.MaxWarn
                };
            }

            /// <summary>
            /// Parses an (r,g,b,a) or (r,g,b) color
            /// </summary>
            /// <param name="colorString">Color String to Parse</param>
            /// <param name="defaultColor"></param>
            /// <returns></returns>
            private Color ParseColor(string colorString, Color defaultColor)
            {
                try
                {
                    string[] rgba = colorString.Replace("(", "").Replace(")", "").Replace(" ", "").Split(',');

                    if (rgba.Length == 4)
                    {
                        return new Color(int.Parse(rgba[0]), int.Parse(rgba[1]), int.Parse(rgba[2]), int.Parse(rgba[3]));
                    }
                    else if (rgba.Length == 3)
                    {
                        return new Color(int.Parse(rgba[0]), int.Parse(rgba[1]), int.Parse(rgba[2]));
                    }

                    return defaultColor;
                }
                catch
                {
                    return defaultColor;
                }
            }
        }
    }
}

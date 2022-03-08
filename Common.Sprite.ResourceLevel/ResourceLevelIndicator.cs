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
        /// Resource Status Indicator
        /// </summary>
        public partial class ResourceLevelIndicator
        {
            /// <summary>
            /// Class for holding the color threshold values.
            /// </summary>
            public partial class DisplayColorThresholds
            {
                /// <summary>
                /// Creates a new instance with no values set.
                /// </summary>
                public DisplayColorThresholds()
                {
                }


                /// <summary>
                /// Creates a new instance with values from the ini. Fields in the ini should be like: fNameMaxError. e.g. fCargoMaxError.
                /// </summary>
                /// <param name="ini">My Ini.</param>
                /// <param name="section">Section name.</param>
                /// <param name="name">Resource Name</param>
                public DisplayColorThresholds(MyIni ini, string section, string name) : this(ini, section, name, -1, -1, 2, 2)
                {
                }

                /// <summary>
                /// Creates a new instance with values from the ini. Fields in the ini should be like: fNameMaxError. e.g. fCargoMaxError.
                /// </summary>
                /// <param name="ini">My Ini.</param>
                /// <param name="section">Section name.</param>
                /// <param name="name">Resource Name</param>
                /// <param name="minError">Min error number.</param>
                /// <param name="minWarn">Min warn number.</param>
                /// <param name="maxError">Max error number.</param>
                /// <param name="maxWarn">Max warn number.</param>
                public DisplayColorThresholds(MyIni ini, string section, string name, float minError = -1, float minWarn = -1, float maxWarn = 2, float maxError = 2)
                {
                    this.MaxError = (float)ini.Get(section, $"f{name}MaxError").ToDouble(maxError);
                    this.MaxWarn = (float)ini.Get(section, $"f{name}MaxWarn").ToDouble(maxWarn);
                    this.MinError = (float)ini.Get(section, $"f{name}MinError").ToDouble(minError);
                    this.MinWarn = (float)ini.Get(section, $"f{name}MinWarn").ToDouble(minWarn);
                }

                /// <summary>
                /// Gets or sets the minimum warn value.
                /// </summary>
                public float? MinWarn { get; set; }

                /// <summary>
                /// Gets or sets the minimum error value.
                /// </summary>
                public float? MinError { get; set; }

                /// <summary>
                /// Gets or sets the max warn value.
                /// </summary>
                public float? MaxWarn { get; set; }

                /// <summary>
                /// Gets or sets the max error value.
                /// </summary>
                public float? MaxError { get; set; }
            }

            /// <summary>
            /// Viewport rectangle.
            /// </summary>
            private RectangleF Viewport;

            /// <summary>
            /// List of sprites.
            /// </summary>
            private readonly List<MySprite> sprites = new List<MySprite>();

            /// <summary>
            /// Gets or sets the Ratio of full, number between 0 and 1.
            /// </summary>
            public float Ratio { get; set; }

            /// <summary>
            /// Gets or sets the sprite name for the center icon.
            /// </summary>
            public string Symbol { get; set; }

            /// <summary>
            /// Gets or sets the size. Defaults to 64, 128. Note, this is the size of the box, text extends further based on panel text settings.
            /// </summary>
            public Vector2 Size { get; set; } = new Vector2(64, 128);

            /// <summary>
            /// Gets or sets the padding size.
            /// </summary>
            public float Padding { get; set; } = 5f;

            /// <summary>
            /// Gets or sets the value of ratio below which the colors should switch to warning colors.
            /// </summary>
            public float? MinWarning { get; set; }

            /// <summary>
            /// Gets or sets the value of ratio below which the colors should switch to error colors.
            /// </summary>
            public float? MinError { get; set; }

            /// <summary>
            /// Gets or sets the value of ratio above which the colors should switch to warning colors.
            /// </summary>
            public float? MaxWarning { get; set; }

            /// <summary>
            /// Gets or sets the value of ratio above which the colors should switch to error colors.
            /// </summary>
            public float? MaxError { get; set; }

            /// <summary>
            /// Gets or sets the warning color.
            /// </summary>
            public Color Warning { get; set; } = Color.Orange;

            /// <summary>
            /// Gets or sets the error color.
            /// </summary>
            public Color Error { get; set; } = Color.Red;

            /// <summary>
            /// Gets or sets the bar color.
            /// </summary>
            public Color BarColor { get; set; } = Color.Green;

            /// <summary>
            /// Gets or sets the border color.
            /// </summary>
            public Color BorderColor { get; set; } = Color.Teal;

            /// <summary>
            /// Gets or sets the texture color.
            /// </summary>
            public Color TextureColor { get; set; } = Color.White;

            /// <summary>
            /// Generates the list of sprites to be added to the screen.
            /// </summary>
            /// <param name="position">Position of the center left side of this collection of sprites.</param>
            /// <returns>List of sprites to display.</returns>
            public List<MySprite> GenerateSprite(Vector2 position)
            {
                this.sprites.Clear();
                this.Viewport = new RectangleF(position + new Vector2(0, this.Padding), this.Size - new Vector2(this.Padding * 2, this.Padding * 4));
                Vector2 center = position + new Vector2(this.Size.X / 2f, 0);
                this.Ratio = this.Ratio > 1 ? 1 : this.Ratio;
                this.Ratio = this.Ratio < 0 ? 0 : this.Ratio;

                Color boarderColor = this.BorderColor;
                Color textureColor = this.TextureColor;
                if (this.MinError != null && this.Ratio < this.MinError)
                {
                    boarderColor = this.Error;
                    textureColor = this.Error;
                }
                else if (this.MinWarning != null && this.Ratio < this.MinWarning)
                {
                    boarderColor = this.Warning;
                    textureColor = this.Warning;
                }
                else if (this.MaxError != null && this.Ratio > this.MaxError)
                {
                    boarderColor = this.Error;
                    textureColor = this.Error;
                }
                else if (this.MaxWarning != null && this.Ratio > this.MaxWarning)
                {
                    boarderColor = this.Warning;
                    textureColor = this.Warning;
                }

                // Create background
                this.sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareHollow",
                    Position = position,
                    Size = this.Size,
                    Color = boarderColor
                });

                // Create the bars.
                Color clear = this.BarColor.Alpha(0f);

                this.sprites.Add(this.CreateBar(0, this.Viewport.Position, this.Ratio >= 0.2 ? this.BarColor : clear));
                this.sprites.Add(this.CreateBar(1, this.Viewport.Position, this.Ratio >= 0.4 ? this.BarColor : clear));
                this.sprites.Add(this.CreateBar(2, this.Viewport.Position, this.Ratio >= 0.6 ? this.BarColor : clear));
                this.sprites.Add(this.CreateBar(3, this.Viewport.Position, this.Ratio >= 0.8 ? this.BarColor : clear));
                this.sprites.Add(this.CreateBar(4, this.Viewport.Position, this.Ratio >= 0.9 ? this.BarColor : clear));

                if (!string.IsNullOrWhiteSpace(this.Symbol))
                {
                    this.sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = this.Symbol,
                        Alignment = TextAlignment.CENTER,
                        Color = textureColor,
                        Size = new Vector2(this.Viewport.Size.X, this.Viewport.Size.X),
                        Position = position + new Vector2(this.Size.X / 2f, 0)
                    });
                }

                this.sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = ((int)(this.Ratio * 100)).ToString() + "%",
                    Alignment = TextAlignment.CENTER,
                    RotationOrScale = 1f,
                    Color = textureColor,
                    FontId = "White",
                    Position = position + new Vector2(this.Size.X / 2f, this.Size.Y / 2f)
                });

                return this.sprites;
            }

            /// <summary>
            /// Calculates the bar height.
            /// </summary>
            /// <returns>Height of inner bars.</returns>
            private float GetBarHeight()
            {
                return (this.Size.Y - (7 * this.Padding)) / 5;
            }

            /// <summary>
            /// Calculates the bar width.
            /// </summary>
            /// <returns>Width of bar.</returns>
            private float GetBarWidth()
            {
                return this.Viewport.Size.X;
            }

            /// <summary>
            /// Creates a vector2 for the bar size.
            /// </summary>
            /// <returns>Bar size vector2.</returns>
            private Vector2 GetBarSize()
            {
                return new Vector2(this.GetBarWidth(), this.GetBarHeight());
            }

            /// <summary>
            /// Creates a bar.
            /// </summary>
            /// <param name="index">Index from the bottom.</param>
            /// <param name="position">Position of the viewport.</param>
            /// <param name="color">Color of the bar.</param>
            /// <returns></returns>
            private MySprite CreateBar(int index, Vector2 position, Color color)
            {
                float barPositionX = position.X + this.Padding;
                float barPositionY = position.Y + (this.Viewport.Height / 2f) - this.Padding * 2;
                barPositionY -= index * (this.GetBarHeight() + this.Padding);
                Vector2 barPosition = new Vector2(barPositionX, barPositionY);
                return new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Size = this.GetBarSize(),
                    Position = barPosition,
                    Color = color
                };
            }
        }
    }
}

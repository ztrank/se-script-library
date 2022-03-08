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
        /// Display Grid.
        /// </summary>
        public partial class DisplayGrid
        {
            /// <summary>
            /// Invalid Display Grid Exception.
            /// </summary>
            public class InvalidDisplayGridException : Exception
            {
                /// <summary>
                /// Creates a new instance of the invalid display grid exception.
                /// </summary>
                /// <param name="message">Message for exception.</param>
                public InvalidDisplayGridException(string message) : base(message)
                {
                }
            }

            /// <summary>
            /// List of Display Panels.
            /// </summary>
            private readonly List<DisplayPanel> DisplayPanels = new List<DisplayPanel>();

            /// <summary>
            /// Logs the message.
            /// </summary>
            private readonly Action<string> Log;

            /// <summary>
            /// Primary Panel.
            /// </summary>
            private readonly DisplayPanel primaryPanel;

            /// <summary>
            /// String builder.
            /// </summary>
            private readonly StringBuilder stringBuilder = new StringBuilder();

            /// <summary>
            /// Display grid only works with text panels. Other displays such as cockpits will not work.
            /// </summary>
            private readonly List<IMyTextPanel> textPanels = new List<IMyTextPanel>();

            /// <summary>
            /// A value indicating if text sprites should be surrounded by a box. Useful for diagnosing alignment problems.
            /// </summary>
            private bool includeTextBoundaries = false;

            /// <summary>
            /// Creates a new instance of the Display Grid
            /// </summary>
            /// <param name="displayBlock">Block group of display panels.</param>
            /// <param name="log">Action to log a message.</param>
            public DisplayGrid(IMyBlockGroup displayBlock, Action<string> log)
            {
                this.Log = log;
                displayBlock.GetBlocksOfType(this.textPanels);
                if (this.textPanels.Count == 1)
                {
                    DisplayPanel displayPanel = new DisplayPanel(this.textPanels[0], true);
                    this.primaryPanel = displayPanel;
                    this.DisplayPanels.Add(displayPanel);
                }
                else
                {
                    foreach (IMyTextPanel panel in this.textPanels)
                    {
                        DisplayPanel displayPanel = new DisplayPanel(panel);
                        this.DisplayPanels.Add(displayPanel);

                        if (displayPanel.IsPrimary)
                        {
                            if (this.PrimaryPanel == null)
                            {
                                this.primaryPanel = displayPanel;
                            }
                            else
                            {
                                throw new InvalidDisplayGridException("Multiple panels marked as primary (0,0).");
                            }
                        }
                    }

                    if (this.PrimaryPanel == null)
                    {
                        throw new InvalidDisplayGridException("No panel in origin (0,0)");
                    }
                }
                

                Vector2 offsetUnitVector = SpriteHelper.GetOffsetUnitVector(this.PrimaryPanel.Origin);

                foreach (DisplayPanel panel in this.DisplayPanels)
                {
                    panel.Offset = offsetUnitVector * new Vector2(panel.Column, panel.Row) * this.PrimaryPanel.Size;
                    if (panel.Column == this.PrimaryPanel.Column)
                    {
                        this.Height += panel.Size.Y;
                    }
                    if (panel.Row == this.PrimaryPanel.Row)
                    {
                        this.Width += panel.Size.X;
                    }
                    log.Invoke($"ViewPort Position: ({panel.ViewPort.Position.X},{panel.ViewPort.Position.Y})");
                }

                this.Log($"Grid initialized with {this.textPanels.Count} Text Panels as {this.DisplayPanels.Count} Display Panels with the origin in the {this.PrimaryPanel.Origin}");
            }

            /// <summary>
            /// Gets or sets the height of the grid.
            /// </summary>
            public float Height { get; private set; }

            /// <summary>
            /// Gets or sets a value indicating whether to surround text sprites with a box. Useful for diagnosing alignment problems.
            /// </summary>
            public bool IncludeTextBoundaries
            {
                get
                {
                    return this.includeTextBoundaries;
                }
                set
                {
                    foreach (DisplayPanel panel in this.DisplayPanels)
                    {
                        panel.IncludeTextBoundaries = value;
                    }
                    this.includeTextBoundaries = value;
                }
            }

            /// <summary>
            /// Gets the origin locations of the grid.
            /// </summary>
            public SpriteOriginPositions Origins
            {
                get
                {
                    return new SpriteOriginPositions(
                        new Vector2(0, 0),
                        new Vector2(this.Width, 0),
                        new Vector2(this.Width, this.Height),
                        new Vector2(this.Width, 0),
                        new Vector2(this.Width / 2f, this.Height / 2f));
                }
            }

            /// <summary>
            /// Gets the primary panel.
            /// </summary>
            public DisplayPanel PrimaryPanel
            {
                get
                {
                    return this.primaryPanel;
                }
            }

            /// <summary>
            /// Gets or sets the width of the grid.
            /// </summary>
            public float Width { get; private set; }

            /// <summary>
            /// Adds the list of sprites to each panel.
            /// </summary>
            /// <param name="sprites">List of sprites to add.</param>
            public void AddSprites(List<MySprite> sprites)
            {
                foreach (DisplayPanel panel in this.DisplayPanels)
                {
                    panel.AddSprites(sprites);
                }
            }

            /// <summary>
            /// Measures the text size in pixels on this display.
            /// </summary>
            /// <param name="value">String to write.</param>
            /// <param name="fontId">Font Id.</param>
            /// <param name="scale">Scale of text.</param>
            /// <returns>Vector with height and width of text.</returns>
            public Vector2 GetTextSize(string value, string fontId = null, float? scale = null)
            {
                this.stringBuilder.Length = 0;
                this.stringBuilder.Append(value);

                return this.PrimaryPanel.TextSurface.MeasureStringInPixels(
                    this.stringBuilder,
                    fontId ?? this.PrimaryPanel.TextSurface.Font, 
                    scale == null ? this.PrimaryPanel.TextSurface.FontSize : (float)scale);
            }

            /// <summary>
            /// Paints the grid.
            /// </summary>
            public void Paint()
            {
                foreach (DisplayPanel panel in this.DisplayPanels)
                {
                    panel.Paint();
                }
            }

            /// <summary>
            /// Sets the sprites on the panels.
            /// </summary>
            /// <param name="sprites">Sprites to paint.</param>
            public void SetSprites(List<MySprite> sprites)
            {
                foreach (DisplayPanel panel in this.DisplayPanels)
                {
                    panel.SetSprites(sprites);
                }
            }
        }

        /// <summary>
        /// Display Panel.
        /// </summary>
        public partial class DisplayPanel
        {
            /// <summary>
            /// Invalid display panel exception.
            /// </summary>
            public class InvalidDisplayPanelException : Exception
            {
                /// <summary>
                /// Gets the name of the display panel.
                /// </summary>
                public string Name { get; }

                /// <summary>
                /// Constructs a new instance of the exception.
                /// </summary>
                /// <param name="name">Text Panel Name.</param>
                /// <param name="message">Message string.</param>
                public InvalidDisplayPanelException(string name, string message) : base(message)
                {
                    this.Name = name;
                }
            }

            /// <summary>
            /// Column number.
            /// </summary>
            private readonly int column;

            /// <summary>
            /// Panel ini.
            /// </summary>
            private readonly MyIni ini = new MyIni();

            /// <summary>
            /// Origin position.
            /// </summary>
            private readonly SpriteOrigins origin;

            /// <summary>
            /// Row number.
            /// </summary>
            private readonly int row;

            /// <summary>
            /// List of sprites to draw.
            /// </summary>
            private readonly List<MySprite> Sprites = new List<MySprite>();


            /// <summary>
            /// String Builder.
            /// </summary>
            private readonly StringBuilder stringBuilder = new StringBuilder();

            /// <summary>
            /// Text Panel.
            /// </summary>
            private readonly IMyTextPanel textPanel;

            /// <summary>
            /// Writable viewport.
            /// </summary>
            private RectangleF viewPort;

            /// <summary>
            /// Creates a new instance of the Display Panel class.
            /// </summary>
            /// <param name="panel">Text panel.</param>
            public DisplayPanel(IMyTextPanel panel, bool isSolo = false)
            {
                this.textPanel = panel;
                this.ViewPort = new RectangleF((this.textPanel.TextureSize - this.textPanel.SurfaceSize) / 2f, this.textPanel.SurfaceSize);
                if (isSolo)
                {
                    this.row = 0;
                    this.column = 0;
                    this.origin = SpriteOrigins.TopLeft;
                }
                else
                {
                    if (this.ini.TryParse(panel.CustomData))
                    {
                        if (!this.ini.Get("display", "row").TryGetInt32(out this.row))
                        {
                            throw new InvalidDisplayPanelException(this.textPanel.CustomName, "Invalid row number.");
                        }

                        if (!this.ini.Get("display", "column").TryGetInt32(out this.column))
                        {
                            throw new InvalidDisplayPanelException(this.textPanel.CustomName, "Invalid column number.");
                        }

                        if (!Enum.TryParse(this.ini.Get("display", "origin").ToString("TopLeft"), true, out this.origin))
                        {
                            throw new InvalidDisplayPanelException(this.textPanel.CustomName, "Invalid origin. Try 'TopLeft', 'TopRight', 'BottomLeft', 'BottomRight'");
                        }
                    }
                    else
                    {
                        throw new InvalidDisplayPanelException(this.textPanel.CustomName, "Invalid INI.");
                    }
                }
            }

            /// <summary>
            /// Gets the column number.
            /// </summary>
            public int Column => this.column;

            /// <summary>
            /// Gets or sets a value indicating whether boxes should be drawn around text sprites.
            /// </summary>
            public bool IncludeTextBoundaries { get; set; }

            /// <summary>
            /// Gets a value indicating whether the panel is marked as the primary panel.
            /// </summary>
            public bool IsPrimary
            {
                get
                {
                    return this.Row == 0 && this.Column == 0;
                }
            }

            /// <summary>
            /// Gets or sets the offset.
            /// </summary>
            public Vector2 Offset { get; set; }

            /// <summary>
            /// Gets the origin position.
            /// </summary>
            public SpriteOrigins Origin
            {
                get
                {
                    return this.origin;
                }
            }

            /// <summary>
            /// Gets the row number.
            /// </summary>
            public int Row => this.row;

            /// <summary>
            /// Gets the size fo the writing surface.
            /// </summary>
            public Vector2 Size
            {
                get
                {
                    return this.ViewPort.Size;
                }
            }

            /// <summary>
            /// Gets the text surface for this panel.
            /// </summary>
            public IMyTextSurface TextSurface
            {
                get
                {
                    return this.textPanel;
                }
            }

            /// <summary>
            /// Gets the viewport for this panel.
            /// </summary>
            public RectangleF ViewPort
            {
                get
                {
                    return this.viewPort;
                }
                private set
                {
                    this.viewPort = value;
                }
            }

            /// <summary>
            /// Adds the sprites to the current sprite list.
            /// </summary>
            /// <param name="sprites">List of additional sprites.</param>
            public void AddSprites(List<MySprite> sprites)
            {
                foreach (MySprite sprite in sprites)
                {
                    this.Sprites.Add(new MySprite()
                    {
                        Type = sprite.Type,
                        Position = this.Offset + sprite.Position + this.ViewPort.Position,
                        Size = sprite.Size,
                        Color = sprite.Color,
                        Data = sprite.Data,
                        FontId = sprite.FontId,
                        Alignment = sprite.Alignment,
                        RotationOrScale = sprite.RotationOrScale
                    });
                    if (this.IncludeTextBoundaries && sprite.Type == SpriteType.TEXT)
                    {
                        this.stringBuilder.Length = 0;
                        this.stringBuilder.Append(sprite.Data);
                        Vector2 size = this.textPanel.MeasureStringInPixels(this.stringBuilder, sprite.FontId ?? this.textPanel.Font, sprite.RotationOrScale) + new Vector2(2, 0);
                        SpriteHelper.AddBox(this.Offset + (Vector2)sprite.Position + this.ViewPort.Position - new Vector2(1, 0), size, 2, this.Sprites, Color.Red);
                    }
                }
            }

            /// <summary>
            /// Draws the sprites to the viewport.
            /// </summary>
            public void Paint()
            {
                using (MySpriteDrawFrame frame = this.textPanel.DrawFrame())
                {
                    frame.AddRange(this.Sprites);
                }
            }

            /// <summary>
            /// Adds the sprites to this panels list of sprites with the position offset.
            /// </summary>
            /// <param name="sprites">Sprites to copy.</param>
            public void SetSprites(List<MySprite> sprites)
            {
                this.Sprites.Clear();
                this.AddSprites(sprites);
            }
        }
    }
}

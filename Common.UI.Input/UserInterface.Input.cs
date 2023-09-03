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
        /// User interface namespace.
        /// </summary>
        public partial class UserInterface
        {
            /// <summary>
            /// Change Event name.
            /// </summary>
            public const string ChangeEvt = "change";

            /// <summary>
            /// Input Class.
            /// </summary>
            public class Input : EventEmitter
            {
                /// <summary>
                /// Button Type.
                /// </summary>
                public enum ButtonType
                {
                    /// <summary>
                    /// Text button.
                    /// </summary>
                    Text,

                    /// <summary>
                    /// Sprite button.
                    /// </summary>
                    Sprite
                }


                /// <summary>
                /// Page of buttons.
                /// </summary>
                public class Page
                {
                    private readonly List<string> EmptyButtonImageIds = new List<string>()
                    {
                        "No Entry"
                    };

                    /// <summary>
                    /// List of buttons.
                    /// </summary>
                    public readonly List<Button> buttons = new List<Button>();

                    /// <summary>
                    /// Gets the button at this index.
                    /// </summary>
                    /// <param name="index">Index of button.</param>
                    /// <returns>Button at that index.</returns>
                    public Button this[int index]
                    {
                        get
                        {
                            return this.buttons[index];
                        }
                    }

                    /// <summary>
                    /// Adds a button.
                    /// </summary>
                    /// <param name="button">Button to add.</param>
                    /// <returns>This page.</returns>
                    public Page With(Button button)
                    {
                        this.buttons.Add(button);
                        this.buttons.Sort((a, b) => a.Index - b.Index);
                        return this;
                    }

                    /// <summary>
                    /// Applies the text or sprites to the buttons on the panel.
                    /// </summary>
                    /// <param name="panel">Panel to write to.</param>
                    public void Apply(IMyButtonPanel panel)
                    {
                        foreach (Button button in this.buttons)
                        {
                            panel.SetCustomButtonName(button.Index, button.Name);

                            if (panel is IMyTextSurfaceProvider)
                            {
                                IMyTextSurface surface = ((IMyTextSurfaceProvider)panel).GetSurface(button.Index);
                                if (surface == null)
                                {
                                    continue;
                                }

                                if (button.Name == null)
                                {
                                    surface.ContentType = ContentType.TEXT_AND_IMAGE;
                                    surface.WriteText("", false);
                                    surface.AddImageToSelection("No Entry", true);
                                    continue;
                                }
                                else
                                {
                                    surface.RemoveImageFromSelection("No Entry", true);
                                }

                                if (button.SpriteType != null)
                                {
                                    surface.ContentType = ContentType.SCRIPT;
                                    surface.Script = "";
                                    using (var frame = surface.DrawFrame())
                                    {
                                        frame.Add(new MySprite()
                                        {
                                            Data = button.Data,
                                            Position = button.Position,
                                            Size = button.Size,
                                            Color = button.Color,
                                            RotationOrScale = button.RotationOrScale ?? 1f,
                                            FontId = button.FontId,
                                            Type = (SpriteType)button.SpriteType,
                                            Alignment = button.TextAlignment
                                        });
                                    }
                                }
                                else
                                {
                                    surface.ContentType = ContentType.TEXT_AND_IMAGE;
                                    surface.WriteText(string.IsNullOrWhiteSpace(button.Data) ? button.Name : button.Data);
                                    surface.Font = button.FontId;
                                    surface.FontColor = button.Color ?? surface.FontColor;
                                    surface.FontSize = button.RotationOrScale ?? surface.FontSize;
                                    surface.Alignment = button.TextAlignment;
                                }
                            }
                        }
                    }
                }

                /// <summary>
                /// Button data class.
                /// </summary>
                public class Button
                {
                    /// <summary>
                    /// Gets or sets the name of the button.
                    /// </summary>
                    public string Name { get; set; }

                    /// <summary>
                    /// Gets or sets the button index.
                    /// </summary>
                    public int Index { get; set; }

                    /// <summary>
                    /// Gets or sets the argument for the button.
                    /// </summary>
                    public string Argument { get; set; }

                    /// <summary>
                    /// Gets or sets the data for the sprite.
                    /// </summary>
                    public string Data { get; set; }

                    /// <summary>
                    /// Gets or sets the sprites position.
                    /// </summary>
                    public Vector2? Position { get; set; }

                    /// <summary>
                    /// Gets or sets the sprite size.
                    /// </summary>
                    public Vector2? Size { get; set; }

                    /// <summary>
                    /// Gets or sets the sprite color.
                    /// </summary>
                    public Color? Color { get; set; }

                    /// <summary>
                    /// Gets or sets the sprite rotation or scale.
                    /// </summary>
                    public float? RotationOrScale { get; set; }

                    /// <summary>
                    /// Gets or sets the font id.
                    /// </summary>
                    public string FontId { get; set; }

                    /// <summary>
                    /// Gets or sets the sprite type.
                    /// </summary>
                    public SpriteType? SpriteType { get; set; }

                    /// <summary>
                    /// Gets or sets the text alignment.
                    /// </summary>
                    public TextAlignment TextAlignment { get; set; } = TextAlignment.CENTER;
                }

                /// <summary>
                /// List of pages.
                /// </summary>
                private readonly List<Page> pages = new List<Page>();

                /// <summary>
                /// Button Panel.
                /// </summary>
                private readonly IMyButtonPanel panel;

                /// <summary>
                /// Current page.
                /// </summary>
                private int currentPage = 0;

                /// <summary>
                /// Creates a new instance of the input class.
                /// </summary>
                /// <param name="panel">Button panel.</param>
                public Input(IMyButtonPanel panel)
                {
                    this.panel = panel;
                }

                /// <summary>
                /// Adds a new page.
                /// </summary>
                /// <param name="page">Page to add.</param>
                public void Add(Page page)
                {
                    this.pages.Add(page);
                }

                /// <summary>
                /// Goes to the next page or starts back at 0.
                /// </summary>
                public void NextPage()
                {
                    this.currentPage = this.currentPage + 1 >= this.pages.Count ? 0 : this.currentPage + 1;
                    this.Emit(ChangeEvt, this);
                }

                /// <summary>
                /// Goes to the previous page or starts back at the last page.
                /// </summary>
                public void PreviousPage()
                {
                    this.currentPage = this.currentPage - 1 < 0 ? this.pages.Count - 1 : this.currentPage - 1;
                    this.Emit(ChangeEvt, this);
                }

                /// <summary>
                /// Draws the current page to the panel.
                /// </summary>
                public void Draw()
                {
                    this.pages[this.currentPage].Apply(this.panel);
                }

                /// <summary>
                /// Gets the argument for the button on the current page at the specified index.
                /// </summary>
                /// <param name="index">Button index, 0-3</param>
                /// <returns>Argument on the button for this page.</returns>
                public string OnPress(int index)
                {
                    if (this.pages[this.currentPage][index] != null)
                    {
                        return this.pages[this.currentPage][index].Argument;
                    }

                    return null;
                }
            }
        }
    }
}

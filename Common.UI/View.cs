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
        /// User Interface namespace class.
        /// </summary>
        public partial class UserInterface
        {
            /// <summary>
            /// View interface.
            /// </summary>
            /// <typeparam name="T">Type of model.</typeparam>
            public abstract partial class View<T>
            {
                /// <summary>
                /// Private list of sprites to render.
                /// </summary>
                private readonly List<MySprite> Sprites = new List<MySprite>();

                /// <summary>
                /// String builder for views to use.
                /// </summary>
                protected readonly StringBuilder stringBuilder = new StringBuilder();

                /// <summary>
                /// Creates a new instance of the view.
                /// </summary>
                /// <param name="surface">Surface to use for calculations of size and position.</param>
                protected View(IMyTextSurface surface)
                {
                    this.Surface = surface;
                    this.ViewPort = new RectangleF((this.Surface.TextureSize - this.Surface.SurfaceSize) / 2f, this.Surface.SurfaceSize);
                }

                /// <summary>
                /// Gets the text surface for the view.
                /// </summary>
                protected IMyTextSurface Surface { get; }

                /// <summary>
                /// Gets the drawable viewport.
                /// </summary>
                protected RectangleF ViewPort { get; }

                /// <summary>
                /// Measures the string in pixels for the surface.
                /// </summary>
                /// <param name="input">Input string.</param>
                /// <param name="fontId">Font Id.</param>
                /// <param name="scale">Scale of the text.</param>
                /// <returns>Vector of the size of the text sprite.</returns>
                protected Vector2 Measure(string input, string fontId, float scale)
                {
                    this.stringBuilder.Length = 0;
                    this.stringBuilder.Append(input);
                    return this.Surface.MeasureStringInPixels(stringBuilder, fontId, scale);
                }

                /// <summary>
                /// Gets the max line heigh of a font/scale combo for the reference surface of this view.
                /// </summary>
                /// <param name="fontId">Font Id.</param>
                /// <param name="scale">Font Scale.</param>
                /// <returns>Max Line Height.</returns>
                public float GetLineHeight(string fontId, float scale)
                {
                    return this.Measure("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}\\|;:\'\",.<>/?`~", fontId, scale).Y;
                }

                /// <summary>
                /// Draws the sprites based on the surface.
                /// </summary>
                /// <param name="offset">Offset to apply.</param>
                public void Draw(Vector2 offset)
                {
                    this.Draw(this.Surface, offset);
                }

                /// <summary>
                /// Draws the sprites to the surface.
                /// </summary>
                /// <param name="surface">Surface to draw on.</param>
                /// <param name="offset">Offset for sprite positioning.</param>
                public void Draw(IMyTextSurface surface, Vector2 offset)
                {
                    using (MySpriteDrawFrame frame = surface.DrawFrame())
                    {
                        foreach (MySprite sprite in this.Sprites)
                        {
                            // Do something with the surface rectangle and add it to the offset
                            frame.Add(new MySprite()
                            {
                                Alignment = sprite.Alignment,
                                FontId = sprite.FontId,
                                Color = sprite.Color,
                                Size = sprite.Size,
                                RotationOrScale = sprite.RotationOrScale,
                                Data = sprite.Data,
                                Type = sprite.Type,
                                Position = sprite.Position + offset
                            });
                        }
                    }
                }

                /// <summary>
                /// Renders the sprites into the view.
                /// </summary>
                /// <param name="model">Model to use when recalculating the sprites.</param>
                public void Render(T model)
                {
                    this.Sprites.Clear();
                    this.Sprites.AddRange(this.OnRender(model));
                }

                /// <summary>
                /// Event call to subclass to recalculate the sprites.
                /// </summary>
                /// <param name="model">Model to use when recalculating the sprites.</param>
                protected abstract List<MySprite> OnRender(T model);
            }
        }
    }
}

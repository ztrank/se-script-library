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
    /// Airlock control program.
    /// </summary>
    partial class Program
    {
        /// <summary>
        /// Standard out for writing messages to a screen.
        /// </summary>
        public class StandardOut
        {
            /// <summary>
            /// Settings object for Standard Out class.
            /// </summary>
            public class Settings
            {
                /// <summary>
                /// Gets or sets the max visible lines.
                /// </summary>
                public int MaxLines { get; set; }

                /// <summary>
                /// Gets or sets the line size.
                /// </summary>
                public int LineSize { get; set; }
            }

            /// <summary>
            /// Text surface to write to.
            /// </summary>
            private readonly IMyTextSurface Surface;

            /// <summary>
            /// Viewport.
            /// </summary>
            private readonly RectangleF Viewport;

            /// <summary>
            /// My Sprites.
            /// </summary>
            private readonly List<MySprite> Sprites = new List<MySprite>();

            /// <summary>
            /// Lines to write.
            /// </summary>
            private readonly List<string> Lines = new List<string>();

            /// <summary>
            /// Creates a new instance of the Standard Out object.
            /// </summary>
            /// <param name="surface">Text Surface.</param>
            public StandardOut(IMyTextSurface surface, Settings settings)
            {
                this.Surface = surface;
                this.MySettings = settings;
                this.Viewport = new RectangleF((this.Surface.TextureSize - this.Surface.SurfaceSize) / 2f, this.Surface.TextureSize);
            }

            /// <summary>
            /// Gets or sets the settings object.
            /// </summary>
            public Settings MySettings { get; set; }

            /// <summary>
            /// Draws the messages to the surface.
            /// </summary>
            /// <param name="messages">List of messages.</param>
            public void Draw(List<string> messages)
            {
                this.Sprites.Clear();
                this.Lines.Clear();
                MySpriteDrawFrame frame = this.Surface.DrawFrame();

                Vector2 position = new Vector2(0, 20 + (this.MySettings.MaxLines * this.MySettings.LineSize)) + this.Viewport.Position;


                foreach (string message in messages)
                {
                    foreach (string line in message.Split('\n'))
                    {
                        this.Lines.Add(line);
                    }
                }
                this.Lines.Reverse();
                foreach (string line in this.Lines)
                {
                    this.Sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = line,
                        RotationOrScale = 0.8f,
                        Color = Color.White,
                        Alignment = TextAlignment.LEFT,
                        Position = position,
                        FontId = "White"
                    });
                    position -= new Vector2(0, this.MySettings.LineSize);
                }

                frame.AddRange(this.Sprites);

                frame.Dispose();
            }
        }
    }
}

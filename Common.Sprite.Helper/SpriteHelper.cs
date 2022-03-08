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
        /// Struct of the position of each of the origin points on a sprite, or rectangle.
        /// </summary>
        public struct SpriteOriginPositions
        {
            /// <summary>
            /// Internal dictionary of the positions.
            /// </summary>
            private readonly IImmutableDictionary<SpriteOrigins, Vector2> positions;

            /// <summary>
            /// Creates a new SpriteOriginPositions based on the given rectangle.
            /// </summary>
            /// <param name="viewport">Rectangle to calculate the positions</param>
            public SpriteOriginPositions(RectangleF viewport) : this(
                viewport.Position, 
                viewport.Position - new Vector2(viewport.Width / 2f, viewport.Height / 2f),
                viewport.Position + new Vector2(viewport.Width / 2f, -1 * viewport.Height / 2f),
                viewport.Position - new Vector2(viewport.Width / 2f, -1 * viewport.Height / 2f),
                viewport.Position + new Vector2(viewport.Width / 2f, viewport.Height / 2f))
            {
            }

            /// <summary>
            /// Creates a new SpriteOriginPositions based on the given position vectors.
            /// </summary>
            /// <param name="center">Center location.</param>
            /// <param name="topLeft">Top Left location.</param>
            /// <param name="topRight">Top Right Location.</param>
            /// <param name="bottomRight">Bottom Right location.</param>
            /// <param name="bottomLeft">Bottom left location.</param>
            public SpriteOriginPositions(Vector2 center, Vector2 topLeft, Vector2 topRight, Vector2 bottomRight, Vector2 bottomLeft)
            {
                Dictionary<SpriteOrigins, Vector2> positions = new Dictionary<SpriteOrigins, Vector2>
                {
                    { SpriteOrigins.Center, center },
                    { SpriteOrigins.TopLeft, topLeft },
                    { SpriteOrigins.TopRight, topRight },
                    { SpriteOrigins.BottomRight, bottomRight },
                    { SpriteOrigins.BottomLeft, bottomLeft }
                };

                this.positions = positions.ToImmutableDictionary();
            }

            /// <summary>
            /// Gets the position of the given origin.
            /// </summary>
            /// <param name="origin">Origin to find.</param>
            /// <returns></returns>
            public Vector2? this[SpriteOrigins origin]
            {
                get
                {
                    if (this.positions == null)
                    {
                        return null;
                    }

                    return this.positions[origin];
                }
            }

            /// <summary>
            /// Tries to get the position vector of the speficied origin. Returns true if successful.
            /// </summary>
            /// <param name="origin">Origin position.</param>
            /// <param name="vector">Position vector.</param>
            /// <returns>True if vector is defined, false if the vector is null.</returns>
            public bool TryGet(SpriteOrigins origin, out Vector2? vector)
            {
                vector = this[origin];

                return vector != null;
            }
        }

        /// <summary>
        /// Origin from where to measure the sprite position.
        /// </summary>
        public enum SpriteOrigins
        {
            /// <summary>
            /// Position is measured from the center.
            /// </summary>
            Center,

            /// <summary>
            /// Position is measured from the top left.
            /// </summary>
            TopLeft,

            /// <summary>
            /// Position is measured from the top right.
            /// </summary>
            TopRight,

            /// <summary>
            /// Position is measured from the bottom right.
            /// </summary>
            BottomRight,

            /// <summary>
            /// Position is measured from the bottom left.
            /// </summary>
            BottomLeft
        }

        /// <summary>
        /// Helper class for sprite functions such as transformations, origins shifting, and common drawings.
        /// </summary>
        public class SpriteHelper
        {
            /// <summary>
            /// Gets a unit vector for the direction of the offset based on the origin position.
            /// </summary>
            /// <param name="origin">Origin position.</param>
            /// <returns>Unit vector for display offsets.</returns>
            public static Vector2 GetOffsetUnitVector(SpriteOrigins origin)
            {
                if (origin == SpriteOrigins.Center)
                {
                    return new Vector2(0, 0);
                }

                int x = origin == SpriteOrigins.TopLeft || origin == SpriteOrigins.BottomLeft ? -1 : 1;
                int y = origin == SpriteOrigins.TopLeft || origin == SpriteOrigins.TopRight ? -1 : 1;

                return new Vector2(x, y);
            }

            /// <summary>
            /// Gets the Top Left corner of a text sprite at the given position.
            /// </summary>
            /// <param name="textPosition">Position of text sprite.</param>
            /// <param name="viewPortPosition">Viewport position.</param>
            /// <param name="offset">Offset, useful for grids where the sprites need to be shifted for each surface.</param>
            /// <returns>Vector2 representing the location of the top left corner of the text sprite.</returns>
            public static Vector2 GetTextTopLeftCorner(Vector2 textPosition, Vector2? viewPortPosition = null, Vector2? offset = null)
            {
                viewPortPosition = viewPortPosition == null ? Vector2.Zero : viewPortPosition;
                offset = offset == null ? Vector2.Zero : offset;
                return (Vector2)offset + textPosition + (Vector2)viewPortPosition; //- new Vector2(1, 0);
            }

            /// <summary>
            /// Vertically centers the sprite, using the sprite's center as origin, in the parent or target vector. 
            /// </summary>
            /// <param name="parentOrTarget">Vector describing the height of the area to center within. e.g., a text measurement from the text surface.</param>
            /// <param name="data">Name of texture.</param>
            /// <param name="size">Size of sprite.</param>
            /// <param name="position">Position of the sprite.</param>
            /// <param name="color">Optional color for the sprite.</param>
            /// <param name="rotation">Optional rotation for the sprite.</param>
            /// <returns>MySprite vertically centered.</returns>
            public static MySprite VerticalCenter(Vector2 parentOrTarget, string data, Vector2 size, Vector2 position, Color? color = null, float rotation = 0)
            {
                return SetOrigin(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = data,
                    Size = size,
                    Position = position + new Vector2(size.X / 2f, parentOrTarget.Y / 2f),
                    RotationOrScale = rotation,
                    Color = color
                }, SpriteOrigins.Center);
            }

            /// <summary>
            /// Creates a box and adds the sprites to the given sprite list.
            /// </summary>
            /// <param name="topLeftCornerPosition">Position of the boxes' top left corner.</param>
            /// <param name="size">Size of the box.</param>
            /// <param name="width">Width of the border.</param>
            /// <param name="sprites">List of sprites the box will be added to.</param>
            /// <param name="color">Optional color for the box.</param>
            public static void AddBox(Vector2 topLeftCornerPosition, Vector2 size, int width, List<MySprite> sprites, Color? color = null)
            {
                // Top Bar
                sprites.Add(SetOrigin(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Size = new Vector2(size.X, width),
                    Position = topLeftCornerPosition - new Vector2(0, (width + 1) % 2),
                    Color = color
                }));

                // Right Bar
                sprites.Add(SetOrigin(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Size = new Vector2(width, size.Y),
                    Position = topLeftCornerPosition + new Vector2(size.X, 0),
                    Color = color
                }, SpriteOrigins.TopRight));

                // Bottom Bar
                sprites.Add(SetOrigin(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Size = new Vector2(size.X, width),
                    Position = topLeftCornerPosition + new Vector2(0, size.Y),
                    Color = color
                }, SpriteOrigins.BottomLeft));

                // Left Bar
                sprites.Add(SetOrigin(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Size = new Vector2(width, size.Y),
                    Position = topLeftCornerPosition,
                    Color = color
                }));

            }

            /// <summary>
            /// Translates the sprite to align its new origin with its given position.
            /// </summary>
            /// <param name="sprite">Sprite to translate.</param>
            /// <param name="origin">Sprite origin.</param>
            /// <returns>Translated sprite.</returns>
            public static MySprite SetOrigin(MySprite sprite, SpriteOrigins origin = SpriteOrigins.TopLeft)
            {
                if (sprite.Type == SpriteType.TEXTURE && sprite.Size != null && sprite.Position != null)
                {
                    Vector2? position = null;
                    switch (origin)
                    {
                        case SpriteOrigins.Center:
                            position = sprite.Position - new Vector2(((int)((Vector2)sprite.Size).Y - ((int)((Vector2)sprite.Size).Y % 2)) / 2f, 0);
                            break;
                        case SpriteOrigins.TopLeft:
                            position = sprite.Position + new Vector2(0, ((int)((Vector2)sprite.Size).Y - ((int)((Vector2)sprite.Size).Y % 2)) / 2f);
                            break;
                        case SpriteOrigins.TopRight:
                            position = sprite.Position + new Vector2(-1 * (int)((Vector2)sprite.Size).X, ((int)((Vector2)sprite.Size).Y - ((int)((Vector2)sprite.Size).Y % 2)) / 2f);
                            break;
                        case SpriteOrigins.BottomRight:
                            position = sprite.Position + new Vector2(-1 * (int)((Vector2)sprite.Size).X, -1 * ((int)((Vector2)sprite.Size).Y - ((int)((Vector2)sprite.Size).Y % 2)) / 2f);
                            break;
                        case SpriteOrigins.BottomLeft:
                            position = sprite.Position + new Vector2(0, -1 * ((int)((Vector2)sprite.Size).Y - ((int)((Vector2)sprite.Size).Y % 2)) / 2f);
                            break;
                    }

                    return new MySprite()
                    {
                        Type = sprite.Type,
                        Data = sprite.Data,
                        Size = sprite.Size,
                        Position = position ?? sprite.Position,
                        Alignment = sprite.Alignment,
                        RotationOrScale = sprite.RotationOrScale,
                        Color = sprite.Color
                    };
                }

                return sprite;

            }
        }
    }
}

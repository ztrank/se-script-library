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
        /// Sprite Serializer because System.Text.Json isn't allowed apparently, and MySerializableSprite doesn't seem to do anything either.
        /// </summary>
        public class SpriteConverter
        {
            /// <summary>
            /// Ini class for reading and writing INIs.
            /// </summary>
            private MyIni ini = new MyIni();

            /// <summary>
            /// Creates a sprite from the ini string.
            /// </summary>
            /// <param name="iniString">Ini string.</param>
            /// <returns>MySprite instance.</returns>
            public MySprite Deserialize(string iniString)
            {
                this.ini.Clear();
                if (this.ini.TryParse(iniString))
                {
                    if (!this.ini.ContainsSection("sprite"))
                    {
                        throw new InvalidCastException();
                    }

                    SpriteType type;
                    TextAlignment alignment;
                    Enum.TryParse(this.ini.Get("sprite", "type").ToString(), true, out type);
                    Enum.TryParse(this.ini.Get("sprite", "alignment").ToString(), true, out alignment);

                    Vector2? position = null;
                    Vector2? size = null;
                    Color? color = null;

                    if (this.ini.ContainsSection("position"))
                    {
                        position = new Vector2(
                            (float)this.ini.Get("position", "x").ToDouble(),
                            (float)this.ini.Get("position", "y").ToDouble());
                    }

                    if (this.ini.ContainsSection("size"))
                    {
                        size = new Vector2(
                            (float)this.ini.Get("size", "x").ToDouble(),
                            (float)this.ini.Get("size", "y").ToDouble());
                    }

                    if (this.ini.ContainsKey("sprite", "color"))
                    {
                        color = new Color(this.ini.Get("sprite", "color").ToUInt32());
                    }

                    return new MySprite()
                    {
                        Type = type,
                        Data = this.ini.Get("sprite", "data").ToString(),
                        RotationOrScale = (float)this.ini.Get("sprite", "scale").ToDouble(),
                        Alignment = alignment,
                        FontId = this.ini.Get("sprite", "font").ToString(),
                        Position = position,
                        Size = size,
                        Color = color
                    };
                }

                throw new InvalidCastException();
            }

            /// <summary>
            /// Creates a ini string from the sprite.
            /// </summary>
            /// <param name="sprite">Sprite to serialize.</param>
            /// <returns>Sprite ini.</returns>
            public string Serialize(MySprite sprite)
            {
                this.ini.Clear();
                this.ini.AddSection("sprite");
                this.ini.AddSection("position");
                this.ini.AddSection("size");
                this.ini.Set("sprite", "type", sprite.Type.ToString());
                this.ini.Set("sprite", "data", sprite.Data);
                this.ini.Set("sprite", "scale", sprite.RotationOrScale.ToString());
                this.ini.Set("sprite", "alignment", sprite.Alignment.ToString());
                this.ini.Set("sprite", "font", sprite.FontId);

                if (sprite.Position != null)
                {
                    this.ini.Set("position", "x", ((Vector2)sprite.Position).X.ToString());
                    this.ini.Set("position", "y", ((Vector2)sprite.Position).Y.ToString());
                }

                if (sprite.Size != null)
                {
                    this.ini.Set("size", "x", ((Vector2)sprite.Size).X.ToString());
                    this.ini.Set("size", "y", ((Vector2)sprite.Size).Y.ToString());
                }

                if (sprite.Color != null)
                {
                    this.ini.Set("sprite", "color", ((Color)sprite.Color).PackedValue);
                }

                return this.ini.ToString();
            }
        }
    }
}

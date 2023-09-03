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
        public class OreFilterCollectionView : UserInterface.SelectionView<OreFilter, OreFilterCollection, OreFilterCollectionModel>
        {
            public OreFilterCollectionView(IMyTextSurface surface) : base(surface)
            {
            }

            /// <summary>
            /// Gets the height of the header. Defaults to 0.
            /// </summary>
            /// <param name="model">Model used to calculate sizes.</param>
            /// <returns>Height of the header.</returns>
            protected override float GetHeaderHeight(OreFilterCollectionModel model)
            {
                return this.GetLineHeight(model.FontId, model.Scale) + 6;
            }

            protected override void GenerateHeader(OreFilterCollectionModel model, List<MySprite> sprites)
            {
                if (model.ShowScreenSaver)
                {
                    sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = "Select a Refinery",
                        RotationOrScale = 1f,
                        Position = this.ViewPort.Center,
                        Alignment = TextAlignment.CENTER,
                        Color = this.Surface.ScriptForegroundColor,
                        FontId = this.Surface.Font
                    });
                }
                else
                {
                    sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = "Select allowed Ores",
                        RotationOrScale = 1f,
                        Position = this.linePosition,
                        Alignment = TextAlignment.LEFT,
                        Color = this.Surface.ScriptForegroundColor,
                        FontId = this.Surface.Font
                    });

                    this.linePosition += new Vector2(0, this.lineHeight);

                    sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = "SquareSimple",
                        Size = new Vector2(this.Surface.SurfaceSize.X - 10, 2),
                        Position = this.linePosition + new Vector2(5, -2),
                        Color = this.Surface.ScriptForegroundColor
                    });

                    this.linePosition += new Vector2(0, 6);
                }
            }

            protected override void GenerateRow(OreFilterCollectionModel model, OreFilter value, int iteration, List<MySprite> sprites)
            {
                if (!model.ShowScreenSaver)
                {
                    Color backgroundColor = model.CursorIndex == iteration ? this.Surface.ScriptForegroundColor : this.Surface.ScriptBackgroundColor;
                    Color textureColor = model.CursorIndex == iteration ? this.Surface.ScriptBackgroundColor : this.Surface.ScriptForegroundColor;

                    sprites.Add(SpriteHelper.VerticalCenter(new Vector2(0, this.lineHeight), "SquareSimple", new Vector2(this.Surface.SurfaceSize.X, this.lineHeight), new Vector2(-1 * this.Surface.SurfaceSize.X / 2f, this.linePosition.Y), backgroundColor));
                    sprites.Add(SpriteHelper.VerticalCenter(new Vector2(0, this.lineHeight), value.IsSelected ? "SquareSimple" : "SquareHollow", new Vector2(this.lineHeight) - new Vector2(2, 2), this.linePosition, textureColor));
                    if (model.CursorIndex == iteration)
                    {
                        sprites.Add(SpriteHelper.VerticalCenter(new Vector2(0, this.lineHeight), "SquareSimple", new Vector2(this.lineHeight) - new Vector2(8, 8), this.linePosition + new Vector2(4, 0), value.IsSelected ? backgroundColor : textureColor));
                    }
                    sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = value.Name,
                        Alignment = TextAlignment.LEFT,
                        Position = this.linePosition + new Vector2(this.lineHeight, 0),
                        FontId = model.FontId,
                        Color = textureColor,
                        RotationOrScale = 1f
                    });
                }
            }
        }
    }
}

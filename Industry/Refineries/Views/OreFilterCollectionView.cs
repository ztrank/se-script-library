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

            protected override void GenerateRow(OreFilterCollectionModel model, OreFilter value, int iteration, List<MySprite> sprites)
            {
                Color backgroundColor = model.CursorIndex == iteration ? this.Surface.FontColor : this.Surface.BackgroundColor;
                Color textureColor = model.CursorIndex == iteration ? this.Surface.BackgroundColor : this.Surface.FontColor;

                sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = this.linePosition,
                    Size = new Vector2(this.Surface.SurfaceSize.X, this.lineHeight),
                    Color = backgroundColor
                });

                sprites.Add(SpriteHelper.VerticalCenter(new Vector2(0, this.lineHeight), value.IsSelected ? "SquareSimple" : "SquareHollow", new Vector2(this.lineHeight) - new Vector2(2, 2), this.linePosition, textureColor));
                sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = value.Name,
                    Alignment = TextAlignment.LEFT,
                    Position = this.linePosition + new Vector2(this.lineHeight, 0),
                    FontId = model.FontId,
                    Color = textureColor
                });

                this.linePosition += new Vector2(0, this.lineHeight);
            }
        }
    }
}

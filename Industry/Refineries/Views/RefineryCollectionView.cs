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
        /// Refinery Collection View.
        /// </summary>
        public class RefineryCollectionView : UserInterface.SelectionView<RefineryGroup, RefineryCollection, RefineryCollectionModel>
        {
            /// <summary>
            /// Creates a new instnace of the refinery collection view.
            /// </summary>
            /// <param name="surface">Text Surface.</param>
            public RefineryCollectionView(IMyTextSurface surface) 
                : base(surface)
            {
            }

            /// <summary>
            /// Gets the height of the header. Defaults to 0.
            /// </summary>
            /// <param name="model">Model used to calculate sizes.</param>
            /// <returns>Height of the header.</returns>
            protected override float GetHeaderHeight(RefineryCollectionModel model)
            {
                return this.GetLineHeight(model.FontId, model.Scale) * 2 + 6;
            }

            /// <summary>
            /// Generates the header sprites.
            /// </summary>
            /// <param name="model">Model to use.</param>
            /// <param name="sprites">List of sprites to add values to.</param>
            protected override void GenerateHeader(RefineryCollectionModel model, List<MySprite> sprites)
            {
                
                sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Refineries",
                    Position = this.linePosition,
                    RotationOrScale = model.Scale,
                    FontId = model.FontId,
                    Color = this.Surface.ScriptForegroundColor,
                    Alignment = TextAlignment.LEFT
                });

                this.linePosition += new Vector2(0, this.lineHeight);

                sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Selection Mode: " + model.SelectionMode.ToString(),
                    Position = this.linePosition,
                    RotationOrScale = model.Scale,
                    FontId = model.FontId,
                    Color = this.Surface.ScriptForegroundColor,
                    Alignment = TextAlignment.LEFT
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

            /// <summary>
            /// Generates the row's sprites.
            /// </summary>
            /// <param name="model">Model used.</param>
            /// <param name="value">Value to check.</param>
            /// <param name="iteration">Iteration count.</param>
            /// <param name="sprites">List of sprites to add values to.</param>
            protected override void GenerateRow(RefineryCollectionModel model, RefineryGroup group, int iteration, List<MySprite> sprites)
            {
                Color backgroundColor = model.CursorIndex == iteration ? this.Surface.ScriptForegroundColor : this.Surface.ScriptBackgroundColor;
                Color textureColor = model.CursorIndex == iteration ? this.Surface.ScriptBackgroundColor : this.Surface.ScriptForegroundColor;

                sprites.Add(SpriteHelper.VerticalCenter(new Vector2(0, this.lineHeight), "SquareSimple", new Vector2(this.Surface.SurfaceSize.X, this.lineHeight), new Vector2(-1 * this.Surface.SurfaceSize.X / 2f, this.linePosition.Y), backgroundColor));
                sprites.Add(SpriteHelper.VerticalCenter(new Vector2(0, this.lineHeight), group.IsSelected ? "SquareSimple" : "SquareHollow", new Vector2(this.lineHeight) - new Vector2(2, 2), this.linePosition + new Vector2(2,0), textureColor));
                if (model.CursorIndex == iteration)
                {
                    sprites.Add(SpriteHelper.VerticalCenter(new Vector2(0, this.lineHeight), "SquareSimple", new Vector2(this.lineHeight) - new Vector2(8, 8), this.linePosition + new Vector2(4, 0), group.IsSelected ? backgroundColor : textureColor));
                }
                
                sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = group.Name,
                    Alignment = TextAlignment.LEFT,
                    Position = this.linePosition + new Vector2(this.lineHeight + 2, 0),
                    FontId = model.FontId,
                    Color = textureColor,
                    RotationOrScale = 1f
                });
            }
        }
    }
}

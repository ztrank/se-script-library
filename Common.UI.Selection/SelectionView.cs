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
        public partial class UserInterface
        {
            /// <summary>
            /// Class for selection view creation.
            /// </summary>
            /// <typeparam name="T">Type of selectable row.</typeparam>
            /// <typeparam name="V">Type of selectable collection.</typeparam>
            /// <typeparam name="M">Type of selectable model.</typeparam>
            public abstract class SelectionView<T, V, M> : View<M>
                where T : ISelectable
                where V : ISelectableCollection<T>
                where M : ISelectableCollectionModel<T, V>
            {
                /// <summary>
                /// Current line position. Reset to 0 with each call to render.
                /// </summary>
                protected Vector2 linePosition = Vector2.Zero;

                /// <summary>
                /// Line height, calculated from the model.
                /// </summary>
                protected float lineHeight;

                /// <summary>
                /// Creates a new instance of the Selection View.
                /// </summary>
                /// <param name="surface">Text Surface.</param>
                protected SelectionView(IMyTextSurface surface) 
                    : base(surface)
                {
                }

                /// <summary>
                /// Gets the size of the header and body of the view.
                /// </summary>
                /// <param name="model">Model for calculating sizes.</param>
                /// <returns>Tuple of header size and body size.</returns>
                public MyTuple<RectangleF, RectangleF> GetSizes(M model)
                {
                    Vector2 headerSize = new Vector2(this.Surface.SurfaceSize.X, this.GetHeaderHeight(model));
                    Vector2 bodySize = new Vector2(this.ViewPort.X, this.ViewPort.Height - headerSize.Y);

                    RectangleF header = new RectangleF((this.ViewPort.Size - bodySize) / 2f, headerSize);

                    RectangleF body = new RectangleF((this.ViewPort.Size - headerSize) / 2f - headerSize, bodySize);

                    return new MyTuple<RectangleF, RectangleF>(header, body);
                }

                /// <summary>
                /// Gets the max number of fully visible rows.
                /// </summary>
                /// <param name="model">Model used to calculate sizes.</param>
                /// <returns>Number of maximum visibile rows inside the body section.</returns>
                public int MaxVisibleRows(M model)
                {
                    MyTuple<RectangleF, RectangleF> sizes = this.GetSizes(model);
                    return (int)(sizes.Item2.Height / this.GetLineHeight(model.FontId, model.Scale));
                }

                public int BottomIndex(M model)
                {
                    return model.TopIndex + this.MaxVisibleRows(model);
                }

                /// <summary>
                /// Implementation of the OnRender method to draw the header and body.
                /// </summary>
                /// <param name="model">Model used for the render.</param>
                /// <returns>List of sprites to render to the view.</returns>
                protected override List<MySprite> OnRender(M model)
                {
                    List<MySprite> sprites = new List<MySprite>();
                    this.linePosition = Vector2.Zero;
                    this.lineHeight = this.GetLineHeight(model.FontId, model.Scale);
                    this.GenerateHeader(model, sprites);
                    this.GenerateRows(model, sprites);
                    return sprites;
                }

                /// <summary>
                /// Gets the height of the header. Defaults to 0.
                /// </summary>
                /// <param name="model">Model used to calculate sizes.</param>
                /// <returns>Height of the header.</returns>
                protected virtual float GetHeaderHeight(M model)
                {
                    return 0;
                }

                /// <summary>
                /// Generates the header sprites.
                /// </summary>
                /// <param name="model">Model to use.</param>
                /// <param name="sprites">List of sprites to add values to.</param>
                protected virtual void GenerateHeader(M model, List<MySprite> sprites)
                {
                }

                /// <summary>
                /// Generates the rows for the body.
                /// </summary>
                /// <param name="model">Model to use.</param>
                /// <param name="sprites">List of sprites to add values to.</param>
                protected virtual void GenerateRows(M model, List<MySprite> sprites)
                {
                    int bottomIndex = this.BottomIndex(model);
                    for (int i = model.TopIndex; i <= bottomIndex && i < model.Collection.Values.Count(); i++)
                    {
                        this.GenerateRow(model, model.Collection.Values[i], i, sprites);
                        this.linePosition += new Vector2(0, this.lineHeight);
                    }
                }

                /// <summary>
                /// Generates the row's sprites.
                /// </summary>
                /// <param name="model">Model used.</param>
                /// <param name="value">Value to check.</param>
                /// <param name="iteration">Iteration count.</param>
                /// <param name="sprites">List of sprites to add values to.</param>
                protected abstract void GenerateRow(M model, T value, int iteration, List<MySprite> sprites);
            }
        }
    }
}

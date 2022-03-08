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

    public partial class Program
    {
        public class UserInterface
        {
            /// <summary>
            /// String Builder for the test string to calculate text size on the surface.
            /// </summary>
            private readonly StringBuilder stringBuilder = new StringBuilder(@"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789[]{}()-=+_|\/!@#$%^&*~`<>,.?;:'");

            /// <summary>
            /// Internal list of sprites.
            /// </summary>
            private readonly List<MySprite> sprites = new List<MySprite>();

            /// <summary>
            /// Cursor position.
            /// </summary>
            private Vector2? cursorPosition;

            /// <summary>
            /// Cursor magnitude.
            /// </summary>
            private Vector2? cursorMagnitude;

            /// <summary>
            /// Cursor minimum.
            /// </summary>
            private Vector2? cursorMinimum;

            /// <summary>
            /// Cursor maximum.
            /// </summary>
            private Vector2? cursorMaximum;

            /// <summary>
            /// Gets or sets the cursor minimum.
            /// </summary>
            public Vector2 CursorMinimum
            {
                get
                {
                    if (this.cursorMinimum == null)
                    {
                        return new Vector2(-1, -1);
                    }

                    return (Vector2)this.cursorMinimum;
                }
                set
                {
                    this.cursorMinimum = value;
                }
            }

            /// <summary>
            /// Gets or sets the curosr maximum.
            /// </summary>
            public Vector2 CursorMaximum
            {
                get
                {
                    if (this.cursorMaximum == null)
                    {
                        if (this.ViewPort != null)
                        {
                            return this.ViewPort.Size + new Vector2(1, 1);
                        }

                        return this.CursorMinimum;
                    }

                    return (Vector2)this.cursorMaximum;
                }
                set
                {
                    this.cursorMaximum = value;
                }
            }

            /// <summary>
            /// Gets or sets the cursor magnitude.
            /// </summary>
            public Vector2 CursorMagnitude
            {
                get
                {
                    if (this.cursorMagnitude == null)
                    {
                        this.cursorMagnitude = new Vector2(1, 1);
                    }
                    return (Vector2)this.cursorMagnitude;
                }
                set
                {
                    this.cursorMagnitude = value;
                }
            }

            /// <summary>
            /// Gets or sets the view port.
            /// </summary>
            public RectangleF ViewPort { get; set; }

            /// <summary>
            /// Gets the list of sprites.
            /// </summary>
            public List<MySprite> Sprites
            {
                get
                {
                    return this.sprites;
                }
            }

            /// <summary>
            /// Gets the current cursor position.
            /// </summary>
            public Vector2 CursorPosition
            {
                get
                {
                    if (this.cursorPosition == null)
                    {
                        if (this.ViewPort != null)
                        {
                            this.cursorMagnitude = this.ViewPort.Center;
                        }
                        else
                        {
                            this.cursorPosition = new Vector2(0, 0);
                        }
                    }
                    return (Vector2)this.cursorPosition;
                }
                private set
                {
                    Vector2 previous = this.CursorPosition;
                    this.cursorPosition = value;
                    this.OnCursorChange(previous, this.CursorPosition);
                }
            }

            /// <summary>
            /// Get Max Text Height
            /// </summary>
            /// <param name="surface">Surface to measure on.</param>
            /// <param name="fontId">Optional font id.</param>
            /// <param name="scale">Optional scale</param>
            /// <returns>Max text height for the surface.</returns>
            public float GetMaxTextHeight(IMyTextSurface surface, string fontId = null, float scale = 1f)
            {
                return surface.MeasureStringInPixels(this.stringBuilder, string.IsNullOrWhiteSpace(fontId) ? surface.Font : fontId, scale).Y;
            }

            /// <summary>
            /// Moves the cursor up by the cursor magnitude within the min and max.
            /// </summary>
            public void Up()
            {
                this.CursorPosition = Vector2.Min(Vector2.Max(this.CursorPosition + new Vector2(0, -1) * this.CursorMagnitude, this.CursorMinimum), this.CursorMaximum);
            }

            /// <summary>
            /// Moves the cursor down by the cursor magnitude within the min and max.
            /// </summary>
            public void Down()
            {
                this.CursorPosition = Vector2.Min(Vector2.Max(this.CursorPosition + new Vector2(0, 1) * this.CursorMagnitude, this.CursorMinimum), this.CursorMaximum);
            }

            /// <summary>
            /// Moves the cursor left by the cursor magnitude within the min and max.
            /// </summary>
            public void Left()
            {
                this.CursorPosition = Vector2.Min(Vector2.Max(this.CursorPosition + new Vector2(-1, 0) * this.CursorMagnitude, this.CursorMinimum), this.CursorMaximum);
            }


            /// <summary>
            /// Moves the cursor right by the cursor magnitude within the min and max.
            /// </summary>
            public void Right()
            {
                this.CursorPosition = Vector2.Min(Vector2.Max(this.CursorPosition + new Vector2(1, 0) * this.CursorMagnitude, this.CursorMinimum), this.CursorMaximum);
            }

            /// <summary>
            /// Executes on cursor change.
            /// </summary>
            protected virtual void OnCursorChange(Vector2 oldPosition, Vector2 newPosition)
            {

            }
        }

        /// <summary>
        /// Abstract class for creating a selectable list for the display.
        /// </summary>
        /// <typeparam name="T">Type of row.</typeparam>
        public abstract class SelectableList<T>
        {
            /// <summary>
            /// Internal list of sprites.
            /// </summary>
            protected readonly List<MySprite> Sprites = new List<MySprite>();

            /// <summary>
            /// Index of the top row in view.
            /// </summary>
            private int TopRowIndex = 0;

            /// <summary>
            /// Selected Index.
            /// </summary>
            private int selectedIndex = -1;

            /// <summary>
            /// High Lighted Index.
            /// </summary>
            private int highlightedIndex = -1;

            /// <summary>
            /// Moving position for sprite creation.
            /// </summary>
            protected Vector2 position;

            /// <summary>
            /// Gets or sets the row height. Defaults to 20.
            /// </summary>
            public float RowHeight { get; set; } = 20;

            /// <summary>
            /// Gets or sets the selected index.
            /// </summary>
            public int SelectedIndex
            {
                get
                {
                    return this.selectedIndex;
                }
                set
                {
                    if (value < -1)
                    {
                        this.selectedIndex = -1;
                    }
                    this.selectedIndex = value;
                }
            }

            /// <summary>
            /// Gets or sets the highlighted index.
            /// </summary>
            public int HighlightedIndex
            {
                get
                {
                    return this.highlightedIndex;
                }
                set
                {
                    if (value < -1)
                    {
                        this.highlightedIndex = -1;
                        this.TopRowIndex = 0;
                    }

                    this.SetScroll();
                    this.highlightedIndex = value;
                }
            }

            /// <summary>
            /// Gets the rows.
            /// </summary>
            public List<T> Rows { get; } = new List<T>();

            /// <summary>
            /// Gets the Display Size.
            /// </summary>
            public virtual Vector2 DisplaySize { get; set; }

            /// <summary>
            /// Gets the max rows visible on the grid.
            /// </summary>
            protected int MaxRows
            {
                get
                {
                    return (int)(this.DisplaySize.X / this.RowHeight);
                }
            }

            /// <summary>
            /// Gets the bottom row index visible.
            /// </summary>
            protected int BottomRowIndex
            {
                get
                {
                    return this.TopRowIndex + this.MaxRows - 1;
                }
            }

            /// <summary>
            /// Paints the screen with the new list of refineries.
            /// </summary>
            /// <param name="refineries"></param>
            public List<MySprite> Paint()
            {
                this.Sprites.Clear();
                this.PaintHeaderSprites();
                this.PaintBody();
                return this.Sprites;
            }

            /// <summary>
            /// Moves the selected visual up. Repaints the screen.
            /// </summary>
            public void Up()
            {
                this.HighlightedIndex--;
                this.Paint();
            }

            /// <summary>
            /// Moves the selected visual down. Repaints the screen.
            /// </summary>
            public void Down()
            {
                this.HighlightedIndex++;
                this.Paint();
            }

            /// <summary>
            /// Selects the highlighted row.
            /// </summary>
            public void Select()
            {
                if (this.SelectedIndex == this.HighlightedIndex)
                {
                    this.SelectedIndex = -1;
                }
                else
                {
                    this.SelectedIndex = this.HighlightedIndex;
                }

                this.Paint();
            }

            /// <summary>
            /// Get a list of sprites that comprise the header.
            /// </summary>
            /// <returns></returns>
            protected abstract void PaintHeaderSprites();

            /// <summary>
            /// Get a list of sprites that comprise the row at the index.
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            protected abstract void PaintRowSprites(int index);

            /// <summary>
            /// Shifts the position down one row and restarts it on the left.
            /// </summary>
            protected void CarriageReturn()
            {
                this.position = new Vector2(0, this.position.Y + this.RowHeight + 10);
            }

            /// <summary>
            /// Paints the body.
            /// </summary>
            private void PaintBody()
            {
                for (int i = this.TopRowIndex; i < this.Rows.Count && i <= this.BottomRowIndex; i++)
                {
                    this.PaintRowSprites(i);
                    this.CarriageReturn();
                }
            }

            /// <summary>
            /// Sets the scroll.
            /// </summary>
            protected void SetScroll()
            {
                // First, find how many rows we can fit
                // Second, find what rows are in view
                // Third, if we move outside, scroll in that direction
                if (this.HighlightedIndex == -1)
                {
                    this.TopRowIndex = 0;
                    return;
                }

                // Move the top row index up one (down on the screen) when the highlighted row goes below the screen.
                while (this.HighlightedIndex > this.BottomRowIndex)
                {
                    this.TopRowIndex += 1;
                }

                // Move the top row index down one (up on the screen) when the highlighted row goes above the screen.
                while (this.HighlightedIndex < this.TopRowIndex)
                {
                    this.TopRowIndex -= 1;
                }
            }
        }

        public abstract class MultiSelectList<T>
        {
            /// <summary>
            /// Internal list of sprites.
            /// </summary>
            protected readonly List<MySprite> Sprites = new List<MySprite>();

            /// <summary>
            /// Index of the top row in view.
            /// </summary>
            private int TopRowIndex = 0;

            /// <summary>
            /// Selected Index.
            /// </summary>
            private readonly List<int> selectedIndexes = new List<int>();

            /// <summary>
            /// High Lighted Index.
            /// </summary>
            private int highlightedIndex = -1;

            /// <summary>
            /// Moving position for sprite creation.
            /// </summary>
            protected Vector2 position;

            /// <summary>
            /// Gets or sets the row height. Defaults to 20.
            /// </summary>
            public float RowHeight { get; set; } = 20;

            /// <summary>
            /// Gets or sets the selected index.
            /// </summary>
            public List<int> SelectedIndexes
            {
                get
                {
                    return this.selectedIndexes;
                }
            }

            /// <summary>
            /// Gets or sets the highlighted index.
            /// </summary>
            public int HighlightedIndex
            {
                get
                {
                    return this.highlightedIndex;
                }
                set
                {
                    if (value < -1)
                    {
                        this.highlightedIndex = -1;
                        this.TopRowIndex = 0;
                    }

                    this.SetScroll();
                    this.highlightedIndex = value;
                }
            }

            /// <summary>
            /// Gets the rows.
            /// </summary>
            public List<T> Rows { get; } = new List<T>();

            /// <summary>
            /// Gets the Display Size.
            /// </summary>
            public virtual Vector2 DisplaySize { get; set; }

            /// <summary>
            /// Gets the max rows visible on the grid.
            /// </summary>
            protected int MaxRows
            {
                get
                {
                    return (int)(this.DisplaySize.X / this.RowHeight);
                }
            }

            /// <summary>
            /// Gets the bottom row index visible.
            /// </summary>
            protected int BottomRowIndex
            {
                get
                {
                    return this.TopRowIndex + this.MaxRows - 1;
                }
            }

            /// <summary>
            /// Paints the screen with the new list of refineries.
            /// </summary>
            /// <param name="refineries"></param>
            public List<MySprite> Paint()
            {
                this.Sprites.Clear();
                this.PaintHeaderSprites();
                this.PaintBody();
                return this.Sprites;
            }

            /// <summary>
            /// Moves the selected visual up. Repaints the screen.
            /// </summary>
            public void Up()
            {
                this.HighlightedIndex--;
                this.Paint();
            }

            /// <summary>
            /// Moves the selected visual down. Repaints the screen.
            /// </summary>
            public void Down()
            {
                this.HighlightedIndex++;
                this.Paint();
            }

            /// <summary>
            /// Selects the highlighted row.
            /// </summary>
            public void Select()
            {
                if (this.SelectedIndexes.Contains(this.HighlightedIndex))
                {
                    this.SelectedIndexes.Remove(this.HighlightedIndex);
                }
                else
                {
                    this.SelectedIndexes.Add(this.HighlightedIndex);
                }

                this.Paint();
            }

            /// <summary>
            /// Get a list of sprites that comprise the header.
            /// </summary>
            /// <returns></returns>
            protected abstract void PaintHeaderSprites();

            /// <summary>
            /// Get a list of sprites that comprise the row at the index.
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            protected abstract void PaintRowSprites(int index);

            /// <summary>
            /// Shifts the position down one row and restarts it on the left.
            /// </summary>
            protected void CarriageReturn()
            {
                this.position = new Vector2(0, this.position.Y + this.RowHeight);
            }

            /// <summary>
            /// Paints the body.
            /// </summary>
            private void PaintBody()
            {
                for (int i = this.TopRowIndex; i < this.Rows.Count && i <= this.BottomRowIndex; i++)
                {
                    this.PaintRowSprites(i);
                    this.CarriageReturn();
                }
            }

            /// <summary>
            /// Sets the scroll.
            /// </summary>
            protected void SetScroll()
            {
                // First, find how many rows we can fit
                // Second, find what rows are in view
                // Third, if we move outside, scroll in that direction
                if (this.HighlightedIndex == -1)
                {
                    this.TopRowIndex = 0;
                    return;
                }

                // Move the top row index up one (down on the screen) when the highlighted row goes below the screen.
                while (this.HighlightedIndex > this.BottomRowIndex)
                {
                    this.TopRowIndex += 1;
                }

                // Move the top row index down one (up on the screen) when the highlighted row goes above the screen.
                while (this.HighlightedIndex < this.TopRowIndex)
                {
                    this.TopRowIndex -= 1;
                }
            }
        }

    }
}

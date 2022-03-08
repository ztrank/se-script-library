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
        public class RefinerySelectableList : SelectableList<Refinery>
        {
            /// <summary>
            /// Display Grid.
            /// </summary>
            private DisplayGrid displayGrid;

            /// <summary>
            /// Gets or sets the display grid.
            /// </summary>
            public DisplayGrid DisplayGrid 
            { 
                get
                {
                    return this.displayGrid;
                }
                set
                {
                    this.displayGrid = value;
                    this.DisplaySize = new Vector2(this.DisplayGrid.Width, this.DisplayGrid.Height);
                }
            }

            /// <summary>
            /// Paints the header sprites.
            /// </summary>
            protected override void PaintHeaderSprites()
            {
                this.position = new Vector2(10, 0);
                // Create a clipping box
                // Create a line for the header
                // Create a line for the table header
                // Create a 
                // Refinery Status
                // [?] | Name               |  Status
                // -----------------------------
                this.CarriageReturn();
                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Refinery Status",
                    Alignment = TextAlignment.CENTER,
                    FontId = "White",
                    Color = Color.White,
                    Position = new Vector2(this.DisplayGrid.Width / 2f, this.position.Y / 2f),
                    RotationOrScale = 1f
                });
                this.CarriageReturn();

                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareHollow",
                    Color = Color.White,
                    Position = this.position,
                    Size = new Vector2(this.RowHeight, this.RowHeight)
                });
                this.position += new Vector2(this.RowHeight + 10, 0);
                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Name",
                    Alignment = TextAlignment.LEFT,
                    FontId = "White",
                    Color = Color.White,
                    Position = position - new Vector2(0, this.RowHeight / 2f),
                    RotationOrScale = 1f
                });

                Vector2 textSize = this.DisplayGrid.GetTextSize("Name", "White", 1);
                this.position += new Vector2(textSize.X, 0);
                /*
                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Status",
                    Alignment = TextAlignment.RIGHT,
                    FontId = "White",
                    Color = Color.White,
                    Position = position,
                    RotationOrScale = 1f
                });
                */
                this.CarriageReturn();
                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Size = new Vector2(this.DisplaySize.X, 4),
                    Position = position
                });
                // Start over at the left side.
                this.CarriageReturn();
            }

            /// <summary>
            /// Paints the row sprites.
            /// </summary>
            /// <param name="index">Row index.</param>
            protected override void PaintRowSprites(int index)
            {
                // Rows should be the same, except the highlighted colors are inverted
                // And the selected should have a checked box
                Refinery refinery = this.Rows[index];
                if (refinery == null)
                {
                    return;
                }
                bool isSelected = index == this.SelectedIndex;
                bool isHighlighted = index == this.HighlightedIndex;

                Color background = Color.Black.Alpha(0);
                Color foreground = Color.White;
                if (isHighlighted)
                {
                    background = Color.White;
                    foreground = Color.Black;
                }

                // Add the background sprite.
                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Color = background,
                    Size = new Vector2(this.DisplaySize.X, this.RowHeight),
                    Position = this.position
                });

                if (isSelected)
                {
                    this.Sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = "SquareTapered",
                        Color = foreground,
                        Size = new Vector2(this.RowHeight, this.RowHeight),
                        Position = this.position
                    });

                }
                else
                {
                    this.Sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = "SquareHollow",
                        Color = foreground,
                        Size = new Vector2(this.RowHeight, this.RowHeight),
                        Position = this.position
                    });
                }

                this.position += new Vector2(this.RowHeight + 10, 0);
                string name = refinery.RefineryBlock.CustomName;
                string status = refinery.Status;
                Vector2 nameSize = this.DisplayGrid.GetTextSize(name, "White", 1);
                Vector2 statusSize = this.DisplayGrid.GetTextSize(status, "White", 1);

                float maxNameSize = this.DisplayGrid.Width - this.position.X - statusSize.X - 10;

                while (nameSize.X > maxNameSize)
                {
                    name = name.Substring(0, name.Length - 4) + "...";
                    nameSize = this.DisplayGrid.GetTextSize(name, "White", 1);
                }

                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = name,
                    Color = foreground,
                    Position = this.position - new Vector2(0, this.RowHeight / 2f),
                    Alignment = TextAlignment.LEFT,
                    RotationOrScale = 1f
                });
                this.position += new Vector2(nameSize.X, 0);
                /*
                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = status,
                    Color = foreground,
                    Position = this.position,
                    Alignment = TextAlignment.RIGHT,
                    RotationOrScale = 1f
                });*/
            }
        }

        public class RefineryViewController : Controller
        {
            /// <summary>
            /// Internal list of refineries for repainting without getting a new list.
            /// </summary>
            private readonly List<Refinery> Refineries = new List<Refinery>();

            /// <summary>
            /// Selectable List
            /// </summary>
            private readonly RefinerySelectableList selectableList = new RefinerySelectableList();

            /// <summary>
            /// Display Grid.
            /// </summary>
            private DisplayGrid DisplayGrid;

            /// <summary>
            /// Gets the selected refinery.
            /// </summary>
            public Refinery Selected
            {
                get
                {
                    if (this.selectableList.SelectedIndex < 0 || this.selectableList.SelectedIndex > this.Refineries.Count - 1)
                    {
                        return null;
                    }

                    return this.Refineries[this.selectableList.SelectedIndex];
                }
            }


            /// <summary>
            /// Moves the selected visual up. Repaints the screen.
            /// </summary>
            public void Up()
            {
                this.selectableList.HighlightedIndex--;
                this.Paint();
            }

            /// <summary>
            /// Moves the selected visual down. Repaints the screen.
            /// </summary>
            public void Down()
            {
                this.selectableList.HighlightedIndex++;
                this.Paint();
            }

            /// <summary>
            /// Selects the highlighted row.
            /// </summary>
            public void Select()
            {
                this.selectableList.Select();
                this.Paint();
            }

            public void SetDisplayGrid(IMyBlockGroup refineryListViewGroup, bool overwrite = false)
            {
                if (overwrite || this.DisplayGrid == null)
                {
                    this.DisplayGrid = new DisplayGrid(refineryListViewGroup, this.Stdout);
                    this.selectableList.DisplayGrid = this.DisplayGrid;
                }
            }

            public void SetRefineries(List<Refinery> refineries)
            {
                // Check if the refinery lists match.
                refineries.Sort();
                bool shouldAdd = false;

                if (this.Refineries.Count != refineries.Count)
                {
                    shouldAdd = true;
                }
                else if (!this.Refineries.All(myR => refineries.Find(r => r.IsSameRefinery(myR)) != null))
                {
                    shouldAdd = true;
                }

                // If the lists don't match, recreate our list and deselect / dehighlight everything.
                // This should only happen if a refinery is added or removed, or at init.
                if (shouldAdd)
                {
                    this.Refineries.Clear();
                    this.Refineries.AddRange(refineries);
                    this.selectableList.Rows.Clear();
                    this.selectableList.Rows.AddRange(this.Refineries);
                    this.selectableList.HighlightedIndex = -1;
                    this.selectableList.SelectedIndex = -1;
                }
            }

            /// <summary>
            /// Paints the screen with the new list of refineries.
            /// </summary>
            /// <param name="refineries"></param>
            public void Paint()
            {
                this.DisplayGrid.SetSprites(this.selectableList.Paint());
                this.DisplayGrid.Paint();
            }
        }
    }
}

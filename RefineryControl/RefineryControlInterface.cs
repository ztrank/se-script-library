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
        public class OreSelectionList : MultiSelectList<string>
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

            public string Mode { get; set; } = "Disallowed";

            /// <summary>
            /// Paints the header sprites.
            /// </summary>
            protected override void PaintHeaderSprites()
            {
                // Allowed Ores / Refinables
                // [] Name
                // -----------------------

                this.position = new Vector2(0, 0);
                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"{this.Mode} Ores / Refinables",
                    Color = Color.White,
                    Position = this.position,
                    Alignment = TextAlignment.CENTER,
                    FontId = "White"
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
                    Color = Color.White,
                    Position = this.position,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                this.CarriageReturn();
                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Color = Color.White,
                    Size = new Vector2(this.DisplaySize.X, 2),
                    Position = this.position
                });

            }

            /// <summary>
            /// Paints the row sprites.
            /// </summary>
            /// <param name="index">Index to paint</param>
            protected override void PaintRowSprites(int index)
            {
                bool isSelected = this.SelectedIndexes.Contains(index);
                bool isHighlighed = this.HighlightedIndex == index;
                string ore = this.Rows[index];
                if (string.IsNullOrWhiteSpace(ore))
                {
                    return;
                }

                Color background = Color.Black.Alpha(0);
                Color foreground = Color.White;
                if (isHighlighed)
                {
                    background = Color.White;
                    foreground = Color.Black;
                }

                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Color = background,
                    Position = this.position,
                    Size = new Vector2(this.DisplaySize.X, this.RowHeight)
                });

                if (isSelected)
                {
                    this.Sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = "SquareTapered",
                        Color = foreground,
                        Position = this.position,
                        Size = new Vector2(this.RowHeight, this.RowHeight)
                    });
                }
                else
                {
                    this.Sprites.Add(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = "SquareHollow",
                        Color = foreground,
                        Position = this.position,
                        Size = new Vector2(this.RowHeight, this.RowHeight)
                    });
                }
                this.position += new Vector2(this.RowHeight + 10, 0);

                this.Sprites.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = ore,
                    Color = foreground,
                    Position = this.position,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
            }
        }

        public class RefineryControlInterface : Controller
        {
            private readonly string ObjectBuilderName = "MyObjectBuilder_Ore";
            private readonly List<MyInventoryItemFilter> Filters = new List<MyInventoryItemFilter>();
            private readonly IImmutableList<string> oreNames = new List<string>() 
            {
                "Cobalt",
                "Gold",
                "Ice",
                "Iron",
                "Magnesium",
                "Nickel",
                "Organic",
                "Platinum",
                "Script",
                "Silicon",
                "Silver",
                "Stone",
                "Uranium"
            }.ToImmutableList();

            public IImmutableList<string> OreNames
            {
                get
                {
                    return this.oreNames;
                }
            }

            private Refinery refinery;

            public MyDefinitionId GetOreDefinitionId(string name)
            {
                return new MyItemType(this.ObjectBuilderName, name);
            }

            private readonly OreSelectionList oreSelectionList = new OreSelectionList();

            public RefineryControlInterface()
            {
                this.oreSelectionList.Rows.AddRange(this.oreNames);
            }


            public void Up()
            {
                this.oreSelectionList.Up();
            }

            public void Down()
            {
                this.oreSelectionList.Down();
            }

            public void Select()
            {
                this.oreSelectionList.Select();
            }

            private List<MySprite> NoRefinerySprites = new List<MySprite>();
            public void Paint()
            {
                if (this.refinery != null)
                {
                    this.oreSelectionList.Paint();
                }
                else
                {
                    this.NoRefinerySprites.Clear();
                    this.NoRefinerySprites.Add(
                        new MySprite()
                        {
                            Type = SpriteType.TEXT,
                            Data = "Select a refinery.",
                            Alignment = TextAlignment.CENTER,
                            FontId = "White",
                            Color = Color.White,
                            Position = new Vector2(this.DisplayGrid.Width / 2f, this.DisplayGrid.Height / 2f)
                        });
                    this.DisplayGrid.SetSprites(this.NoRefinerySprites);
                }
            }

            public void SetRefinery(Refinery refinery)
            {
                this.refinery = refinery;
                this.oreSelectionList.SelectedIndexes.Clear();
                if (refinery != null)
                {
                    this.oreSelectionList.Mode = refinery.Mode == MyConveyorSorterMode.Blacklist ? "Disallowed" : "Allowed";
                    // Get the selected ores and set them as selected indexes.
                    foreach (MyInventoryItemFilter filter in refinery.Filter)
                    {
                        int oreIndex = this.oreSelectionList.Rows.FindIndex(ore => this.GetOreDefinitionId(ore) == filter.ItemId);
                        if (oreIndex > -1)
                        {
                            this.oreSelectionList.SelectedIndexes.Add(oreIndex);
                        }
                    }
                }
            }

            public void ToggleMode()
            {
                this.oreSelectionList.Mode = this.oreSelectionList.Mode == "Disallowed" ? "Allowed" : "Disallowed";
            }

            public string GetMode()
            {
                return this.oreSelectionList.Mode;
            }

            private DisplayGrid DisplayGrid;

            public void SetDisplayGrid(DisplayGrid grid)
            {
                this.DisplayGrid = grid;
                this.oreSelectionList.DisplayGrid = grid;
            }

            public List<MyInventoryItemFilter> GetFilters()
            {
                this.Filters.Clear();
                foreach(int index in this.oreSelectionList.SelectedIndexes)
                {
                    this.Filters.Add(new MyInventoryItemFilter(this.GetOreDefinitionId(this.oreSelectionList.Rows[index])));
                }

                return this.Filters;
            }
        }
    }
}

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
        /// User Interface namespace.
        /// </summary>
        public partial class UserInterface
        {
            public const string SurfaceSection = "surface";
            /// <summary>
            /// Invalid Surface Exception.
            /// </summary>
            public class InvalidSurfaceException : Exception
            {
                /// <summary>
                /// Creates a new Invalid Surface Exception.
                /// </summary>
                /// <param name="name">Name of block.</param>
                /// <param name="message">Exception Message.</param>
                public InvalidSurfaceException(string name, string message) 
                    : base($"${name}: {message}")
                {
                    this.Name = name;
                }

                /// <summary>
                /// Gets the name of the invalid block.
                /// </summary>
                public string Name { get; }
            }

            /// <summary>
            /// Surface class.
            /// </summary>
            public partial class Surface
            {
                /// <summary>
                /// Internal INI for the controlling block.
                /// </summary>
                private readonly MyIni ini = new MyIni();

                /// <summary>
                /// Internal row.
                /// </summary>
                private readonly int row;

                /// <summary>
                /// Internal column.
                /// </summary>
                private readonly int column;

                /// <summary>
                /// Internal origin.
                /// </summary>
                private readonly SpriteOrigins origin;

                /// <summary>
                /// Creates a new Surface from a text panel.
                /// </summary>
                /// <param name="panel">Text Panel.</param>
                /// <param name="isSolo">Is not in a grid.</param>
                public Surface(IMyTextPanel panel, bool isSolo = false)
                    : this(panel, panel, isSolo)
                {    
                }

                /// <summary>
                /// Creates a new Surface from a surface provider and index.
                /// </summary>
                /// <param name="provider">Surface provider.</param>
                /// <param name="index">Surface index.</param>
                /// <param name="isSolo">Is not in a grid.</param>
                public Surface(IMyTextSurfaceProvider provider, int index, bool isSolo = false)
                    : this((IMyTerminalBlock)provider, provider.GetSurface(index), isSolo)
                {
                    
                }

                /// <summary>
                /// Creates a new surface from a block and surface.
                /// </summary>
                /// <param name="block">Terminal Block.</param>
                /// <param name="surface">Text Surface.</param>
                /// <param name="isSolo">Is not in a grid.</param>
                public Surface(IMyTerminalBlock block, IMyTextSurface surface, bool isSolo = false)
                {
                    this.TextSurface = surface;
                    this.ViewPort = new RectangleF((this.TextSurface.TextureSize - this.TextSurface.SurfaceSize) / 2f, this.TextSurface.SurfaceSize);

                    if (isSolo)
                    {
                        this.row = 0;
                        this.column = 0;
                        this.origin = SpriteOrigins.TopLeft;
                    }
                    else
                    {
                        if (this.ini.TryParse(block.CustomData))
                        {
                            if (!this.ini.Get(SurfaceSection, "row").TryGetInt32(out this.row))
                            {
                                throw new InvalidSurfaceException(block.CustomName, "Invalid row number.");
                            }

                            if (!this.ini.Get(SurfaceSection, "column").TryGetInt32(out this.column))
                            {
                                throw new InvalidSurfaceException(block.CustomName, "Invalid column number.");
                            }

                            if (!Enum.TryParse(this.ini.Get(SurfaceSection, "origin").ToString("TopLeft"), true, out this.origin))
                            {
                                throw new InvalidSurfaceException(block.CustomName, "Invalid origin. Try 'TopLeft', 'TopRight', 'BottomLeft', 'BottomRight'");
                            }
                        }
                        else
                        {
                            throw new InvalidSurfaceException(block.CustomName, "Invalid INI.");
                        }
                    }
                }

                /// <summary>
                /// Gets the sprite origin.
                /// </summary>
                public SpriteOrigins Origin => this.origin;

                /// <summary>
                /// Gets the column.
                /// </summary>
                public int Column => this.column;

                /// <summary>
                /// Gets the row.
                /// </summary>
                public int Row => this.row;

                /// <summary>
                /// Gets the text surface.
                /// </summary>
                public IMyTextSurface TextSurface { get; }

                /// <summary>
                /// Gets a value indicating whether this is the primary panel in a grid.
                /// </summary>
                public bool IsPrimary => this.row == 0 && this.column == 0;

                /// <summary>
                /// Gets the view port.
                /// </summary>
                public RectangleF ViewPort { get; private set; }

                /// <summary>
                /// Gets the writable size.
                /// </summary>
                public Vector2 Size => this.ViewPort.Size;

                /// <summary>
                /// Gets the offset.
                /// </summary>
                public Vector2 Offset { get; set; }
            }
        }
    }
}

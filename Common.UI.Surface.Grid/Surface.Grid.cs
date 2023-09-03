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
        /// User interface namespace.
        /// </summary>
        public partial class UserInterface
        {
            /// <summary>
            /// Surface namespace.
            /// </summary>
            public partial class Surface
            {
                /// <summary>
                /// Invalid Surface Grid Excetpion.
                /// </summary>
                public class InvalidSurfaceGridException : Exception
                {
                    /// <summary>
                    /// Creates a new instance of the Invalid Surface Grid Excetpion.
                    /// </summary>
                    /// <param name="message">Message to send.</param>
                    public InvalidSurfaceGridException(string message)
                        : base(message)
                    {
                    }
                }

                /// <summary>
                /// Grid of surfaces.
                /// </summary>
                public partial class Grid
                {
                    /// <summary>
                    /// Internal terminal block list.
                    /// </summary>
                    private readonly List<IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();

                    /// <summary>
                    /// List of surfaces.
                    /// </summary>
                    private readonly List<Surface> surfaces = new List<Surface>();

                    /// <summary>
                    /// Primary surface.
                    /// </summary>
                    private readonly Surface primary;

                    /// <summary>
                    /// Ini reader.
                    /// </summary>
                    private readonly MyIni ini = new MyIni();

                    /// <summary>
                    /// Creates a new instance of the surface grid.
                    /// </summary>
                    /// <param name="blockGroup">Block group.</param>
                    /// <param name="referenceBlock">Reference block.</param>
                    public Grid(IMyBlockGroup blockGroup, IMyTerminalBlock referenceBlock)
                    {
                        blockGroup.GetBlocks(terminalBlocks, b => b.IsSameConstructAs(referenceBlock) && (b is IMyTextPanel || b is IMyTextSurfaceProvider));

                        if (this.terminalBlocks.Count == 1)
                        {
                            this.primary = this.CreateSurface(this.terminalBlocks[0], true);
                            this.surfaces.Add(this.primary);
                        }
                        else
                        {
                            foreach(IMyTerminalBlock block in this.terminalBlocks)
                            {
                                Surface surface = this.CreateSurface(block, false);
                                this.surfaces.Add(surface);

                                if (surface.IsPrimary && this.primary == null)
                                {
                                    this.primary = surface;
                                }
                                else if (surface.IsPrimary && this.primary != null)
                                {
                                    throw new InvalidSurfaceGridException("Multiple panels marked as Primary: (0,0)");
                                }
                            }

                            if (this.primary == null)
                            {
                                throw new InvalidSurfaceGridException("No panel marked as Primary: (0,0)");
                            }
                        }

                        Vector2 offsetUnitVector = SpriteHelper.GetOffsetUnitVector(this.Primary.Origin);

                        foreach (Surface panel in this.surfaces)
                        {
                            panel.Offset = offsetUnitVector * new Vector2(panel.Column, panel.Row) * this.Primary.Size;
                            if (panel.Column == this.Primary.Column)
                            {
                                this.Height += panel.Size.Y;
                            }
                            if (panel.Row == this.Primary.Row)
                            {
                                this.Width += panel.Size.X;
                            }
                        }
                    }

                    /// <summary>
                    /// Gets the total grid height.
                    /// </summary>
                    public float Height { get; private set; }

                    /// <summary>
                    /// Gets the total grid width.
                    /// </summary>
                    public float Width { get; private set; }

                    /// <summary>
                    /// Gets the origin locations of the grid.
                    /// </summary>
                    public SpriteOriginPositions Origins
                    {
                        get
                        {
                            return new SpriteOriginPositions(
                                new Vector2(0, 0),
                                new Vector2(this.Width, 0),
                                new Vector2(this.Width, this.Height),
                                new Vector2(this.Width, 0),
                                new Vector2(this.Width / 2f, this.Height / 2f));
                        }
                    }

                    /// <summary>
                    /// Gets the list of all surfaces.
                    /// </summary>
                    public List<Surface> Surfaces => this.surfaces;

                    /// <summary>
                    /// Gets the primary surface.
                    /// </summary>
                    public Surface Primary => this.primary;

                    /// <summary>
                    /// Creates a new surface.
                    /// </summary>
                    /// <param name="block">Terminal block to get the settings and surface.</param>
                    /// <param name="isSolo">Is the block part of a grid.</param>
                    /// <returns>New Surface instance.</returns>
                    private Surface CreateSurface(IMyTerminalBlock block, bool isSolo)
                    {
                        if (block is IMyTextPanel)
                        {
                            return new Surface((IMyTextPanel)block, isSolo);
                        }
                        else
                        {
                            this.ini.Clear();
                            if (this.ini.TryParse(block.CustomData))
                            {
                                int index = this.ini.Get(SurfaceSection, "index").ToInt32(0);
                                return new Surface(block, ((IMyTextSurfaceProvider)block).GetSurface(index), isSolo);
                            }
                            else
                            {
                                return new Surface(block, ((IMyTextSurfaceProvider)block).GetSurface(0), isSolo);
                            }
                        }
                    }
                }

            }
        }
    }
}

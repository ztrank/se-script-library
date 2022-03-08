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
        /// <summary>
        /// Capacity, Level, and Fill Ratio of a gas tank or collection of tanks.
        /// </summary>
        public struct GasLevel
        {
            /// <summary>
            /// Gets the total gas level for the list of tanks.
            /// </summary>
            /// <param name="tanks">Tanks to sum.</param>
            /// <returns>Total gas level.</returns>
            public static GasLevel GetGasLevel(IEnumerable<IMyGasTank> tanks)
            {
                GasLevel level = new GasLevel(0, 0);
                foreach (IMyGasTank tank in tanks)
                {
                    level += new GasLevel(tank);
                }

                return level;
            }

            /// <summary>
            /// Creates a new GasLevel object.
            /// </summary>
            /// <param name="tank">Gas Tank.</param>
            public GasLevel(IMyGasTank tank) : this(tank.Capacity, tank.Capacity * tank.FilledRatio)
            {
            }

            /// <summary>
            /// Creates a new gas level with the given capacity and level.
            /// </summary>
            /// <param name="capacity">Tank Capacity.</param>
            /// <param name="level">Tank Level.</param>
            private GasLevel(float capacity, double level)
            {
                this.Capacity = capacity;
                this.Level = level;
            }

            /// <summary>
            /// Gets the total capacity of the tank.
            /// </summary>
            public float Capacity { get; }

            /// <summary>
            /// Gets the current fill amount of the tank.
            /// </summary>
            public double Level { get; }

            /// <summary>
            /// Gets the current fill ratio of the tank.
            /// </summary>
            public double FillRatio
            {
                get
                {
                    if (this.Capacity <= 0 || this.Level <= 0)
                    {
                        return 0;
                    }

                    return this.Level / this.Capacity;
                }
            }

            /// <summary>
            /// Adds the level of two tanks.
            /// </summary>
            /// <param name="a">First level.</param>
            /// <param name="b">Second level.</param>
            /// <returns>Sum of the two levels.</returns>
            public static GasLevel operator +(GasLevel a, GasLevel b) => new GasLevel(a.Capacity + b.Capacity, a.Level + b.Level);

            /// <summary>
            /// Subtracts the level of two tanks.
            /// </summary>
            /// <param name="a">First level.</param>
            /// <param name="b">Second level.</param>
            /// <returns>Difference of the two levels.</returns>
            public static GasLevel operator -(GasLevel a, GasLevel b) => new GasLevel(a.Capacity - b.Capacity, a.Level - b.Level);
        }
    }
}

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
        public partial class Ship
        {
            /// <summary>
            /// List of ship connectors for this grid.
            /// </summary>
            public List<IMyShipConnector> Connectors { get; } = new List<IMyShipConnector>();

            public Ship WithConnectors()
            {
                this.SetMyConnectors();
                return this;
            }

            /// <summary>
            /// Tries to get the first attached connector.
            /// </summary>
            /// <param name="otherConnector">First found attached connector.</param>
            /// <returns>True if an attached connector is found.</returns>
            public bool TryGetOtherConnector(out IMyShipConnector otherConnector)
            {
                if (!this.Connectors.Any())
                {
                    this.SetMyConnectors();
                }

                foreach (IMyShipConnector connector in this.Connectors)
                {
                    if (connector.Status == MyShipConnectorStatus.Connected && connector.OtherConnector != null && !connector.OtherConnector.IsSameConstructAs(this.Me))
                    {
                        otherConnector = connector.OtherConnector;
                        return true;
                    }
                }
                otherConnector = null;
                return false;
            }

            /// <summary>
            /// Sets the connectors of this dockable grid.
            /// </summary>
            public void SetMyConnectors()
            {
                this.GridTerminalSystem.GetBlocksOfType(this.Connectors, b => b.IsSameConstructAs(this.Me));
            }
        }
    }
}

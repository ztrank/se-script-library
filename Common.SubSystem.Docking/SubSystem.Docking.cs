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
        public interface IDockingSubsystem : ISubSystem
        {
            /// <summary>
            /// Gets the list of connectors.
            /// </summary>
            List<IMyShipConnector> Connectors { get; }

            /// <summary>
            /// Tries to get an attached connector.
            /// </summary>
            /// <param name="otherConnector">Other connector.</param>
            /// <returns>True if otherConnector is not null.</returns>
            bool TryGetOtherConnector(out IMyShipConnector otherConnector);
        }

        public class DockingSubSystem : SubSystem, IDockingSubsystem
        {
            /// <summary>
            /// List of ship connectors for this grid.
            /// </summary>
            public List<IMyShipConnector> Connectors { get; } = new List<IMyShipConnector>();

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
                    if (connector.Status == MyShipConnectorStatus.Connected && connector.OtherConnector != null && !connector.OtherConnector.IsSameConstructAs(this.CPU))
                    {
                        otherConnector = connector.OtherConnector;
                        return true;
                    }
                }
                otherConnector = null;
                return false;
            }

            /// <summary>
            /// Initializes the connectors.
            /// </summary>
            protected override void OnInitialize()
            {
                this.SetMyConnectors();
            }

            /// <summary>
            /// Sets the connectors of this dockable grid.
            /// </summary>
            private void SetMyConnectors()
            {
                this.Connectors.Clear();
                this.GridTerminalSystem.GetBlocksOfType(this.Connectors, b => b.IsSameConstructAs(this.CPU));
            }
        }
    }
}

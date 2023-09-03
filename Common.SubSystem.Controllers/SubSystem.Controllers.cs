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
        /// Controller Subsystem.
        /// </summary>
        public partial interface IControllerSubSystem : ISubSystem
        {
            /// <summary>
            /// Gets the main controller.
            /// </summary>
            IMyShipController Main { get; }

            /// <summary>
            /// Gets the list of all controllers.
            /// </summary>
            List<IMyShipController> Controllers { get; }
        }

        /// <summary>
        /// Controller sub system.
        /// </summary>
        public partial class ControllerSubSystem : SubSystem, IControllerSubSystem
        {
            /// <summary>
            /// Gets or sets the main ship controller.
            /// </summary>
            public IMyShipController Main { get; set; }

            /// <summary>
            /// Gets all controllers on the ship.
            /// </summary>
            public List<IMyShipController> Controllers { get; } = new List<IMyShipController>();

            /// <summary>
            /// Finds the controllers and attempts to find the main controller on the ship.
            /// </summary>
            /// <returns>The ship instance.</returns>
            protected override void OnInitialize()
            {
                this.GridTerminalSystem.GetBlocksOfType(this.Controllers, b => b.IsSameConstructAs(this.CPU));
                if (this.Controllers.Count > 1)
                {
                    foreach (IMyShipController controller in this.Controllers)
                    {
                        if (controller.IsMainCockpit)
                        {
                            this.Main = controller;
                        }
                    }
                }

                if (this.Main == null)
                {
                    this.Main = this.Controllers.FirstOrDefault();
                }
            }
        }
    }
}

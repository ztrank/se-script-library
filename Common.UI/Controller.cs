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
        /// User interface namespace class.
        /// </summary>
        public partial class UserInterface
        {
            /// <summary>
            /// Controller class for User Interfaces.
            /// </summary>
            /// <typeparam name="V">View type.</typeparam>
            /// <typeparam name="M">Model type.</typeparam>
            public abstract class Controller<V, M> : EventEmitter
                where V : View<M>
            {
                /// <summary>
                /// Grid terminal system.
                /// </summary>
                protected readonly IMyGridTerminalSystem GridTerminalSystem;

                /// <summary>
                /// Programmable block.
                /// </summary>
                protected readonly IMyProgrammableBlock CPU;

                /// <summary>
                /// Block Groups.
                /// </summary>
                protected readonly List<IMyBlockGroup> groups = new List<IMyBlockGroup>();

                /// <summary>
                /// Terminal Blocks.
                /// </summary>
                protected readonly List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

                /// <summary>
                /// Gets the view for this controller.
                /// </summary>
                protected abstract V View { get; }

                /// <summary>
                /// Gets the model for this controller.
                /// </summary>
                protected abstract M Model { get; }


                /// <summary>
                /// Creates a new instance of the controller.
                /// </summary>
                /// <param name="surface">Surface to draw on.</param>
                /// <param name="gridTerminalSystem">Grid Terminal System.</param>
                /// <param name="cpu">Programmable block.</param>
                public Controller(IMyTextSurface surface, IMyGridTerminalSystem gridTerminalSystem, IMyProgrammableBlock cpu)
                {
                    this.Surface = surface;
                    this.GridTerminalSystem = gridTerminalSystem;
                    this.CPU = cpu;
                }

                /// <summary>
                /// Gets the text surface.
                /// </summary>
                protected IMyTextSurface Surface { get; }

                /// <summary>
                /// Draws the sprites based on the model.
                /// </summary>
                /// <param name="offset">Offset for multiple screens.</param>
                /// <returns>Sprites drawn by view using the current model.</returns>
                public void Draw(Vector2 offset)
                {
                    this.View.Draw(offset);
                }

                /// <summary>
                /// Rerenders the view based on the model.
                /// </summary>
                protected void Rerender()
                {
                    this.View.Render(this.Model);
                    this.Emit("rerender", this.Model);
                }
            }
        }
    }
}

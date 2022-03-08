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
        /// Network of assemblers.
        /// </summary>
        public class AssemblerNetwork
        {
            /// <summary>
            /// List of assemblers and their connected containers.
            /// </summary>
            private readonly List<AssemblerContainerNetwork> Assemblers = new List<AssemblerContainerNetwork>();

            /// <summary>
            /// Main assembler.
            /// </summary>
            private readonly IMyAssembler MainAssembler;

            /// <summary>
            /// Standard out.
            /// </summary>
            private readonly Action<string> stdout;

            /// <summary>
            /// Creates a new instance of the assembler network.
            /// </summary>
            /// <param name="mainAssembler">Main assembler.</param>
            /// <param name="assemblers">All assemblers in the network.</param>
            /// <param name="stdout">Standard out.</param>
            public AssemblerNetwork(IMyAssembler mainAssembler, List<IMyAssembler> assemblers, Action<string> stdout)
            {
                this.MainAssembler = mainAssembler;
                this.stdout = stdout;
                this.Assemblers.AddRange(assemblers.Select(assembler => new AssemblerContainerNetwork(assembler, this.stdout)));
            }

            /// <summary>
            /// Connects the assemblers to their networked containers.
            /// </summary>
            /// <param name="cargoContainers">Cargo containers.</param>
            public void ConnectContainers(List<IMyCargoContainer> cargoContainers)
            {
                foreach(AssemblerContainerNetwork assembler in this.Assemblers)
                {
                    assembler.ConnectNetwork(cargoContainers);
                }
            }

            public void ClearQueue()
            {
                foreach(AssemblerContainerNetwork assembler in this.Assemblers)
                {
                    assembler.ClearQueue();
                }
            }

            public void EmptyInput()
            {
                foreach (AssemblerContainerNetwork assembler in this.Assemblers)
                {
                    assembler.EmptyInput();
                }
            }

            public void EmptyOutput()
            {
                foreach (AssemblerContainerNetwork assembler in this.Assemblers)
                {
                    assembler.EmptyOutput();
                }
            }
        }
    }
}

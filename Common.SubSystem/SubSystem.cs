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
        /// SubSystem Not Attached Exception.
        /// </summary>
        public class SubSystemNotAttachedException : Exception
        {
            /// <summary>
            /// Creates a new exception with the subsystem name.
            /// </summary>
            /// <param name="name">Subsystem name.</param>
            public SubSystemNotAttachedException(string name) 
                : base($"[{name}]: Not attached.")
            {
                this.Name = name;
            }

            /// <summary>
            /// Gets the name of the unattached subsystem.
            /// </summary>
            public string Name { get; }
        }

        /// <summary>
        /// Abstract SubSytem class for constituent components.
        /// </summary>
        public abstract class SubSystem
        {
            /// <summary>
            /// Terminal Block list.
            /// </summary>
            protected readonly List<IMyTerminalBlock> TerminalBlocks = new List<IMyTerminalBlock>();

            /// <summary>
            /// Grid Terminal System.
            /// </summary>
            private IMyGridTerminalSystem gridTerminalSystem;

            /// <summary>
            /// Programmable block running this script.
            /// </summary>
            private IMyProgrammableBlock cpu;

            /// <summary>
            /// Runtime info.
            /// </summary>
            private IMyGridProgramRuntimeInfo runtime;

            /// <summary>
            /// Inter Grid Communication.
            /// </summary>
            private IMyIntergridCommunicationSystem igc;

            /// <summary>
            /// Log Function.
            /// </summary>
            private Action<string> log;

            /// <summary>
            /// Error function.
            /// </summary>
            private Action<string> error;

            /// <summary>
            /// Debug value.
            /// </summary>
            private bool debug;

            /// <summary>
            /// Attaches this component to running program values.
            /// </summary>
            /// <param name="program">Running program.</param>
            /// <param name="log">Log function.</param>
            /// <param name="error">Error function.</param>
            /// <param name="debug">Debug value.</param>
            public void Attach(MyGridProgram program, Action<string> log, Action<string> error, bool debug = false)
            {
                this.Attach(program.GridTerminalSystem, program.Me, program.Runtime, program.IGC, log, error, debug);
            }

            /// <summary>
            /// Attaches this component to running program values.
            /// </summary>
            /// <param name="gridTerminalSystem">Grid terminal system.</param>
            /// <param name="cpu">Programmable block.</param>
            /// <param name="runtime">Runtime info.</param>
            /// <param name="igc">Intergrid communication.</param>
            /// <param name="log">Log function.</param>
            /// <param name="error">Error function.</param>
            /// <param name="debug">Debug flag.</param>
            public void Attach(IMyGridTerminalSystem gridTerminalSystem, IMyProgrammableBlock cpu, IMyGridProgramRuntimeInfo runtime, IMyIntergridCommunicationSystem igc, Action<string> log, Action<string> error, bool debug = false)
            {
                this.gridTerminalSystem = gridTerminalSystem;
                this.cpu = cpu;
                this.runtime = runtime;
                this.log = log;
                this.error = error;
                this.debug = debug;
                this.igc = igc;
            }

            /// <summary>
            /// Gets the Grid Terminal System.
            /// </summary>
            public IMyGridTerminalSystem GridTerminalSystem => this.AssertAttached(this.gridTerminalSystem);

            /// <summary>
            /// Gets the running CPU.
            /// </summary>
            public IMyProgrammableBlock CPU => this.AssertAttached(this.cpu);

            /// <summary>
            /// Gets the Runtime Info.
            /// </summary>
            public IMyGridProgramRuntimeInfo Runtime => this.AssertAttached(this.runtime);

            /// <summary>
            /// Gets the Intergrid communication system.
            /// </summary>
            public IMyIntergridCommunicationSystem IGC => this.AssertAttached(this.igc);
            
            /// <summary>
            /// Gets the debug flat.
            /// </summary>
            public bool Debug => this.debug;

            /// <summary>
            /// Gets a value indicating whether this subsystem has been attached or not.
            /// </summary>
            public bool IsAttached => this.GridTerminalSystem != null && this.CPU != null && this.Runtime != null && this.IGC != null;

            /// <summary>
            /// Gets a value indicating whether the subsystem has been initialized or not.
            /// </summary>
            public bool IsInitialized { get; private set; }

            /// <summary>
            /// Initializes the subsystem.
            /// </summary>
            public void Initialize()
            {
                if (!this.IsInitialized && this.IsAttached)
                {
                    this.OnInitialize();
                    this.IsInitialized = true;
                }
                else if (this.IsInitialized && this.IsAttached)
                {
                    this.OnReinitialize();
                }
                else if (!this.IsAttached)
                {
                    throw new SubSystemNotAttachedException(this.GetType().Name);
                }
            }

            /// <summary>
            /// For subsystem specific initialization.
            /// </summary>
            protected virtual void OnInitialize()
            {
            }

            /// <summary>
            /// For subystem specific reinitialization.
            /// </summary>
            protected virtual void OnReinitialize()
            {
            }

            /// <summary>
            /// Attaches the subsystem name and Logs the message if debug is true.
            /// </summary>
            /// <param name="message">Message to log.</param>
            public void Log(string message)
            {
                if (this.debug)
                {
                    this.log?.Invoke($"[{this.GetType().Name}]: {message}");
                }
            }

            /// <summary>
            /// Attaches the subsystem name and logs the error.
            /// </summary>
            /// <param name="message"></param>
            public void Error(string message)
            {
                this.error?.Invoke($"[{this.GetType().Name}]: {message}");
            }

            /// <summary>
            /// Asserts the object is not null, throws a Subsystem not attached exception if it is null.
            /// </summary>
            /// <typeparam name="T">Input / Output type.</typeparam>
            /// <param name="obj">Object to null check.</param>
            /// <returns>Non-null object.</returns>
            private T AssertAttached<T>(T obj)
            {
                if (obj == null)
                {
                    throw new SubSystemNotAttachedException(this.GetType().Name);
                }

                return obj;
            }
        }
    }
}

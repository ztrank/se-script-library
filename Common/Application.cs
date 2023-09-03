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
        /// Controller Abstract Class
        /// </summary>
        public abstract class Application
        {
            /// <summary>
            /// Internal dictionary of triggers to actions.
            /// </summary>
            private readonly Dictionary<UpdateType, List<Action<MyCommandLine>>> triggers = new Dictionary<UpdateType, List<Action<MyCommandLine>>>();

            /// <summary>
            /// Internal dictionary of actions.
            /// </summary>
            private readonly Dictionary<string, Dictionary<string, Action<MyCommandLine>>> controllerActions = new Dictionary<string, Dictionary<string, Action<MyCommandLine>>>();

            /// <summary>
            /// Command Line.
            /// </summary>
            private readonly MyCommandLine commandLine = new MyCommandLine();

            /// <summary>
            /// Flag to check if the application has been initialized.
            /// </summary>
            private bool IsInitialized = false;

            /// <summary>
            /// Creates a new application instance.
            /// </summary>
            /// <param name="gridTerminalSystem">Grid Terminal System.</param>
            /// <param name="me">Programmable Block.</param>
            /// <param name="runtime">Runtime Info.</param>
            /// <param name="echo">Echo action.</param>
            /// <param name="debug">Debug flag.</param>
            public Application(IMyGridTerminalSystem gridTerminalSystem, IMyProgrammableBlock me, IMyGridProgramRuntimeInfo runtime, Action<string> echo, bool debug = false)
            {
                this.GridTerminalSystem = gridTerminalSystem;
                this.Me = me;
                this.Runtime = runtime;
                this.Echo = echo;
                this.Debug = debug;
            }

            /// <summary>
            /// Gets or sets the Grid Terminal System.
            /// </summary>
            public IMyGridTerminalSystem GridTerminalSystem { get; set; }

            /// <summary>
            /// Gets or sets the programmable block for this grid.
            /// </summary>
            public IMyProgrammableBlock Me { get; set; }

            /// <summary>
            /// Gets or sets the runtime info.
            /// </summary>
            public IMyGridProgramRuntimeInfo Runtime { get; set; }

            /// <summary>
            /// Gets or sets the echo function.
            /// </summary>
            public Action<string> Echo { get; set; }

            /// <summary>
            /// Gets or sets the debug flag.
            /// </summary>
            public bool Debug { get; set; }

            /// <summary>
            /// Converts the parameterless action into a command line action.
            /// </summary>
            /// <param name="action">Parameterless action.</param>
            /// <returns>Command Line Action.</returns>
            private static Action<MyCommandLine> ToAction(Action action)
            {
                return (MyCommandLine cmd) => action();
            }

            /// <summary>
            /// Initializes the application once.
            /// </summary>
            public void Initialize()
            {
                if (!this.IsInitialized)
                {
                    this.OnInitialize();
                    this.IsInitialized = true;
                }
            }

            /// <summary>
            /// Initializes the Application.
            /// </summary>
            protected virtual void OnInitialize()
            {
            }

            /// <summary>
            /// Executes the application's main method, checking for triggers, then application actions, then controller actions.
            /// </summary>
            /// <param name="command">Parsed argument.</param>
            /// <param name="updateSource">Update type.</param>
            public void Execute(string argument, UpdateType updateSource)
            {
                this.commandLine.Clear();
                if (string.IsNullOrEmpty(argument) || this.commandLine.TryParse(argument))
                {
                    // Run update type triggers
                    foreach (UpdateType type in this.triggers.Keys)
                    {
                        if ((updateSource & type) > 0)
                        {
                            this.triggers[type]?.ForEach(trigger => trigger?.Invoke(commandLine));
                        }
                    }

                    // Run application actions next: actionName ...
                    if (commandLine.ArgumentCount >= 1 && !this.controllerActions.ContainsKey(commandLine.Argument(0)))
                    {
                        if (this.controllerActions.ContainsKey(string.Empty) && this.controllerActions[string.Empty].ContainsKey(commandLine.Argument(0)))
                        {
                            this.controllerActions[string.Empty][commandLine.Argument(0)]?.Invoke(commandLine);
                            return;
                        }
                    }

                    // Run controller actions last: controllerName actionName ...
                    if (commandLine.ArgumentCount >= 2)
                    {
                        if (this.controllerActions.ContainsKey(commandLine.Argument(0)) && this.controllerActions[commandLine.Argument(0)].ContainsKey(commandLine.Argument(1)))
                        {
                            this.controllerActions[commandLine.Argument(0)][commandLine.Argument(1)]?.Invoke(commandLine);
                            return;
                        }
                    }
                }
                
            }

            /// <summary>
            /// Adds a trigger to the application to be called when the flags are present on the update type of the execution.
            /// </summary>
            /// <param name="type">Type to check for during triggering.</param>
            /// <param name="trigger">Action to perform during trigger.</param>
            protected void AddTrigger(UpdateType type, Action trigger)
            {
                this.AddTrigger(type, ToAction(trigger));
            }

            /// <summary>
            /// Adds a trigger to the application to be called when the flags are present on the update type of the execution.
            /// </summary>
            /// <param name="type">Type to check for during triggering.</param>
            /// <param name="trigger">Action to perform during trigger.</param>
            protected void AddTrigger(UpdateType type, Action<MyCommandLine> trigger)
            {
                if (!this.triggers.ContainsKey(type))
                {
                    this.triggers.Add(type, new List<Action<MyCommandLine>>());
                }

                this.triggers[type].Add(trigger);
            }

            /// <summary>
            /// Adds an application action that executes before controller actions if there is overlap in the first argument.
            /// </summary>
            /// <param name="actionName">Name of action found in the first argument during execution.</param>
            /// <param name="action">Action to perform.</param>
            protected void AddAction(string actionName, Action action)
            {
                this.AddControllerAction(string.Empty, actionName, ToAction(action));
            }

            /// <summary>
            /// Adds an application action that executes before controller actions if there is overlap in the first argument.
            /// </summary>
            /// <param name="actionName">Name of action found in the first argument during execution.</param>
            /// <param name="action">Action to perform.</param>
            protected void AddAction(string actionName, Action<MyCommandLine> action)
            {
                this.AddControllerAction(string.Empty, actionName, action);
            }

            /// <summary>
            /// Adds a controller action that executes after triggers and application actions.
            /// </summary>
            /// <param name="controllerName">Name of the controller, the first argument.</param>
            /// <param name="actionName">Name of the action, the second argument.</param>
            /// <param name="action">Action to perform.</param>
            protected void AddControllerAction(string controllerName, string actionName, Action action)
            {
                this.AddControllerAction(controllerName, actionName, ToAction(action));
            }

            /// <summary>
            /// Adds a controller action that executes after triggers and application actions.
            /// </summary>
            /// <param name="controllerName">Name of the controller, the first argument.</param>
            /// <param name="actionName">Name of the action, the second argument.</param>
            /// <param name="action">Action to perform.</param>
            protected void AddControllerAction(string controllerName, string actionName, Action<MyCommandLine> action)
            {
                if (!this.controllerActions.ContainsKey(controllerName))
                {
                    this.controllerActions.Add(controllerName, new Dictionary<string, Action<MyCommandLine>>());
                }

                if (!this.controllerActions[controllerName].ContainsKey(actionName))
                {
                    this.controllerActions[controllerName].Add(actionName, action);
                }
                else
                {
                    throw new Exception($"Action already exists: {controllerName}.{actionName}");
                }
            }
        }
    }
}

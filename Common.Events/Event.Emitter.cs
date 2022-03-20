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
        /// Controller class for User Interfaces.
        /// </summary>
        /// <typeparam name="V">View type.</typeparam>
        /// <typeparam name="M">Model type.</typeparam>
        public class EventEmitter
        {
            /// <summary>
            /// Internal dictionary of event handlers.
            /// </summary>
            private readonly Dictionary<string, List<Action<string, object>>> eventHandlers = new Dictionary<string, List<Action<string, object>>>();

            /// <summary>
            /// Adds the event handler.
            /// </summary>
            /// <param name="event">Event name.</param>
            /// <param name="handler">Event handler.</param>
            public void On(string @event, Action<string, object> handler)
            {
                if (!this.eventHandlers.ContainsKey(@event))
                {
                    this.eventHandlers.Add(@event, new List<Action<string, object>>());
                }

                this.eventHandlers[@event].Add(handler);
            }

            /// <summary>
            /// Removes the event handler.
            /// </summary>
            /// <param name="event">Event name.</param>
            /// <param name="handler">Event handler.</param>
            public void Off(string @event, Action<string, object> handler)
            {
                if (this.eventHandlers.ContainsKey(@event))
                {
                    this.eventHandlers[@event].Remove(handler);
                }
            }

            /// <summary>
            /// Emits the event.
            /// </summary>
            /// <param name="event">Event name.</param>
            /// <param name="value">Event value.</param>
            public void Emit(string @event, object value)
            {
                if (this.eventHandlers.ContainsKey(@event))
                {
                    this.eventHandlers[@event].ForEach(handler => handler?.Invoke(@event, value));
                }
            }
        }
    }
}

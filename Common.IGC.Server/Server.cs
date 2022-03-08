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
        /// Server class for collecting channels for direct communication across grids, and relaying from one connection to another.
        /// </summary>
        public class Server
        {
            /// <summary>
            /// Intergrid Communication System.
            /// </summary>
            private readonly IMyIntergridCommunicationSystem IGC;

            /// <summary>
            /// Dictionary of channel names to listeners.
            /// </summary>
            private readonly Dictionary<string, IMyBroadcastListener> Channels = new Dictionary<string, IMyBroadcastListener>();

            /// <summary>
            /// Dictionary of channel names to list of handlers.
            /// </summary>
            private readonly Dictionary<string, List<Action<MyIGCMessage>>> Handlers = new Dictionary<string, List<Action<MyIGCMessage>>>();

            /// <summary>
            /// List of unhealthy channels.
            /// </summary>
            private readonly List<string> UnhealthyChannels = new List<string>();

            /// <summary>
            /// List of pending health checks.
            /// </summary>
            private readonly List<string> PendingHealthChecks = new List<string>();

            /// <summary>
            /// Log delegate.
            /// </summary>
            private readonly Action<string> LogDelegate;

            /// <summary>
            /// Error delegate.
            /// </summary>
            private readonly Action<string> Error;

            /// <summary>
            /// DNS Connection.
            /// </summary>
            private readonly IMyBroadcastListener DNS;

            /// <summary>
            /// Creates a new instance of the server.
            /// </summary>
            /// <param name="igc">Intergrid communication system.</param>
            /// <param name="dns">DNS Channel name.</param>
            /// <param name="log">Log delegate.</param>
            /// <param name="error">Error delegate.</param>
            public Server(IMyIntergridCommunicationSystem igc, string dns, Action<string> log, Action<string> error)
            {
                this.LogDelegate = log;
                this.ServerName = dns;
                this.Error = error;
                this.IGC = igc;
                this.DNS = this.IGC.RegisterBroadcastListener(dns);
                this.DNS.SetMessageCallback("");
            }

            /// <summary>
            /// Gets the Name of this server.
            /// </summary>
            public string ServerName { get; }

            /// <summary>
            /// Gets or sets a value indicating if debug messages are sent to the log delegate.
            /// </summary>
            public bool Debug { get; set; }

            /// <summary>
            /// Gets the handlers for a channel.
            /// </summary>
            /// <param name="channel">Channel name.</param>
            /// <returns>Non-null list of handlers.</returns>
            public List<Action<MyIGCMessage>> GetHandlers(string channel)
            {
                if (!this.Handlers.ContainsKey(channel))
                {
                    this.Handlers.Add(channel, new List<Action<MyIGCMessage>>());
                }

                return this.Handlers[channel];
            }

            /// <summary>
            /// Registers the handler to a channel.
            /// </summary>
            /// <param name="channel">Channel name.</param>
            /// <param name="handler">Handler delegate.</param>
            public void RegisterHandler(string channel, Action<MyIGCMessage> handler)
            {
                this.Log($"Handler Registered: {handler.Method.Name}");
                this.GetHandlers(channel).Add(handler);
            }

            /// <summary>
            /// Removes a handler from a channel.
            /// </summary>
            /// <param name="channel">Channel name.</param>
            /// <param name="handler">Handler delegate.</param>
            public void RemoveHandler(string channel, Action<MyIGCMessage> handler)
            {
                this.Log($"Handler Removed: {handler.Method.Name}");
                this.GetHandlers(channel).Remove(handler);
            }

            /// <summary>
            /// Processes the healthcheck.
            /// </summary>
            public void ProcessHealthChecks()
            {
                foreach (string channel in this.PendingHealthChecks)
                {
                    if (this.UnhealthyChannels.Contains(channel))
                    {
                        this.Log($"Channel is unhealth two checks in a row: {channel}");
                        this.CleanUpConnection(channel);
                        this.UnhealthyChannels.Remove(channel);
                    }
                }
                this.PendingHealthChecks.RemoveAll(channel => !this.Channels.ContainsKey(channel));

                foreach (string channel in this.Channels.Keys)
                {
                    if (this.PendingHealthChecks.Contains(channel))
                    {
                        this.UnhealthyChannels.Add(channel);
                        continue;
                    }

                    this.IGC.SendBroadcastMessage(channel, "healthcheck");
                    this.PendingHealthChecks.Add(channel);
                }
            }

            /// <summary>
            /// Processes messages. Intended to be used in the Main Method to check for messages each tick.
            /// </summary>
            public void ProcessMessages()
            {
                while (this.DNS.HasPendingMessage)
                {
                    MyIGCMessage message = this.DNS.AcceptMessage();
                    if (message.Data is MyTuple<string, string>)
                    {
                        MyTuple<string, string> data = (MyTuple<string, string>)message.Data;

                        // Connection Requests.
                        if (data.Item1 == "connection:request")
                        {
                            this.Log("Connection Request: " + data.Item2);
                            // Channel already is open, send accepted but don't create another listener.
                            if (this.Channels.ContainsKey(data.Item2))
                            {
                                this.Log("Connection Exists: " + data.Item2);
                                this.IGC.SendBroadcastMessage(this.DNS.Tag, new MyTuple<string, string>("connection:accepted", data.Item2));
                            }
                            // Channel is available, accept the request.
                            else
                            {
                                this.Log("Connection Accepted: " + data.Item2);
                                IMyBroadcastListener listener = this.IGC.RegisterBroadcastListener(data.Item2);
                                listener.SetMessageCallback("");
                                this.Channels.Add(data.Item2, listener);
                                this.IGC.SendBroadcastMessage(this.DNS.Tag, new MyTuple<string, string>("connection:accepted", data.Item2));
                            }
                        }
                        // Connection disconnect messages.
                        else if (data.Item1 == "connection:disconnect")
                        {
                            this.Log("Connection Disconnect: " + message.Tag);
                            if (this.Channels.ContainsKey(message.Tag))
                            {
                                this.IGC.DisableBroadcastListener(this.Channels[message.Tag]);
                                this.Channels.Remove(message.Tag);
                            }
                        }
                        else if (data.Item1 == "healthcheck")
                        {
                            this.IGC.SendBroadcastMessage(data.Item2, new MyTuple<string, string>("healthcheck", "OK"));
                        }
                        else
                        {
                            this.Error("Unrecognized DNS Request: " + data.Item1);
                        }
                    }
                    else if (message.Data is MyTuple<string, string, string> && message.As<MyTuple<string, string, string>>().Item1 == "healthcheck")
                    {
                        MyTuple<string, string, string> data = message.As<MyTuple<string, string, string>>();
                        if (data.Item3 == "OK")
                        {
                            this.Log($"Health Check OK: {data.Item2}");
                            this.UnhealthyChannels.Remove(data.Item2);
                            this.PendingHealthChecks.Remove(data.Item2);
                        }
                        else
                        {
                            this.CleanUpConnection(data.Item2);
                        }
                    }
                    else
                    {
                        this.Error("Malformed DNS Request.");
                    }
                }

                // Check each listener for messages.
                foreach (IMyBroadcastListener listener in this.Channels.Values)
                {
                    while (listener.HasPendingMessage)
                    {
                        MyIGCMessage message = listener.AcceptMessage();
                        this.Invoke(message);
                    }
                }
            }

            /// <summary>
            /// Cleans up a connection.
            /// </summary>
            /// <param name="channel">Channel to clean up.</param>
            private void CleanUpConnection(string channel)
            {
                if (this.Channels.ContainsKey(channel))
                {
                    this.Log($"Removing connection: {channel}");
                    this.IGC.DisableBroadcastListener(this.Channels[channel]);
                    this.Channels.Remove(channel);
                }
                else
                {
                    this.Log($"Can't clean up, connection does not exist: {channel}");
                }
            }

            /// <summary>
            /// Invokes the handlers for a given message and logs any errors from the handler.
            /// </summary>
            /// <param name="message">Message to invoke.</param>
            private void Invoke(MyIGCMessage message)
            {
                this.Log($"Invoking Message: {message.Tag} | {message.Data}");
                foreach (Action<MyIGCMessage> handler in this.GetHandlers(message.Tag))
                {
                    try
                    {
                        handler?.Invoke(message);
                    }
                    catch (Exception e)
                    {
                        this.Error?.Invoke($"{handler.Method.Name}: {e.Message}");
                    }
                }
            }

            /// <summary>
            /// Logs the message if Debug is true.
            /// </summary>
            /// <param name="message">Message to log.</param>
            private void Log(string message)
            {
                if (this.Debug)
                {
                    this.LogDelegate($"{this.ServerName}: {message}");
                }
            }
        }
    }
}

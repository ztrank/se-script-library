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
        /// Communications Array for IGC Communication.
        /// </summary>
        public interface ICommunicationArray : ISubSystem
        {
            /// <summary>
            /// Adds a broadcast handler.
            /// </summary>
            /// <param name="channelTag">Channel tag.</param>
            /// <param name="handler">Handler reference.</param>
            /// <param name="setCallback">Sets a callback for the listener.</param>
            void AddBroadcastHandler(string channelTag, Action<MyIGCMessage> handler, bool setCallback = true);

            /// <summary>
            /// Adds a unicast handler.
            /// </summary>
            /// <param name="handler">Handler reference.</param>
            void AddUnicastHandler(Action<MyIGCMessage> handler);

            /// <summary>
            /// Processes the messages across ticks.
            /// </summary>
            /// <returns>IEnumerator for yielding across ticks.</returns>
            IEnumerator<bool> ProcessMessagesAsync();

            /// <summary>
            /// Processes messages synchronously.
            /// </summary>
            void ProcessMessages();

            /// <summary>
            /// Broadcasts the message.
            /// </summary>
            /// <typeparam name="T">Message Type.</typeparam>
            /// <param name="channelTag">Channel Tag.</param>
            /// <param name="message">Message to send.</param>
            /// <param name="distance">Transmission Distance.</param>
            void Broadcast<T>(string channelTag, T message, TransmissionDistance distance);

            /// <summary>
            /// Removes the broadcast handler.
            /// </summary>
            /// <param name="channelTag">Channel Tag.</param>
            /// <param name="handler">Handler reference.</param>
            void RemoveBroadcastHandler(string channelTag, Action<MyIGCMessage> handler);

            /// <summary>
            /// Removes the unicast handler.
            /// </summary>
            /// <param name="handler">Handler reference.</param>
            void RemoveUnicastHandler(Action<MyIGCMessage> handler);

            /// <summary>
            /// Removes all broadcast handlers.
            /// </summary>
            void RemoveBroadcastHandlers();

            /// <summary>
            /// Removes all unicast handlers.
            /// </summary>
            void RemoveUnicastHandlers();

            /// <summary>
            /// Removes all handlers.
            /// </summary>
            void RemoveAllHandlers();

            /// <summary>
            /// Checks if the communication array can connect to the address.
            /// </summary>
            /// <param name="addressee">Address to check.</param>
            /// <param name="distance">Transmission distance.</param>
            /// <returns>True if this array can reach the address.</returns>
            bool IsConnected(long addressee, TransmissionDistance distance = TransmissionDistance.AntennaRelay);

            /// <summary>
            /// Sends a unicast message.
            /// </summary>
            /// <typeparam name="T">Message type.</typeparam>
            /// <param name="target">Target address.</param>
            /// <param name="tag">Tag for the message.</param>
            /// <param name="message">Message to send.</param>
            /// <returns>True if successful.</returns>
            bool Send<T>(long target, string tag, T message);
        }

        /// <summary>
        /// Ships Comms class.
        /// </summary>
        public partial class CommunicationArray : SubSystem, ICommunicationArray
        {
            /// <summary>
            /// List of unicast handlers.
            /// </summary>
            private readonly List<Action<MyIGCMessage>> unicastHandlers = new List<Action<MyIGCMessage>>();

            /// <summary>
            /// Dictionary of channels to broadcast handlers.
            /// </summary>
            private readonly Dictionary<string, List<Action<MyIGCMessage>>> broadcastHandlers = new Dictionary<string, List<Action<MyIGCMessage>>>();

            /// <summary>
            /// Broadcast channels.
            /// </summary>
            private readonly List<IMyBroadcastListener> broadcastChannels = new List<IMyBroadcastListener>();

            /// <summary>
            /// Unicast listener.
            /// </summary>
            private IMyUnicastListener unicastListener;

            /// <summary>
            /// Adds a broadcast handler.
            /// </summary>
            /// <param name="channelTag">Channel tag.</param>
            /// <param name="handler">Handler to call.</param>
            /// <param name="setCallback">Should the message be called with the tag as an argument to the programmable block?</param>
            public void AddBroadcastHandler(string channelTag, Action<MyIGCMessage> handler, bool setCallback = true)
            {
                this.Log($"Comms: Adding Broadcast Channel [{channelTag}]");
                IMyBroadcastListener listener = this.IGC.RegisterBroadcastListener(channelTag);

                if (setCallback)
                {
                    this.Log($" With callback");
                    listener.SetMessageCallback(channelTag);
                }

                this.broadcastChannels.Add(listener);

                if (!this.broadcastHandlers.ContainsKey(channelTag))
                {
                    this.broadcastHandlers.Add(channelTag, new List<Action<MyIGCMessage>>());
                }

                this.broadcastHandlers[channelTag].Add(handler);
            }

            /// <summary>
            /// Adds a unicast listener with the optional callback.
            /// </summary>
            /// <param name="handler">Unicast handler.</param>
            public void AddUnicastHandler(Action<MyIGCMessage> handler)
            {
                this.Log($"Comms: Adding Unicast Handler");
                if (this.unicastListener == null)
                {
                    this.unicastListener = this.IGC.UnicastListener;
                }

                this.unicastHandlers.Add(handler);
            }

            /// <summary>
            /// Sends a broadcast message on the channel.
            /// </summary>
            /// <typeparam name="T">Type of message.</typeparam>
            /// <param name="channelTag">Channel Tag.</param>
            /// <param name="message">Message to send.</param>
            /// <param name="transmissionDistance">Transmission distance.</param>
            public void Broadcast<T>(string channelTag, T message, TransmissionDistance transmissionDistance)
            {
                this.IGC.SendBroadcastMessage(channelTag, message, transmissionDistance);
            }

            /// <summary>
            /// Checks if the grid address is connected.
            /// </summary>
            /// <param name="addressee">Address to check.</param>
            /// <param name="distance">Distance to check.</param>
            /// <returns>True if connected.</returns>
            public bool IsConnected(long addressee, TransmissionDistance distance = TransmissionDistance.AntennaRelay)
            {
                return this.IGC.IsEndpointReachable(addressee, distance);
            }

            /// <summary>
            /// Removes a broadcast handler.
            /// </summary>
            /// <param name="channelTag">Channel Tag.</param>
            /// <param name="handler">Handler reference.</param>
            public void RemoveBroadcastHandler(string channelTag, Action<MyIGCMessage> handler)
            {
                if (this.broadcastHandlers.ContainsKey(channelTag))
                {
                    this.broadcastHandlers[channelTag].Remove(handler);
                }
            }

            /// <summary>
            /// Removes the unicast handler.
            /// </summary>
            /// <param name="handler">Handler reference.</param>
            public void RemoveUnicastHandler(Action<MyIGCMessage> handler)
            {
                this.unicastHandlers.Remove(handler);
            }

            /// <summary>
            /// Removes all broadcast listeners.
            /// </summary>
            public void RemoveBroadcastHandlers()
            {
                foreach (IMyBroadcastListener channel in this.broadcastChannels)
                {
                    this.IGC.DisableBroadcastListener(channel);
                }
                this.broadcastChannels.Clear();
                this.broadcastHandlers.Clear();
            }

            /// <summary>
            /// Removes all unicast listeners.
            /// </summary>
            public void RemoveUnicastHandlers()
            {
                this.IGC.UnicastListener.DisableMessageCallback();
                this.unicastHandlers.Clear();
                this.unicastListener = null;
            }

            /// <summary>
            /// Disables and removes all listeners and handlers.
            /// </summary>
            public void RemoveAllHandlers()
            {
                this.RemoveBroadcastHandlers();
                this.RemoveUnicastHandlers();
            }

            /// <summary>
            /// Sends a message to a specific IGC target.
            /// </summary>
            /// <typeparam name="T">Message type.</typeparam>
            /// <param name="target">Target address.</param>
            /// <param name="tag">Message tag.</param>
            /// <param name="message">Message to send.</param>
            /// <returns>True if successful.</returns>
            public bool Send<T>(long target, string tag, T message)
            {
                return this.IGC.SendUnicastMessage(target, tag, message);
            }

            /// <summary>
            /// Processes the messages using an IEnumerator to spread each handler invocation across ticks.
            /// </summary>
            /// <returns>IEnumerator for async operations.</returns>
            public IEnumerator<bool> ProcessMessagesAsync()
            {
                foreach (IMyBroadcastListener channel in this.broadcastChannels)
                {
                    while (channel.HasPendingMessage)
                    {
                        MyIGCMessage message = channel.AcceptMessage();
                        this.Log($"Comms: Broadcast message recieved on [{message.Tag}]");
                        this.Log($" Source: {message.Source}X");

                        if (this.broadcastHandlers.ContainsKey(channel.Tag))
                        {
                            this.Log($" Executing {this.broadcastHandlers[channel.Tag].Count} handlers");
                            foreach (Action<MyIGCMessage> handler in this.broadcastHandlers[channel.Tag])
                            {
                                handler?.Invoke(message);
                                yield return true;
                            }
                        }
                        else
                        {
                            this.Log($" No Handlers registered.");
                        }
                    }
                }

                if (this.unicastListener != null)
                {
                    while (this.unicastListener.HasPendingMessage)
                    {
                        MyIGCMessage message = this.unicastListener.AcceptMessage();

                        this.Log($"Comms: Unicast message recieved.");
                        this.Log($" Source: {message.Source}X");
                        this.Log($" Executing {this.unicastHandlers.Count} handlers");

                        foreach (Action<MyIGCMessage> handler in this.unicastHandlers)
                        {
                            handler?.Invoke(message);
                            yield return true;
                        }
                    }
                }
            }

            /// <summary>
            /// Processes the messages in a single tick.
            /// </summary>
            public void ProcessMessages()
            {
                foreach (IMyBroadcastListener channel in this.broadcastChannels)
                {
                    while (channel.HasPendingMessage)
                    {
                        MyIGCMessage message = channel.AcceptMessage();
                        this.Log($"Comms: Broadcast message recieved on [{message.Tag}]");
                        this.Log($" Source: {message.Source}X");

                        if (this.broadcastHandlers.ContainsKey(channel.Tag))
                        {
                            this.Log($" Executing {this.broadcastHandlers[channel.Tag].Count} handlers");
                            foreach (Action<MyIGCMessage> handler in this.broadcastHandlers[channel.Tag])
                            {
                                handler?.Invoke(message);
                            }
                        }
                        else
                        {
                            this.Log($" No Handlers registered.");
                        }
                    }
                }

                if (this.unicastListener != null)
                {
                    while (this.unicastListener.HasPendingMessage)
                    {
                        MyIGCMessage message = this.unicastListener.AcceptMessage();

                        this.Log($"Comms: Unicast message recieved.");
                        this.Log($" Source: {message.Source}X");
                        this.Log($" Executing {this.unicastHandlers.Count} handlers");

                        foreach (Action<MyIGCMessage> handler in this.unicastHandlers)
                        {
                            handler?.Invoke(message);
                        }
                    }
                }
            }
        }
    }
    
}

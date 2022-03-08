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
        /// Connection class for programs that connect to a server.
        /// </summary>
        public class Connection
        {
            public enum ConnectionStatus
            {
                Disconnected,
                Connected,
                SendingHealthCheck,
                WaitingHealthCheck
            }

            /// <summary>
            /// Intergrid communication system.
            /// </summary>
            private readonly IMyIntergridCommunicationSystem IGC;

            /// <summary>
            /// DNS channel name.
            /// </summary>
            private readonly string DNS;

            /// <summary>
            /// List of handlers.
            /// </summary>
            private readonly List<Action<MyIGCMessage>> Handlers = new List<Action<MyIGCMessage>>();

            /// <summary>
            /// Channel listener.
            /// </summary>
            private IMyBroadcastListener broadcastListener;

            /// <summary>
            /// DNS listener.
            /// </summary>
            private IMyBroadcastListener dnsListner;

            /// <summary>
            /// Log delegate.
            /// </summary>
            private readonly Action<string> LogDelegate;

            /// <summary>
            /// Error delegate.
            /// </summary>
            private readonly Action<string> Error;

            private ConnectionStatus status = ConnectionStatus.Disconnected;

            /// <summary>
            /// Creates a new connection, disconnected at start.
            /// </summary>
            /// <param name="igc">Intergrid communication system.</param>
            /// <param name="dns">DNS server name.</param>
            /// <param name="tag">Tag for this connection's channel.</param>
            /// <param name="log">Log delegate.</param>
            /// <param name="error">Error delegate.</param>
            public Connection(IMyIntergridCommunicationSystem igc, string dns, string tag, Action<string> log, Action<string> error)
            {
                this.LogDelegate = log;
                this.Error = error;
                this.IGC = igc;
                this.DNS = dns;
                this.Tag = tag;
            }

            /// <summary>
            /// Gets the tag for this connection.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Gets or sets a value indicating whether debug messages should be logged.
            /// </summary>
            public bool Debug { get; set; }

            public bool IsConnected
            {
                get
                {
                    return this.broadcastListener != null && this.status != ConnectionStatus.Disconnected;
                }
            }

            /// <summary>
            /// Sends the connection request to the DNS channel.
            /// </summary>
            public void Connect()
            {
                this.Log("Sending DNS Connection Request.");
                this.dnsListner = this.IGC.RegisterBroadcastListener(this.DNS);
                this.dnsListner.SetMessageCallback("");
                this.IGC.SendBroadcastMessage(this.DNS, new MyTuple<string, string>("connection:request", this.Tag));
            }

            /// <summary>
            /// Sends the disconnect message to the DNS channel.
            /// </summary>
            public void Disconnect()
            {
                this.Log("Sending DNS Disconnect Message.");
                this.IGC.SendBroadcastMessage(this.Tag, new MyTuple<string, string>("connection:disconnect", this.Tag));
                this.IGC.DisableBroadcastListener(this.broadcastListener);
                this.broadcastListener = null;
                this.status = ConnectionStatus.Disconnected;
            }

            /// <summary>
            /// Registers the handler.
            /// </summary>
            /// <param name="handler">Handler delegate.</param>
            public void RegisterHandler(Action<MyIGCMessage> handler)
            {
                this.Log($"Adding Handler: {handler.Method.Name}");
                this.Handlers.Add(handler);
            }

            /// <summary>
            /// Removes the handler.
            /// </summary>
            /// <param name="handler">Handler delegate.</param>
            public void RemoveHandler(Action<MyIGCMessage> handler)
            {
                this.Log($"Removing Handler: {handler.Method.Name}");
                this.Handlers.Remove(handler);
            }

            public void Send<T>(string channel, T message)
            {
                this.Log($"Sending Message: {message}");
                try
                {
                    this.IGC.SendBroadcastMessage(channel, message);
                }
                catch (Exception ex)
                {
                    this.Error(ex.Message);
                }
            }

            /// <summary>
            /// Sends the message on the open channel.
            /// </summary>
            /// <typeparam name="T">Type of message.</typeparam>
            /// <param name="message">Message contents.</param>
            public bool Send<T>(T message)
            {
                if (this.broadcastListener != null)
                {
                    this.Send(this.Tag, message);
                    return true;
                }
                else
                {
                    this.Error("Unable to send message: Disconnected");
                    return false;
                }
            }

            /// <summary>
            /// Processes the connection health.
            /// </summary>
            public void ProcessHealth()
            {
                if (this.status == ConnectionStatus.Disconnected)
                {
                    return;
                }

                if (this.status == ConnectionStatus.SendingHealthCheck)
                {
                    this.Log("Awaiting healthcheck answer...");
                    this.status = ConnectionStatus.WaitingHealthCheck;
                    return;
                }

                if (this.status == ConnectionStatus.WaitingHealthCheck)
                {
                    this.Log("Connection timeout.");
                    this.Disconnect();
                    return;
                }

                this.Log("Ping");
                this.IGC.SendBroadcastMessage(this.DNS, new MyTuple<string, string>("healthcheck", this.Tag));
            }

            /// <summary>
            /// Processes the messages. Intended to be run in the main method to check on IGC updates for messages.
            /// </summary>
            public void ProcessMessages()
            {
                // If the DNS listener is active, check for connection messages.
                while (this.dnsListner != null && this.dnsListner.HasPendingMessage)
                {
                    MyIGCMessage message = this.dnsListner.AcceptMessage();
                    if (message.Data is MyTuple<string, string>)
                    {
                        MyTuple<string, string> data = (MyTuple<string, string>)message.Data;
                        if (data.Item1 == "connection:accepted")
                        {
                            this.Log("Connection Accepted!");
                            this.broadcastListener = this.IGC.RegisterBroadcastListener(this.Tag);
                            this.broadcastListener.SetMessageCallback("");
                            this.status = ConnectionStatus.Connected;
                            this.IGC.DisableBroadcastListener(this.dnsListner);
                            this.dnsListner = null;
                        }
                        else if (data.Item1 == "connection:refused")
                        {
                            this.Log("Connection Refused!");
                            this.IGC.DisableBroadcastListener(this.dnsListner);
                            this.dnsListner = null;
                            //throw new Exception($"DNS Error: Server refused connection: [{this.DNS}/{this.Tag}]");
                        }
                        else
                        {
                            this.Error("Unrecognized DNS Response: " + data.Item1);
                        }
                    }
                    else
                    {
                        this.Error("Malformed DNS Response");
                    }
                }

                // If the broadcast listener is active, check for messages.
                while (this.broadcastListener != null && this.broadcastListener.HasPendingMessage)
                {
                    MyIGCMessage message = this.broadcastListener.AcceptMessage();

                    if (message.Data is string && message.As<string>() == "healthcheck")
                    {
                        this.Log("Health Check received.");
                        this.IGC.SendBroadcastMessage(this.DNS, MyTuple.Create("healthcheck", this.Tag, "OK"));
                        continue;
                    }
                    else if (message.Data is MyTuple<string, string> && message.As<MyTuple<string, string>>().Item1 == "healthcheck")
                    {
                        this.status = ConnectionStatus.Connected;
                        continue;
                    }

                    this.Log("Processing Message");
                    foreach (Action<MyIGCMessage> handler in this.Handlers)
                    {
                        try
                        {
                            handler?.Invoke(message);
                        }
                        catch (Exception e)
                        {
                            this.Error("Error Processing Message: " + e.Message);
                        }
                    }
                }
            }

            /// <summary>
            /// Logs the message if debug is set to true.
            /// </summary>
            /// <param name="message">Message to log.</param>
            private void Log(string message)
            {
                if (this.Debug)
                {
                    this.LogDelegate($"Connection[{this.Tag}]: {message}");
                }
            }
        }
    }
}

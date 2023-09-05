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
        public class AirlockCommandMessage
        {
            /// <summary>
            /// Default Command Tag
            /// </summary>
            public const string CommandTag = "airlock-command";

            /// <summary>
            /// Gets or sets the airlock name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the command
            /// </summary>
            public string Command { get; set; }

            /// <summary>
            /// Gets or sets the arguments
            /// </summary>
            public string Arguments { get; set; }

            /// <summary>
            /// Extracts an AirlockCommandMessage from the IGC Message
            /// </summary>
            /// <param name="message">Incoming IGC Message</param>
            /// <returns>Airlock Command Message</returns>
            public static AirlockCommandMessage FromMessage(MyIGCMessage message)
            {
                MyTuple<string, string, string> data = message.As<MyTuple<string, string, string>>();
                return new AirlockCommandMessage()
                {
                    Name = data.Item1,
                    Command = data.Item2,
                    Arguments = data.Item3
                };
            }

            /// <summary>
            /// Transforms to sendable data for an IGC Message
            /// </summary>
            /// <returns>Data for IGC Message</returns>
            public MyTuple<string, string, string> ToData()
            {
                return new MyTuple<string, string, string>(
                    this.Name,
                    this.Command,
                    this.Arguments
                );
            }
        }
    }
}

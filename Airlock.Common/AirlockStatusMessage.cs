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
        public class AirlockStatusMessage
        {

            public const string MessageTag = "airlock-status";

            /// <summary>
            /// Gets or sets the Airlock Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the airlock State
            /// </summary>
            public AirlockState Status { get; set; }

            /// <summary>
            /// Gets or sets the oxygen fill ratio
            /// </summary>
            public double OxygenRatio { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether any interior doors are open
            /// </summary>
            public bool InteriorDoorsOpen { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether all interior doors are locked
            /// </summary>
            public bool InteriorDoorsLocked { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether any exterior doors are open
            /// </summary>
            public bool ExteriorDoorsOpen { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether all exterior doors are locked
            /// </summary>
            public bool ExteriorDoorsLocked { get; set; }

            /// <summary>
            /// Transforms a MyTuple into a AirlockStatusMessage for use when receiving a status message
            /// </summary>
            /// <param name="data">Message Data</param>
            /// <returns>An Airlock Status Message</returns>
            public static AirlockStatusMessage FromMessage(MyIGCMessage message)
            {
                MyTuple<string, int, double, MyTuple<bool, bool>, MyTuple<bool, bool>> data = message.As<MyTuple<string, int, double, MyTuple<bool, bool>, MyTuple<bool, bool>>>();
                return new AirlockStatusMessage()
                {
                    Name = data.Item1,
                    Status = (AirlockState)data.Item2,
                    OxygenRatio = data.Item3,
                    InteriorDoorsOpen = data.Item4.Item1,
                    InteriorDoorsLocked = data.Item4.Item2,
                    ExteriorDoorsOpen = data.Item5.Item1,
                    ExteriorDoorsLocked = data.Item5.Item2
                };
            }

            /// <summary>
            /// Transforms this AirlockStatusMessage into a MyTuple for sending over IGC
            /// </summary>
            /// <returns>MyTuple of immutable types for sending over IGC</returns>
            public MyTuple<string, int, double, MyTuple<bool, bool>, MyTuple<bool, bool>> ToData()
            {
                return new MyTuple<string, int, double, MyTuple<bool, bool>, MyTuple<bool, bool>>(
                    this.Name,
                    (int)this.Status,
                    this.OxygenRatio,
                    new MyTuple<bool, bool>(
                        this.InteriorDoorsOpen,
                        this.InteriorDoorsLocked
                    ),
                    new MyTuple<bool, bool>(
                        this.ExteriorDoorsOpen,
                        this.ExteriorDoorsLocked
                    )
                );
            }

        }
    }
}

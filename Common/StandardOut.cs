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

    /// <summary>
    /// Airlock control program.
    /// </summary>
    partial class Program
    {
        /// <summary>
        /// Gets the running program name.
        /// </summary>
        public string ProgramName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the standard out transmission distance.
        /// </summary>
        public TransmissionDistance StdoutTransmissionDistance { get; private set; } = TransmissionDistance.CurrentConstruct;

        /// <summary>
        /// Standard Out.
        /// </summary>
        /// <param name="message">Message to send.</param>
        public void Stdout(string message)
        {
            message = $"[{this.ProgramName}]: {message}";
            this.Echo(message);
            this.IGC.SendBroadcastMessage("stdout", message, this.StdoutTransmissionDistance);
        }

        /// <summary>
        /// Standard Error.
        /// </summary>
        /// <param name="message">Boardcasts to the same tag as stdout but prepends "ERROR >> "to the message.</param>
        public void Stderr(string message)
        {
            message = $"ERROR >> [{this.ProgramName}]: {message}";
            this.Echo(message);
            this.IGC.SendBroadcastMessage("stdout", message, this.StdoutTransmissionDistance);
        }
    }
}

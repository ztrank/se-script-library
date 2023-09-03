namespace IngameScript
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using VRage.Game.ModAPI.Ingame.Utilities;

    partial class Program : MyGridProgram
    {
        /// <summary>
        /// Auto Doors list
        /// </summary>
        private readonly List<Autodoor> m_Autodoors = new List<Autodoor>();

        /// <summary>
        /// Doors List
        /// </summary>
        private readonly List<IMyDoor> m_IMyDoors = new List<IMyDoor>();

        /// <summary>
        /// Command Line
        /// </summary>
        private readonly MyCommandLine m_CommandLine = new MyCommandLine();

        /// <summary>
        /// Custom Data
        /// </summary>
        private readonly MyIni m_Ini = new MyIni();

        /// <summary>
        /// Action dictionary
        /// </summary>
        private readonly Dictionary<string, Action> m_Actions = new Dictionary<string, Action>();

        /// <summary>
        /// Auto Close Delay
        /// </summary>
        private readonly double m_Delay;

        /// <summary>
        /// Whether to output to log.
        /// </summary>
        private readonly bool m_Debug;

        /// <summary>
        /// Creates a new instance of the <see cref="Program"/> class.
        /// </summary>
        public Program()
        {
            if (this.m_Ini.TryParse(this.Me.CustomData))
            {
                this.m_Delay = this.m_Ini.Get("global", "delay").ToDouble(5);
                this.m_Debug = this.m_Ini.Get("settings", "debug").ToBoolean(false);
            }
            else
            {
                this.m_Delay = 5;
                this.m_Debug = false;
            }

            this.m_Actions.Add("init", this.Initialize);
            this.Initialize();
            this.Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        /// <summary>
        /// Causes all doors to check their state and close if necessary. Also runs commands.
        /// </summary>
        /// <remarks>
        /// Every ten ticks checks the doors. Also runs the following commands/actions:
        /// - init : Initialize() 
        /// </remarks>
        /// <param name="argument">Command line argument</param>
        /// <param name="updateSource">Update type</param>
        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update10) > 0)
            {
                foreach(Autodoor door in this.m_Autodoors)
                {
                    door.OnUpdate();
                }
            }
            else if (this.m_CommandLine.TryParse(argument))
            {
                Action action;
                if (this.m_Actions.TryGetValue(this.m_CommandLine.Argument(0), out  action))
                {
                    action?.Invoke();
                }
            }
        }

        private void Log(string value)
        {
            if (this.m_Debug)
            {
                this.Echo(value);
            }
        }

        /// <summary>
        /// Initializes the autodoors list
        /// </summary>
        private void Initialize()
        {
            this.m_Autodoors.Clear();
            this.m_IMyDoors.Clear();

            this.GridTerminalSystem.GetBlocksOfType(this.m_IMyDoors, block => block.CustomName.ToLower().Contains("auto") && block.IsSameConstructAs(this.Me));
            foreach (IMyDoor door in this.m_IMyDoors)
            {
                this.m_Autodoors.Add(new Autodoor(door, this.m_Delay, this.Log));
            }
            this.Log($"Autodoors Initialized: {this.m_Autodoors.Count}");
        }

        /// <summary>
        /// State machine for auto door
        /// </summary>
        public class Autodoor
        {
            /// <summary>
            /// Door reference
            /// </summary>
            private readonly IMyDoor m_Door;

            /// <summary>
            /// Auto close delay
            /// </summary>
            private readonly double m_Delay;

            /// <summary>
            /// Custom Door Settings
            /// </summary>
            private readonly MyIni m_Ini = new MyIni();

            /// <summary>
            /// Echo Function.
            /// </summary>
            private readonly Action<string> m_Log;

            /// <summary>
            /// Time the door should close
            /// </summary>
            private DateTime? m_CloseIn;

            /// <summary>
            /// Creates a new instance of the <see cref="Autodoor"/> class.
            /// </summary>
            /// <param name="door">Door Reference</param>
            /// <param name="delay">Delay timer</param>
            /// <param name="log">Log Action</param>
            public Autodoor(IMyDoor door, double delay, Action<string> log)
            {
                this.m_Door = door;
                this.m_Log = log;

                if (this.m_Ini.TryParse(door.CustomData))
                {
                    this.m_Delay = this.m_Ini.Get("autodoor", "delay").ToDouble(delay);
                }
                else
                {
                    this.m_Delay = delay;
                }

                this.m_Log($"{this.m_Door.CustomName} Delay : {this.m_Delay}");
            }

            /// <summary>
            /// Gets the door status
            /// </summary>
            public DoorStatus DoorStatus
            {
                get
                {
                    return this.m_Door.Status;
                }
            }

            /// <summary>
            /// Gets the door name
            /// </summary>
            public string Name
            {
                get
                {
                    return this.m_Door.Name;
                }
            }

            /// <summary>
            /// On update checks for door status and if the door should be closed.
            /// </summary>
            public void OnUpdate()
            {
                DoorStatus status = this.DoorStatus;
                if (this.m_CloseIn == null && status == DoorStatus.Open)
                {
                    this.m_CloseIn = DateTime.Now.AddSeconds(this.m_Delay);
                }

                if (this.m_CloseIn != null && DateTime.Now > this.m_CloseIn && status == DoorStatus.Open)
                {
                    this.m_Door.CloseDoor();
                    this.m_CloseIn = null;
                }

                if (status == DoorStatus.Closed || status == DoorStatus.Closing)
                {
                    this.m_CloseIn = null;
                }
            }
        }
    }
}

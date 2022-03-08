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
    /// Airlock Controller Program.
    /// </summary>
    partial class Program
    {
        /// <summary>
        /// Airlock Cycler.
        /// </summary>
        public class AirlockCycler : IDisposable
        {
            /// <summary>
            /// Cycle status.
            /// </summary>
            public enum CyclerStatus
            {
                Init,
                DoorsClosing,
                DoorsClosed,
                DoorsLocking,
                DoorsLocked,
                Pressurizing,
                Pressurized,
                Depressurizing,
                Depressurized,
                DoorsOpening,
                DoorsUnlocking,
                DoorsUnlocked,
                DoorsOpen,
                Complete
            }

            /// <summary>
            /// Direction of cycling.
            /// </summary>
            public enum CycleDirection
            {
                Pressurize,
                Depressurize
            }

            /// <summary>
            /// Reference to parent airlock.
            /// </summary>
            private Airlock airlock;

            /// <summary>
            /// Direction the airlock is cycling.
            /// </summary>
            private CycleDirection Direction;

            /// <summary>
            /// Airlock status.
            /// </summary>
            private CyclerStatus status;
            
            /// <summary>
            /// Creates a new instance of the airlock cycler.
            /// </summary>
            /// <param name="airlock">Reference to the parent airlock.</param>
            /// <param name="direction">Direction the airlock is cycling.</param>
            public AirlockCycler(
                Airlock airlock,
                CycleDirection direction)
            {
                this.airlock = airlock;
                this.Direction = direction;
                this.status = CyclerStatus.Init;
            }

            public CyclerStatus Check()
            {
                // Pressurize: 1 Close Doors, 2 Lock Doors, 3 Pressurize, 4 Check for pressure, 5 Unlock interior doors 6 Complete
                // Depressurize: 1 Close doors, 2 Lock Doors, 3 Depressurize, 4 check for depressurization, 5 unlock exterior doors, 6 complete
                if (this.Direction == CycleDirection.Pressurize)
                {
                    this.RunPressurize();
                }
                else
                {
                    this.RunDepressurize();
                }

                return this.status;
            }

            /// <summary>
            /// Reverses the airlock direction and sets the status back to Init.
            /// </summary>
            public void Reverse()
            {
                this.Direction = this.Direction == CycleDirection.Depressurize ? CycleDirection.Pressurize : CycleDirection.Depressurize;
                this.status = CyclerStatus.Init;
            }

            /// <summary>
            /// Disposes of the airlock cycler.
            /// </summary>
            public void Dispose()
            {
                this.airlock = null;
            }

            /// <summary>
            /// Runs through the steps to depressurize the airlock.
            /// </summary>
            private void RunDepressurize()
            {
                switch(this.status)
                {
                    case CyclerStatus.Init:
                        this.status = CyclerStatus.DoorsClosing;
                        this.airlock.CloseDoors();
                        break;
                    case CyclerStatus.DoorsClosing:
                        if (this.airlock.Closed)
                        {
                            this.status = CyclerStatus.DoorsClosed;
                        }
                        break;
                    case CyclerStatus.DoorsClosed:
                        this.status = CyclerStatus.DoorsLocking;
                        this.airlock.LockDoors();
                        break;
                    case CyclerStatus.DoorsLocking:
                        if (this.airlock.Locked)
                        {
                            this.status = CyclerStatus.DoorsLocked;
                        }
                        break;
                    case CyclerStatus.DoorsLocked:
                        this.status = CyclerStatus.Depressurizing;
                        this.airlock.Depressurize();
                        break;
                    case CyclerStatus.Depressurizing:
                        if(this.airlock.IsDepressurized)
                        {
                            this.status = CyclerStatus.Depressurized;
                        }
                        break;
                    case CyclerStatus.Depressurized:
                        this.status = CyclerStatus.DoorsUnlocking;
                        this.airlock.UnlockExteriorDoors();
                        break;
                    case CyclerStatus.DoorsUnlocking:
                        if (this.airlock.ExteriorDoorsUnlocked)
                        {
                            this.status = CyclerStatus.DoorsUnlocked;
                        }
                        break;
                    case CyclerStatus.DoorsUnlocked:
                        this.status = CyclerStatus.DoorsOpening;
                        this.airlock.OpenExteriorDoors();
                        break;
                    case CyclerStatus.DoorsOpening:
                        if (this.airlock.ExteriorDoorsOpen)
                        {
                            this.status = CyclerStatus.DoorsOpen;
                        }
                        break;
                    case CyclerStatus.DoorsOpen:
                        this.status = CyclerStatus.Complete;
                        break;
                }
            }

            /// <summary>
            /// Runs through the steps to pressurize the airlock.
            /// </summary>
            private void RunPressurize()
            {
                switch(this.status)
                {
                    case CyclerStatus.Init:
                        this.status = CyclerStatus.DoorsClosing;
                        this.airlock.CloseDoors();
                        break;
                    case CyclerStatus.DoorsClosing:
                        if (this.airlock.Closed)
                        {
                            this.status = CyclerStatus.DoorsClosed;
                        }
                        break;
                    case CyclerStatus.DoorsClosed:
                        this.status = CyclerStatus.DoorsLocking;
                        this.airlock.LockDoors();
                        break;
                    case CyclerStatus.DoorsLocking:
                        if (this.airlock.Locked)
                        {
                            this.status = CyclerStatus.DoorsLocked;
                        }
                        break;
                    case CyclerStatus.DoorsLocked:
                        this.status = CyclerStatus.Pressurizing;
                        this.airlock.Pressurize();
                        break;
                    case CyclerStatus.Pressurizing:
                        if(this.airlock.IsPressurized)
                        {
                            this.status = CyclerStatus.Pressurized;
                        }
                        break;
                    case CyclerStatus.Pressurized:
                        this.status = CyclerStatus.DoorsUnlocking;
                        this.airlock.UnlockInteriorDoors();
                        break;
                    case CyclerStatus.DoorsUnlocking:
                        if(this.airlock.InteriorDoorsUnlocked)
                        {
                            this.status = CyclerStatus.DoorsUnlocked;
                        }
                        break;
                    case CyclerStatus.DoorsUnlocked:
                        this.status = CyclerStatus.DoorsOpening;
                        this.airlock.OpenInteriorDoors();
                        break;
                    case CyclerStatus.DoorsOpening:
                        if (this.airlock.InteriorDoorsOpen)
                        {
                            this.status = CyclerStatus.DoorsOpen;
                        }
                        break;
                    case CyclerStatus.DoorsOpen:
                        this.status = CyclerStatus.Complete;
                        break;

                }
            }
        }
    }
}

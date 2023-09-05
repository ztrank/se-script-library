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
    /// Airlock Controller Class.
    /// </summary>
    partial class Program
    {
        /// <summary>
        /// Airlock class.
        /// </summary>
        public class Airlock
        {
            /// <summary>
            /// Invalid Airlock Exception.
            /// </summary>
            public class InvalidAirlockException : Exception
            {
                /// <summary>
                /// Creates a new Invalid airlock exception.
                /// </summary>
                public InvalidAirlockException() : base()
                {
                }

                /// <summary>
                /// Creates a new Invalid airlock exception.
                /// </summary>
                /// <param name="message">Error Message.</param>
                public InvalidAirlockException(string message) : base(message)
                {
                }
            }

            /// <summary>
            /// List of all doors.
            /// </summary>
            private readonly List<IMyDoor> AllDoors = new List<IMyDoor>();

            /// <summary>
            /// List of interior doors.
            /// </summary>
            private readonly List<IMyDoor> InteriorDoors = new List<IMyDoor>();

            /// <summary>
            /// List of exterior doors.
            /// </summary>
            private readonly List<IMyDoor> ExteriorDoors = new List<IMyDoor>();
            
            /// <summary>
            /// List of Air Vents.
            /// </summary>
            private readonly List<IMyAirVent> AirVents = new List<IMyAirVent>();

            /// <summary>
            /// List of oxygen tanks.
            /// </summary>
            private readonly List<IMyGasTank> OxygenTanks = new List<IMyGasTank>();

            /// <summary>
            /// List of Interior door lock indicator lights.
            /// </summary>
            private readonly List<IMyLightingBlock> InteriorDoorLockIndicators = new List<IMyLightingBlock>();

            /// <summary>
            /// List of Exterior door lock indicator lights.
            /// </summary>
            private readonly List<IMyLightingBlock> ExteriorDoorLockIndicators = new List<IMyLightingBlock>();

            /// <summary>
            /// List of cycle indicator lights.
            /// </summary>
            private readonly List<IMyLightingBlock> CycleIndicators = new List<IMyLightingBlock>();

            /// <summary>
            /// List of all lights.
            /// </summary>
            private readonly List<IMyLightingBlock> AllLights = new List<IMyLightingBlock>();

            /// <summary>
            /// List of errors.
            /// </summary>
            private readonly List<string> Errors = new List<string>();

            /// <summary>
            /// Echo action.
            /// </summary>
            private readonly Action<string> Echo;

            /// <summary>
            /// Airlock Cycler.
            /// </summary>
            private AirlockCycler cycler;

            /// <summary>
            /// Creates a new airlock.
            /// </summary>
            /// <param name="blockGroup">Block Group.</param>
            /// <param name="controller">Programmable Block Controller.</param>
            public Airlock(IMyBlockGroup blockGroup, IMyProgrammableBlock controller, Action<string> echo)
            {
                this.Name = blockGroup.Name;
                this.Echo = (string message) => echo($"{this.Name}: {message}");

                // Get all the blocks needed from the block group.
                blockGroup.GetBlocksOfType(this.AllDoors, b => b.IsSameConstructAs(controller));
                blockGroup.GetBlocksOfType(this.AirVents, b => b.IsSameConstructAs(controller));
                blockGroup.GetBlocksOfType(this.OxygenTanks, b => b.IsSameConstructAs(controller));
                blockGroup.GetBlocksOfType(this.AllLights, b => b.IsSameConstructAs(controller));

                // Sort the doors to external and internal
                foreach(IMyDoor door in this.AllDoors)
                {
                    if (door.CustomName.ToLower().Contains("exterior"))
                    {
                        this.ExteriorDoors.Add(door);
                    }
                    else
                    {
                        this.InteriorDoors.Add(door);
                    }
                }

                // Sort lights to interior, exterior, and cycle
                foreach(IMyLightingBlock lighting in this.AllLights)
                {
                    string lightName = lighting.CustomName.ToLower();
                    if (lightName.Contains("interior"))
                    {
                        this.InteriorDoorLockIndicators.Add(lighting);
                    }
                    else if (lightName.Contains("exterior"))
                    {
                        this.ExteriorDoorLockIndicators.Add(lighting);
                    }
                    else if (lightName.Contains("cycle"))
                    {
                        this.CycleIndicators.Add(lighting);
                    }
                }

                // Check the airlock for errors. We need at least one interior door, exterior door, airvent and oxygen tank.
                if (!this.InteriorDoors.Any())
                {
                    this.Errors.Add("No Interior Doors");
                }

                if (!this.ExteriorDoors.Any())
                {
                    this.Errors.Add("No Exterior Doors");
                }

                if (!this.AirVents.Any())
                {
                    this.Errors.Add("No Air Vents");
                }

                if (!this.OxygenTanks.Any())
                {
                    this.Errors.Add("No Oxygen Tanks");
                }

                // If there are any errors, we can't proceed so throw the errors.
                if (this.Errors.Any())
                {
                    throw new InvalidAirlockException($"Invalid Airlock: {this.Name}\n" + string.Join("\n>  ", this.Errors));
                }

                if (this.IsPressurized)
                {
                    this.State = AirlockState.Pressurized;
                }
                else if (this.CanPressurize)
                {
                    this.State = AirlockState.Pressurizing;
                }
                else if (this.IsDepressurized)
                {
                    this.State = AirlockState.Depressurized;
                }
                else
                {
                    this.State = AirlockState.Depressurizing;
                }
            }

            /// <summary>
            /// Gets the block group name.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the airlock's state.
            /// </summary>
            public AirlockState State { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the airlock is depressurized or not.
            /// </summary>
            public bool IsDepressurized
            {
                get
                {
                    bool isDepressurized = true;
                    foreach (IMyAirVent vent in this.AirVents)
                    {

                        if (vent.Status != VentStatus.Depressurized)
                        {
                            // Vents sometimes get stuck in Depressurizing even after they have depressurized the room.
                            // Oxygen level is a float, so we just check for under 1% oxygen
                            if (vent.Status == VentStatus.Depressurizing && vent.GetOxygenLevel() > 0.01)
                            {
                                isDepressurized = false;
                                break;
                            }
                            else if (vent.Status != VentStatus.Depressurizing)
                            {
                                isDepressurized = false;
                                break;
                            }
                        }
                    }

                    return isDepressurized;
                }
            }


            /// <summary>
            /// Gets a value indicating whether the room is pressurized.
            /// </summary>
            public bool IsPressurized
            {
                get
                {
                    bool isPressurized = true;
                    foreach (IMyAirVent vent in this.AirVents)
                    {
                        if (vent.Status != VentStatus.Pressurized)
                        {
                            isPressurized = false;
                            break;
                        }
                    }

                    return isPressurized;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the room is airtight.
            /// </summary>
            public bool CanPressurize
            {
                get
                {
                    bool canPressurize = true;
                    foreach (IMyAirVent vent in this.AirVents)
                    {
                        if (!vent.CanPressurize)
                        {
                            canPressurize = false;
                            break;
                        }
                    }

                    return canPressurize;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the doors are closed or not.
            /// </summary>
            public bool Closed
            {
                get
                {
                    bool closed = true;
                    this.Perform(this.AllDoors, door =>
                    {
                        if (door.Status != DoorStatus.Closed)
                        {
                            closed = false;
                        }
                    });

                    return closed;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the doors are open or not.
            /// </summary>
            public bool Open
            {
                get
                {
                    bool open = true;
                    this.Perform(this.AllDoors, door =>
                    {
                        if (door.Status != DoorStatus.Open)
                        {
                            open = false;
                        }
                    });

                    return open;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the interior doors are open.
            /// </summary>
            public bool InteriorDoorsOpen
            {
                get
                {
                    bool open = true;
                    this.Perform(this.InteriorDoors, door =>
                    {
                        if (door.Status != DoorStatus.Open)
                        {
                            open = false;
                        }
                    });

                    return open;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the exterior doors are open.
            /// </summary>
            public bool ExteriorDoorsOpen
            {
                get
                {
                    bool open = true;
                    this.Perform(this.ExteriorDoors, door =>
                    {
                        if (door.Status != DoorStatus.Open)
                        {
                            open = false;
                        }
                    });

                    return open;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the doors are locked or not.
            /// </summary>
            public bool Locked
            {
                get
                {
                    bool locked = true;
                    this.Perform(this.AllDoors, door =>
                    {
                        if (door.Enabled)
                        {
                            locked = false;
                        }
                    });

                    return locked;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the interior doors are unlocked.
            /// </summary>
            public bool InteriorDoorsUnlocked
            {
                get
                {
                    bool unlocked = true;

                    this.Perform(this.InteriorDoors, door =>
                    {
                        if (!door.Enabled)
                        {
                            unlocked = false;
                        }
                    });

                    return unlocked;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the exterior doors are unlocked.
            /// </summary>
            public bool ExteriorDoorsUnlocked
            {
                get
                {
                    bool open = true;

                    this.Perform(this.ExteriorDoors, door =>
                    {
                        if (!door.Enabled)
                        {
                            open = false;
                        }
                    });

                    return open;
                }
            }

            /// <summary>
            /// Gets the oxygen level of the airlock.
            /// </summary>
            public GasLevel Oxygen => GasLevel.GetGasLevel(this.OxygenTanks);

            /// <summary>
            /// Checks the airlock for updates to the cycler.
            /// </summary>
            public void Check()
            {
                if (this.cycler != null)
                {
                    AirlockCycler.CyclerStatus status = this.cycler.Check();

                    if (status == AirlockCycler.CyclerStatus.Complete)
                    {
                        this.DisableCycleLights();
                        this.Echo("Cycle complete.");
                        this.cycler.Dispose();
                        this.cycler = null;
                    }
                }
            }

            /// <summary>
            /// Starts cycling the airlock.
            /// </summary>
            public void Cycle()
            {
                if (this.cycler == null)
                {
                    this.Echo("Beginning cycling: " + (this.CanPressurize ? "Depressurization." : "Pressurization."));
                    this.EnableCycleLights();
                    this.cycler = new AirlockCycler(this, this.IsPressurized ? AirlockCycler.CycleDirection.Depressurize : AirlockCycler.CycleDirection.Pressurize);
                }
                else
                {
                    this.Echo("Cycle in progress.");
                }
            }

            /// <summary>
            /// Cycles the airlock towards the interior.
            /// </summary>
            public void OpenToInterior()
            {
                if (this.cycler == null)
                {
                    this.Echo("Beginning cycling: Pressurization.");
                    this.EnableCycleLights();
                    this.cycler = new AirlockCycler(this, AirlockCycler.CycleDirection.Pressurize);
                }
                else
                {
                    this.Echo("Cycle in progress.");
                }
            }

            /// <summary>
            /// Cycles the airlock towards the exterior.
            /// </summary>
            public void OpenToExterior()
            {
                if (this.cycler == null)
                {
                    this.Echo("Beginning cycling: Depressurization.");
                    this.EnableCycleLights();
                    this.cycler = new AirlockCycler(this, AirlockCycler.CycleDirection.Depressurize);
                }
                else
                {
                    this.Echo("Cycle in progress.");
                }
            }

            /// <summary>
            /// Cancels the cycle.
            /// </summary>
            public void Cancel()
            {
                if (this.cycler != null)
                {
                    this.Echo("Canceling cyling!");
                    this.cycler.Reverse();
                }
                else
                {
                    this.Echo("Cycle not in progress.");
                }
            }

            /// <summary>
            /// Closes doors.
            /// </summary>
            public void CloseDoors()
            {
                this.Echo("Closing doors...");
                this.Perform(this.AllDoors, door => 
                {
                    if (!door.Enabled)
                    {
                        door.Enabled = true;
                    }
                    door.CloseDoor();
                });
            }

            /// <summary>
            /// Locks doors.
            /// </summary>
            public void LockDoors()
            {
                this.Echo("Locking doors...");
                this.TurnLightsRed(this.InteriorDoorLockIndicators);
                this.TurnLightsRed(this.ExteriorDoorLockIndicators);
                this.Perform(this.AllDoors, door => door.Enabled = false);
            }

            /// <summary>
            /// Unlock Interior Doors.
            /// </summary>
            public void UnlockInteriorDoors()
            {
                this.Echo("Unlocking Interior Doors...");
                this.TurnLightsGreen(this.InteriorDoorLockIndicators);
                this.Perform(this.InteriorDoors, door => door.Enabled = true);
            }

            /// <summary>
            /// Unlock Exterior Doors.
            /// </summary>
            public void UnlockExteriorDoors()
            {
                this.Echo("Unlocking Exterior Doors...");
                this.TurnLightsGreen(this.ExteriorDoorLockIndicators);
                this.Perform(this.ExteriorDoors, door => door.Enabled = true);
            }

            /// <summary>
            /// Opens interior doors.
            /// </summary>
            public void OpenInteriorDoors()
            {
                this.Echo("Opening Interior Doors...");
                this.Perform(this.InteriorDoors, door => door.OpenDoor());
            }

            /// <summary>
            /// Opens exterior doors.
            /// </summary>
            public void OpenExteriorDoors()
            {
                this.Echo("Opening Exterior Doors...");
                this.Perform(this.ExteriorDoors, door => door.OpenDoor());
            }

            /// <summary>
            /// Sets air vents to pressurize.
            /// </summary>
            public void Pressurize()
            {
                this.Echo("Pressurizing...");
                this.Perform(this.AirVents, vent => vent.Depressurize = false);
            }

            /// <summary>
            /// Sets air vents to depressurize.
            /// </summary>
            public void Depressurize()
            {
                this.Echo("Depressurizing...");
                this.Perform(this.AirVents, vent => vent.Depressurize = true);
            }

            /// <summary>
            /// Opens the exterior doors without cycling
            /// </summary>
            public void ForceOpenExterior()
            {
                this.Echo("Manual Release of Exterior Doors...");
                this.TurnLightsRed(this.AllLights);
                this.Perform(this.InteriorDoors, door => door.Enabled = false);
                this.UnlockExteriorDoors();
                this.OpenExteriorDoors();
            }

            /// <summary>
            /// Opens the interior doors without cycling
            /// </summary>
            public void ForceOpenInterior()
            {
                this.Echo("Manual Release of Interior Doors...");
                this.TurnLightsRed(this.AllLights);
                this.Perform(this.ExteriorDoors, door => door.Enabled = false);
                this.UnlockInteriorDoors();
                this.OpenInteriorDoors();
            }

            /// <summary>
            /// Gets the status message of this airlock's current status.
            /// </summary>
            /// <returns>Airlock Status Message</returns>
            public AirlockStatusMessage GetStatusMessage()
            {
                return new AirlockStatusMessage()
                {
                    Name = this.Name,
                    Status = this.State,
                    OxygenRatio = this.Oxygen.FillRatio,
                    InteriorDoorsOpen = this.InteriorDoorsOpen,
                    InteriorDoorsLocked = !this.InteriorDoorsUnlocked,
                    ExteriorDoorsOpen = this.ExteriorDoorsOpen,
                    ExteriorDoorsLocked = !this.ExteriorDoorsUnlocked
                };
            }

            /// <summary>
            /// Enables Cycle lights.
            /// </summary>
            private void EnableCycleLights()
            {
                this.Perform(this.CycleIndicators, light => light.Enabled = true);
            }

            /// <summary>
            /// Disables cycle lights.
            /// </summary>
            private void DisableCycleLights()
            {
                this.Perform(this.CycleIndicators, light => light.Enabled = false);
            }

            /// <summary>
            /// Turns the lights red.
            /// </summary>
            /// <param name="lights">Lights to turn red.</param>
            private void TurnLightsRed(List<IMyLightingBlock> lights)
            {
                this.Perform(lights, light =>
                {
                    light.Color = Color.Red;
                });
            }

            /// <summary>
            /// Turns the lights green.
            /// </summary>
            /// <param name="lights">Lights to turn green.</param>
            private void TurnLightsGreen(List<IMyLightingBlock> lights)
            {
                this.Perform(lights, light =>
                {
                    light.Color = Color.Green;
                });
            }

            /// <summary>
            /// Performs the callback on each member of the list.
            /// </summary>
            /// <typeparam name="T">Type of block.</typeparam>
            /// <param name="blocks">Blocks to perform callback on.</param>
            /// <param name="callback">Callback to perform.</param>
            private void Perform<T>(List<T> blocks, Action<T> callback)
                where T : IMyTerminalBlock
            {
                foreach(T block in blocks)
                {
                    callback(block);
                }
            }
        }
    }
}

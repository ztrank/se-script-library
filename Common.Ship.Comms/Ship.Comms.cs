﻿namespace IngameScript
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

        public partial class Ship
        {
            /// <summary>
            /// Ship Communication's Array.
            /// </summary>
            private ICommunicationArray communicationArray;

            /// <summary>
            /// Gets the communication array.
            /// </summary>
            public ICommunicationArray CommunicationArray
            {
                get
                {
                    if (this.communicationArray == null)
                    {
                        this.communicationArray = (ICommunicationArray)this.SubSystems.Find(s => s is ICommunicationArray);
                    }

                    return this.CommunicationArray;
                }
            }
        }
    }
}

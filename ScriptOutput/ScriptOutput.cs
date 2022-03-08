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

    partial class Program : MyGridProgram
    {
        private readonly List<string> Lines = new List<string>();
        private readonly IMyBroadcastListener channel;
        private readonly StandardOut standardOut;
        private readonly MyIni ini = new MyIni();

        public Program()
        {
            this.Runtime.UpdateFrequency = UpdateFrequency.Update100;
            this.channel = this.IGC.RegisterBroadcastListener("stdout");
            this.channel.SetMessageCallback("stdout");

            if(this.ini.TryParse(this.Me.CustomData))
            {
                string stdOut = this.ini.Get("output", "stdout").ToString(null);
                if (!string.IsNullOrWhiteSpace(stdOut))
                {
                    IMyTerminalBlock block = this.GridTerminalSystem.GetBlockWithName(stdOut);

                    IMyTextSurface surface = null;
                    if (block is IMyTextSurface)
                    {
                        surface = (IMyTextSurface)block;
                    }
                    else if (block is IMyTextSurfaceProvider)
                    {
                        int stdIndex = this.ini.Get("output", "stdoutIndex").ToInt32(0);
                        surface = ((IMyTextSurfaceProvider)block).GetSurface(stdIndex);
                    }

                    if (surface != null)
                    {
                        this.standardOut = new StandardOut(surface, new StandardOut.Settings()
                        {
                            LineSize = this.ini.Get("output", "stdoutLineSize").ToInt32(20),
                            MaxLines = this.ini.Get("output", "stdoutMaxLines").ToInt32(10)
                        });
                    }
                }
            }
            

            if (this.standardOut == null)
            {
                this.Echo("NO CONNECTED SCREEN");
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update100) > 0)
            {
                this.Draw();
            } 
            else if ((updateSource & UpdateType.IGC) > 0)
            {
                while(this.channel.HasPendingMessage)
                {
                    MyIGCMessage message = this.channel.AcceptMessage();
                    this.Echo($"Message: {message.Tag} | {message.Data}");
                    if (message.Tag == "stdout")
                    {
                        if (message.Data is string)
                        {
                            this.Lines.Add((string)message.Data);
                        }
                    }
                }
            }
        }

        private void Draw()
        {
            if (this.standardOut != null)
            {
                this.standardOut.Draw(this.Lines);
            }
        }
    }
}

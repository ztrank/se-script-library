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
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.
        private readonly bool IsServer = false;
        private readonly Server server;
        private readonly Connection connection;
        private readonly IMyTextPanel panel;
        private readonly IMyTextPanel panel2;
        private readonly IMyTextPanel panel3;
        private readonly List<MySprite> sprites = new List<MySprite>();
        private Vector2 linePosition = new Vector2();
        private readonly MyIni ini = new MyIni();
        private int count = 0;
        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.

            if (this.ini.TryParse(this.Me.CustomData))
            {
                if (this.ini.Get("igc", "server").ToBoolean())
                {
                    this.IsServer = true;
                }

                if (this.IsServer)
                {
                    this.server = new Server(this.IGC, this.ini.Get("igc", "dns").ToString("dns"), this.Log, this.Error)
                    {
                        Debug = true
                    };
                }
                else
                {
                    this.connection = new Connection(this.IGC, this.ini.Get("igc", "dns").ToString("dns"), this.ini.Get("igc", "connection").ToString(), this.Log, this.Error)
                    {
                        Debug = true
                    };
                }

                this.panel = (IMyTextPanel)this.GridTerminalSystem.GetBlockWithName(this.ini.Get("igc", "output").ToString("Unicast Test") + " 1");
                this.panel.ContentType = ContentType.SCRIPT;
                this.panel.Script = "";

                this.panel2 = (IMyTextPanel)this.GridTerminalSystem.GetBlockWithName(this.ini.Get("igc", "output").ToString("Unicast Test") + " 2");
                this.panel2.ContentType = ContentType.SCRIPT;
                this.panel2.Script = "";

                this.panel3 = (IMyTextPanel)this.GridTerminalSystem.GetBlockWithName(this.ini.Get("igc", "output").ToString("Unicast Test") + " 3");
                this.panel3.ContentType = ContentType.SCRIPT;
                this.panel3.Script = "";

                RectangleF viewPort = new RectangleF(this.panel.TextureSize - this.panel.SurfaceSize / 2f, this.panel.SurfaceSize);
                this.linePosition = new Vector2(-246, -256) + viewPort.Position;

            }

            if (this.server != null)
            {
                this.Log("Server Configuration Complete");
                this.Log($" > {this.server.ServerName}");
            }
            else if (this.connection != null)
            {
                this.connection.RegisterHandler(this.OnMessage);
                this.Log("Connection Configuration Complete");
                this.Log($" > {this.connection.Tag}");
            }

            this.Runtime.UpdateFrequency = UpdateFrequency.Update10 | UpdateFrequency.Update100;
        }

        private void OnMessage(MyIGCMessage message)
        {
            if (message.Data is string)
            {
                this.Log((string)message.Data);
            }
            else if (message.Data is MyTuple<string, object>)
            {
                MyTuple<string, object> data = (MyTuple<string, object>)message.Data;
                this.Log($"{data.Item1} : {data.Item2}");
            }
        }

        private void Log(string message)
        {
            this.sprites.Add(new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = message,
                RotationOrScale = .8f,
                Alignment = TextAlignment.LEFT,
                Color = Color.White,
                FontId = "White",
                Position = this.linePosition
            });
            this.linePosition += new Vector2(0, 20);
        }

        private void Error(string message)
        {
            this.sprites.Add(new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = message,
                RotationOrScale = .8f,
                Alignment = TextAlignment.LEFT,
                Color = Color.Red,
                FontId = "White",
                Position = this.linePosition
            });
            this.linePosition += new Vector2(0, 20);
        }

        private void Draw()
        {
            Vector2 vertOffset = new Vector2(0, -20 * Math.Max(this.sprites.Count - 20, 0));
            Vector2 horOffset = new Vector2(0, 0);

            using (MySpriteDrawFrame frame = this.panel.DrawFrame())
            {
                foreach (MySprite sprite in this.sprites)
                {
                    frame.Add(new MySprite()
                    {
                        Type = sprite.Type,
                        Data = sprite.Data,
                        RotationOrScale = sprite.RotationOrScale,
                        Alignment = sprite.Alignment,
                        Color = sprite.Color,
                        FontId = sprite.FontId,
                        Position = sprite.Position + vertOffset + horOffset
                    });
                }
            }

            horOffset += new Vector2(-512, 0);

            using (MySpriteDrawFrame frame = this.panel2.DrawFrame())
            {
                foreach (MySprite sprite in this.sprites)
                {
                    frame.Add(new MySprite()
                    {
                        Type = sprite.Type,
                        Data = sprite.Data,
                        RotationOrScale = sprite.RotationOrScale,
                        Alignment = sprite.Alignment,
                        Color = sprite.Color,
                        FontId = sprite.FontId,
                        Position = sprite.Position + vertOffset + horOffset
                    });
                }
            }
            horOffset += new Vector2(-512, 0);

            using (MySpriteDrawFrame frame = this.panel3.DrawFrame())
            {
                foreach (MySprite sprite in this.sprites)
                {
                    frame.Add(new MySprite()
                    {
                        Type = sprite.Type,
                        Data = sprite.Data,
                        RotationOrScale = sprite.RotationOrScale,
                        Alignment = sprite.Alignment,
                        Color = sprite.Color,
                        FontId = sprite.FontId,
                        Position = sprite.Position + vertOffset + horOffset
                    });
                }
            }

        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.
            try
            {
                if (this.server != null && (updateSource & UpdateType.Update100) > 0)
                {
                    this.count++;
                    if (count % 10 == 0)
                    {
                        this.server.ProcessHealthChecks();
                        count = 0;
                    }
                    
                }
                else if (this.connection != null && (updateSource & UpdateType.Update100) > 0)
                {
                    this.count++;
                    if (count % 5 == 0)
                    {
                        this.connection.ProcessHealth();
                        count = 0;
                    }
                }

                if ((updateSource & UpdateType.Update10) > 0)
                {
                    this.Draw();
                }
                else if ((updateSource & UpdateType.IGC) > 0)
                {
                    if (this.server != null)
                    {
                        this.server.ProcessMessages();
                    }
                    else if (this.connection != null)
                    {
                        this.connection.ProcessMessages();
                    }
                }
                else if (argument == "connect")
                {
                    if (this.connection == null)
                    {
                        this.Error("Connection is null");
                    }
                    else
                    {
                        this.connection.Connect();
                    }
                }
                else if (argument == "broadcast")
                {
                    if (this.connection == null)
                    {
                        this.Error("Connection is null");
                    }
                    else
                    {
                        this.connection.Send("broadcast");
                    }
                }
                else if (argument == "relay")
                {
                    this.Echo("Sending relay");
                    if (this.connection == null)
                    {
                        this.Error("Connection is null");
                    }
                    else
                    {
                        this.Echo("Sending relay for real");
                        this.connection.Send(this.ini.Get("igc", "relay").ToString(), "relay message");
                    }
                }
            }
            catch(Exception ex)
            {
                this.Error(ex.Message);
            }
        }
    }
}

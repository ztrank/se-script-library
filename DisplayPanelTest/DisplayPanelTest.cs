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
        private readonly MyIni ini = new MyIni();
        private readonly DisplayGrid textGrid;
        private readonly DisplayGrid lcdGrid;
        private readonly DisplayGrid tLcdGrid;
        private readonly DisplayGrid wLcdGrid;
        private readonly Dictionary<string, DisplayGrid> grids = new Dictionary<string, DisplayGrid>();
        private readonly Dictionary<string, List<MySprite>> spriteKeys = new Dictionary<string, List<MySprite>>();
        private bool SpritesSet = false;

        private void FakeEcho(string message)
        {

        }

        
        public Program()
        {
            this.Runtime.UpdateFrequency = UpdateFrequency.Update100;
            if(this.ini.TryParse(this.Me.CustomData))
            {
                this.textGrid = new DisplayGrid(this.GridTerminalSystem.GetBlockGroupWithName(this.ini.Get("display", "textGrid").ToString()), this.FakeEcho);
                this.lcdGrid = new DisplayGrid(this.GridTerminalSystem.GetBlockGroupWithName(this.ini.Get("display", "lcdGrid").ToString()), this.FakeEcho);
                this.tLcdGrid = new DisplayGrid(this.GridTerminalSystem.GetBlockGroupWithName(this.ini.Get("display", "tLcdGrid").ToString()), this.FakeEcho);
                this.wLcdGrid = new DisplayGrid(this.GridTerminalSystem.GetBlockGroupWithName(this.ini.Get("display", "wLcdGrid").ToString()), this.FakeEcho);
                this.grids.Add("text", this.textGrid);
                this.grids.Add("lcd", this.lcdGrid);
                this.grids.Add("tlcd", this.tLcdGrid);
                this.grids.Add("wlcd", this.wLcdGrid);
                this.spriteKeys.Add("text", this.textSprites);
                this.spriteKeys.Add("lcd", this.lcdSprites);
                this.spriteKeys.Add("tlcd", this.tLcdSprites);
                this.spriteKeys.Add("wlcd", this.wLcdSprites);

                this.SetSprites();
            }

        }

        private readonly List<MySprite> textSprites = new List<MySprite>();
        private readonly List<MySprite> lcdSprites = new List<MySprite>();
        private readonly List<MySprite> tLcdSprites = new List<MySprite>();
        private readonly List<MySprite> wLcdSprites = new List<MySprite>();
        

        private void SetSprites()
        {
            



            foreach (string key in this.spriteKeys.Keys)
            {
                DisplayGrid grid = this.grids[key];
                SpriteOriginPositions corners = grid.Origins;
                this.Echo($"Corners:  {key}");
                this.Echo($"[({corners[SpriteOrigins.TopLeft]?.X},{corners[SpriteOrigins.TopLeft]?.Y}), ({corners[SpriteOrigins.TopRight]?.X},{corners[SpriteOrigins.TopRight]?.Y}), ({corners[SpriteOrigins.BottomRight]?.X},{corners[SpriteOrigins.BottomRight]?.Y}), ({corners[SpriteOrigins.BottomLeft]?.X},{corners[SpriteOrigins.BottomLeft]?.Y}), ({corners[SpriteOrigins.Center]?.X},{corners[SpriteOrigins.Center]?.Y})]");
                
                grid.SetSprites(this.spriteKeys[key]);
            }
            this.SpritesSet = true;
        }
        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update100) > 0)
            {
                this.textGrid.Paint();
                this.lcdGrid.Paint();
                this.tLcdGrid.Paint();
                this.wLcdGrid.Paint();
            }
            else
            {
                if (this.SpritesSet)
                {
                    foreach(string key in this.spriteKeys.Keys)
                    {
                        this.spriteKeys[key].Clear();
                        this.grids[key].SetSprites(this.spriteKeys[key]);
                        this.grids[key].Paint();
                    }
                    this.SpritesSet = false;
                }
                else
                {
                    this.SetSprites();
                    this.textGrid.Paint();
                    this.lcdGrid.Paint();
                    this.tLcdGrid.Paint();
                    this.wLcdGrid.Paint();
                }
            }
        }
    }
}

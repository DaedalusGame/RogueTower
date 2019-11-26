using Humper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    class GameWorld : World
    {
        public Map Map;
        public Player Player;

        public float Hitstop = 0;

        public int Width => Map.Width * 16;
        public int Height => Map.Height * 16;

        public GameWorld(int width, int height, float cellSize = 64) : base(width * 16, height * 16, cellSize)
        {
            Map = new Map(this, width, height);
        }

        public void Update(float delta)
        {
            Map.Update();

            float globalDelta = delta;

            if (Hitstop > 0)
                globalDelta = 0;

            Player.Update(globalDelta);

            Hitstop -= delta;
        }
    }
}

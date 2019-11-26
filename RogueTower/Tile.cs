using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    abstract class Tile
    {
        public Map Map;
        public int X, Y;
        public bool Passable;

        public Tile(Map map, int x, int y, bool passable)
        {
            Map = map;
            X = x;
            Y = y;
            Passable = passable;
        }
    }

    class EmptySpace : Tile
    {
        public EmptySpace(Map map, int x, int y) : base(map, x, y, true)
        {
        }
    }

    class Wall : Tile
    {
        public Wall(Map map, int x, int y) : base(map, x, y, false)
        {
        }
    }
}

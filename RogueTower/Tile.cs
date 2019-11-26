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
        public float Friction = 1.0f;

        public Tile(Map map, int x, int y, bool passable)
        {
            Map = map;
            X = x;
            Y = y;
            Passable = passable;
        }

        public void Replace(Tile tile)
        {
            Map.Tiles[X, Y] = tile;
            Map.CollisionDirty = true;
            CopyTo(tile);
        }

        public void ReplaceEmpty()
        {
            Replace(new EmptySpace(Map, X, Y));
        }

        //Copy values over here
        public virtual void CopyTo(Tile tile)
        {
            //NOOP
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

    class WallIce : Wall
    {
        public WallIce(Map map, int x, int y) : base(map, x, y)
        {
            Friction = 0.3f;
        }
    }

    class WallBlock : Wall
    {
        public WallBlock(Map map, int x, int y) : base(map, x, y)
        {
        }
    }
}

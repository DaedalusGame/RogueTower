using Humper.Base;
using static RogueTower.Game;
using ChaiFoxes.FMODAudio;
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
        public bool CanDamage = false;
        public float Friction = 1.0f;
        public double Health = 100.0;
        public double Damage = 0;
        public virtual Sound breakSound => sfx_tile_break;

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

        public void HandleTileDamage(double damagein)
        {
            if(CanDamage == false)
                return;
            Health -= damagein;
            if(Health <= 0)
            {
                PlaySFX(breakSound, 1.0f, 0.1f, 0.2f);
                Replace(new EmptySpace(Map, X, Y));
            }
        }
        public virtual RectangleF GetBoundingBox()
        {
            return new RectangleF(0, 0, 16, 16);
        }

        public virtual bool CanClimb(HorizontalFacing side)
        {
            return false;
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
            
            CanDamage = true;
            Friction = 0.3f;
            Health = 25.0;
        }
        public override Sound breakSound => sfx_tile_icebreak;
    }

    class WallBlock : Wall
    {
        public WallBlock(Map map, int x, int y) : base(map, x, y)
        {
        }
    }

    class Spike : Wall
    {
        public Spike(Map map, int x, int y) : base(map, x, y)
        {
            Damage = 10.0;
        }
    }

    class Ladder : Wall
    {
        public HorizontalFacing Facing;

        public Ladder(Map map, int x, int y, HorizontalFacing facing) : base(map, x, y)
        {
            Facing = facing;
        }

        public override RectangleF GetBoundingBox()
        {
            switch(Facing)
            {
                case (HorizontalFacing.Right):
                    return new RectangleF(13, 0, 3, 16);
                case (HorizontalFacing.Left):
                    return new RectangleF(0, 0, 3, 16);
            }
            

            return base.GetBoundingBox();
        }

        public override bool CanClimb(HorizontalFacing side)
        {
            return Facing == side.Mirror();
        }
    }
}

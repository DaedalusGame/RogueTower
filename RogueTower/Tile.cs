using Humper.Base;
using static RogueTower.Game;
using ChaiFoxes.FMODAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Humper;

namespace RogueTower
{
    abstract class Tile
    {
        public GameWorld World => Map.World;
        public Map Map;
        public int X, Y;
        public TileBG Background => Map.Background[X,Y];
        public bool Passable;
        public bool CanDamage = false;
        public float Friction = 1.0f;
        public double Health, MaxHealth;
        public virtual Sound breakSound => sfx_tile_break;
        public Color Color = Color.White;
        public List<Box> Boxes = new List<Box>();

        public Tile(Map map, int x, int y, bool passable, double health)
        {
            Map = map;
            X = x;
            Y = y;
            Passable = passable;
            Health = health;
            MaxHealth = Health;
        }


        protected Box CreateBox(RectangleF bounds)
        {
            Box box = new Box(World, X * 16 + bounds.X, Y * 16 + bounds.Y, bounds.Width, bounds.Height);
            box.Data = this;
            return box;
        }

        public virtual void AddCollisions()
        {
            if (!Passable)
                Boxes.Add(CreateBox(new RectangleF(0,0,16,16)));
        }

        public void Replace(Tile tile)
        {
            Map.RemoveTileCollisions(this);
            Map.Tiles[X, Y] = tile;
            Map.AddTileCollisions(tile);
            CopyTo(tile);
        }

        public void ReplaceEmpty()
        {
            Replace(new EmptySpace(Map, X, Y));
        }

        public virtual void HandleTileDamage(double damagein)
        {
            if(CanDamage == false)
                return;
            Health -= damagein;
            if(Health <= 0)
            {
                PlaySFX(breakSound, 1.0f, 0.1f, 0.2f);
                Replace(new EmptySpace(Map, X, Y));
            }
            new DamagePopup(Map.World, new Vector2(X*16+8,Y*16 + 8) + new Vector2(0, -16), damagein.ToString(), 30);
        }

        public virtual bool CanClimb(HorizontalFacing side)
        {
            return false;
        }

        public Tile GetNeighbor(int dx, int dy)
        {
            return Map.GetTile(X + dx, Y + dy);
        }

        public IEnumerable<Tile> GetAdjacentNeighbors()
        {
            return new[] { GetNeighbor(1, 0), GetNeighbor(0, 1), GetNeighbor(-1, 0), GetNeighbor(0, -1) }.Shuffle();
        }

        //Copy values over here
        public virtual void CopyTo(Tile tile)
        {
            tile.Color = Color;
        }

        public virtual void StepOn(EnemyHuman human)
        {
            //NOOP
        }
    }

    class EmptySpace : Tile
    {
        public EmptySpace(Map map, int x, int y) : base(map, x, y, true, double.PositiveInfinity)
        {
        }
    }

    class Grass : Tile
    {
        public Grass(Map map, int x, int y) : base(map, x, y, false, 0)
        {
        }
    }

    class Wall : Tile
    {
        public enum WallFacing
        {
            Normal,
            Top,
            Bottom,
            BottomTop,
        }

        public WallFacing Facing;

        public Wall(Map map, int x, int y, double health) : base(map, x, y, false, health)
        {
        }

        public Wall(Map map, int x, int y, WallFacing facing) : this(map, x, y, 100)
        {
            Facing = facing;
        }

        public Wall(Map map, int x, int y) : this(map, x, y, WallFacing.Normal)
        {
        }
    }

    class WallIce : Wall
    {
        public WallIce(Map map, int x, int y) : base(map, x, y, 25)
        {
            CanDamage = true;
            Friction = 0.3f;
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
        public double Damage = 10.0;

        public Spike(Map map, int x, int y) : base(map, x, y)
        {
        }

        public override void StepOn(EnemyHuman human)
        {
            if (human is MoaiMan)
                return; //Moaimen are spike-immune
            human.Hit(-Util.GetFacingVector(human.Facing) * 1 + new Vector2(0, -2), 20, 50, Damage);
        }
    }

    class SpikeDeath : Spike
    {
        public SpikeDeath(Map map, int x, int y) : base(map, x, y)
        {
            Damage = double.PositiveInfinity;
        }
    }

    abstract class Trap : Wall
    {
        public float LastTrigger;
        public abstract float RetriggerTime
        {
            get;
        }

        public bool Triggered => World.Frame - LastTrigger <= RetriggerTime;

        public Trap(Map map, int x, int y) : base(map, x, y)
        {
        }

        public Trap(Map map, int x, int y, WallFacing facing) : base(map, x, y, facing)
        {
        }

        public abstract void Trigger(EnemyHuman human);

        public override void StepOn(EnemyHuman human)
        {
            if (!Triggered)
            {
                Trigger(human);
            }
            LastTrigger = World.Frame;
        }
    }

    class BumpTrap : Trap
    {
        public override float RetriggerTime => 30;

        public BumpTrap(Map map, int x, int y) : base(map, x, y)
        {
        }

        public BumpTrap(Map map, int x, int y, WallFacing facing) : base(map, x, y, facing)
        {
        }

        public override void Trigger(EnemyHuman human)
        {
            if (human is MoaiMan)
                return;
            PlaySFX(sfx_player_hurt, 0.4f, 0.3f, 0.3f);
        }
    }

    class Ladder : Wall
    {
        public HorizontalFacing Facing;

        public Ladder(Map map, int x, int y, HorizontalFacing facing) : base(map, x, y)
        {
            Facing = facing;
        }

        public override void AddCollisions()
        {
            switch (Facing)
            {
                case (HorizontalFacing.Right):
                    Boxes.Add(CreateBox(new RectangleF(13, 0, 3, 16)));
                    break;
                case (HorizontalFacing.Left):
                    Boxes.Add(CreateBox(new RectangleF(0, 0, 3, 16)));
                    break;
            }
        }

        public override bool CanClimb(HorizontalFacing side)
        {
            return Facing == side.Mirror();
        }
    }

    class LadderExtend : Ladder
    {
        public LadderExtend(Map map, int x, int y, HorizontalFacing facing) : base(map, x, y, facing)
        {
        }

        public override void AddCollisions()
        {
            switch (Facing)
            {
                case (HorizontalFacing.Right):
                    Boxes.Add(CreateBox(new RectangleF(11, 0, 5, 16)));
                    break;
                case (HorizontalFacing.Left):
                    Boxes.Add(CreateBox(new RectangleF(0, 0, 5, 16)));
                    break;
            }
        }

        public override bool CanClimb(HorizontalFacing side)
        {
            return Facing == side.Mirror();
        }

        public IEnumerable<Wait> Unfold()
        {
            bool end = false;
            LadderExtend ladder = this;
            while (!end) {
                var newLadder = ladder;
                var bottom = ladder.GetNeighbor(0, 1);
                if (bottom is EmptySpace)
                {
                    newLadder = new LadderExtend(ladder.Map, ladder.X, ladder.Y + 1, ladder.Facing);
                    bottom.Replace(newLadder);
                }
                else
                {
                    end = true;
                }
                ladder.Replace(new Ladder(ladder.Map, ladder.X, ladder.Y, ladder.Facing));
                ladder = newLadder;
                new ScreenShakeRandom(World, 1, 20);
                yield return new WaitDelta(World,3);
            }
        }

        public override void HandleTileDamage(double damagein)
        {
            Scheduler.Instance.Run(new Coroutine(Unfold()));
        }
    }
}

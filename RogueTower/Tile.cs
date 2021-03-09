using ChaiFoxes.FMODAudio;
using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using RogueTower.Effects;
using RogueTower.Effects.Particles;
using RogueTower.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;
using static RogueTower.Game;

namespace RogueTower
{
    [Flags]
    enum Connectivity
    {
        None        = 0,
        //Directions
        North       = 1,
        NorthEast   = 2,
        East        = 4,
        SouthEast   = 8,
        South       = 16,
        SouthWest   = 32,
        West        = 64,
        NorthWest   = 128,
        //Kill Flags
        KillNorth = ~(North | NorthEast | NorthWest),
        KillEast = ~(East | NorthEast | SouthEast),
        KillSouth = ~(South | SouthEast | SouthWest),
        KillWest = ~(West | NorthWest | SouthWest),
    }

    abstract class Tile
    {
        static Dictionary<int, int> BlobTileMap = new Dictionary<int, int>() //Mapper for the minimal tileset, index in memory -> index in image
        {
            {0, 0},
            {4, 1},
            {92, 2},
            {124, 3},
            {116, 4},
            {80, 5},
            //{0, 6},
            {16, 7},
            {20, 8},
            {87, 9},
            {223, 10},
            {241, 11},
            {21, 12},
            {64, 13},
            {29, 14},
            {117, 15},
            {85, 16},
            {71, 17},
            {221, 18},
            {125, 19},
            {112, 20},
            {31, 21},
            {253, 22},
            {113, 23},
            {28, 24},
            {127, 25},
            {247, 26},
            {209, 27},
            {23, 28},
            {199, 29},
            {213, 30},
            {95, 31},
            {255, 32},
            {245, 33},
            {81, 34},
            {5, 35},
            {84, 36},
            {93, 37},
            {119, 38},
            {215, 39},
            {193, 40},
            {17, 41},
            //{0, 42},
            {1, 43},
            {7, 44},
            {197, 45},
            {69, 46},
            {68, 47},
            {65, 48},
        };

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

        public Mechanism Mechanism;

        public RoomTile Room;
        public FlagConnect ConnectFlag = FlagConnect.Any;
        public Connectivity Connectivity;
        public bool ConnectionDirty = true;
        public int BlobIndex
        {
            get
            {
                Connectivity connectivity = CullDiagonals();
                return BlobTileMap.ContainsKey((int)connectivity) ? BlobTileMap[(int)connectivity] : BlobTileMap[0];
            }
        }

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

        public Vector2? GetRandomPosition(Random random)
        {
            if (Boxes.Any())
            {
                var box = Boxes.Pick(random);
                return Util.GetRandomPosition(box.Bounds, random);
            }
            return null;
        }

        public void Replace(Tile tile)
        {
            Map.RemoveTileCollisions(this);
            Map.Tiles[X, Y] = tile;
            Map.AddTileCollisions(tile);
            ClearConnection();
            CopyTo(tile);
        }

        public void ReplaceEmpty()
        {
            Replace(new EmptySpace(Map, X, Y));
        }

        public virtual bool Connects(Tile other)
        {
            return false;
        }

        private Connectivity CullDiagonals()
        {
            Connectivity connectivity = Connectivity;
            if (!connectivity.HasFlag(Connectivity.North))
                connectivity &= Connectivity.KillNorth;
            if (!connectivity.HasFlag(Connectivity.East))
                connectivity &= Connectivity.KillEast;
            if (!connectivity.HasFlag(Connectivity.South))
                connectivity &= Connectivity.KillSouth;
            if (!connectivity.HasFlag(Connectivity.West))
                connectivity &= Connectivity.KillWest;
            return connectivity;
        }

        public void ClearConnection() //Bulky?
        {
            Tile north = GetNeighbor(0, -1);
            Tile west = GetNeighbor(-1, 0);
            Tile northwest = GetNeighbor(-1, -1);

            Map.ConnectionDirty = true;
            Map.Disconnect(this, north, Connectivity.North);
            Map.Disconnect(this, west, Connectivity.West);
            Map.Disconnect(this, northwest, Connectivity.NorthWest);
            Map.Disconnect(this, GetNeighbor(1, -1), Connectivity.NorthEast);
            Map.Disconnect(this, GetNeighbor(1, 0), Connectivity.East);
            Map.Disconnect(this, GetNeighbor(1, 1), Connectivity.SouthEast);
            Map.Disconnect(this, GetNeighbor(0, 1), Connectivity.South);
            Map.Disconnect(this, GetNeighbor(-1, 1), Connectivity.SouthWest);
            ConnectionDirty = true;
            north.ConnectionDirty = true;
            west.ConnectionDirty = true;
            northwest.ConnectionDirty = true;
        }

        public void ChainDestroy()
        {
            foreach(var neighbor in GetAdjacentNeighbors())
            {
                if(neighbor.Mechanism is ChainDestroy)
                {
                    Scheduler.Instance.RunTimer(neighbor.ChainDestroy, new WaitDelta(World,3));
                }
            }

            new WallBreakEffect(World, new Vector2(X * 16 + 8, Y * 16 + 8), Color, 10);
            ReplaceEmpty();
        }


        public virtual void HandleTileDamage(double damagein)
        {
            if (Mechanism is ChainDestroyStart)
            {
                ChainDestroy();   
            }
            if (CanDamage == false)
                return;
            Health -= damagein;
            if(Health <= 0)
            {
                PlaySFX(breakSound, 1.0f, 0.1f, 0.2f);
                ReplaceEmpty();
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

        public IEnumerable<Tile> GetFullNeighbors()
        {
            return new[] { GetNeighbor(1, 0), GetNeighbor(0, 1), GetNeighbor(-1, 0), GetNeighbor(0, -1), GetNeighbor(1, 1), GetNeighbor(1, -1), GetNeighbor(-1, 1), GetNeighbor(-1, -1) };
        }

        public IEnumerable<Tile> GetDownNeighbors()
        {
            return new[] { GetNeighbor(1, 0), GetNeighbor(0, 1), GetNeighbor(-1, 0) };
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
        public bool Top => Facing == WallFacing.BottomTop || Facing == WallFacing.Top;

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
        public override Sound breakSound => sfx_tile_icebreak;

        public WallIce(Map map, int x, int y) : base(map, x, y, 25)
        {
            CanDamage = true;
            Friction = 0.3f;
        }

        public override bool Connects(Tile other)
        {
            return other is Wall && !(other is Ladder);
        }
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

        public virtual bool Triggered => Pressed;
        public bool Pressed => World.Frame - LastTrigger <= RetriggerTime;

        public Trap(Map map, int x, int y) : base(map, x, y)
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

        public override void Trigger(EnemyHuman human)
        {
            if (human is MoaiMan)
                return;
            PlaySFX(sfx_player_hurt, 0.4f, 0.3f, 0.3f);
        }
    }

    class PoisonTrap : Trap
    {
        public override float RetriggerTime => 30;

        public PoisonTrap(Map map, int x, int y) : base(map, x, y)
        {
        }

        public override void Trigger(EnemyHuman human)
        {
            if (human is Player)
            {
                World.Hitstop = 10;
            }
            Scheduler.Instance.Run(new Coroutine(PoisonBlast()));
        }

        public IEnumerable<Wait> PoisonBlast()
        {
            yield return new WaitDelta(World, 1);

            new PoisonBreath(World, new Vector2(X * 16 + 8, Y * 16))
            {
                Velocity = new Vector2(0, -1),
                FrameEnd = 20,
            };
        }
    }

    class SlowTrap : Trap
    {
        public override float RetriggerTime => 30;

        public SlowTrap(Map map, int x, int y) : base(map, x, y)
        {
        }

        public override void Trigger(EnemyHuman human)
        {
            if (human is Player)
            {
                World.Hitstop = 10;
            }
            Scheduler.Instance.Run(new Coroutine(SlowBlast(human)));
        }

        public IEnumerable<Wait> SlowBlast(EnemyHuman human)
        {
            yield return new WaitDelta(World, 1);

            human.AddStatusEffect(new Slow(human, 0.6f, 1000));
        }
    }

    class DoomTrap : Trap
    {
        public override float RetriggerTime => 30;

        public DoomTrap(Map map, int x, int y) : base(map, x, y)
        {
        }

        public override void Trigger(EnemyHuman human)
        {
            //NOOP
        }

        public override void StepOn(EnemyHuman human)
        {
            base.StepOn(human);

            if(!human.Dead)
                human.AddStatusEffect(new Doom(human, 1, 30));
        }
    }

    abstract class TeleportTrap : Trap
    {
        public abstract Vector2 Destination
        {
            get;
        }
        public override float RetriggerTime => 30;

        public TeleportTrap(Map map, int x, int y) : base(map, x, y)
        {
        }

        public override void Trigger(EnemyHuman human)
        {
            Scheduler.Instance.Run(new Coroutine(Teleport(human, Destination)));
        }

        public IEnumerable<Wait> Teleport(EnemyHuman human, Vector2 destination)
        {
            human.Hitstop = 30;

            yield return new WaitDelta(World, 30);

            human.Position = Destination;
        }
    }

    class TeleportTrapLinked : TeleportTrap
    {
        public override Vector2 Destination => new Vector2(LinkX*16+8,LinkY*16-8);
        public int LinkX, LinkY;

        public Tile Link => Map.GetTile(LinkX, LinkY);
        public override bool Triggered => Pressed || (Link is TeleportTrap teleport && teleport.Pressed);

        public TeleportTrapLinked(Map map, int x, int y) : base(map, x, y)
        {
        }
    }

    class VelocityTrap : Trap
    {
        public Random SpeedRand = new Random();
        public override float RetriggerTime => 5;

        public VelocityTrap(Map map, int x, int y) : base(map, x, y)
        {

        }

        public override void Trigger(EnemyHuman human)
        {
            var HumanVelocity = human.Velocity;
            human.Velocity = HumanVelocity * SpeedRand.Next(3,8);
        }
    }

    class LaunchTrap : VelocityTrap
    {
        public LaunchTrap(Map map, int x, int y) : base(map, x, y)
        {
        }

        public override void Trigger(EnemyHuman human)
        {
            human.Velocity.Y = -6 * SpeedRand.Next(1, 5);
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

    class Switch : Tile, IWireNode
    {
        public Vector2 Position => new Vector2(X * 16 + 8, Y * 16 + 8);
        public bool Powered
        {
            get;
            set;
        }

        public Switch(Map map, int x, int y) : base(map, x, y, false, double.PositiveInfinity)
        {
        }

        public override void AddCollisions()
        {
            var box = CreateBox(new RectangleF(0, 0, 16, 16));
            box.AddTags(CollisionTag.NoCollision);
            Boxes.Add(box);
        }

        public override void HandleTileDamage(double damagein)
        {
            Powered = !Powered;
        }

        public void ConnectIn(IWireConnector connector)
        {
            //NOOP
        }

        public void ConnectOut(IWireConnector connector)
        {
            //NOOP
        }
    }
}

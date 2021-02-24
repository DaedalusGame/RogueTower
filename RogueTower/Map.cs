using Humper;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueTower.Enemies;

namespace RogueTower
{
    class SubTemplate
    {
        public string Tag;
        public int X;
        public int Y;

        public SubTemplate(int x, int y, string tag)
        {
            X = x;
            Y = y;
            Tag = tag;
        }

        public Template Pick(Random random)
        {
            WeightedList<Template> templates = new WeightedList<Template>();
            foreach(var template in Template.Puzzles[Tag])
            {
                templates.Add(template, template.Weight);
            }
            return templates.GetWeighted(random);
        }
    }

    class WireConnection
    {
        public Point Start;
        public Point End;

        public WireConnection(Point start, Point end)
        {
            Start = start;
            End = end;
        }
    }

    enum PuzzleType
    {
        Input,
        Output,
    }

    class PuzzleNode
    {
        public List<Node> Inputs = new List<Node>();
        public List<Node> Outputs = new List<Node>();
    }

    class PuzzleConnection
    {
        public Point Position;
        public PuzzleType Type;

        public PuzzleConnection(Point position, PuzzleType type)
        {
            Position = position;
            Type = type;
        }
    }

    class Map
    {
        public WeightedList<Func<Map, int, int, Trap>> PossibleTraps = new WeightedList<Func<Map, int, int, Trap>>()
        {
            { (map,x,y) => new BumpTrap(map, x, y), 1 },
            { (map,x,y) => new PoisonTrap(map, x, y), 5 },
            { (map,x,y) => new SlowTrap(map, x, y), 5 },
            { (map,x,y) => new DoomTrap(map, x, y), 5 },
            { (map,x,y) => new VelocityTrap(map, x, y), 5 },
            { (map,x,y) => new LaunchTrap(map, x, y), 5 },

        };

        public int Width, Height;
        public Tile[,] Tiles;
        public TileBG[,] Background;

        public GameWorld World;
        public List<IBox> CollisionTiles = new List<IBox>();
        public bool CollisionDirty = true;
        public bool ConnectionDirty = true;

        public Queue<SubTemplate> SubTemplates = new Queue<SubTemplate>();
        public IWireNode[,] WireNodes;
        public List<WireConnection> WireConnections = new List<WireConnection>();
        public List<PuzzleConnection> PuzzleConnections = new List<PuzzleConnection>();
        public List<PuzzleNode> Puzzles = new List<PuzzleNode>();

        public Map(GameWorld world, int width, int height)
        {
            SetWorld(world);
            SetSize(width, height);
        }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[Width, Height];
            Background = new TileBG[Width, Height];
            WireNodes = new IWireNode[Width, Height];

            MapGenerator generator;

            do
            {
                generator = new MapGenerator(10, Height / 8);
                generator.Generate();
            }
            while (!generator.Finished);

            Random random = new Random();
            /*for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var rand = random.NextDouble();
                    if (x > 10 && x <= Width - 10 && rand <= 0.2)
                    {
                        if (random.NextDouble() < 0.3)
                            Tiles[x, y] = new WallBlock(this, x, y);
                        else
                            Tiles[x, y] = new Wall(this, x, y);
                    }
                    else if (x > 8 && x <= Width - 8 && rand <= 0.3)
                        Tiles[x, y] = new WallIce(this, x, y);
                    else
                        Tiles[x, y] = new EmptySpace(this, x, y);
                }
            }*/

            generator.Build(this);

            /*for (int i = 0; i < 40; i++)
            {
                int spikewidth = random.Next(4) + 1;
                int spikex = 8 + random.Next(Width - spikewidth - 16);
                int spikey = random.Next(Height - 2) + 2;

                for (int x = 0; x < spikewidth; x++)
                {
                    for(int y = 1; y <= 2; y++)
                        Tiles[spikex + x, spikey - y] = new EmptySpace(this, spikex + x, spikey - y);
                    Tiles[spikex + x, spikey] = new Spike(this, spikex + x, spikey);
                }
            }*/

            /*for (int i = 0; i < 20; i++)
            {
                int ladderheight = random.Next(15) + 3;
                int ladderx = 8 + random.Next(Width - 16);
                int laddery = random.Next(Height - ladderheight);
                HorizontalFacing ladderfacing = HorizontalFacing.Left;
                if (random.NextDouble() < 0.5)
                    ladderfacing = HorizontalFacing.Right;
                BuildLadder(ladderx, laddery, ladderheight, ladderfacing);
            }*/

            List<Tile> walls = EnumerateTiles().Where(tile => tile is Wall && tile.GetAdjacentNeighbors().Any(x => x.Passable)).ToList();
            /*for(int i = 0; i < 70; i++)
            {
                int select = random.Next(walls.Count);
                Tile pickWall = walls[select];
                walls.RemoveAt(select);

                int n = 1;

                float angle = random.NextFloat() * MathHelper.TwoPi;
                float speed = 0.025f + random.NextFloat() * 0.05f;
                speed = 0.05f;
                int length = random.Next(60) + 40;
                bool swings = false;

                if (random.NextDouble() < 0.5)
                    swings = true;
                if (random.NextDouble() < 0.3)
                {
                    n = random.Next(3) + 2;
                    swings = false;
                }

                for (int e = 0; e < n; e++)
                {
                    var ballAndChain = new BallAndChain(World, new Vector2(pickWall.X * 16 + 8,pickWall.Y * 16 + 8), angle + e * MathHelper.TwoPi / n, speed, length);
                    ballAndChain.Swings = swings;
                }
            }*/

            List<Tile> holes = EnumerateTiles().Where(tile => tile is EmptySpace && tile.Background == TileBG.Black).ToList();
            for (int i = 0; i < Math.Min(70,holes.Count); i++)
            {
                Tile pick = holes.PickAndRemove(random);

                new Snake(World, new Vector2(pick.X * 16 + 8, pick.Y * 16 + 8));
            }

            List<Tile> floors = EnumerateTiles().Where(tile => tile is EmptySpace && tile.GetNeighbor(0,1) is Wall).ToList();
            for (int i = 0; i < 80; i++)
            {
                int select = random.Next(floors.Count);
                Tile pickWall = floors[select];
                floors.RemoveAt(select);

                new BlueDemon(World, new Vector2(pickWall.X * 16 + 8, pickWall.Y * 16 + 8));
            }

            /*for (int i = 0; i < 50; i++)
            {
                int select = random.Next(floors.Count);
                Tile pickWall = floors[select];
                floors.RemoveAt(select);

                new Hydra(World, new Vector2(pickWall.X * 16 + 8, pickWall.Y * 16 + 8));
            }*/

            UpdateConnectivity();
        }

        public void BuildLadder(int ladderx, int laddery, int ladderheight, HorizontalFacing ladderfacing)
        {
            for (int y = 0; y < ladderheight; y++)
            {
                int facingOffset = (ladderfacing == HorizontalFacing.Right ? 1 : -1);
                Tiles[ladderx, laddery + y] = new Ladder(this, ladderx, laddery + y, ladderfacing);
                Tiles[ladderx + facingOffset, laddery + y] = new Wall(this, ladderx + facingOffset, laddery + y);
            }
        }

        public Trap BuildTrap(int x, int y, Random random)
        {
            Tile tile = GetTile(x, y);
            var generator = PossibleTraps.GetWeighted(random);
            return generator(this,x,y);
        }

        public Tile GetTile(int x, int y)
        {
            if (InMap(x, y))
                return Tiles[x, y];
            else
                return new Wall(this, x, y);
        }

        public TileBG GetBackground(int x, int y)
        {
            if (InMap(x, y))
                return Background[x, y];
            else
                return TileBG.Empty;
        }

        public Tile FindTile(Vector2 pos)
        {
            int x = (int)(pos.X / 16);
            int y = (int)(pos.Y / 16);
            return GetTile(x, y);
        }

        public TileBG FindBackground(Vector2 pos)
        {
            int x = (int)(pos.X / 16);
            int y = (int)(pos.Y / 16);
            return GetBackground(x, y);
        }

        public bool HasWire(int x, int y)
        {
            return WireNodes[x, y] != null;
        }

        public bool InMap(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public IEnumerable<Tile> EnumerateTiles()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    yield return Tiles[x, y];
                }
            }
        }

        public void SetWorld(GameWorld world)
        {
            World = world;
            CollisionTiles.Clear();
            CollisionDirty = true;
        }

        public void Update()
        {
            if (CollisionDirty)
            {
                UpdateCollisions();
                CollisionDirty = false;
            }

            if(ConnectionDirty)
            {
                UpdateConnectivity();
                ConnectionDirty = false;
            }
        }

        public void UpdateCollisions()
        {
            foreach (IBox box in CollisionTiles)
            {
                World.Remove(box);
            }
            CollisionTiles.Clear();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    AddTileCollisions(Tiles[x, y]);
                }
            }

            IBox leftBox = World.Create(-16+1, 0, 16, Height * 16);
            IBox rightBox = World.Create(Width * 16 - 1, 0, 16, Height * 16);
            IBox baseBox = World.Create(0, Height * 16-1, Width * 16, 16);
            CollisionTiles.Add(leftBox);
            CollisionTiles.Add(rightBox);
            CollisionTiles.Add(baseBox);
        }

        public void AddTileCollisions(Tile tile)
        {
            tile.AddCollisions();
            foreach(Box box in tile.Boxes)
            {
                World.Add(box);
                CollisionTiles.Add(box);
            }
        }

        public void RemoveTileCollisions(Tile tile)
        {
            foreach(Box box in tile.Boxes)
            {
                World.Remove(box);
                CollisionTiles.Remove(box);
            }
        }

        public void UpdateConnectivity()
        {
            IEnumerable<Tile> dirtyTiles = EnumerateTiles().Where(tile => tile.ConnectionDirty).ToList();

            foreach (Tile tile in dirtyTiles)
            {
                Tile south = tile.GetNeighbor(0, 1);
                Tile east = tile.GetNeighbor(1, 0);
                Tile southeast = tile.GetNeighbor(1, 1);
                bool doEast = tile.X < Width - 1;
                bool doSouth = tile.Y < Height - 1;

                if (doEast && (tile.Connects(east) || east.Connects(tile)))
                    Connect(tile, east, Connectivity.East);
                if (doSouth && (tile.Connects(south) || south.Connects(tile)))
                    Connect(tile, south, Connectivity.South);
                if (doEast && doSouth)
                {
                    if (tile.Connects(southeast) || southeast.Connects(tile))
                        Connect(tile, southeast, Connectivity.SouthEast);
                    if (east.Connects(south) || south.Connects(east))
                        Connect(east, south, Connectivity.SouthWest);
                }

                tile.ConnectionDirty = false;
            }
        }

        public void Connect(Tile a, Tile b, Connectivity connection)
        {
            Connectivity rotated = connection.Rotate(4);
            a.Connectivity |= connection;
            b.Connectivity |= rotated;
        }

        public void Disconnect(Tile a, Tile b, Connectivity connection)
        {
            Connectivity rotated = connection.Rotate(4);
            a.Connectivity &= ~connection;
            b.Connectivity &= ~rotated;
        }
    }
}

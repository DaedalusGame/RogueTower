using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    enum Direction
    {
        Up,
        Left,
        Down,
        Right,
    }

    enum TravelDirection
    {
        Any,
        Up,
        Left,
        Down,
        Right,
    }

    /// <summary>
    /// How the 
    /// </summary>
    enum FlagConnect
    {
        Any = -1,
        In = 0,
        Out = 1,
        InOut = 2,
        Blocked = 3,
        Fast = 4,
        Teleport = 5,
        Ignore = 15,
    }

    class Template
    {
        public static List<Template> Templates = new List<Template>();
        public static ILookup<string, Template> Puzzles;

        public string Name;
        public int Weight;
        public TravelDirection TravelDirection;

        public int Width;
        public int Height;

        public string Up;
        public string Down;
        public string Right;
        public string Left;

        public IEnumerable<string> Tags;

        public int[,] Foreground;
        public int[,] Background;
        private FlagConnect[,] Connect;
        public int[,] Mechanisms;

        public int Connections => GetConnections();
        public List<JObject> Entities = new List<JObject>();
        public List<JObject> EntitiesMechanism = new List<JObject>();

        public FlagConnect GetConnectFlag(int x, int y) => Connect?[x, y] ?? FlagConnect.Any;
        public int GetMechanismFlag(int x, int y) => Mechanisms?[x, y] ?? -1;

        public string GetConnection(Direction dir)
        {
            switch(dir)
            {
                default:
                    return "none";
                case (Direction.Up):
                    return Up;
                case (Direction.Down):
                    return Down;
                case (Direction.Left):
                    return Left;
                case (Direction.Right):
                    return Right;
            }
        } 

        public int GetConnections()
        {
            int i = 4;
            foreach(Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (GetConnection(direction) == "none")
                    i--;
            }
            return i;
        }

        public static void LoadAll()
        {
            Templates.Clear();
            foreach(var jsonFile in Directory.EnumerateFiles("content/templates", "*.json"))
            {
                Template template = new Template();
                template.Load(jsonFile);
                if (template.Width != 8 || template.Height != 8)
                {
                    Console.WriteLine($"Room {template.Name} isn't 8x8!");
                    continue;
                }
                Templates.Add(template);
            }
            List<Template> puzzles = new List<Template>();
            foreach (var jsonFile in Directory.EnumerateFiles("content/templates/puzzle", "*.json"))
            {
                Template template = new Template();
                template.Load(jsonFile);
                puzzles.Add(template);
            }
            Puzzles = puzzles.ToMultiLookup(puzzle => puzzle.Tags);
        }

        public void Load(string filename)
        {
            var stream = File.OpenText(filename);

            var reader = new JsonTextReader(stream);
            JObject root = JObject.Load(reader);

            Name = Path.GetFileNameWithoutExtension(filename);
            Width = root["width"].ToObject<int>() / 16;
            Height = root["height"].ToObject<int>() / 16;

            var values = root["values"];

            Up = values["ConnectionUp"].ToObject<string>();
            Down = values["ConnectionDown"].ToObject<string>();
            Left = values["ConnectionLeft"].ToObject<string>();
            Right = values["ConnectionRight"].ToObject<string>();
            Weight = int.Parse(values["Weight"].ToObject<string>());
            if(values["Tags"] != null)
                Tags = values["Tags"].ToObject<string>().Split(',').Select(x => x.Trim());

            TravelDirection = (TravelDirection)Enum.Parse(typeof(TravelDirection), values["Direction"].ToObject<string>());

            foreach (var layer in root["layers"])
            {
                var name = layer["name"].ToObject<string>();
                if(name == "Foreground")
                {
                    LoadTileLayer(layer, (width, height) => Foreground = new int[width, height], (x, y, id) => Foreground[x, y] = id);
                }
                if (name == "Background")
                {
                    LoadTileLayer(layer, (width, height) => Background = new int[width, height], (x, y, id) => Background[x, y] = id);
                }
                if (name == "Entities")
                {
                    foreach (var entity in layer["entities"])
                    {
                        Entities.Add((JObject)entity);
                    }
                }
                if (name == "FlagConnect")
                {
                    LoadTileLayer(layer, (width, height) => Connect = new FlagConnect[width, height], (x, y, id) => Connect[x, y] = (FlagConnect)id);
                }
                if (name == "FlagMechanism")
                {
                    LoadTileLayer(layer, (width, height) => Mechanisms = new int[width, height], (x, y, id) => Mechanisms[x, y] = id);
                }
                if (name == "Mechanism")
                {
                    foreach (var entity in layer["entities"])
                    {
                        EntitiesMechanism.Add((JObject)entity);
                    }
                }
            }

            reader.Close();
        }

        private delegate void SetupTileDelegate(int width, int height);

        private delegate void LoadTileDelegate(int x, int y, int id);

        private void LoadTileLayer(JToken layer, SetupTileDelegate setup, LoadTileDelegate loader)
        {
            int width = layer["gridCellsX"].ToObject<int>();
            int height = layer["gridCellsY"].ToObject<int>();
            setup(width, height);
            int i = 0;
            foreach (var tile in layer["data"])
            {
                int x = i % width;
                int y = i / width;
                loader(x,y,tile.ToObject<int>());
                i++;
            }
        }

        public void PrintForeground(int id, Map map, int x, int y, Random random)
        {
            switch (id)
            {
                default:
                    map.Tiles[x, y] = new EmptySpace(map, x, y);
                    break;
                case (0):
                    map.Tiles[x, y] = new Wall(map, x, y, Wall.WallFacing.Normal);
                    break;
                case (1):
                    if (random.NextDouble() < 0.07)
                    {
                        var trap = map.BuildTrap(x, y, random);
                        trap.Facing = Wall.WallFacing.Top;
                        map.Tiles[x, y] = trap;
                    }
                    else
                        map.Tiles[x, y] = new Wall(map, x, y, Wall.WallFacing.Top);
                    break;
                case (2):
                    map.Tiles[x, y] = new Wall(map, x, y, Wall.WallFacing.Bottom);
                    break;
                case (3):
                    map.Tiles[x, y] = new WallBlock(map, x, y);
                    break;
                case (4):
                    map.Tiles[x, y] = new Spike(map, x, y);
                    break;
                case (6):
                    if (random.NextDouble() < 0.07)
                    {
                        var trap = map.BuildTrap(x, y, random);
                        trap.Facing = Wall.WallFacing.BottomTop;
                        map.Tiles[x, y] = trap;
                    }
                    else
                        map.Tiles[x, y] = new Wall(map, x, y, Wall.WallFacing.BottomTop);
                    break;
                case (8):
                    map.Tiles[x, y] = new Grass(map, x, y);
                    break;
                case (9):
                    map.Tiles[x,y] = new WallIce(map, x, y);
                    break;
                case (10):
                    map.Tiles[x, y] = new Ladder(map, x, y, HorizontalFacing.Left);
                    break;
                case (11):
                    map.Tiles[x, y] = new Ladder(map, x, y, HorizontalFacing.Right);
                    break;
                case (17):
                    if (random.NextDouble() < 0.5)
                        map.Tiles[x, y] = new EmptySpace(map, x, y);
                    else
                        map.Tiles[x, y] = new Wall(map, x, y);
                    break;
                case (18):
                    if (random.NextDouble() < 0.5)
                        map.Tiles[x, y] = new WallBlock(map, x, y);
                    else
                        map.Tiles[x, y] = new Wall(map, x, y);
                    break;
                case (19):
                    if (random.NextDouble() < 0.5)
                        map.Tiles[x, y] = new WallIce(map,x, y);
                    else
                        map.Tiles[x, y] = new EmptySpace(map, x, y);
                    break;
                case (20):
                    map.Tiles[x, y] = new SpikeDeath(map, x, y);
                    break;
                case (21):
                    map.Tiles[x, y] = new LadderExtend(map, x, y, HorizontalFacing.Left);
                    break;
                case (22):
                    map.Tiles[x, y] = new LadderExtend(map, x, y, HorizontalFacing.Right);
                    break;
                case (63): //Keep as is
                    break;
            }
        }

        public void PrintBackground(int id, Map map, int x, int y, Random random)
        {
            WeightedList<TileBG> RandomBricks = new WeightedList<TileBG>();
            RandomBricks.Add(TileBG.Brick, 80);
            RandomBricks.Add(TileBG.BrickMiss1, 30);
            RandomBricks.Add(TileBG.BrickMiss2, 15);

            switch (id)
            {
                default:
                    map.Background[x, y] = TileBG.Empty;
                    break;
                case (0):
                    map.Background[x, y] = TileBG.Brick;
                    break;
                case (1):
                    map.Background[x, y] = TileBG.Tile4;
                    break;
                case (2):
                    map.Background[x, y] = TileBG.TileDetail;
                    break;
                case (3):
                    map.Background[x, y] = TileBG.Statue;
                    break;
                case (4):
                    map.Background[x, y] = TileBG.BrickMiss1;
                    break;
                case (5):
                    map.Background[x, y] = TileBG.BrickMiss2;
                    break;
                case (6):
                    map.Background[x, y] = TileBG.BrickOpening;
                    break;
                case (7):
                    map.Background[x, y] = RandomBricks.GetWeighted(random);
                    break;
                case (8):
                    map.Background[x, y] = TileBG.RailLeft;
                    break;
                case (9):
                    map.Background[x, y] = TileBG.RailMiddle;
                    break;
                case (10):
                    map.Background[x, y] = TileBG.RailRight;
                    break;
                case (11):
                    map.Background[x, y] = TileBG.PillarTop;
                    break;
                case (12):
                    map.Background[x, y] = TileBG.BrickPlatform;
                    break;
                case (13):
                    map.Background[x, y] = TileBG.Black;
                    break;
                case (14):
                    map.Background[x, y] = TileBG.BrickHole;
                    break;
                case (16):
                    map.Background[x, y] = TileBG.Window;
                    break;
                case (17):
                    map.Background[x, y] = TileBG.WindowBigLeft;
                    break;
                case (18):
                    map.Background[x, y] = TileBG.WindowBigRight;
                    break;
                case (19):
                    map.Background[x, y] = TileBG.PillarDetail;
                    break;
                case (27):
                    map.Background[x, y] = TileBG.Pillar;
                    break;
                case (35):
                    map.Background[x, y] = TileBG.PillarBottomBroken;
                    break;
                case (63): //Keep as is
                    break;
            }
        }

        public void PrintMechanism(JObject entity, Map map, int px, int py)
        {
            GameWorld world = map.World;
            string type = entity["name"].ToObject<string>();
            float ox = entity["x"].ToObject<float>();
            float oy = entity["y"].ToObject<float>();
            var values = entity["values"];

            float x = px * 16 + ox;
            float y = py * 16 + oy;
            var tile = map.Tiles[px + (int)(ox / 16), py + (int)(oy / 16)];

            if(type == "chain_destroy")
            {
                tile.Mechanism = new ChainDestroyStart();
            }
            if(type == "template_builder")
            {
                string tag = values["Tag"].ToObject<string>();
                map.SubTemplates.Enqueue(new SubTemplate(tile.X, tile.Y, tag));
            }
            if(type == "wire_in" && !map.HasWire(tile.X, tile.Y))
            {
                var node = new NodeRandom(world, new Vector2(x + 8, y + 8));
                map.WireNodes[tile.X, tile.Y] = node;
                ConnectWires(entity, map, px, py);
            }
            if (type == "wire_out" && !map.HasWire(tile.X, tile.Y))
            {
                var node = new NodeCombine(world, new Vector2(x + 8, y + 8), 1);
                map.WireNodes[tile.X, tile.Y] = node;
            }
            if (type == "wire_or" && !map.HasWire(tile.X, tile.Y))
            {
                var node = new NodeCombine(world, new Vector2(x + 8, y + 8), 1);
                map.WireNodes[tile.X, tile.Y] = node;
                ConnectWires(entity, map, px, py);
            }
            if (type == "wire_and" && !map.HasWire(tile.X, tile.Y))
            {
                var node = new NodeCombine(world, new Vector2(x + 8, y + 8), 1000);
                map.WireNodes[tile.X, tile.Y] = node;
                ConnectWires(entity, map, px, py);
            }
            if (type == "puzzle_in" && !map.HasWire(tile.X, tile.Y))
            {
                var node = new NodeCombine(world, new Vector2(x + 8, y + 8), 1000);
                map.WireNodes[tile.X, tile.Y] = node;
                ConnectWires(entity, map, px, py);
            }
            if (type == "puzzle_out" && !map.HasWire(tile.X, tile.Y))
            {
                var node = new NodeCombine(world, new Vector2(x + 8, y + 8), 1);
                map.WireNodes[tile.X, tile.Y] = node;
            }
            if (type == "switch" && !map.HasWire(tile.X, tile.Y))
            {
                bool powered = values["Powered"].ToObject<bool>();
                var switchTile = new Switch(map, tile.X, tile.Y)
                {
                    Powered = powered,
                };
                map.Tiles[tile.X, tile.Y] = switchTile;
                map.WireNodes[tile.X, tile.Y] = switchTile;
                ConnectWires(entity, map, px, py);
            }
        }

        private static void ConnectWires(JObject entity, Map map, int px, int py)
        {
            var nodes = entity["nodes"];

            float ox = entity["x"].ToObject<float>();
            float oy = entity["y"].ToObject<float>();

            int tx = px + (int)(ox / 16);
            int ty = py + (int)(oy / 16);

            foreach (var connection in nodes)
            {
                int connectionX = px + connection["x"].ToObject<int>() / 16;
                int connectionY = py + connection["y"].ToObject<int>() / 16;
                map.WireConnections.Add(new WireConnection(new Point(tx, ty), new Point(connectionX, connectionY)));
            }
        }

        public void PrintMechanism(int id, Map map, int x, int y)
        {
            var tile = map.Tiles[x, y];

            switch (id)
            {
                default:
                    tile.Mechanism = Mechanism.None;
                    break;
                case (0):
                    tile.CanDamage = false;
                    break;
                case (3):
                    tile.Mechanism = new ChainDestroy();
                    break;
                case (4):
                    tile.Mechanism = new ChainDestroyDirection(Direction.Up);
                    break;
                case (5):
                    tile.Mechanism = new ChainDestroyDirection(Direction.Right);
                    break;
                case (6):
                    tile.Mechanism = new ChainDestroyDirection(Direction.Down);
                    break;
                case (7):
                    tile.Mechanism = new ChainDestroyDirection(Direction.Left);
                    break;
                case (63): //Leave as is
                    break;
            }
        }

        public void PrintConnectFlag(FlagConnect flag, Map map, int x, int y)
        {
            switch (flag) {
                default:
                    map.Tiles[x, y].ConnectFlag = flag;
                    break;
                case FlagConnect.Ignore: //Leave as is
                    break;
            }
        }

        public void PrintEntity(JObject entity, Map map, int px, int py)
        {
            GameWorld world = map.World;
            string type = entity["name"].ToObject<string>();
            float ox = entity["x"].ToObject<float>();
            float oy = entity["y"].ToObject<float>();
            var values = entity["values"];

            float x = px * 16 + ox;
            float y = py * 16 + oy;

            if (type == "ball_and_chain") //TODO: make a dictionary of (name -> generator delegate)
            {
                float rotation = entity["rotation"].ToObject<float>();
                float speed = values["Speed"].ToObject<float>();
                float distance = values["Distance"].ToObject<float>();
                bool swings = values["Swings"].ToObject<bool>();

                var ballandchain = new BallAndChain(world, new Vector2(x, y), MathHelper.ToRadians(rotation), speed, distance);
                ballandchain.Swings = swings;
            }
            if(type == "moaiman")
            {
                bool flipped = entity["flippedX"].ToObject<bool>();
                var moaiman = new MoaiMan(world, new Vector2(x, y));
                moaiman.Facing = flipped ? HorizontalFacing.Left : HorizontalFacing.Right;
            }
            if (type == "cannon")
            {
                float rotation = entity["rotation"].ToObject<float>();
                float delay = values["Delay"].ToObject<float>();
                var cannon = new CannonFire(world, new Vector2(x, y), MathHelper.ToRadians(rotation));
                cannon.DelayTime = delay;
            }
            if (type == "cannon_fire")
            {
                float rotation = entity["rotation"].ToObject<float>();
                float delay = values["Delay"].ToObject<float>();
                var cannon = new CannonFire(world, new Vector2(x, y), MathHelper.ToRadians(rotation));
                cannon.DelayTime = delay;
            }
            if (type == "cannon_poison")
            {
                float rotation = entity["rotation"].ToObject<float>();
                float delay = values["Delay"].ToObject<float>();
                var cannon = new CannonPoisonBreath(world, new Vector2(x, y), MathHelper.ToRadians(rotation));
                cannon.DelayTime = delay;
            }
            if (type == "snake")
            {
                bool flipped = entity["flippedX"].ToObject<bool>();
                var snake = new Snake(world, new Vector2(x, y));
                snake.Facing = flipped ? HorizontalFacing.Left : HorizontalFacing.Right;
            }
            if (type == "hydra")
            {
                bool flipped = entity["flippedX"].ToObject<bool>();
                var hydra = new Hydra(world, new Vector2(x, y));
                hydra.Facing = flipped ? HorizontalFacing.Left : HorizontalFacing.Right;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

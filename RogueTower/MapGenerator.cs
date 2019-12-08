﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    enum RoomType
    {
        Empty,
        Blocked,
        Horizontal,
        HubUp,
        HubDown,
        HubDownHorizontal,
        HubVertical,
        Entrance,
        Exit,
        LeftUp,
        RightUp,
        LeftDown,
        RightDown,
    }

    class PossibleTemplate
    {
        public Template Template;
        public bool Forbidden;

        public PossibleTemplate(Template template)
        {
            Template = template;
        }

        public override string ToString()
        {
            return $"{Template.Name} {Forbidden}";
        }
    }

    class RoomTile
    {
        public MapGenerator Generator;
        public int X, Y;
        public RoomType Type;
        public double Weight;
        public List<PossibleTemplate> PossibleTemplates = new List<PossibleTemplate>();
        public double Entropy => GetEntropy();

        public string ConnectUp => string.Join(", ", GetNeighbor(0, -1).PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Down).Distinct());
        public string ConnectDown => string.Join(", ", GetNeighbor(0, 1).PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Up).Distinct());
        public string ConnectLeft => string.Join(", ", GetNeighbor(-1, 0).PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Left).Distinct());
        public string ConnectRight => string.Join(", ", GetNeighbor(1, 0).PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Right).Distinct());

        public IEnumerable<string> EdgeLeft => X >= Generator.Width ? new[] { "none" } : PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Left);
        public IEnumerable<string> EdgeRight => X < 0 ? (Y == Generator.Height - 1 ? new[] { "none", "entrance" } : new[] { "none" }) : PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Right);
        public IEnumerable<string> EdgeUp => Y >= Generator.Height ? new[] { "none" } : PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Up);
        public IEnumerable<string> EdgeDown => Y < 0 ? new[] { "none", "exit" } : PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Down);

        public RoomTile(MapGenerator generator, int x, int y)
        {
            Generator = generator;
            X = x;
            Y = y;
        }

        public void InitWave()
        {
            foreach (Template template in Template.Templates)
            {
                PossibleTemplate possibleTemplate = new PossibleTemplate(template);
                PossibleTemplates.Add(possibleTemplate);
            }

            if (Type == RoomType.Entrance)
            {

            }
            if (Type == RoomType.Horizontal)
            {

            }
            if (Type == RoomType.HubVertical)
            {

            }
            if (Type == RoomType.HubUp)
            {

            }
        }

        public void InitConstraints()
        {
            var right = GetNeighbor(1, 0).EdgeLeft.ToHashSet();
            var left = GetNeighbor(-1, 0).EdgeRight.ToHashSet();
            var up = GetNeighbor(0, -1).EdgeDown.ToHashSet();
            var down = GetNeighbor(0, 1).EdgeUp.ToHashSet();

            Forbid(Direction.Left, connection => !left.Contains(connection));
            Forbid(Direction.Right, connection => !right.Contains(connection));
            Forbid(Direction.Up, connection => !up.Contains(connection));
            Forbid(Direction.Down, connection => !down.Contains(connection));

            if (Type != RoomType.Empty)
            {
                Forbid(template => template.GetConnections() < 2);
            }

            //var topology = Generator.DebugPrint((x) => Generator.GetChar(x.Type));
            switch (Type)
            {
                case (RoomType.Entrance):
                    Forbid(Direction.Left, connection => connection != "entrance");
                    break;
                case (RoomType.Horizontal):
                    Forbid(Direction.Left, connection => connection == "none");
                    Forbid(Direction.Right, connection => connection == "none");
                    if(Generator.Random.NextDouble() > 0.5)
                    {
                        Forbid(Direction.Up, connection => connection != "none");
                        Forbid(Direction.Down, connection => connection != "none");
                    }
                    break;
                case (RoomType.HubVertical):
                    Forbid(Direction.Up, connection => connection == "none");
                    Forbid(Direction.Down, connection => connection == "none");
                    break;
                case (RoomType.HubUp):
                    Forbid(Direction.Up, connection => connection == "none");
                    break;
                case (RoomType.HubDown):
                    Forbid(Direction.Down, connection => connection == "none");
                    break;
                case (RoomType.RightUp):
                    Forbid(Direction.Up, connection => connection == "none");
                    Forbid(Direction.Right, connection => connection == "none");
                    break;
                case (RoomType.LeftUp):
                    Forbid(Direction.Left, connection => connection == "none");
                    Forbid(Direction.Up, connection => connection == "none");
                    break;
                case (RoomType.RightDown):
                    Forbid(Direction.Down, connection => connection == "none");
                    Forbid(Direction.Right, connection => connection == "none");
                    break;
                case (RoomType.LeftDown):
                    Forbid(Direction.Left, connection => connection == "none");
                    Forbid(Direction.Down, connection => connection == "none");
                    break;
                    /*case (RoomType.Empty):
                        Forbid(template => template.Connections > 0);
                        break;*/
            }
        }

        /*public bool PropagateWave()
        {
            var right = GetNeighbor(1, 0);
            var left = GetNeighbor(-1, 0);
            var up = GetNeighbor(0, -1);
            var down = GetNeighbor(0, 1);

            bool noChange = true;

            foreach(var template in PossibleTemplates)
            {
                bool hasRight = right.PossibleTemplates.Any() && !right.PossibleTemplates.Any(x => !x.Forbidden && x.Template.Left == template.Template.Right);
                bool hasLeft = left.PossibleTemplates.Any() && !left.PossibleTemplates.Any(x => !x.Forbidden && x.Template.Right == template.Template.Left);
                bool hasUp = up.PossibleTemplates.Any() && !up.PossibleTemplates.Any(x => !x.Forbidden && x.Template.Down == template.Template.Up);
                bool hasDown = down.PossibleTemplates.Any() && !down.PossibleTemplates.Any(x => !x.Forbidden && x.Template.Up == template.Template.Down);
                if (hasRight ||
                    hasLeft ||
                    hasUp ||
                    hasDown)
                {
                    template.Forbidden = true;
                    noChange = false;
                }
            }

            return noChange;
        }*/

        public void Forbid(Direction direction, Func<string, bool> check)
        {
            Forbid(temp => check(temp.GetConnection(direction)));
        }

        public void Forbid(Func<Template, bool> check)
        {
            Queue<_internal> calls = new Queue<_internal>();

            calls.Enqueue(() => ForbidInternal(check));

            while(calls.Count > 0)
            {
                var call = calls.Dequeue();
                foreach(var subcall in call())
                {
                    calls.Enqueue(subcall);
                }
            }
        }

        delegate IEnumerable<_internal> _internal();

        private IEnumerable<_internal> ForbidInternal(Direction direction, Func<string, bool> check)
        {
            return ForbidInternal(temp => check(temp.GetConnection(direction)));
        }

        private IEnumerable<_internal> ForbidInternal(Func<Template, bool> check)
        {
            bool verboten = false;

            foreach (var template in PossibleTemplates)
            {
                if (!template.Forbidden && check(template.Template))
                {
                    verboten = true;
                    template.Forbidden = true;
                }
            }

            if (verboten)
            {
                var left = PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Left).ToHashSet();
                var right = PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Right).ToHashSet();
                var up = PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Up).ToHashSet();
                var down = PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Down).ToHashSet();

                yield return () => GetNeighbor(-1, 0).ForbidInternal(Direction.Right, connection => !left.Contains(connection));
                yield return () => GetNeighbor(1, 0).ForbidInternal(Direction.Left, connection => !right.Contains(connection));
                yield return () => GetNeighbor(0, -1).ForbidInternal(Direction.Down, connection => !up.Contains(connection));
                yield return () => GetNeighbor(0, 1).ForbidInternal(Direction.Up, connection => !down.Contains(connection));
            }
        }

        public void Collapse()
        {
            var weightedList = new WeightedList<Template>();
            foreach(var template in PossibleTemplates.Where(x => !x.Forbidden))
            {
                weightedList.Add(template.Template, (int)Math.Pow(2,5 - template.Template.Connections));
            }
            var selected = weightedList.GetWeighted(Generator.Random);
            Forbid(temp => temp != selected);
        }

        public double GetEntropy()
        {
            var freqs = PossibleTemplates.GroupBy(x => x.Forbidden).Select(x => (double)x.Count() / PossibleTemplates.Count);
            freqs = freqs.Select(x => x * Math.Log(x) / Math.Log(2));
            return PossibleTemplates.Count(x => !x.Forbidden) - 1;
            //return -freqs.Sum();
        }

        public RoomTile GetNeighbor(int dx, int dy)
        {
            return Generator.GetRoom(X + dx, Y + dy);
        }

        public IEnumerable<RoomTile> GetAdjacentNeighbors()
        {
            return new[] { GetNeighbor(1, 0), GetNeighbor(0, 1), GetNeighbor(-1, 0), GetNeighbor(0, -1) }.Shuffle();
        }


        public override string ToString()
        {
            return Entropy.ToString();
        }
    }

    class MapGenerator
    {
        public RoomTile[,] Rooms;
        public Stack<Action> UndoStack;
        public int Width, Height;
        public bool Finished;

        public Random Random;

        public MapGenerator(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public IEnumerable<RoomTile> EnumerateTiles()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    yield return Rooms[x, y];
                }
            }
        }

        public bool WaveformCollapse()
        {
            var tiles = EnumerateTiles().ToList();
            var topology = DebugPrint((x) => GetChar(x.Type));

            Console.WriteLine("started");
            int i = 0;
            while (true)
            {
                i++;
                IEnumerable<RoomTile> nonZeroEntropy = tiles.Where(x => x.Entropy > 0).Shuffle();
                if (!nonZeroEntropy.Any())
                    break;
                var tile = nonZeroEntropy.WithMin(x => x.Entropy);
                //var entropies = DebugPrint((x) => (char)('1' + (int)x.Entropy));

                tile.Collapse();

                var dead = tiles.Where(x => x.Entropy < 0);
                if (dead.Any())
                {
                    Console.WriteLine($"extinct {i} iterations");
                    return false;
                }

            }

            var fillCount = tiles.Where(x => x.Entropy == 0).Count();

            return !tiles.Any(x => x.Entropy < 0);
        }

        public void Generate()
        {
            Random = new Random();

            Rooms = new RoomTile[Width, Height];
            UndoStack = new Stack<Action>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Rooms[x, y] = new RoomTile(this, x, y);
                    Rooms[x, y].Weight = Random.Next(100) + 1;
                }
            }

            /*while (!Finished)
            {
                Step();
            }*/

            for (int i = 0; i < 5; i++)
            {
                int height = Random.Next(3) + 4;
                int x1 = 1 + Random.Next(Width - 2);
                int x2 = 1 + Random.Next(Width - 2);
                int y = Random.Next(Height - height);

                for (int e = 0; e < Width; e++)
                {
                    for (int g = 0; g < height; g++)
                    {
                        RoomTile room = GetRoom(e, y + g);
                        if ((e <= Math.Max(x1, x2) && g == 0) || (e >= Math.Min(x1, x2) && g == height - 1))
                        {
                            room.Weight = 10000;
                        }
                        else if ((e == Math.Max(x1, x2) && g < height - 2) || (e == Math.Min(x1, x2) && g > 1))
                        {
                            room.Weight = double.PositiveInfinity;
                        }
                        else
                        {
                            room.Weight = Random.Next(100) + 1;
                        }
                    }
                }
            }

            var dijkstra = Util.Dijkstra(new Point(0, Height - 1), Width, Height, GetMainWeight, (pos) => GetRoom(pos.X, pos.Y).GetAdjacentNeighbors().Select(room => new Point(room.X, room.Y)));

            Console.WriteLine("Generate Path");
            var path = dijkstra.FindPath(new Point(Random.Next(Width), 0)).ToList();

            var lastPos = new Point(0, Height - 1);
            var lastHorizontal = 0;
            var lastVertical = 0;
            GetRoom(lastPos.X, lastPos.Y).Type = RoomType.Entrance;
            foreach (var pos in path)
            {
                var lastRoom = GetRoom(lastPos.X, lastPos.Y);
                var dx = pos.X - lastPos.X;
                var dy = pos.Y - lastPos.Y;
                if(lastRoom.Type == RoomType.Entrance)
                {
                    //NOOP
                }
                else if (dx == 1)
                {
                    if (lastVertical > 0)
                        lastRoom.Type = RoomType.RightUp;
                    else if (lastVertical < 0)
                        lastRoom.Type = RoomType.RightDown;
                    else
                        lastRoom.Type = RoomType.Horizontal;
                }
                else if(dx == -1)
                {
                    if (lastVertical > 0)
                        lastRoom.Type = RoomType.LeftUp;
                    else if (lastVertical < 0)
                        lastRoom.Type = RoomType.LeftDown;
                    else
                        lastRoom.Type = RoomType.Horizontal;
                }
                else if (dy == -1)
                {
                    if (lastHorizontal > 0)
                        lastRoom.Type = RoomType.LeftUp;
                    else if (lastHorizontal < 0)
                        lastRoom.Type = RoomType.RightUp;
                    else
                        lastRoom.Type = RoomType.HubVertical;
                }
                else if (dy == 1)
                {
                    if (lastHorizontal > 0)
                        lastRoom.Type = RoomType.LeftDown;
                    else if (lastHorizontal < 0)
                        lastRoom.Type = RoomType.RightDown;
                    else
                        lastRoom.Type = RoomType.HubVertical;
                }
                lastHorizontal = dx;
                lastVertical = dy;
                lastPos = pos;
            }
            GetRoom(lastPos.X, lastPos.Y).Type = RoomType.Exit;

            Console.WriteLine("Generate Wave");
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GetRoom(x, y).InitWave();
                }
            }

            Console.WriteLine("Set Constraints");
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GetRoom(x, y).InitConstraints();
                }
            }

            var topology = DebugPrint((x) => GetChar(x.Type));
            var entropies = DebugPrint((x) => (char)('1' + (int)x.Entropy));

            if (EnumerateTiles().Any(x => x.Entropy < 0))
            {

            }

            Finished = WaveformCollapse();
        }

        public void Build(Map map)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var rand = Random.NextDouble();
                    var rand2 = Random.NextDouble();
                    if ((x > 10 && x <= map.Width - 10) || (x > 8 && x <= map.Width - 8 && rand2 <= 0.7))
                    {
                        map.Background[x, y] = TileBG.Wall;
                    }

                    if (x > 10 && x <= map.Width - 10)
                    {

                        if (Random.NextDouble() < 0.1)
                            map.Tiles[x, y] = new WallBlock(map, x, y);
                        else
                            map.Tiles[x, y] = new Wall(map, x, y);
                    }
                    else if (x > 8 && x <= map.Width - 8 && rand <= 0.3)
                        map.Tiles[x, y] = new WallIce(map, x, y);
                    else if (y >= map.Height - 1)
                        map.Tiles[x, y] = new Grass(map, x, y);
                    else
                        map.Tiles[x, y] = new EmptySpace(map, x, y);
                }
            }

            int origin = map.Width / 2 - Width * 8 / 2 + 1;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var room = Rooms[x, y];
                    switch (room.Type)
                    {
                        case (RoomType.Horizontal):
                            BuildHorizontal(map, origin + x * 8, y * 8);
                            break;
                        case (RoomType.HubVertical):
                            BuildShaft(map, origin + x * 8, y * 8);
                            break;
                        case (RoomType.LeftDown):
                        case (RoomType.RightDown):
                        case (RoomType.HubDown):
                            BuildHubDown(map, origin + x * 8, y * 8);
                            break;
                        case (RoomType.LeftUp):
                        case (RoomType.RightUp):
                        case (RoomType.HubUp):
                            BuildHubUp(map, origin + x * 8, y * 8);
                            break;
                    }
                    if (room.PossibleTemplates.Count(temp => !temp.Forbidden) == 1)
                        BuildTemplate(map, origin + x * 8, y * 8, room.PossibleTemplates.First(temp => !temp.Forbidden).Template);
                }
            }
        }

        private void BuildTemplate(Map map, int px, int py, Template template)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    switch (template.Foreground[x, y])
                    {
                        default:
                            map.Tiles[px + x, py + y] = new EmptySpace(map, px + x, py + y);
                            break;
                        case (0):
                        case (1):
                        case (2):
                            map.Tiles[px + x, py + y] = new Wall(map, px + x, py + y);
                            break;
                        case (3):
                            map.Tiles[px + x, py + y] = new WallBlock(map, px + x, py + y);
                            break;
                        case (4):
                            map.Tiles[px + x, py + y] = new Spike(map, px + x, py + y);
                            break;
                        case (10):
                            map.Tiles[px + x, py + y] = new Ladder(map, px + x, py + y, HorizontalFacing.Left);
                            break;
                        case (11):
                            map.Tiles[px + x, py + y] = new Ladder(map, px + x, py + y, HorizontalFacing.Right);
                            break;
                    }
                }
            }
        }

        private void BuildHorizontal(Map map, int px, int py)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (y == 0 || y == 8 - 1)
                        map.Tiles[px + x, py + y] = new Wall(map, px + x, py + y);
                    else
                    {
                        map.Tiles[px + x, py + y] = new EmptySpace(map, px + x, py + y);
                        map.Background[px + x, py + y] = TileBG.Empty;
                    }
                }
            }
        }

        private void BuildShaft(Map map, int px, int py)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (x == 0 || x == 8 - 1)
                        map.Tiles[px + x, py + y] = new Wall(map, px + x, py + y);
                    else
                    {
                        map.Tiles[px + x, py + y] = new EmptySpace(map, px + x, py + y);
                        map.Background[px + x, py + y] = TileBG.Empty;
                    }
                }
            }

            map.BuildLadder(px + 4, py, 8, HorizontalFacing.Left);
        }

        private void BuildHubUp(Map map, int px, int py)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if ((y == 0 && (x == 0 || x == 8 - 1)) || y == 8 - 1)
                        map.Tiles[px + x, py + y] = new Wall(map, px + x, py + y);
                    else
                    {
                        map.Tiles[px + x, py + y] = new EmptySpace(map, px + x, py + y);
                        map.Background[px + x, py + y] = TileBG.Empty;
                    }
                }
            }

            map.BuildLadder(px + 4, py, 6, HorizontalFacing.Left);
        }

        private void BuildHubDown(Map map, int px, int py)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if ((y == 8 - 1 && (x == 0 || x == 8 - 1)) || y == 0)
                        map.Tiles[px + x, py + y] = new Wall(map, px + x, py + y);
                    else
                    {
                        map.Tiles[px + x, py + y] = new EmptySpace(map, px + x, py + y);
                        map.Background[px + x, py + y] = TileBG.Empty;
                    }
                }
            }

            map.BuildLadder(px + 4, py + 6, 2, HorizontalFacing.Left);
        }

        private double GetMainWeight(Point start, Point stop)
        {
            double weight = GetRoom(stop.X, stop.Y).Weight;
            if (stop.Y - start.Y < 0)
                return weight * 1000;
            else if (stop.Y - start.Y > 0)
                return weight * 0;
            else
                return weight * 1;
        }

        public RoomTile GetRoom(int x, int y)
        {
            if (x >= 0 && x <= Width - 1 && y >= 0 && y <= Height - 1)
                return Rooms[x, y];
            else
                return new RoomTile(this, x, y);
        }

        /*public void Step()
        {
            WeightedList<Action> possibleActions = new WeightedList<Action>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int rx = x;
                    int ry = y;
                    switch (Rooms[rx, ry])
                    {
                        case (RoomType.Horizontal):
                            if (GetRoom(rx,ry-1) == RoomType.Empty)
                                possibleActions.Add(() => CreateHub(rx, ry), 5);
                            if (GetRoom(rx - 1, ry) == RoomType.Empty)
                                possibleActions.Add(() => CreateHorizontal(rx - 1, ry), 100);
                            if (GetRoom(rx + 1, ry) == RoomType.Empty)
                                possibleActions.Add(() => CreateHorizontal(rx + 1, ry), 100);
                            break;
                        case (RoomType.HubDownHorizontal):
                            if (GetRoom(rx - 1, ry) == RoomType.Empty)
                                possibleActions.Add(() => CreateHorizontal(rx - 1, ry), 100);
                            if (GetRoom(rx + 1, ry) == RoomType.Empty)
                                possibleActions.Add(() => CreateHorizontal(rx + 1, ry), 100);
                            break;
                        case (RoomType.HubDown):
                            if (GetRoom(rx, ry - 1) == RoomType.Empty)
                                possibleActions.Add(() => ExtendHub(rx, ry), 5);
                            if (GetRoom(rx - 1, ry) == RoomType.Empty)
                                possibleActions.Add(() => CreateHorizontal(rx - 1, ry), 100);
                            if (GetRoom(rx + 1, ry) == RoomType.Empty)
                                possibleActions.Add(() => CreateHorizontal(rx + 1, ry), 100);
                            break;
                        case (RoomType.HubVertical):
                            break;
                        case (RoomType.HubUp):
                            if (GetRoom(rx - 1, ry) == RoomType.Empty)
                                possibleActions.Add(() => CreateHorizontal(rx - 1, ry), 100);
                            if (GetRoom(rx + 1, ry) == RoomType.Empty)
                                possibleActions.Add(() => CreateHorizontal(rx + 1, ry), 100);
                            break;
                    }
                }
            }

            if (possibleActions.Count > 0)
            {
                Action picked = possibleActions.GetWeighted(Random);
                picked();
            }
            else
            {
                string debug = DebugPrint();
                Finished = true;
            }
        }*/

        public string DebugPrint(Func<RoomTile, char> selector)
        {
            string s = "";
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    s += selector(Rooms[x, y]);
                }
                s += "\n";
            }
            return s;
        }

        public char GetChar(RoomType room)
        {
            switch (room)
            {
                default:
                    return ' ';
                case (RoomType.Horizontal):
                    return '─';
                case (RoomType.HubUp):
                    return '┴';
                case (RoomType.HubDown):
                    return '┬';
                case (RoomType.HubVertical):
                    return '│';
                case (RoomType.LeftUp):
                    return '┘';
                case (RoomType.RightUp):
                    return '└';
                case (RoomType.LeftDown):
                    return '┐';
                case (RoomType.RightDown):
                    return '┌';
                case (RoomType.Blocked):
                    return 'X';
                case (RoomType.Exit):
                    return 'E';
                case (RoomType.Entrance):
                    return 'S';
            }
        }

        /*public void CreateHorizontal(int x, int y)
        {
            if (GetRoom(x - 1, y) == RoomType.Horizontal && GetRoom(x + 1, y) == RoomType.Horizontal)
                Rooms[x, y] = RoomType.Blocked;
            else
            Rooms[x, y] = RoomType.Horizontal;
        }

        public void CreateHub(int x, int y)
        {
            Rooms[x, y] = RoomType.HubUp;
            Rooms[x, y - 1] = RoomType.HubDown;
        }

        public void ExtendHub(int x, int y)
        {
            Rooms[x, y] = RoomType.HubVertical;
            Rooms[x, y - 1] = RoomType.HubDown;
        }*/

        public void Undo()
        {

        }
    }
}

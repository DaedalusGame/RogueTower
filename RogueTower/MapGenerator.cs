﻿using Microsoft.Xna.Framework;
using RogueTower.Actions;
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
        public bool Memory;

        public PossibleTemplate(Template template)
        {
            Template = template;
        }

        public void Save()
        {
            Memory = Forbidden;
        }

        public void Revert()
        {
            Forbidden = Memory;
        }

        public override string ToString()
        {
            return $"{Template.Name} {Forbidden}";
        }
    }

    enum KComponentType
    {
        Source,
        Sink,
        Traversal,
        Vault,
    }

    class KComponent
    {
        public List<RoomTile> Tiles = new List<RoomTile>();
        public KComponentType Type;
        public Color Color = Color.White;

        public void Add(RoomTile tile)
        {
            Tiles.Add(tile);
            tile.KComponent = this;
        }

        public override string ToString()
        {
            return $"KComponent ({Type}) ({Tiles.Count} elements)";
        }

        public void Qualify()
        {
            var inNeighbors = Tiles.SelectMany(tile => tile.GetInNeighbors()).Where(neighbor => neighbor.KComponent != this && neighbor.KComponent != null).Select(neighbor => neighbor.KComponent).Distinct();
            var outNeighbors = Tiles.SelectMany(tile => tile.GetOutNeighbors()).Where(neighbor => neighbor.KComponent != this && neighbor.KComponent != null).Select(neighbor => neighbor.KComponent).Distinct();

            bool hasIn = inNeighbors.Any();
            bool hasOut = outNeighbors.Any();
            if (hasIn && hasOut)
                Type = KComponentType.Traversal;
            else if (hasIn)
                Type = KComponentType.Sink;
            else if (hasOut)
                Type = KComponentType.Source;
            else
                Type = KComponentType.Vault;
        }
    }

    class EnvironmentComponent
    {
        List<EnvironmentComponent> Outside;
    }

    class RoomTile
    {
        static HashSet<string> SolidEdges = new HashSet<string>() {
            "none",
            //"towerleft",
            //"towerright",
            "outside",
        };

        public MapGenerator Generator;
        public int X, Y;
        public RoomType Type;
        public double Weight;
        public Template LockedTemplate;
        public Template SelectedTemplate => LockedTemplate ?? (PossibleTemplates.Count(x => !x.Forbidden) == 1 ? PossibleTemplates.First(x => !x.Forbidden).Template : null);
        public List<PossibleTemplate> PossibleTemplates = new List<PossibleTemplate>();
        public double Entropy => GetEntropy();

        public bool KVisited;
        public KComponent KComponent;

        public int Counter;

        public string ConnectUp => string.Join(", ", GetNeighbor(0, -1).PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Down).Distinct());
        public string ConnectDown => string.Join(", ", GetNeighbor(0, 1).PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Up).Distinct());
        public string ConnectLeft => string.Join(", ", GetNeighbor(-1, 0).PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Left).Distinct());
        public string ConnectRight => string.Join(", ", GetNeighbor(1, 0).PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Right).Distinct());

        public IEnumerable<string> EdgeLeft => X >= Generator.Width ? (Y == Generator.Height - 1 ? new[] { "entrance" } : new[] { "outside" }) : PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Left);
        public IEnumerable<string> EdgeRight => X < 0 ? (Y == Generator.Height - 1 ? new[] { "entrance" } : new[] { "outside" }) : PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Right);
        public IEnumerable<string> EdgeUp => Y >= Generator.Height ? new[] { "none" } : PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Up);
        public IEnumerable<string> EdgeDown => Y < 0 ? new[] { "none", "exit" } : PossibleTemplates.Where(x => !x.Forbidden).Select(x => x.Template.Down);

        public IEnumerable<string> InEdgeLeft => GetNeighbor(-1, 0).EdgeRight;
        public IEnumerable<string> InEdgeRight => GetNeighbor(1, 0).EdgeLeft;
        public IEnumerable<string> InEdgeUp => GetNeighbor(0, -1).EdgeDown;
        public IEnumerable<string> InEdgeDown => GetNeighbor(0, 1).EdgeUp;

        public List<RoomTile> ExtraNeighbors = new List<RoomTile>();

        public List<RoomTile> EnvironmentNext = new List<RoomTile>();
        public List<RoomTile> EnvironmentPrevious = new List<RoomTile>();
        public Object Environment;

        public RoomTile(MapGenerator generator, int x, int y)
        {
            Generator = generator;
            X = x;
            Y = y;
        }

        public void InitWave()
        {
            PossibleTemplates.Clear();

            foreach (Template template in Template.Templates)
            {
                PossibleTemplate possibleTemplate = new PossibleTemplate(template);
                PossibleTemplates.Add(possibleTemplate);
            }

            LockedTemplate = null;
        }

        private void CheckDead(string pass)
        {
            if(Generator.EnumerateTiles().Any(x => x.Entropy<0))
            {
                Console.WriteLine($"Suffocation at {X},{Y}. Pass: {pass}");                
                throw new Exception();
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
            //CheckDead("base");

            if (Type != RoomType.Empty)
            {
                Forbid(template => template.GetConnections() < 2);
            }
            //CheckDead("open path");

            //var topology = Generator.DebugPrint((x) => Generator.GetChar(x.Type));
            switch (Type)
            {
                /*case (RoomType.Entrance):
                    Forbid(Direction.Left, connection => connection != "entrance");
                    Forbid(template => template.TravelDirection == TravelDirection.Down);
                    break;*/
                case (RoomType.Horizontal):
                    Forbid(Direction.Left, connection => SolidEdges.Contains(connection));
                    Forbid(Direction.Right, connection => SolidEdges.Contains(connection));
                    /*if(Generator.Random.NextDouble() > 0.5)
                    {
                        Forbid(Direction.Up, connection => connection != "none");
                        Forbid(Direction.Down, connection => connection != "none");
                    }*/
                    break;
                case (RoomType.HubVertical):
                    Forbid(Direction.Up, connection => SolidEdges.Contains(connection));
                    Forbid(Direction.Down, connection => SolidEdges.Contains(connection));
                    Forbid(template => template.TravelDirection == TravelDirection.Down);
                    break;
                case (RoomType.HubUp):
                    Forbid(Direction.Up, connection => SolidEdges.Contains(connection));
                    Forbid(template => template.TravelDirection == TravelDirection.Down);
                    break;
                case (RoomType.HubDown):
                    Forbid(Direction.Down, connection => SolidEdges.Contains(connection));
                    //Forbid(template => template.TravelDirection == TravelDirection.Down);
                    break;
                case (RoomType.RightUp):
                    Forbid(Direction.Up, connection => SolidEdges.Contains(connection));
                    Forbid(Direction.Right, connection => SolidEdges.Contains(connection));
                    Forbid(template => template.TravelDirection == TravelDirection.Down);
                    break;
                case (RoomType.LeftUp):
                    Forbid(Direction.Left, connection => SolidEdges.Contains(connection));
                    Forbid(Direction.Up, connection => SolidEdges.Contains(connection));
                    Forbid(template => template.TravelDirection == TravelDirection.Down);
                    break;
                case (RoomType.RightDown):
                    Forbid(Direction.Down, connection => SolidEdges.Contains(connection));
                    Forbid(Direction.Right, connection => SolidEdges.Contains(connection));
                    //Forbid(template => template.TravelDirection == TravelDirection.Down);
                    break;
                case (RoomType.LeftDown):
                    Forbid(Direction.Left, connection => SolidEdges.Contains(connection));
                    Forbid(Direction.Down, connection => SolidEdges.Contains(connection));
                    //Forbid(template => template.TravelDirection == TravelDirection.Down);
                    break;
                    /*case (RoomType.Empty):
                        Forbid(template => template.Connections > 0);
                        break;*/
            }
            //CheckDead("special room");
        }

        public void Save()
        {
            foreach(var template in PossibleTemplates)
            {
                template.Save();
            }
        }

        public void Revert()
        {
            LockedTemplate = null;
            foreach (var template in PossibleTemplates)
            {
                template.Revert();
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
            Template previousBest = null;
            if (PossibleTemplates.Count(x => !x.Forbidden) == 1)
                previousBest = PossibleTemplates.First(x => !x.Forbidden).Template;

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
                if (!PossibleTemplates.Any(x => !x.Forbidden))
                    LockedTemplate = previousBest;

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
                weightedList.Add(template.Template, template.Template.Weight);
            }
            var selected = weightedList.GetWeighted(Generator.Random);
            Forbid(temp => temp != selected);
        }

        public double GetEntropy()
        {
            //var freqs = PossibleTemplates.GroupBy(x => x.Forbidden).Select(x => (double)x.Count() / PossibleTemplates.Count);
            //freqs = freqs.Select(x => x * Math.Log(x) / Math.Log(2));
            return PossibleTemplates.Count(x => !x.Forbidden) - 1;
            //return -freqs.Sum();
        }

        public bool InBounds()
        {
            return X >= 0 && Y >= 0 && X <= Generator.Width - 1 && Y <= Generator.Height - 1;
        }

        public RoomTile GetNeighbor(int dx, int dy)
        {
            return Generator.GetRoom(X + dx, Y + dy);
        }

        public IEnumerable<RoomTile> GetAdjacentNeighbors()
        {
            return new[] { GetNeighbor(1, 0), GetNeighbor(0, 1), GetNeighbor(-1, 0), GetNeighbor(0, -1) }.Shuffle();
        }

        public IEnumerable<RoomTile> GetOutNeighbors()
        {
            var template = SelectedTemplate;
            if (!SolidEdges.Contains(template.Up) && template.TravelDirection != TravelDirection.Down)
                yield return GetNeighbor(0, -1);
            if (!SolidEdges.Contains(template.Down))
                yield return GetNeighbor(0, 1);
            if (!SolidEdges.Contains(template.Right))
                yield return GetNeighbor(1, 0);
            if (!SolidEdges.Contains(template.Left))
                yield return GetNeighbor(-1, 0);
            foreach(var neighbor in ExtraNeighbors)
            {
                yield return neighbor;
            }
        }

        public IEnumerable<RoomTile> GetInNeighbors()
        {
            var template = SelectedTemplate;
            if (!SolidEdges.Contains(template.Up))
                yield return GetNeighbor(0, -1);
            if (!SolidEdges.Contains(template.Down) && GetNeighbor(0, 1).SelectedTemplate.TravelDirection != TravelDirection.Down)
                yield return GetNeighbor(0, 1);
            if (!SolidEdges.Contains(template.Right))
                yield return GetNeighbor(1, 0);
            if (!SolidEdges.Contains(template.Left))
                yield return GetNeighbor(-1, 0);
        }

        public IEnumerable<RoomTile> GetEnvironmentNeighbors()
        {
            foreach (var neighbor in EnvironmentNext)
            {
                yield return neighbor;
            }
        }

        public void ConnectEnvironment(RoomTile room)
        {
            if (!EnvironmentNext.Contains(room))
            {
                EnvironmentNext.Add(room);
                room.EnvironmentPrevious.Add(this);
            }
        }

        public void KInit()
        {
            KVisited = false;
            KComponent = null;
        }

        public void KVisit(List<RoomTile> toVisit)
        {
            if(!KVisited && InBounds())
            {
                KVisited = true;
                foreach(var neighbor in GetOutNeighbors())
                {
                    neighbor.KVisit(toVisit);
                }
                toVisit.Insert(0, this);
            }
        }

        public void KAssign(RoomTile root)
        {
            if (KComponent == null && InBounds())
            {
                if (root.KComponent == null)
                    root.KComponent = new KComponent();
                root.KComponent.Add(this);
                foreach (var neighbor in GetInNeighbors())
                {
                    neighbor.KAssign(root);
                }
            }
        }

        public void Connect(RoomTile room)
        {
            if (!ExtraNeighbors.Contains(room))
                ExtraNeighbors.Add(room);
        }

        public override string ToString()
        {
            return $"{SelectedTemplate?.Name ?? "unknown"} (entropy: {Entropy.ToString()})";
        }
    }

    class MapGenerator
    {
        public RoomTile[,] Rooms;
        public Stack<ActionBase> UndoStack;
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

            var templateCoverage = new HashSet<Template>();
            foreach(var tile in tiles.SelectMany(x => x.PossibleTemplates))
            {
                if (!tile.Forbidden)
                    templateCoverage.Add(tile.Template);
            }
            var templateMisses = Template.Templates.Except(templateCoverage);
            foreach(var miss in templateMisses)
            {
                Console.WriteLine($"Template \"{miss.Name}\" is contradictory.");
            }

            if(tiles.Any(x => x.Entropy < 0))
            {
                Console.WriteLine("DOA");
                return false;
            }

            Console.WriteLine("started");
            int i = 0;
            int tilesSinceSave = 0;
            int reverts = 0;
            int saves = 0;
            while (true)
            {
                i++;
                IEnumerable<RoomTile> nonZeroEntropy = tiles.Where(x => x.Entropy > 0).Shuffle();
                if (!nonZeroEntropy.Any())
                {
                    break;
                }
                //var tile = nonZeroEntropy.WithMin(x => x.Entropy);
                var tile = nonZeroEntropy.WithMax(x => x.Type != RoomType.Empty);
                //var tile = nonZeroEntropy.First();
                //var entropies = DebugPrint((x) => (char)('1' + (int)x.Entropy));

                tile.Collapse();
                tile.Counter = i;
                tilesSinceSave++;

                var dead = tiles.Where(x => x.Entropy < 0);
                if (dead.Any())
                {
                    reverts++;
                    tilesSinceSave = 0;
                    if (reverts > 100)
                    {
                        Console.WriteLine($"extinct {i} iterations at {tile.X},{tile.Y}");
                        return false;
                    }
                    Console.WriteLine($"Revert #{reverts}");
                    Revert();
                }
                else if(tilesSinceSave > 10)
                {
                    saves++;
                    Console.WriteLine($"Save #{saves}");
                    Save();
                    tilesSinceSave = 0;
                }
            }

            var fillCount = tiles.Where(x => x.Entropy == 0).Count();

            return !tiles.Any(x => x.Entropy < 0);
        }

        public void Save()
        {
            foreach (var tile in EnumerateTiles())
                tile.Save();
        }

        public void Revert()
        {
            foreach (var tile in EnumerateTiles())
                tile.Revert();
        }

        public void KConnectivity()
        {
            foreach(var tile in EnumerateTiles())
            {
                tile.KInit();
            }

            var toVisit = new List<RoomTile>();

            foreach (var tile in EnumerateTiles())
            {
                tile.KVisit(toVisit);
            }

            foreach (var visit in toVisit)
            {
                visit.KAssign(visit);
            }

            var components = EnumerateTiles().Select(tile => tile.KComponent).Distinct().ToList();

            foreach (var component in components)
            {
                component.Qualify();
            }
        }

        public bool HasEnvironmentPath(RoomTile start, RoomTile end)
        {
            HashSet<RoomTile> visited = new HashSet<RoomTile>();
            Queue<RoomTile> toVisit = new Queue<RoomTile>();

            visited.Add(start);
            toVisit.Enqueue(start);

            while (toVisit.Count > 0)
            {
                var visit = toVisit.Dequeue();

                if (visit == end)
                    return true;

                foreach (var neighbor in visit.GetEnvironmentNeighbors())
                {
                    if (neighbor.InBounds() && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        toVisit.Enqueue(neighbor);
                    }
                }
            }

            return false;
        }

        public void CreateEnvironmentTree(RoomTile start)
        {
            BuildEnvironmentTree(start);

            var orphans = EnumerateTiles().Where(x => x.EnvironmentPrevious.Empty()).ToList();
            foreach(var orphan in orphans)
            {
                if (orphan.EnvironmentPrevious.Any())
                    continue; //Not an orphan anymore

                BuildEnvironmentTree(orphan);
                orphan.GetNeighbor(0, -1).ConnectEnvironment(orphan);
            }
        }

        public void PaintEnvironment(RoomTile start, Object environment)
        {
            HashSet<RoomTile> visited = new HashSet<RoomTile>();
            Queue<RoomTile> toVisit = new Queue<RoomTile>();

            visited.Add(start);
            toVisit.Enqueue(start);

            while (toVisit.Count > 0)
            {
                var visit = toVisit.Dequeue();

                if (visit.Environment == null)
                    visit.Environment = environment;
                else
                    break;

                foreach (var neighbor in visit.GetEnvironmentNeighbors())
                {
                    if (neighbor.InBounds() && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        toVisit.Enqueue(neighbor);
                    }
                }
            }
        }

        public void BuildEnvironmentTree(RoomTile start)
        {
            HashSet<RoomTile> visited = new HashSet<RoomTile>();
            Queue<RoomTile> toVisit = new Queue<RoomTile>();

            visited.Add(start);
            toVisit.Enqueue(start);

            while(toVisit.Count > 0)
            {
                var visit = toVisit.Dequeue();

                foreach(var neighbor in visit.GetOutNeighbors())
                {
                    if (neighbor.InBounds() && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        toVisit.Enqueue(neighbor);

                        if (!HasEnvironmentPath(visit, neighbor))
                            visit.ConnectEnvironment(neighbor);
                    }
                }
            }
        }

        public IEnumerable<RoomTile> GetEnvironmentEnds()
        {
            return EnumerateTiles().Where(x => !x.EnvironmentNext.Any());
        }

        public void Generate()
        {
            Random = new Random();

            Rooms = new RoomTile[Width, Height];
            UndoStack = new Stack<ActionBase>();

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

            var dijkstra = Util.Dijkstra(new Point(0, Height - 1), Width, Height, double.PositiveInfinity, GetMainWeight, (pos) => GetRoom(pos.X, pos.Y).GetAdjacentNeighbors().Select(room => new Point(room.X, room.Y)));

            Console.WriteLine("Generate Path");
            var path = dijkstra.FindPath(new Point(Random.Next(Width), 0)).ToList();

            var lastPos = new Point(0, Height - 1);
            var lastHorizontal = 1;
            var lastVertical = 0;
            GetRoom(lastPos.X, lastPos.Y).Type = RoomType.Entrance;
            foreach (var pos in path)
            {
                var lastRoom = GetRoom(lastPos.X, lastPos.Y);
                var dx = pos.X - lastPos.X;
                var dy = pos.Y - lastPos.Y;
                /*if(lastRoom.Type == RoomType.Entrance)
                {
                    //NOOP
                }
                else*/ if (dx == 1)
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
            
            var topology = DebugPrint((x) => GetChar(x.Type));
            Console.WriteLine(topology);

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


            var entropies = DebugPrint((x) => (char)('1' + (int)x.Entropy));

            if (EnumerateTiles().Any(x => x.Entropy < 0))
            {

            }

            Finished = WaveformCollapse();

            var stats = EnumerateTiles().GroupBy(x => x.SelectedTemplate);
            var statsString = string.Join("\n", stats.Select(x => $"{x.Key}: {x.Count()}").OrderBy(x => x));

            Console.WriteLine($"Statistics:\n\n{statsString}");

            if (Finished)
            {
                KConnectivity();
                var components = EnumerateTiles().Select(x => x.KComponent).Distinct().ToList();

                foreach(var component in components)
                {
                    component.Color = new Color(255, 128, 128).RotateHue(Random.NextDouble());
                }
            }
        }

        [Flags]
        enum ConnectionResult
        {
            None = 0,
            ConnectIn = 1,
            ConnectOut = 2,
            ConnectInOut = 3,
        }

        struct Connector
        {
            public ConnectionResult Result;
            Func<KComponent, ConnectionResult> Function;

            public Connector(ConnectionResult result, Func<KComponent, ConnectionResult> function)
            {
                Result = result;
                Function = function;
            }

            public ConnectionResult Run(KComponent component)
            {
                return Function(component);
            }
        }

        public void Build(Map map)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    /*var rand = Random.NextDouble();
                    var rand2 = Random.NextDouble();
                    if ((x > 10 && x <= map.Width - 10) || (x > 8 && x <= map.Width - 8 && rand2 <= 0.7))
                    {
                        map.Background[x, y] = TileBG.Brick;
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
                    else */if (y >= map.Height - 1)
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
                    Color counterColor = room.Counter > 0 ? Color.Red.RotateHue(room.Counter * 0.01) : Color.Gray;
                    Color color = room.KComponent?.Color ?? counterColor;
                    switch (room.Type)
                    {
                        case (RoomType.Horizontal):
                            color = Color.White;
                            BuildHorizontal(map, origin + x * 8, y * 8);
                            break;
                        case (RoomType.HubVertical):
                            color = Color.White;
                            BuildShaft(map, origin + x * 8, y * 8);
                            break;
                        case (RoomType.LeftDown):
                        case (RoomType.RightDown):
                        case (RoomType.HubDown):
                            color = Color.White;
                            BuildHubDown(map, origin + x * 8, y * 8);
                            break;
                        case (RoomType.LeftUp):
                        case (RoomType.RightUp):
                        case (RoomType.HubUp):
                            color = Color.White;
                            BuildHubUp(map, origin + x * 8, y * 8);
                            break;
                    }
                    if (room.SelectedTemplate != null)
                        BuildRoom(map, origin + x * 8, y * 8, room, color);
                }
            }

            BuildEnvironmentTree(Rooms[0, Height - 1]);

            while (map.SubTemplates.Count > 0)
            {
                var first = map.SubTemplates.Dequeue();
                BuildTemplate(map, first.X, first.Y, first.Pick(Random), Color.White);
            }

            while (map.Puzzles.Any())
            {
                var end = map.Puzzles.WithMin(x => x.Outputs.Count);
                map.Puzzles.Remove(end);
                foreach (var input in end.Inputs)
                {
                    if (!map.Puzzles.Any(puzzle => puzzle.Outputs.Any()))
                        break;
                    var start = map.Puzzles.WithMin(puzzle => puzzle.Outputs.Any() ? puzzle.Outputs.Min(node => (node.Position - input.Position).LengthSquared()) : float.PositiveInfinity);
                    var output = start.Outputs.WithMin(node => (node.Position - input.Position).LengthSquared());
                    start.Outputs.Remove(output);
                    new Wire(map.World, output, input);
                }
            }

            foreach(var connection in map.WireConnections)
            {
                var nodeStart = map.WireNodes[connection.Start.X, connection.Start.Y];
                var nodeEnd = map.WireNodes[connection.End.X, connection.End.Y];

                if(nodeStart != null && nodeEnd != null)
                    new Wire(map.World, nodeStart, nodeEnd);
            }

            var floors = map.EnumerateTiles().Where(x => x is EmptySpace && IsFloor(x.GetNeighbor(0, 1))).ToList();
            var floorCheck = floors.ToHashSet();
            
            var entries = new List<Tile>();
            var exits = new List<Tile>();
            var teleports = new List<Tile>();

            foreach (var tile in map.EnumerateTiles())
            {
                switch (tile.ConnectFlag)
                {
                    case (FlagConnect.Any):
                        if(IsWall(tile) && (floorCheck.Contains(tile.GetNeighbor(1, 0)) || floorCheck.Contains(tile.GetNeighbor(-1, 0))))
                        {
                            entries.Add(tile);
                            exits.Add(tile);
                        }
                        if(tile is Wall wall && wall.Top && floorCheck.Contains(tile.GetNeighbor(0,-1)))
                        {
                            teleports.Add(tile);
                        }
                        break;
                    case (FlagConnect.In):
                        entries.Add(tile);
                        break;
                    case (FlagConnect.Out):
                        exits.Add(tile);
                        break;
                    case (FlagConnect.InOut):
                        entries.Add(tile);
                        exits.Add(tile);
                        break;
                    case (FlagConnect.Teleport):
                        teleports.Add(tile);
                        break;
                }
            }

            var components = floors.Where(x => x.Room != null).Select(x => x.Room.KComponent).Distinct().ToList();
            var entryGroups = entries.ToLookup(x => x.Room?.KComponent);
            var exitGroups = exits.ToLookup(x => x.Room?.KComponent);
            var teleportGroups = teleports.ToLookup(x => x.Room?.KComponent);

            var mainGroup = components.WithMax(x => x.Tiles.Count);
            var mainComponents = new HashSet<KComponent>() { mainGroup };
            components.Remove(mainGroup);

            Func<KComponent, ConnectionResult> ConnectPathIn = (component) =>
            {
                IEnumerable<Tile> pathEntries = entries.Where(entry => mainComponents.Contains(entry.Room?.KComponent)).ToList();
                IEnumerable<Tile> pathExits = exitGroups[component];
                var connected = ConnectPath(map, pathEntries, pathExits);
                if (connected == PathResult.TwoWay)
                    return ConnectionResult.ConnectInOut;
                else if(connected == PathResult.OneWay)
                    return ConnectionResult.ConnectIn;
                else
                    return ConnectionResult.None;
            };
            Func<KComponent, ConnectionResult> ConnectPathOut = (component) =>
            {
                IEnumerable<Tile> pathEntries = entryGroups[component];
                IEnumerable<Tile> pathExits = exits.Where(exit => mainComponents.Contains(exit.Room?.KComponent)).ToList();
                var connected = ConnectPath(map, pathEntries, pathExits);
                if (connected == PathResult.TwoWay)
                    return ConnectionResult.ConnectInOut;
                else if (connected == PathResult.OneWay)
                    return ConnectionResult.ConnectOut;
                else
                    return ConnectionResult.None;
            };
            Func<KComponent, ConnectionResult> ConnectTeleportIn = (component) =>
            {
                IEnumerable<Tile> teleportEntries = teleports.Where(teleport => mainComponents.Contains(teleport.Room?.KComponent)).ToList();
                IEnumerable<Tile> teleportExits = teleportGroups[component];
                var connected = ConnectTeleport(map, teleportEntries, teleportExits, ConnectionResult.ConnectIn);
                return connected ? ConnectionResult.ConnectIn : ConnectionResult.None;
            };
            Func<KComponent, ConnectionResult> ConnectTeleportOut = (component) =>
            {
                IEnumerable<Tile> teleportEntries = teleportGroups[component];
                IEnumerable<Tile> teleportExits = teleports.Where(teleport => mainComponents.Contains(teleport.Room?.KComponent)).ToList();
                var connected = ConnectTeleport(map, teleportEntries, teleportExits, ConnectionResult.ConnectOut);
                return connected ? ConnectionResult.ConnectOut : ConnectionResult.None;
            };
            Func<KComponent, ConnectionResult> ConnectTeleportIO = (component) =>
            {
                IEnumerable<Tile> teleportEntries = teleports.Where(teleport => mainComponents.Contains(teleport.Room?.KComponent)).ToList();
                IEnumerable<Tile> teleportExits = teleportGroups[component];
                var connected = ConnectTeleport(map, teleportEntries, teleportExits, ConnectionResult.ConnectInOut);
                return connected ? ConnectionResult.ConnectInOut : ConnectionResult.None;
            };

            while (components.Any())
            {
                var component = components.First();

                WeightedList<Connector> possibleConnections = new WeightedList<Connector>();
                ConnectionResult wantedConnection = ConnectionResult.None;
              
                switch(component.Type)
                {
                    case (KComponentType.Vault):
                        wantedConnection = ConnectionResult.ConnectInOut;
                        break;
                    case (KComponentType.Sink):
                        wantedConnection = ConnectionResult.ConnectOut;
                        break;
                    case (KComponentType.Source):
                        wantedConnection = ConnectionResult.ConnectIn;
                        break;
                }

                if (wantedConnection.HasFlag(ConnectionResult.ConnectIn))
                {
                    possibleConnections.Add(new Connector(ConnectionResult.ConnectIn, ConnectPathIn), 100);
                    possibleConnections.Add(new Connector(ConnectionResult.ConnectIn, ConnectTeleportIn), 100);
                }
                if (wantedConnection.HasFlag(ConnectionResult.ConnectOut))
                {
                    possibleConnections.Add(new Connector(ConnectionResult.ConnectOut, ConnectPathOut), 100);
                    possibleConnections.Add(new Connector(ConnectionResult.ConnectOut, ConnectTeleportOut), 100);
                }
                if (wantedConnection.HasFlag(ConnectionResult.ConnectInOut))
                {
                    possibleConnections.Add(new Connector(ConnectionResult.ConnectInOut, ConnectTeleportIO), 100);
                }

                while (wantedConnection != ConnectionResult.None && possibleConnections.Count > 0)
                {
                    var connect = possibleConnections.GetWeighted(Random);
                    if(wantedConnection.HasFlag(connect.Result))
                        wantedConnection &= ~connect.Run(component);
                    possibleConnections.Remove(connect);
                }

                /*if(connectIn)
                {
                    IEnumerable<Tile> pathEntries = entries.Where(entry => mainComponents.Contains(entry.Room?.KComponent)).ToList();
                    IEnumerable<Tile> pathExits = exitGroups[component];
                    var connected = ConnectPath(map, pathEntries, pathExits);
                    if (connected == null)
                    {
                        IEnumerable<Tile> teleportEntries = teleports.Where(teleport => mainComponents.Contains(teleport.Room?.KComponent)).ToList();
                        IEnumerable<Tile> teleportExits = teleportGroups[component];
                        ConnectTeleport(map, teleportEntries, teleportExits);
                    }
                }
                if(connectOut)
                {
                    IEnumerable<Tile> pathEntries = entryGroups[component];
                    IEnumerable<Tile> pathExits = exits.Where(exit => mainComponents.Contains(exit.Room?.KComponent)).ToList();
                    var connected = ConnectPath(map, pathEntries, pathExits);
                    if (connected == null)
                    {
                        IEnumerable<Tile> teleportEntries = teleportGroups[component];
                        IEnumerable<Tile> teleportExits = teleports.Where(teleport => mainComponents.Contains(teleport.Room?.KComponent)).ToList();
                        ConnectTeleport(map, teleportEntries, teleportExits);
                    }
                }*/
                components.Remove(component);
                mainComponents.Add(component);
            }

            foreach(var roomTile in Rooms)
            {
                roomTile.PossibleTemplates.Clear();
            }
        }

        private bool IsFloor(Tile tile)
        {
            return tile is Wall && !(tile is Spike);
        }

        private bool IsWall(Tile tile)
        {
            return tile is Wall;
        }

        private bool ConnectTeleport(Map map, IEnumerable<Tile> entries, IEnumerable<Tile> exits, ConnectionResult connection)
        {
            if (entries.Any() && exits.Any())
            {
                Tile entry, exit;
                if(connection == ConnectionResult.ConnectOut)
                {
                    entry = entries.Shuffle().First();
                    exit = exits.WithMin(tile => SquaredDistance(entry.X, entry.Y, tile.X, tile.Y));
                }
                else
                {
                    exit = exits.Shuffle().First();
                    entry = entries.WithMin(tile => SquaredDistance(exit.X, exit.Y, tile.X, tile.Y));
                }

                if (exit != null)
                {
                    map.Tiles[entry.X, entry.Y] = new TeleportTrapLinked(map, entry.X, entry.Y)
                    {
                        LinkX = exit.X,
                        LinkY = exit.Y,
                    };
                    if (connection == ConnectionResult.ConnectInOut)
                        map.Tiles[exit.X, exit.Y] = new TeleportTrapLinked(map, exit.X, exit.Y)
                        {
                            LinkX = entry.X,
                            LinkY = entry.Y,
                        };
                    else
                        map.Tiles[exit.X, exit.Y] = new BumpTrap(map, exit.X, exit.Y);
                    return true;
                }
            }

            return false; //Bad times
        }

        private int SquaredDistance(int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            return dx * dx + dy * dy;
        }

        enum PathResult
        {
            None,
            OneWay,
            TwoWay,
        }

        private PathResult ConnectPath(Map map, IEnumerable<Tile> entries, IEnumerable<Tile> exits)
        {
            var wallCheck = map.EnumerateTiles().Where(x => IsWall(x) && x.GetFullNeighbors().All(y => IsWall(y))).ToHashSet();
            var dijkstra = Util.Dijkstra(entries.Select(entry => new Point(entry.X,entry.Y)).ToHashSet(), map.Width, map.Height, double.PositiveInfinity, (a, b) => {
                var tile = map.Tiles[b.X, b.Y];
                switch (tile.ConnectFlag)
                {
                    default:
                    case (FlagConnect.Any):
                        if (wallCheck.Contains(tile))
                            return 1;
                        else
                            return double.PositiveInfinity;
                    case (FlagConnect.Blocked):
                        return double.PositiveInfinity;
                    case (FlagConnect.Fast):
                        return 0.01;
                    case (FlagConnect.Out):
                    case (FlagConnect.InOut):
                        return 100;
                }
            }, (a) => {
                return map.Tiles[a.X, a.Y].GetDownNeighbors().Select(neighbor => new Point(neighbor.X, neighbor.Y));
            });
            var ends = dijkstra.FindEnds(p => exits.Contains(map.Tiles[p.X, p.Y]));
            if (ends.Any())
            {
                foreach (var end in ends.OrderBy(p => dijkstra[p.X, p.Y].Distance))
                {
                    var component = map.Tiles[end.X, end.Y].Room?.KComponent;
                    IEnumerable<Point> path = dijkstra.FindPath(end).ToList();
                    var start = dijkstra.FindStart(end);
                    //if (!CheckPath(start, path))
                    //    continue;
                    //map.Tiles[start.X, start.Y] = new EmptySpace(map, start.X, start.Y);
                    map.Tiles[start.X, start.Y].Mechanism = new ChainDestroyStart();
                    foreach (var point in path)
                    {
                        //map.Tiles[point.X, point.Y] = new EmptySpace(map, point.X, point.Y);
                        map.Tiles[point.X, point.Y].Mechanism = new ChainDestroy();
                    }
                    if(Math.Abs(start.Y - end.Y) < 1)
                    {
                        map.Tiles[end.X, end.Y].Mechanism = new ChainDestroyStart();
                        return PathResult.TwoWay;
                    }
                    else 
                        return PathResult.OneWay;
                }
            }
            return PathResult.None;
        }

        private bool CheckPath(Point start, IEnumerable<Point> path)
        {
            var previous = start;
            int height = 0;
            int maxHeight = 0;
            foreach(var next in path)
            {
                int dy = next.Y - previous.Y;
                if (dy < 0)
                    height++;
                else
                {
                    if (height > maxHeight)
                        maxHeight = height;
                    height = 0;
                }
                previous = next;
            }

            return maxHeight <= 2;
        }

        private void BuildRoom(Map map, int px, int py, RoomTile room, Color color)
        {
            var template = room.SelectedTemplate;
            BuildTemplate(map, px, py, template, color);
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    map.Tiles[px + x, py + y].Room = room;
                }
            }
        }

        private void BuildTemplate(Map map, int px, int py, Template template, Color color)
        {
            for (int x = 0; x < template.Width; x++)
            {
                for (int y = 0; y < template.Height; y++)
                {
                    template.PrintForeground(template.Foreground[x, y], map, px + x, py + y, Random);
                    template.PrintBackground(template.Background[x, y], map, px + x, py + y, Random);
                    template.PrintConnectFlag(template.GetConnectFlag(x, y), map, px + x, py + y);
                    template.PrintMechanism(template.GetMechanismFlag(x, y), map, px + x, py + y);

                    map.Tiles[px + x, py + y].Color = color;
                }
            }

            foreach (var entity in template.Entities)
                template.PrintEntity(entity, map, px, py);

            PuzzleNode puzzle = new PuzzleNode();
            foreach (var mechanism in template.EntitiesMechanism)
                template.PrintMechanism(mechanism, map, px, py, puzzle);

            if(puzzle.Inputs.Any() || puzzle.Outputs.Any())
            {
                map.Puzzles.Add(puzzle);
            }
        }

        private TileBG GetBackground(int id)
        {
            WeightedList<TileBG> RandomBricks = new WeightedList<TileBG>();
            RandomBricks.Add(TileBG.Brick, 80);
            RandomBricks.Add(TileBG.BrickMiss1, 30);
            RandomBricks.Add(TileBG.BrickMiss2, 15);

            switch (id)
            {
                default:
                    return TileBG.Empty;
                case (0):
                    return TileBG.Brick;
                case (1):
                    return TileBG.Tile4;
                case (2):
                    return TileBG.TileDetail;
                case (3):
                    return TileBG.Statue;
                case (4):
                    return TileBG.BrickMiss1;
                case (5):
                    return TileBG.BrickMiss2;
                case (6):
                    return TileBG.BrickOpening;
                case (7):
                    return RandomBricks.GetWeighted(Random);
                case (8):
                    return TileBG.RailLeft;
                case (9):
                    return TileBG.RailMiddle;
                case (10):
                    return TileBG.RailRight;
                case (11):
                    return TileBG.PillarTop;
                case (12):
                    return TileBG.BrickPlatform;
                case (13):
                    return TileBG.Black;
                case (14):
                    return TileBG.BrickHole;
                case (16):
                    return TileBG.Window;
                case (17):
                    return TileBG.WindowBigLeft;
                case (18):
                    return TileBG.WindowBigRight;
                case (19):
                    return TileBG.PillarDetail;
                case (27):
                    return TileBG.Pillar;
                case (35):
                    return TileBG.PillarBottomBroken;
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

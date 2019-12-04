using Microsoft.Xna.Framework;
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
    }

    class RoomTile
    {
        public MapGenerator Generator;
        public int X, Y;
        public RoomType Type;
        public double Weight;

        public RoomTile(MapGenerator generator, int x, int y)
        {
            Generator = generator;
            X = x;
            Y = y;
        }

        public RoomTile GetNeighbor(int dx, int dy)
        {
            return Generator.GetRoom(X + dx, Y + dy);
        }

        public IEnumerable<RoomTile> GetAdjacentNeighbors()
        {
            return new[] { GetNeighbor(1, 0), GetNeighbor(0, 1), GetNeighbor(-1, 0), GetNeighbor(0, -1) }.Shuffle();
        }
    }

    class MapGenerator
    {
        public RoomTile[,] Rooms;
        public Stack<Action> UndoStack;
        public int Width, Height;
        public bool Finished;

        Random Random;

        public MapGenerator(int width, int height)
        {
            Width = width;
            Height = height;
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

            for(int i = 0; i < 5; i++)
            {
                int height = Random.Next(3) + 4;
                int x1 = 1 + Random.Next(Width - 2);
                int x2 = 1 + Random.Next(Width - 2);
                int y = Random.Next(Height - height);

                for(int e = 0; e < Width; e++)
                {
                    for (int g = 0; g < height; g++)
                    {
                        RoomTile room = GetRoom(e, y+g);
                        if((e <= Math.Max(x1,x2) && g == 0) || (e >= Math.Min(x1, x2) && g == height - 1))
                        {
                            room.Weight = 10000;
                        }
                        else if ((e == Math.Max(x1, x2) && g < height-2) || (e == Math.Min(x1, x2) && g > 1))
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

            var path = dijkstra.FindPath(new Point(Random.Next(Width), 0)).ToList();

            var lastPos = new Point(0, Height - 1);
            GetRoom(lastPos.X, lastPos.Y).Type = RoomType.Horizontal;
            foreach (var pos in path)
            {
                var dx = pos.X - lastPos.X;
                var dy = pos.Y - lastPos.Y;
                if (dy == 0)
                    GetRoom(pos.X, pos.Y).Type = RoomType.Horizontal;
                else if(dy == -1)
                {
                    var lastRoom = GetRoom(lastPos.X, lastPos.Y);
                    if (lastRoom.Type == RoomType.Horizontal)
                        lastRoom.Type = RoomType.HubUp;
                    else if(lastRoom.Type == RoomType.HubDown)
                        lastRoom.Type = RoomType.HubVertical;
                    GetRoom(pos.X, pos.Y).Type = RoomType.HubDown;
                }
                else if (dy == 1)
                {
                    var lastRoom = GetRoom(lastPos.X, lastPos.Y);
                    if (lastRoom.Type == RoomType.Horizontal)
                        lastRoom.Type = RoomType.HubDown;
                    else if (lastRoom.Type == RoomType.HubUp)
                        lastRoom.Type = RoomType.HubVertical;
                    GetRoom(pos.X, pos.Y).Type = RoomType.HubUp;
                }
                lastPos = pos;
            }

            Finished = true;

            var debug = DebugPrint();
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
                    switch(room.Type)
                    {
                        case (RoomType.Horizontal):
                            BuildHorizontal(map, origin + x * 8, y * 8);
                            break;
                        case (RoomType.HubVertical):
                            BuildShaft(map, origin + x * 8, y * 8);
                            break;
                        case (RoomType.HubDown):
                            BuildHubDown(map, origin + x * 8, y * 8);
                            break;
                        case (RoomType.HubUp):
                            BuildHubUp(map, origin + x * 8, y * 8);
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
                    if(y == 0 || y == 8-1)
                        map.Tiles[px + x, py + y] = new Wall(map, px + x, py + y);
                    else
                        map.Tiles[px + x, py + y] = new EmptySpace(map, px + x, py + y);
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
                        map.Tiles[px + x, py + y] = new EmptySpace(map, px + x, py + y);
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
                        map.Tiles[px + x, py + y] = new EmptySpace(map, px + x, py + y);
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
                        map.Tiles[px + x, py + y] = new EmptySpace(map, px + x, py + y);
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
                return new RoomTile(this,x,y);
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

        public string DebugPrint()
        {
            string s = "";
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    s += GetChar(Rooms[x,y].Type);
                }
                s += "\n";
            }
            return s;
        }

        private char GetChar(RoomType room)
        {
            switch (room)
            {
                default:
                    return ' ';
                case (RoomType.Horizontal):
                    return '-';
                case (RoomType.HubUp):
                    return 'V';
                case (RoomType.HubDown):
                    return 'T';
                case (RoomType.HubVertical):
                    return '|';
                case (RoomType.Blocked):
                    return 'X';
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

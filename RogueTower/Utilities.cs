using FibonacciHeap;
using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    class DijkstraTile
    {
        public Point Tile;
        public double Distance;
        public double MoveDistance;
        public DijkstraTile Previous;

        public DijkstraTile(Point tile, double dist, double moveDist)
        {
            Tile = tile;
            Distance = dist;
            MoveDistance = moveDist;
        }

        public override string ToString()
        {
            return $"{Tile.ToString()} ({Distance})";
        }
    }
    
    static class Util
    {
        public static Color RotateHue(this Color color, double amount)
        {
            double U = Math.Cos(amount * Math.PI * 2);
            double W = Math.Sin(amount * Math.PI * 2);

            double r = (.299 + .701 * U + .168 * W) * color.R
                        + (.587 - .587 * U + .330 * W) * color.G
                        + (.114 - .114 * U - .497 * W) * color.B;
            double g = (.299 - .299 * U - .328 * W) * color.R
                        + (.587 + .413 * U + .035 * W) * color.G
                        + (.114 - .114 * U + .292 * W) * color.B;
            double b = (.299 - .3 * U + 1.25 * W) * color.R
                        + (.587 - .588 * U - 1.05 * W) * color.G
                        + (.114 + .886 * U - .203 * W) * color.B;

            return new Color((int)r, (int)g, (int)b, color.A);
        }

        public static Vector2 GetRandomPosition(RectangleF area, Random random)
        {
            return new Vector2(area.X + random.NextFloat() * area.Width, area.Y + random.NextFloat() * area.Height);
        }

        public static float GetAngleDistance(float a0, float a1)
        {
            var max = Math.PI * 2;
            var da = (a1 - a0) % max;
            return (float)(2 * da % max - da);
        }

        public static float AngleLerp(float a0, float a1, float t)
        {
            return a0 + GetAngleDistance(a0, a1) * t;
        }

        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle), (float)-Math.Cos(angle));
        }

        public static float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.X, -vector.Y);
        } 

        private static T PickInternal<T>(List<T> enumerable, Random random, bool remove)
        {
            int select = random.Next(enumerable.Count());
            T pick = enumerable[select];
            if(remove)
                enumerable.RemoveAt(select);
            return pick;
        }

        public static T Pick<T>(this List<T> enumerable,Random random)
        {
            return PickInternal(enumerable, random, false);
        }

        public static T PickAndRemove<T>(this List<T> enumerable, Random random)
        {
            return PickInternal(enumerable, random, true);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }

        public static Point GetOffset(this Direction dir)
        {
            switch(dir)
            {
                default:
                    return Point.Zero;
                case (Direction.Up):
                    return new Point(0, -1);
                case (Direction.Down):
                    return new Point(0, 1);
                case (Direction.Left):
                    return new Point(-1, 0);
                case (Direction.Right):
                    return new Point(1, 0);
            }
        }

        public static T WithMin<T,V>(this IEnumerable<T> enumerable, Func<T,V> selector) where V : IComparable
        {
            return enumerable.Aggregate((i1, i2) => selector(i1).CompareTo(selector(i2)) < 0 ? i1 : i2);
        }

        public static T WithMax<T, V>(this IEnumerable<T> enumerable, Func<T, V> selector) where V : IComparable
        {
            return enumerable.Aggregate((i1, i2) => selector(i1).CompareTo(selector(i2)) > 0 ? i1 : i2);
        }

        public static bool Empty<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }

        public static DijkstraTile[,] Dijkstra(Point start, int width, int height, double maxDist, Func<Point, Point, double> length, Func<Point, IEnumerable<Point>> neighbors)
        {
            return Dijkstra(new[] { start }, width, height, maxDist, length, neighbors);
        }

        public static DijkstraTile[,] Dijkstra(IEnumerable<Point> start, int width, int height, double maxDist, Func<Point, Point, double> length, Func<Point, IEnumerable<Point>> neighbors)
        {
            var dijkstraMap = new DijkstraTile[width, height];
            var nodeMap = new FibonacciHeapNode<DijkstraTile,double>[width, height];
            var heap = new FibonacciHeap<DijkstraTile, double>(0);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Point tile = new Point(x, y);
                    bool isStart = start.Contains(tile);
                    DijkstraTile dTile = new DijkstraTile(tile, isStart ? 0 : double.PositiveInfinity, isStart ? 0 : double.PositiveInfinity);
                    var node = new FibonacciHeapNode<DijkstraTile, double>(dTile, dTile.Distance);
                    dijkstraMap[x, y] = dTile;
                    nodeMap[x, y] = node;
                    heap.Insert(node);
                }
            }

            while (!heap.IsEmpty())
            {
                var node = heap.RemoveMin();
                var dTile = node.Data;

                if (dTile.Distance >= maxDist)
                    break;

                foreach (var neighbor in neighbors(dTile.Tile))
                {
                    if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
                        continue;
                    var nodeNeighbor = nodeMap[neighbor.X, neighbor.Y];
                    var dNeighbor = nodeNeighbor.Data;
                    double newDist = dTile.Distance + length(dTile.Tile, dNeighbor.Tile);

                    if (newDist < dNeighbor.Distance)
                    {
                        dNeighbor.Distance = newDist;
                        dNeighbor.Previous = dTile;
                        dNeighbor.MoveDistance = dTile.MoveDistance + 1;
                        heap.DecreaseKey(nodeNeighbor, dNeighbor.Distance);
                    }
                }
            }

            return dijkstraMap;
        }

        public static IEnumerable<Point> FindEnds(this DijkstraTile[,] dijkstra, Func<Point,bool> predicate)
        {
            for(int x = 0; x < dijkstra.GetLength(0); x++)
            {
                for (int y = 0; y < dijkstra.GetLength(1); y++)
                {
                    var dTile = dijkstra[x, y];
                    if (!double.IsInfinity(dTile.Distance) && predicate(dTile.Tile))
                        yield return dTile.Tile;
                }
            }
        }

        private static IEnumerable<Point> FindPathInternal(DijkstraTile[,] dijkstra, Point end)
        {
            DijkstraTile dTile = dijkstra[end.X, end.Y];

            while (dTile.Previous != null)
            {
                yield return dTile.Tile;
                dTile = dTile.Previous;
            }
        }

        public static Point FindStart(this DijkstraTile[,] dijkstra, Point end)
        {
            DijkstraTile dTile = dijkstra[end.X, end.Y];

            while (dTile.Previous != null)
            {
                dTile = dTile.Previous;
            }

            return dTile.Tile;
        }

        public static IEnumerable<Point> FindPath(this DijkstraTile[,] dijkstra, Point end)
        {
            return FindPathInternal(dijkstra, end).Reverse();
        }

        public static double GetMove(this DijkstraTile[,] dijkstra, Tile end)
        {
            return dijkstra[end.X, end.Y].MoveDistance;
        }

        public static double GetCost(this DijkstraTile[,] dijkstra, Tile end)
        {
            return dijkstra[end.X, end.Y].Distance;
        }

        public static bool Reachable(this DijkstraTile[,] dijkstra, Tile end)
        {
            DijkstraTile dTile = dijkstra[end.X, end.Y];
            return dTile.Previous != null;
        }

        public static void Message(Enemy enemy, Message message)
        {
            if(enemy is Player player)
            {
                player.History.Add(message);
            }
        }

        public static bool Parry(IParryGiver giver, IParryReceiver receiver, RectangleF box)
        {
            if (giver != receiver && receiver.CanParry)
            {
                giver.ParryGive(receiver, box);
                receiver.ParryReceive(giver, box);
                return true;
            }

            return false;
        }

        public static string EnglishJoin(string seperator, string finalSeperator, IEnumerable<string> values)
        {
            values = values.ToList();
            var first = values.Take(values.Count() - 1);
            var last = values.Last();
            if (!first.Any())
                return last;
            else
                return $"{String.Join(seperator,first)}{finalSeperator}{last}";
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> toShuffle)
        {
            List<T> shuffled = new List<T>();
            Random random = new Random();
            foreach (T value in toShuffle)
            {
                shuffled.Insert(random.Next(shuffled.Count + 1), value);
            }
            return shuffled;
        }

        public static Vector2 Mirror(this Vector2 vector, SpriteEffects mirror)
        {
            if (mirror.HasFlag(SpriteEffects.FlipHorizontally))
                vector.X *= -1;
            if (mirror.HasFlag(SpriteEffects.FlipVertically))
                vector.Y *= -1;

            return vector;
        }

        public static HorizontalFacing Mirror(this HorizontalFacing facing)
        {
            switch(facing)
            {
                default:
                case HorizontalFacing.Left:
                    return HorizontalFacing.Right;
                case HorizontalFacing.Right:
                    return HorizontalFacing.Left;
            }
        }

        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }

        public static int PositiveMod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        public static int FloorDiv(int x, int m)
        {
            if (((x < 0) ^ (m < 0)) && (x % m != 0))
            {
                return (x / m - 1);
            }
            else
            {
                return (x / m);
            }
        }

        public static float MirrorAngle(float angle)
        {
            return MathHelper.Pi - angle;
        }

        public static Vector2 GetFacingVector(HorizontalFacing facing)
        {
            switch (facing)
            {
                default:
                    return Vector2.Zero;
                case HorizontalFacing.Left:
                    return new Vector2(-1, 0);
                case HorizontalFacing.Right:
                    return new Vector2(1, 0);
            }
        }

        public static SpriteEffects ToMirror(this HorizontalFacing facing)
        {
            switch(facing)
            {
                default:
                case HorizontalFacing.Right:
                    return SpriteEffects.None;
                case HorizontalFacing.Left:
                    return SpriteEffects.FlipHorizontally;
            }
        }

        public static int GetX(this HorizontalFacing facing)
        {
            switch (facing)
            {
                default:
                    return 0;
                case HorizontalFacing.Left:
                    return -1;
                case HorizontalFacing.Right:
                    return 1;
            }
        }

        public static Connectivity Rotate(this Connectivity connectivity, int halfTurns)
        {
            return (Connectivity)(ShiftWrap((int)connectivity,PositiveMod(halfTurns,8),8) & 255);
        }

        private static int Shift(int a, int b)
        {
            if (b > 0)
                return a >> b;
            else
                return a << -b;
        }

        private static int ShiftWrap(int a, int b, int size)
        {
            int left = Shift(a, b);
            int right = Shift(a, -(size - b));
            return left | right;
        }

        /// <summary>
        /// Convert HSV to RGB
        /// h is from 0-360
        /// s,v values are 0-1
        /// r,g,b values are 0-255
        /// Based upon http://ilab.usc.edu/wiki/index.php/HSV_And_H2SV_Color_Space#HSV_Transformation_C_.2F_C.2B.2B_Code_2
        /// </summary>
        public static Color HSVA2RGBA(double hue, double saturation, double value, int alpha)
        {
            // ######################################################################
            // T. Nathan Mundhenk
            // mundhenk@usc.edu
            // (Editors Note: I edited this to work slightly better than the original -Church)
            // C/C++ Macro HSV to RGB

            double H = hue;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (value <= 0)
            { R = G = B = 0; }
            else if (saturation <= 0)
            {
                R = G = B = value;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = value * (1 - saturation);
                double qv = value * (1 - saturation * f);
                double tv = value * (1 - saturation * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = value;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = value;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = value;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = value;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = value;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = value;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = value;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = value;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = value; // Just pretend its black/white
                        break;
                }
            }
            var r = RGBClamp((int)(R * 255.0));
            var g = RGBClamp((int)(G * 255.0));
            var b = RGBClamp((int)(B * 255.0));
            var a = RGBClamp(alpha);

            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        public static int RGBClamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
    }
}

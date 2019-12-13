using Microsoft.Xna.Framework;
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

        public static DijkstraTile[,] Dijkstra(Point start, int width, int height, Func<Point, Point, double> length, Func<Point, IEnumerable<Point>> neighbors)
        {
            var dijkstraMap = new DijkstraTile[width, height];
            var dTiles = new List<DijkstraTile>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Point tile = new Point(x, y);
                    DijkstraTile dTile = new DijkstraTile(tile, tile == start ? 0 : double.PositiveInfinity, tile == start ? 0 : double.PositiveInfinity);
                    dijkstraMap[x, y] = dTile;
                    dTiles.Add(dTile);
                }
            }

            while (dTiles.Any())
            {
                var dTile = dTiles.Aggregate((i1, i2) => i1.Distance < i2.Distance ? i1 : i2);

                dTiles.Remove(dTile);

                foreach (var neighbor in neighbors(dTile.Tile))
                {
                    if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
                        continue;
                    var dNeighbor = dijkstraMap[neighbor.X, neighbor.Y];
                    double newDist = dTile.Distance + length(dTile.Tile, dNeighbor.Tile);

                    if (newDist < dNeighbor.Distance)
                    {
                        dNeighbor.Distance = newDist;
                        dNeighbor.Previous = dTile;
                        dNeighbor.MoveDistance = dTile.MoveDistance + 1;
                    }
                }
            }

            return dijkstraMap;
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
    }
}

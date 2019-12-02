using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    static class Util
    {
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

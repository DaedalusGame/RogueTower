using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Humper.Base
{
    static class Utilities
    {
        public static Vector2 RoundVector(this Vector2 v)
        {
            return new Vector2((int)v.X, (int)v.Y);
        }

        public static bool IsNonInteger(this Vector2 v)
        {
            return v.X % 1 != 0 || v.Y % 1 != 0;
        }
    }
}

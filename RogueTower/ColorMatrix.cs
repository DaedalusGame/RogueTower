using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    struct ColorMatrix
    {
        public static ColorMatrix Identity => new ColorMatrix(Matrix.Identity, Vector4.Zero);

        public Matrix Matrix;
        public Vector4 Add;

        public ColorMatrix(Matrix matrix, Vector4 add)
        {
            Matrix = matrix;
            Add = add;
        }

        public ColorMatrix Multiply(ColorMatrix other)
        {
            return new ColorMatrix(Matrix * other.Matrix, Vector4.Transform(other.Add, Matrix) + Add);
        }
    }
}

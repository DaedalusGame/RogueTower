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

        public float this[int row, int column]
        {
            get
            {
                if (column < 4 && row < 4)
                    return Matrix[row, column];
                else if (column == 0)
                    return Add.X;
                else if (column == 1)
                    return Add.Y;
                else if (column == 2)
                    return Add.Z;
                else if (column == 3)
                    return Add.W;
                else
                    throw new ArgumentOutOfRangeException();
            }
            set
            {
                if (column < 4 && row < 4)
                    Matrix[row, column] = value;
                else if (column == 0)
                    Add.X = value;
                else if (column == 1)
                    Add.Y = value;
                else if (column == 2)
                    Add.Z = value;
                else if (column == 3)
                    Add.W = value;
                else
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ColorMatrix(Matrix matrix, Vector4 add)
        {
            Matrix = matrix;
            Add = add;
        }

        public static ColorMatrix operator *(ColorMatrix a, ColorMatrix b)
        {
            return a.Multiply(b);
        }

        public ColorMatrix Multiply(ColorMatrix other)
        {
            return Multiply(this, other);
        }

        public static ColorMatrix Multiply(ColorMatrix a, ColorMatrix b)
        {
            ColorMatrix resultMatrix = Identity;
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    float result = a[y, 0] * b[0, x] + a[y, 1] * b[1, x] + a[y, 2] * b[2, x] + a[y, 3] * b[3, x];
                    if (y == 4)
                        result += b[4, x];
                    resultMatrix[y, x] = result;
                }
            }
            //return resultMatrix;
            ColorMatrix test = new ColorMatrix(b.Matrix * a.Matrix, Vector4.Transform(a.Add, Matrix.Transpose(b.Matrix)) + b.Add);
            return test;
        }

        public static ColorMatrix Lerp(ColorMatrix a, ColorMatrix b, float s)
        {
            return new ColorMatrix(Matrix.Lerp(a.Matrix, b.Matrix, s), Vector4.Lerp(a.Add, b.Add, s));
        }

        public static ColorMatrix Greyscale()
        {
            float lumR = 0.33f;
            float lumG = 0.59f;
            float lumB = 0.11f;

            return new ColorMatrix(new Matrix(
              lumR, lumG, lumB, 0,
              lumR, lumG, lumB, 0,
              lumR, lumG, lumB, 0,
              0, 0, 0, 1),
              new Vector4(0, 0, 0, 0));
        }

        public static ColorMatrix TwoColor(Color black, Color white)
        {
            float aR = black.R / 255f;
            float aG = black.G / 255f;
            float aB = black.B / 255f;
            float aA = black.A / 255f;
            float bR = white.R / 255f;
            float bG = white.G / 255f;
            float bB = white.B / 255f;
            float bA = white.A / 255f;

            return new ColorMatrix(new Matrix(
              (bR - aR), 0, 0, 0,
              0, (bG - aG), 0, 0,
              0, 0, (bB - aB), 0,
              0, 0, 0, (bA - aA)),
              new Vector4(aR, aG, aB, aA));
        }

        public static ColorMatrix Saturate(float saturation)
        {
            return Lerp(Greyscale(), Identity, saturation);
        }

        public static ColorMatrix Scale(float factor)
        {
            return new ColorMatrix(new Matrix(
              factor, 0, 0, 0,
              0, factor, 0, 0,
              0, 0, factor, 0,
              0, 0, 0, 1),
              new Vector4(0, 0, 0, 0));
        }

        public static ColorMatrix Tint(Color color)
        {
            return new ColorMatrix(new Matrix(
              color.R / 255f, 0, 0, 0,
              0, color.G / 255f, 0, 0,
              0, 0, color.B / 255f, 0,
              0, 0, 0, color.A / 255f),
              new Vector4(0, 0, 0, 0));
        }

        public static ColorMatrix Translate(Color color)
        {
            return new ColorMatrix(new Matrix(
              1, 0, 0, 0,
              0, 1, 0, 0,
              0, 0, 1, 0,
              0, 0, 0, 1),
              new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 0));
        }

        public static ColorMatrix Flat(Color color)
        {
            return new ColorMatrix(new Matrix(
              0, 0, 0, 0,
              0, 0, 0, 0,
              0, 0, 0, 0,
              0, 0, 0, color.A / 255f),
              new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 0));
        }
    }
}

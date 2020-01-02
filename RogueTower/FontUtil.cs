using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    public enum Alignment
    {
        Left,
        Center,
        Right,
    }

    public delegate Color TextColorFunction(int dialogIndex);
    public delegate Vector2 TextOffsetFunction(int dialogIndex);

    public class TextParameters
    {
        public bool Bold;
        public TextColorFunction Color = (index) => Microsoft.Xna.Framework.Color.Black;
        public TextColorFunction Border = (index) => Microsoft.Xna.Framework.Color.Transparent;
        public TextOffsetFunction Offset = (index) => Vector2.Zero;
        public int DialogIndex = int.MaxValue;
        public int? MaxWidth = null;
        public int? MaxHeight = null;
        public int CharSeperator = 1;
        public int LineSeperator = 16;
        public int ScriptOffset = 0;
        internal bool Underline;

        public TextParameters SetBold(bool bold)
        {
            Bold = bold;
            return this;
        }

        public TextParameters SetUnderline(bool underline)
        {
            Underline = underline;
            return this;
        }

        public TextParameters SetColor(Color color, Color border)
        {
            Color = (index) => color;
            Border = (index) => border;
            return this;
        }

        public TextParameters SetColor(TextColorFunction color, TextColorFunction border)
        {
            Color = color;
            Border = border;
            return this;
        }

        public TextParameters SetOffset(TextOffsetFunction offset)
        {
            Offset = offset;
            return this;
        }

        public TextParameters SetDialogIndex(int dialogIndex)
        {
            DialogIndex = dialogIndex;
            return this;
        }

        public TextParameters SetConstraints(int maxWidth, int maxHeight)
        {
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            return this;
        }

        public TextParameters SetConstraints(Rectangle rect)
        {
            return SetConstraints(rect.Width, rect.Height);
        }

        public TextParameters SetSeperators(int charSeperator, int lineSeperator)
        {
            CharSeperator = charSeperator;
            LineSeperator = lineSeperator;
            return this;
        }

        public TextParameters Copy()
        {
            return new TextParameters()
            {
                Bold = Bold,
                Color = Color,
                Border = Border,
                Offset = Offset,
                DialogIndex = DialogIndex,
                MaxWidth = MaxWidth,
                MaxHeight = MaxHeight,
                CharSeperator = CharSeperator,
                LineSeperator = LineSeperator,
                ScriptOffset = ScriptOffset,
                Underline = Underline,
            };
        }
    }

    struct CharInfo
    {
        public int Offset;
        public int Width;
        public bool Predefined;

        public CharInfo(int offset, int width, bool predefined)
        {
            Offset = offset;
            Width = width;
            Predefined = predefined;
        }
    }

    class FontUtil
    {
        public const int CharWidth = 16;
        public const int CharHeight = 16;
        public const int CharsPerRow = 32;
        public const int CharsPerColumn = 32;
        public const int CharsPerPage = CharsPerColumn * CharsPerRow;

        public static CharInfo[] CharInfo = new CharInfo[65536];

        public static Rectangle GetCharRect(int index)
        {
            int x = index % CharsPerRow;
            int y = index / CharsPerRow;

            return new Rectangle(x * CharWidth, y * CharHeight, CharWidth, CharHeight);
        }

        public static int GetCharWidth(char chr)
        {
            return CharInfo[chr].Width;
        }

        public static int GetCharOffset(char chr)
        {
            return CharInfo[chr].Offset;
        }

        public static int GetStringLines(string str)
        {
            return (str.Count(x => x == '\n') + 1);
        }

        public static int GetStringWidth(string str, TextParameters parameters)
        {
            parameters = parameters.Copy();
            int n = 0;

            foreach (char chr in str)
            {
                n += GetCharWidth(chr) + parameters.CharSeperator + (parameters.Bold ? 1 : 0);

                switch (chr)
                {
                    case (Game.FORMAT_BOLD):
                        parameters.Bold = !parameters.Bold;
                        break;
                }
            }

            return n;
        }

        public static int GetStringHeight(string str)
        {
            return GetStringLines(str) * CharHeight;
        }

        public static void RegisterChar(Color[] blah, int width, int height, char chr, int index)
        {
            Rectangle rect = GetCharRect(index);

            int left = rect.Width - 1;
            int right = 0;
            bool empty = true;

            for (int x = rect.Left; x < rect.Right; x++)
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    if (blah[y * width + x].A > 0)
                    {
                        left = Math.Min(left, x - rect.Left);
                        right = Math.Max(right, x - rect.Left);
                        empty = false;
                    }
                }

            if (!CharInfo[chr].Predefined)
                CharInfo[chr] = new CharInfo(left, empty ? 0 : right - left + 1, false);
        }

        public static string FitString(string str, TextParameters parameters)
        {
            parameters = parameters.Copy();
            int maxwidth = parameters.MaxWidth ?? int.MaxValue;
            int lastspot = 0;
            int idealspot = 0;
            int width = 0;
            string newstr = "";

            for (int i = 0; i < str.Length; i++)
            {
                char chr = str[i];
                var charWidth = GetCharWidth(chr) + parameters.CharSeperator + (parameters.Bold ? 1 : 0);
                if (charWidth > maxwidth)
                    return "";
                width += charWidth;

                switch (chr)
                {
                    case (Game.FORMAT_BOLD):
                        parameters.Bold = !parameters.Bold;
                        break;
                }

                if (chr == ' ')
                {
                    idealspot = i + 1;
                }

                if (chr == '\n')
                {
                    width = 0;
                }

                if (width > maxwidth)
                {
                    if (idealspot == lastspot)
                        idealspot = i;
                    string substr = str.Substring(lastspot, idealspot - lastspot);
                    newstr += substr.Trim() + "\n";
                    lastspot = idealspot;
                    i = idealspot - 1;
                    width = 0;
                }
            }

            newstr += str.Substring(lastspot, str.Length - lastspot);

            return newstr;
        }
    }
}


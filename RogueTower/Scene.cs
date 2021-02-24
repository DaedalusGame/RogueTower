using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    abstract class Scene
    {
        protected Game Game;

        public Scene(Game game)
        {
            Game = game;
        }

        public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;
        public SpriteBatch SpriteBatch => Game.SpriteBatch;
        public PrimitiveBatch<VertexPositionColorTexture> PrimitiveBatch => Game.PrimitiveBatch;
        public Texture2D Pixel => Game.Pixel;
        public int Frame => Game.Frame;
        public GameWindow Window => Game.Window;
        public Viewport Viewport => GraphicsDevice.Viewport;
        public Effect Shader => Game.Shader;
        /*public MouseState MouseState => Game.MouseState;
        public MouseState LastMouseState => Game.LastMouseState;

        public KeyboardState KeyState => Game.KeyState;
        public KeyboardState LastKeyState => Game.LastKeyState;

        public GamePadState PadState => Game.GamePadState;
        public GamePadState LastPadState => Game.LastGamePadState;*/

        public InputTwinState InputState => Game.InputState;

        public int GetNoiseValue(int x, int y)
        {
            return Game.GetNoiseValue(x, y);
        }

        public abstract bool ShowCursor
        {
            get;
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime);

        public void DrawUI(SpriteReference sprite, Rectangle area, Color color)
        {
            sprite.ShouldLoad = true;
            if (sprite.Width % 2 == 0 || sprite.Height % 2 == 0)
                return;
            int borderX = sprite.Width / 2;
            int borderY = sprite.Height / 2;
            var leftBorder = area.X - borderX;
            var topBorder = area.Y - borderY;
            var rightBorder = area.X + area.Width;
            var bottomBorder = area.Y + area.Height;
            SpriteBatch.Draw(sprite.Texture, new Rectangle(leftBorder, topBorder, borderX, borderY), new Rectangle(0, 0, borderX, borderY), color);
            SpriteBatch.Draw(sprite.Texture, new Rectangle(leftBorder, bottomBorder, borderX, borderY), new Rectangle(0, borderY + 1, borderX, borderY), color);
            SpriteBatch.Draw(sprite.Texture, new Rectangle(rightBorder, topBorder, borderX, borderY), new Rectangle(borderX + 1, 0, borderX, borderY), color);
            SpriteBatch.Draw(sprite.Texture, new Rectangle(rightBorder, bottomBorder, borderX, borderY), new Rectangle(borderX + 1, borderY + 1, borderX, borderY), color);

            SpriteBatch.Draw(sprite.Texture, new Rectangle(area.X, topBorder, area.Width, borderY), new Rectangle(borderX, 0, 1, borderY), color);
            SpriteBatch.Draw(sprite.Texture, new Rectangle(area.X, bottomBorder, area.Width, borderY), new Rectangle(borderX, borderY + 1, 1, borderY), color);

            SpriteBatch.Draw(sprite.Texture, new Rectangle(leftBorder, area.Y, borderX, area.Height), new Rectangle(0, borderY, borderX, 1), color);
            SpriteBatch.Draw(sprite.Texture, new Rectangle(rightBorder, area.Y, borderX, area.Height), new Rectangle(borderX + 1, borderY, borderX, 1), color);

            SpriteBatch.Draw(sprite.Texture, new Rectangle(area.X, area.Y, area.Width, area.Height), new Rectangle(borderX, borderY, 1, 1), color);
        }

        public string ConvertToPixelText(string text)
        {
            return Game.ConvertToPixelText(text);
        }

        public string ConvertToSmallPixelText(string text)
        {
            return Game.ConvertToSmallPixelText(text);
        }

        public void DrawText(string str, Vector2 drawpos, Alignment alignment, TextParameters parameters)
        {
            Game.DrawText(str, drawpos, alignment, parameters);
        }
    }
}

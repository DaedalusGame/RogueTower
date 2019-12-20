using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RogueTower
{
    class Background
    {
        SceneGame SceneGame;
        /// <summary>
        /// The image we use in the background.
        /// </summary>
        private SpriteReference BackgroundImage;
        public Vector2 BackgroundSize;
        /// <summary>
        /// Offset to start drawing our image.
        /// </summary>
        private Vector2 Offset;
        /// <summary>
        /// Speed of movement of our parallax effect
        /// </summary>
        public Vector2 Speed;
        /// <summary>
        /// Do we loop this horizontally?
        /// </summary>
        public bool XLooping = false;
        /// <summary>
        /// Do we loop this vertically?
        /// </summary>
        public bool YLooping = false;
        private Viewport Viewport;      //Our game viewport

        //Calculate Rectangle dimensions, based on offset/viewport/zoom values
        private Rectangle Rectangle => new Rectangle((int)(Speed.X * SceneGame.Camera.X + Offset.X), (int)(Speed.Y * SceneGame.Camera.Y + Offset.Y), (int)(Viewport.Width), (int)(Viewport.Height));

        public Background(SceneGame sceneGame, SpriteReference backgroundImage, Vector2 offset, Vector2 speed)
        {
            SceneGame = sceneGame;
            Viewport = sceneGame.Viewport;
            BackgroundImage = backgroundImage;
            BackgroundSize = backgroundImage.Texture.Bounds.Size.ToVector2();
            Offset = offset;
            Speed = speed;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 position = new Vector2(Viewport.X, Viewport.Y);
            Rectangle rectangle = Rectangle;
            if (!XLooping)
            {
                position.X = rectangle.X;
                rectangle.X = 0;
                rectangle.Width = BackgroundImage.Width;
            }
            if (!YLooping)
            {
                position.Y = rectangle.Y;
                rectangle.Y = 0;
                rectangle.Height = BackgroundImage.Height;
            }
            spriteBatch.Draw(BackgroundImage.Texture, position, rectangle, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
        }
    }
}
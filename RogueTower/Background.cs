using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

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
        private Func<Vector2> Offset;
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
        private GameWorld World => SceneGame.World;
        private Viewport Viewport => SceneGame.Viewport;      //Our game viewport

        private Vector2 CurrentOffset => Offset();
        //Calculate Rectangle dimensions, based on offset/viewport/zoom values
        private Rectangle Rectangle => new Rectangle((int)(Speed.X * SceneGame.CameraPosition.X + CurrentOffset.X), (int)(Speed.Y * SceneGame.CameraPosition.Y + CurrentOffset.Y), World.Width, World.Height);

        public Background(SceneGame sceneGame, SpriteReference backgroundImage, Func<Vector2> offset, Vector2 speed)
        {
            SceneGame = sceneGame;
            BackgroundImage = backgroundImage;
            BackgroundSize = backgroundImage.Texture.Bounds.Size.ToVector2();
            Offset = offset;
            Speed = speed;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 position = new Vector2(0, 0);
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
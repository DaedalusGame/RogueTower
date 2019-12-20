using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RogueTower
{
    class Background
    {
        SceneGame SceneGame;
        private SpriteReference BackgroundImage;     //The image to use
        private Vector2 Offset;         //Offset to start drawing our image
        public Vector2 Speed;           //Speed of movement of our parallax effect
        public float Zoom;              //Zoom level of our image

        private Viewport Viewport;      //Our game viewport

        //Calculate Rectangle dimensions, based on offset/viewport/zoom values
        private Rectangle Rectangle => new Rectangle((int)(Speed.X * SceneGame.Camera.X + Offset.X), (int)(Speed.Y * SceneGame.Camera.Y + Offset.Y), (int)(Viewport.Width), (int)(Viewport.Height));

        public Background(SceneGame sceneGame, SpriteReference backgroundImage, Vector2 offset, Vector2 speed)
        {
            SceneGame = sceneGame;
            Viewport = sceneGame.Viewport;
            BackgroundImage = backgroundImage;
            Offset = offset;
            Speed = speed;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BackgroundImage.Texture, new Vector2(Viewport.X, Viewport.Y), Rectangle, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
        }
    }
}
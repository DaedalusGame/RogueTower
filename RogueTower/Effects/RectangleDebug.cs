using Humper.Base;
using Microsoft.Xna.Framework;

namespace RogueTower.Effects
{
    class RectangleDebug : VisualEffect
    {
        public RectangleF Rectangle;
        public Color Color;
        public int FrameEnd;

        public RectangleDebug(GameWorld world, RectangleF rect, Color color, int time) : base(world)
        {
            Rectangle = rect;
            Color = color;
            FrameEnd = time;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            scene.SpriteBatch.Draw(scene.Pixel, Rectangle.ToRectangle(), Color);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueTower.Effects.Particles
{
    class WallBreakEffect : Particle
    {
        public float FrameEnd;
        public Color Color;

        public WallBreakEffect(GameWorld world, Vector2 position, Color color, float time) : base(world, position)
        {
            FrameEnd = time;
            Color = color;
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
            var wallBreak = SpriteLoader.Instance.AddSprite("content/rockfall_end");
            scene.DrawSpriteExt(wallBreak, scene.AnimationFrame(wallBreak, Frame, FrameEnd), Position - wallBreak.Middle, wallBreak.Middle, 0, Vector2.One, SpriteEffects.None, Color, 0);
        }
    }
}

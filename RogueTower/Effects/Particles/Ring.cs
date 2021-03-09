using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RogueTower.Effects.Particles
{
    class Ring : Particle
    {
        public float FrameEnd;
        public Color StartColor, EndColor;
        public float StartSize, EndSize;

        public Color Color => Color.Lerp(StartColor, EndColor, Frame / FrameEnd);
        public float Size => MathHelper.Lerp(StartSize, EndSize, Frame / FrameEnd);

        public Ring(GameWorld world, Vector2 position, float startSize, float endSize, Color startColor, Color endColor, float time) : base(world, position)
        {
            StartSize = startSize;
            EndSize = endSize;
            StartColor = startColor;
            EndColor = endColor;
            FrameEnd = time;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Invisible;
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var ring = SpriteLoader.Instance.AddSprite("content/circle_shockwave");
            scene.DrawCircle(ring, 0, Position, Size, Color);
        }
    }
}

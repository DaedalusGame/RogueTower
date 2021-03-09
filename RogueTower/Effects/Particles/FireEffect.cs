using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueTower.Effects.Particles
{
    class FireEffect : Particle
    {
        public float Angle;
        public float FrameEnd;

        public FireEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position)
        {
            Angle = angle;
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
            var fire = SpriteLoader.Instance.AddSprite("content/fire_small");
            var middle = new Vector2(8, 12);
            scene.DrawSpriteExt(fire, scene.AnimationFrame(fire, Frame, FrameEnd), Position - middle, middle, Angle, SpriteEffects.None, 0);
        }
    }

    class BigFireEffect : FireEffect
    {
        public BigFireEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position, angle, time)
        {
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var fireBig = SpriteLoader.Instance.AddSprite("content/fire_big");
            var middle = new Vector2(8, 12);
            scene.DrawSpriteExt(fireBig, scene.AnimationFrame(fireBig, Frame, FrameEnd), Position - middle, middle, Angle, SpriteEffects.None, 0);
        }
    }
}

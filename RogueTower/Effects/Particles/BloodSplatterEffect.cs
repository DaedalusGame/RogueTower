using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueTower.Effects.Particles
{
    class BloodSpatterEffect : FireEffect
    {
        public BloodSpatterEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position, angle, time)
        {
        }

        public override void Update(float delta)
        {
            base.Update(1.0f);
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var bloodSpatter = SpriteLoader.Instance.AddSprite("content/blood_spatter");
            scene.DrawSpriteExt(bloodSpatter, scene.AnimationFrame(bloodSpatter, Frame, FrameEnd), Position - bloodSpatter.Middle, bloodSpatter.Middle, Angle, SpriteEffects.None, 0);
        }
    }
}

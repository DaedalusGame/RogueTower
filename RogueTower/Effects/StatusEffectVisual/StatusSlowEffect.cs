using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueTower.Effects.StatusEffectVisual
{
    class StatusSlowEffect : StatusEffectVisual<Slow>
    {
        public StatusSlowEffect(GameWorld world, Slow effect) : base(world, effect)
        {
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var statusSlowed = SpriteLoader.Instance.AddSprite("content/status_slowed");
            float slide = (Frame * 0.01f) % 1;
            float angle = 0;
            if (slide < 0.1f)
            {
                angle = MathHelper.Lerp(0, MathHelper.Pi, slide / 0.1f);
            }
            scene.DrawSpriteExt(statusSlowed, 0, HeadPosition - statusSlowed.Middle, statusSlowed.Middle, angle, SpriteEffects.None, 0);
        }
    }
}

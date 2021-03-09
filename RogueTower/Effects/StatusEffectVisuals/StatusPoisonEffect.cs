using Microsoft.Xna.Framework.Graphics;

namespace RogueTower.Effects.StatusEffectVisuals
{
    class StatusPoisonEffect : StatusEffectVisual<Poison>
    {
        public StatusPoisonEffect(GameWorld world, Poison effect) : base(world, effect)
        {
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var statusPoisoned = SpriteLoader.Instance.AddSprite("content/status_poisoned");
            scene.DrawSpriteExt(statusPoisoned, (int)(Frame * 0.25f), HeadPosition - statusPoisoned.Middle, statusPoisoned.Middle, 0, SpriteEffects.None, 0);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static RogueTower.Util;

namespace RogueTower.Effects.StatusEffectVisuals
{
    class StatusBombEffect : StatusEffectVisual<Bomb>
    {
        public StatusBombEffect(GameWorld world, Bomb effect) : base(world, effect)
        {
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var bombsigil = SpriteLoader.Instance.AddSprite("content/orb_orange_firesigil");
            for (int i = 0; i < Effect.Stacks; i++)
            {
                scene.DrawSpriteExt(bombsigil, 0, Position + 16 * AngleToVector(((float)i / Effect.Stacks) * MathHelper.TwoPi) - bombsigil.Middle, bombsigil.Middle, ((float)i / Effect.Stacks) * MathHelper.TwoPi, SpriteEffects.None, 0);
            }
        }
    }
}

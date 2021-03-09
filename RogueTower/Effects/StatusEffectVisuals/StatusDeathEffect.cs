using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Effects.Particles;
using System.Collections.Generic;

namespace RogueTower.Effects.StatusEffectVisuals
{
    class StatusDeathEffect : StatusEffectVisual<Doom>
    {
        public StatusDeathEffect(GameWorld world, Doom effect) : base(world, effect)
        {
        }

        protected override void UpdateDiscrete()
        {
            base.UpdateDiscrete();

            if (Effect.Triggered)
            {
                new DeathEffect(World, Effect.Enemy, 30);
                Destroy();
            }
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Effect;
            yield return DrawPass.EffectDeath;
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var runeBackground = SpriteLoader.Instance.AddSprite("content/magic_death3");
            var runeA = SpriteLoader.Instance.AddSprite("content/magic_death2");
            var runeB = SpriteLoader.Instance.AddSprite("content/magic_death");
            if (pass == DrawPass.EffectDeath)
            {
                float fill = Effect.Fill;
                scene.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.Additive, rasterizerState: RasterizerState.CullNone, transformMatrix: scene.WorldTransform, effect: scene.Shader);
                scene.SetupClockBetween(0, fill * MathHelper.TwoPi);
                scene.DrawSprite(runeA, 0, Position - runeA.Middle, SpriteEffects.None, 0);
                scene.SpriteBatch.End();
                scene.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.Additive, rasterizerState: RasterizerState.CullNone, transformMatrix: scene.WorldTransform, effect: scene.Shader);
                scene.SetupClockBetween(fill * MathHelper.TwoPi, MathHelper.TwoPi);
                scene.DrawSprite(runeB, 0, Position - runeB.Middle, SpriteEffects.None, 0);
                scene.SpriteBatch.End();
            }
            else
            {
                scene.DrawSprite(runeBackground, 0, Position - runeBackground.Middle, SpriteEffects.None, 0);
            }
        }
    }
}

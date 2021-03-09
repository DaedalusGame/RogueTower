using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Enemies;
using System.Collections.Generic;

namespace RogueTower.Effects.Particles
{
    class DeathEffect : Particle
    {
        public override Vector2 Position
        {
            get { return Enemy.HomingTarget; }
            set { }
        }

        public Enemy Enemy;
        public float FrameEnd;

        public DeathEffect(GameWorld world, Enemy enemy, float time) : base(world, Vector2.Zero)
        {
            Enemy = enemy;
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
            yield return DrawPass.Effect;
            yield return DrawPass.EffectDeath;
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var runeBackground = SpriteLoader.Instance.AddSprite("content/magic_death3");
            var runeA = SpriteLoader.Instance.AddSprite("content/magic_death2");
            var runeB = SpriteLoader.Instance.AddSprite("content/magic_death");

            float lerp = (float)LerpHelper.CubicOut(1, 0, Frame / FrameEnd);
            Color color = new Color(1, 1, 1, lerp);

            if (pass == DrawPass.EffectDeath)
            {
                scene.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.Additive, rasterizerState: RasterizerState.CullNone, transformMatrix: scene.WorldTransform);
                scene.DrawSpriteExt(runeA, 0, Position - runeA.Middle, runeA.Middle, 0, Vector2.One, SpriteEffects.None, color, 0);
                scene.DrawSpriteExt(runeA, 0, Position - runeA.Middle, runeA.Middle, 0, Vector2.One, SpriteEffects.None, color, 0);
                scene.DrawSpriteExt(runeB, 0, Position - runeB.Middle, runeB.Middle, 0, Vector2.One, SpriteEffects.None, color, 0);
                scene.SpriteBatch.End();
            }
            else
            {
                scene.DrawSpriteExt(runeBackground, 0, Position - runeBackground.Middle, runeBackground.Middle, 0, Vector2.One, SpriteEffects.None, color, 0);
            }
        }
    }
}

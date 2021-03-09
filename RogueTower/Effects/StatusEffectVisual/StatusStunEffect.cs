using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RogueTower.Effects.StatusEffectVisual
{
    class StatusStunEffect : StatusEffectVisual<Stun>
    {
        public StatusStunEffect(GameWorld world, Stun effect) : base(world, effect)
        {
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var statusStunned = SpriteLoader.Instance.AddSprite("content/status_stunned");
            float radius = 8;
            float circleSpeed = 0.15f;
            var offset = new Vector2(radius * (float)Math.Sin(Frame * Math.PI * circleSpeed), (radius / 2) * (float)Math.Cos(Frame * Math.PI * circleSpeed));
            scene.DrawSpriteExt(statusStunned, (int)(Frame * 0.3f), HeadPosition + offset - statusStunned.Middle, statusStunned.Middle, 0, SpriteEffects.None, 0);
        }
    }
}

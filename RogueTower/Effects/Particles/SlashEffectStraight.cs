using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower.Effects.Particles
{
    class SlashEffectStraight : SlashEffectRound
    {
        public SlashEffectStraight(GameWorld world, Func<Vector2> anchor, float size, float angle, SpriteEffects mirror, float time) : base(world, anchor, size, angle, mirror, time)
        {
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var stab = SpriteLoader.Instance.AddSprite("content/slash_straight");
            var slashAngle = Angle;
            if (Mirror.HasFlag(SpriteEffects.FlipHorizontally))
                slashAngle = -slashAngle;
            scene.SpriteBatch.Draw(stab.Texture, Position + new Vector2(8, 8) - new Vector2(8, 8), stab.GetFrameRect(Math.Min(stab.SubImageCount - 1, (int)(stab.SubImageCount * Frame / FrameEnd))), Color.White, slashAngle, stab.Middle, Size, Mirror, 0);
        }
    }
}

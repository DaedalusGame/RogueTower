using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower.Effects.Particles
{
    class PunchEffectStraight : SlashEffectRound
    {
        public PunchEffectStraight(GameWorld world, Func<Vector2> anchor, float size, float angle, SpriteEffects mirror, float time) : base(world, anchor, size, angle, mirror, time)
        {
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var punchStraight = SpriteLoader.Instance.AddSprite("content/punch");
            var punchAngle = Angle;
            if (Mirror.HasFlag(SpriteEffects.FlipHorizontally))
                punchAngle = -punchAngle;
            scene.DrawSpriteExt(punchStraight, scene.AnimationFrame(punchStraight, Frame, FrameEnd), Position - punchStraight.Middle, punchStraight.Middle, punchAngle, Mirror, 0);
        }
    }
}

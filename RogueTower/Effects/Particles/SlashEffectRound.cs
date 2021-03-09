using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower.Effects.Particles
{
    class SlashEffectRound : Particle
    {
        public Func<Vector2> Anchor;
        public float Angle;
        public SpriteEffects Mirror;
        public float FrameEnd;
        public float Size;

        public override Vector2 Position
        {
            get
            {
                return Anchor();
            }
            set
            {
                //NOOP
            }
        }

        public SlashEffectRound(GameWorld world, Func<Vector2> anchor, float size, float angle, SpriteEffects mirror, float time) : base(world, Vector2.Zero)
        {
            Anchor = anchor;
            Angle = angle;
            Size = size;
            Mirror = mirror;
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
            var slash = SpriteLoader.Instance.AddSprite("content/slash_round");
            var slashAngle = Angle;
            if (Mirror.HasFlag(SpriteEffects.FlipHorizontally))
                slashAngle = -slashAngle;
            if (Size > 0.2)
                scene.SpriteBatch.Draw(slash.Texture, Position + new Vector2(8, 8) - new Vector2(8, 8), slash.GetFrameRect(Math.Min(slash.SubImageCount - 1, (int)(slash.SubImageCount * Frame / FrameEnd) - 1)), Color.LightGray, slashAngle, slash.Middle, Size - 0.2f, Mirror, 0);
            scene.SpriteBatch.Draw(slash.Texture, Position + new Vector2(8, 8) - new Vector2(8, 8), slash.GetFrameRect(Math.Min(slash.SubImageCount - 1, (int)(slash.SubImageCount * Frame / FrameEnd))), Color.White, slashAngle, slash.Middle, Size, Mirror, 0);
        }
    }
}

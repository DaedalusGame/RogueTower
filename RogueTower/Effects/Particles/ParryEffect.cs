using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower.Effects.Particles
{
    class ParryEffect : Particle
    {
        public float Angle;
        public float FrameEnd;

        public ParryEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position)
        {
            Angle = angle;
            FrameEnd = time;
        }

        public override void Update(float delta)
        {
            base.Update(1.0f);
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
            var crit = SpriteLoader.Instance.AddSprite("content/crit");
            scene.DrawSpriteExt(crit, scene.AnimationFrame(crit, Frame, FrameEnd), Position - crit.Middle, crit.Middle, Angle, SpriteEffects.None, 0);
        }
    }
}

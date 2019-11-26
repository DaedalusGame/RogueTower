using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    class VisualEffect : GameObject
    {
        public float Frame;

        protected override void UpdateDelta(float delta)
        {
            Frame += delta;
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }
    }

    class SlashEffect : VisualEffect
    {
        public float Angle;
        public bool Mirror;
        public float FrameEnd;
        public bool Dead;

        public SlashEffect(float angle, bool mirror, float time)
        {
            Angle = angle;
            Mirror = mirror;
            FrameEnd = time;
        }

        protected override void UpdateDiscrete()
        {
            if(Frame >= FrameEnd)
            {
                Dead = true;
            }
        }
    }
}

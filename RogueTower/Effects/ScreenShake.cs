using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower.Effects
{
    class ScreenShake : VisualEffect
    {
        public Vector2 Offset;
        public float FrameEnd;

        public ScreenShake(GameWorld world, float time) : base(world)
        {
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
            //NOOP
        }
    }

    class ScreenShakeRandom : ScreenShake
    {
        float Amount;

        public ScreenShakeRandom(GameWorld world, float amount, float time) : base(world, time)
        {
            Amount = amount;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);

            double amount = Amount * (1 - Frame / FrameEnd);
            double shakeAngle = Random.NextDouble() * Math.PI * 2;
            int x = (int)Math.Round(Math.Cos(shakeAngle) * amount);
            int y = (int)Math.Round(Math.Sin(shakeAngle) * amount);
            Offset = new Vector2(x, y);
        }
    }

    class ScreenShakeJerk : ScreenShake
    {
        Vector2 Jerk;

        public ScreenShakeJerk(GameWorld world, Vector2 jerk, float time) : base(world, time)
        {
            Jerk = jerk;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);

            float amount = (1 - Frame / FrameEnd);
            Offset = Jerk * amount;
        }
    }
}

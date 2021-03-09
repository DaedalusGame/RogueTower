using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower.Effects
{
    class ScreenFlash : VisualEffect
    {
        public Func<ColorMatrix> Color = () => ColorMatrix.Identity;
        public float FrameEnd;

        public ScreenFlash(GameWorld world, Func<ColorMatrix> color, float time) : base(world)
        {
            Color = color;
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
}

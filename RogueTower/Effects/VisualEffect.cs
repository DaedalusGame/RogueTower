using Humper.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower.Effects
{
    abstract class VisualEffect : GameObject
    {
        public float Frame;

        public override RectangleF ActivityZone => World.Bounds;

        public VisualEffect(GameWorld world) : base(world)
        {
        }

        protected override void UpdateDelta(float delta)
        {
            Frame += delta;
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Effect;
        }
    }

}

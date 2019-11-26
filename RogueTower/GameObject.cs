using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    abstract class GameObject
    {
        private float LastDelta;

        public virtual void Update(float delta)
        {
            while (LastDelta + delta >= 1)
            {
                float needed = 1-LastDelta;
                UpdateDelta(needed);
                UpdateDiscrete();
                delta -= needed;
                LastDelta = 0;
            }

            if (delta > 0)
            {
                UpdateDelta(delta);
                LastDelta += delta;
            }
        }

        protected abstract void UpdateDelta(float delta);

        protected abstract void UpdateDiscrete();
    }
}

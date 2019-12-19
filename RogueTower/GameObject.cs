using Humper.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    abstract class GameObject
    {
        public GameWorld World;
        private float LastDelta;
        public bool Destroyed
        {
            get;
            private set;
        }
        public Random Random => World.Random;

        public abstract RectangleF ActivityZone
        {
            get;
        }
        public bool Active => ActivityZone.Contains(World.Player.Position);

        public virtual double DrawOrder => 0;

        public GameObject(GameWorld world)
        {
            World = world;
            World.Add(this);
        }

        public virtual void Destroy()
        {
            Destroyed = true;
        }

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

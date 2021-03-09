using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower.Effects.Particles
{
    abstract class Particle : VisualEffect
    {
        public virtual Vector2 Position
        {
            get;
            set;
        }

        public Particle(GameWorld world, Vector2 position) : base(world)
        {
            Position = position;
        }
    }
}

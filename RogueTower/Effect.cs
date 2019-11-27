using Microsoft.Xna.Framework;
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
        public bool Destroyed;

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
                Destroyed = true;
            }
        }
    }

    abstract class Particle : VisualEffect
    {
        public Vector2 Position;
        public Vector2 Velocity;

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Position += Velocity * delta;
        }

        public Particle(Vector2 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
        }
    }

    class KnifeBounced : Particle
    {
        public float FrameEnd;
        public float Rotation;

        public KnifeBounced(Vector2 position, Vector2 velocity, float rotation, float time) : base(position, velocity)
        {
            Rotation = rotation;
            FrameEnd = time;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroyed = true;
            }
            Velocity += new Vector2(0, 0.4f);
        }
    }
}

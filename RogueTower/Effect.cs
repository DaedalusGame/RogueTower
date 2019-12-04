using Humper.Base;
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

        public override void ShowDamage(double damage)
        {
            //NOOP
        }
    }

    class SlashEffect : VisualEffect
    {
        public float Angle;
        public bool Mirror;
        public float FrameEnd;

        public SlashEffect(GameWorld world, float angle, bool mirror, float time) : base(world)
        {
            Angle = angle;
            Mirror = mirror;
            FrameEnd = time;
        }

        protected override void UpdateDiscrete()
        {
            if(Frame >= FrameEnd)
            {
                Destroy();
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

        public Particle(GameWorld world, Vector2 position, Vector2 velocity) : base(world)
        {
            Position = position;
            Velocity = velocity;
        }
    }

    class ParryEffect : Particle
    {
        public float Angle;
        public float FrameEnd;

        public ParryEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position, Vector2.Zero)
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
    }

    class KnifeBounced : Particle
    {
        public float FrameEnd;
        public float Rotation;

        public KnifeBounced(GameWorld world, Vector2 position, Vector2 velocity, float rotation, float time) : base(world, position, velocity)
        {
            Rotation = rotation;
            FrameEnd = time;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
            Velocity += new Vector2(0, 0.4f);
        }
    }

    class DamagePopup : Particle
    {
        public float FrameEnd;
        public string Text;
        public Vector2 Offset => new Vector2(0,-16) * (float)LerpHelper.QuadraticOut(0,1,Frame/FrameEnd);

        public DamagePopup(GameWorld world, Vector2 position, string text, float time) : base(world, position, new Vector2(0,0f))
        {
            Text = text;
            FrameEnd = time;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }
    }

    class RectangleDebug : VisualEffect
    {
        public RectangleF Rectangle;
        public Color Color;
        public int FrameEnd;
        
        public RectangleDebug(GameWorld world, RectangleF rect, Color color, int time) : base(world)
        {
            Rectangle = rect;
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
    }
}

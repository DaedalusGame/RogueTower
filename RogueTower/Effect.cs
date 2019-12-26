using ChaiFoxes.FMODAudio;
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
    }

    class ScreenShake : VisualEffect
    {
        public Vector2 Offset;
        public float FrameEnd;

        public ScreenShake(GameWorld world, float time) : base(world)
        {
            FrameEnd = time;
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

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
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

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }
    }

    class SlashEffectRound : Particle
    {
        public Func<Vector2> Anchor;
        public float Angle;
        public SpriteEffects Mirror;
        public float FrameEnd;
        public float Size;

        public override Vector2 Position
        {
            get
            {
                return Anchor();
            }
            set
            {
                //NOOP
            }
        }

        public SlashEffectRound(GameWorld world, Func<Vector2> anchor, float size, float angle, SpriteEffects mirror, float time) : base(world, Vector2.Zero)
        {
            Anchor = anchor;
            Angle = angle;
            Size = size;
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

    class SlashEffectStraight : SlashEffectRound
    {
        public SlashEffectStraight(GameWorld world, Func<Vector2> anchor, float size, float angle, SpriteEffects mirror, float time) : base(world, anchor, size, angle, mirror, time)
        {
        }
    }

    class PunchEffectStraight : SlashEffectRound
    {
        public PunchEffectStraight(GameWorld world, Func<Vector2> anchor, float size, float angle, SpriteEffects mirror, float time) : base(world, anchor, size, angle, mirror, time)
        {
        }
    }
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
    }

    class FireEffect : Particle
    {
        public float Angle;
        public float FrameEnd;

        public FireEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position)
        {
            Angle = angle;
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

    class BigFireEffect : FireEffect
    {
        public BigFireEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position, angle, time)
        {
        }
    }

    class BloodSpatterEffect : FireEffect
    {
        public BloodSpatterEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position, angle, time)
        {
        }

        public override void Update(float delta)
        {
            base.Update(1.0f);
        }
    }

    class BloodDrop : Particle
    {
        public Vector2 Velocity;
        public float FrameEnd;
        public float Rotation;

        public BloodDrop(GameWorld world, Vector2 position, Vector2 velocity, float rotation, float time) : base(world, position)
        {
            Velocity = velocity;
            Rotation = rotation;
            FrameEnd = time;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Position += Velocity * delta;
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

    class KnifeBounced : Particle
    {
        public Vector2 Velocity;
        public float FrameEnd;
        public float Rotation;

        public KnifeBounced(GameWorld world, Vector2 position, Vector2 velocity, float rotation, float time) : base(world, position)
        {
            Velocity = velocity;
            Rotation = rotation;
            FrameEnd = time;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Position += Velocity * delta;
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

    class SnakeHead : Particle
    {
        public Vector2 Velocity;
        public SpriteEffects Mirror;
        public float Rotation;
        public float FrameEnd;

        public SnakeHead(GameWorld world, Vector2 position, Vector2 velocity, SpriteEffects mirror, float rotation, float time) : base(world, position)
        {
            Velocity = velocity;
            FrameEnd = time;
            Mirror = mirror;
            Rotation = rotation;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Position += Velocity * delta;
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
        public Color FontColor;
        public Color BorderColor;
        public Vector2 Offset => new Vector2(0,-16) * (float)LerpHelper.QuadraticOut(0,1,Frame/FrameEnd);

        public DamagePopup(GameWorld world, Vector2 position, string text, float time, Color? fontColor = null, Color? borderColor = null) : base(world, position)
        {
            Text = text;
            FrameEnd = time;
            FontColor = fontColor ?? Color.White;
            BorderColor = borderColor ?? Color.Black;
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

    class ChargeEffect : Particle
    {
        public override Vector2 Position
        {
            get
            {
                return Human.Position;
            }
            set
            {
                //NOOP
            }
        }

        public float Angle;
        public float FrameEnd;
        public EnemyHuman Human;
        SoundChannel Sound;

        public ChargeEffect(GameWorld world, Vector2 position, float angle, float time, EnemyHuman human) : base(world, position)
        {
            Angle = angle;
            FrameEnd = time;
            Human = human;
            Sound = Game.sfx_player_charging.Play();
            Sound.Pitch = 0;
            Sound.Looping = true;
        }

        public override void Destroy()
        {
            base.Destroy();
            StopSound();
        }

        private void StopSound()
        {
            Sound.Looping = false;
            Sound.Stop();
        }

        public override void Update(float delta)
        {
            if (!(Human.CurrentAction is ActionCharge))
            {
                Destroy();
            }
            base.Update(1.0f);
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Sound.Pitch = (float)LerpHelper.CircularOut(0f, 2f, Frame / FrameEnd);
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }
    }

   class StatusPoisonEffect : Particle
    {
        public float Angle;
        public float FrameEnd;
        public StatusPoisonEffect(GameWorld world, Vector2 position, float angle, float time) : base(world, position)
        {
            Angle = angle;
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    abstract class StatusEffect
    {
        public Enemy Enemy;
        private float LastDelta;
        public float Duration, DurationMax;
        public bool Added;
        public bool Removed;
        public virtual ColorMatrix ColorMatrix => ColorMatrix.Identity;
 
        public StatusEffect(Enemy enemy, float duration = float.PositiveInfinity)
        {
            Enemy = enemy;
            DurationMax = duration;
        }

        public void Update(float delta)
        {
            if(!Added)
            {
                OnAdd();
                Added = true;
            }

            while (LastDelta + delta >= 1)
            {
                float needed = 1 - LastDelta;
                Duration += needed;
                UpdateDelta(needed);
                UpdateDiscrete();
                delta -= needed;
                LastDelta = 0;
            }

            if (Duration >= DurationMax)
                Remove();

            if (delta > 0 && !Removed)
            {
                Duration += delta;
                UpdateDelta(delta);
                LastDelta += delta;
            }
        }

        public void Remove()
        {
            OnRemove();
            Removed = true;
        }

        //Default behavior is keep existing if we apply the same type of state
        public virtual bool CanCombine(StatusEffect other)
        {
            return GetType() == other.GetType();
        }

        public virtual StatusEffect[] Combine(StatusEffect other)
        {
            return new[] { this };
        }

        protected abstract void OnAdd();

        protected abstract void OnRemove();

        protected abstract void UpdateDelta(float delta);

        protected abstract void UpdateDiscrete();
    }

    class Poison : StatusEffect
    {
        public float PoisonTick;
        public StatusPoisonEffect PoisonFX;
        public Vector2 Offset;
        private ColorMatrix PoisonSkin = new ColorMatrix(new Matrix(
              0.5f, 0.5f, 0.5f, 0,
              0.5f, 0.5f, 0.5f, 0,
              0.5f, 0.5f, 0.5f, 0,
              0, 0, 0, 1),
              new Vector4(-0.6f, -1.2f, 0, 0)) * ColorMatrix.Saturate(0.5f);
        public override ColorMatrix ColorMatrix => ColorMatrix.Lerp(ColorMatrix.Identity, PoisonSkin, MathHelper.Lerp(0.0f,0.5f,(float)Math.Sin(Duration / 10f) * 0.5f + 0.5f));

        public Poison(Enemy enemy, float duration = float.PositiveInfinity) : base(enemy, duration)
        {
        }

        protected override void OnAdd()
        {
            PoisonFX = new StatusPoisonEffect(Enemy.World, Enemy.Position, 0, DurationMax);
            //You're poisoned!
        }

        protected override void OnRemove()
        {
            if (PoisonFX != null)
                PoisonFX.Destroy();
            //You're no longer poisoned
        }

        protected override void UpdateDelta(float delta)
        {
            if(PoisonFX != null)
            {
                if (Enemy is EnemyHuman enemy)
                {
                    Offset = new Vector2(enemy.Box.Bounds.Center.X, enemy.Box.Bounds.Top - 16);
                }
                else
                    Offset = Enemy.Position - new Vector2(0, 16);

                PoisonFX.Position = Offset;
            }
            PoisonTick += delta;
        }

        protected override void UpdateDiscrete()
        {
            if(PoisonTick > 120)
            {
                if (Enemy.Health > 1)
                {
                    var HealthLoss = Math.Max(Enemy.Health - 5, 1);
                    new DamagePopup(Enemy.World, Enemy.Position, $"{Enemy.Health - HealthLoss}", 30, new Color(212, 1, 254));
                    Enemy.Health = HealthLoss;
                }
                Enemy.Hitstop = 6;
                Enemy.VisualOffset = Enemy.OffsetHitStun(6);
                new ScreenShakeJerk(Enemy.World, Util.AngleToVector(Enemy.Random.NextFloat() * MathHelper.TwoPi) * 4, 3);
                PoisonTick -= 120;
            }
        }
    }

    class Stun : StatusEffect
    {
        public StatusStunEffect StunFX;
        public Vector2 Offset;
        public float Radius = 8;
        public float CircleSpeed = 0.15f;
        public Stun(Enemy enemy, float duration = float.PositiveInfinity) : base(enemy, duration)
        {
        }

        protected override void OnAdd()
        {
            StunFX = new StatusStunEffect(Enemy.World, Enemy.Position, 0, DurationMax);
            //You're stunned!
        }

        protected override void OnRemove()
        {
            if (StunFX != null)
                StunFX.Destroy();
            //You're no longer stunned
        }

        protected override void UpdateDelta(float delta)
        {
            if(StunFX != null)
            {
                if (Enemy is EnemyHuman enemy)
                {
                    Offset = new Vector2(enemy.Box.Bounds.Center.X, enemy.Box.Bounds.Top - 16) - new Vector2(Radius * (float)Math.Sin(StunFX.Frame * Math.PI * CircleSpeed), (Radius / 2) * (float)Math.Cos(StunFX.Frame * Math.PI * CircleSpeed));
                }
                else
                    Offset = Enemy.Position - new Vector2(Radius * (float)Math.Sin(StunFX.Frame * Math.PI * CircleSpeed), (Radius / 2) * (float)Math.Cos(StunFX.Frame * Math.PI * CircleSpeed));

                StunFX.Position = Offset;
            }
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }
    }

    class Slow : StatusEffect
    {
        public float SpeedModifier;
        public StatusSlowEffect SlowFX;
        public Vector2 Offset;
        public override ColorMatrix ColorMatrix => ColorMatrix.Lerp(ColorMatrix.Identity, ColorMatrix.Greyscale() * ColorMatrix.Scale(0.25f), MathHelper.Lerp(0.0f, 1f, (float)Math.Sin(Duration / 50f) * 0.5f + 0.5f));

        public Slow(Enemy enemy, float speedModifier, float duration = float.PositiveInfinity) : base(enemy, duration)
        {
            SpeedModifier = speedModifier;
        }

        public override bool CanCombine(StatusEffect other)
        {
            if(base.CanCombine(other))
            {
                Slow slowA = this;
                Slow slowB = (Slow)other;
                return slowA.SpeedModifier == slowB.SpeedModifier;
            }
            return false;
        }

        protected override void OnAdd()
        {
            SlowFX = new StatusSlowEffect(Enemy.World, Enemy.Position, 0, DurationMax);
            //You slow down!
        }

        protected override void OnRemove()
        {
            if (SlowFX != null)
                SlowFX.Destroy();
            //You're no longer slow
        }

        protected override void UpdateDelta(float delta)
        {
            if (SlowFX != null)
            {
                if(Enemy is EnemyHuman enemy)
                {
                    Offset = new Vector2(enemy.Box.Bounds.Center.X, enemy.Box.Bounds.Top - 16);
                }
                else
                    Offset = Enemy.Position - new Vector2(0, 16);

                SlowFX.Position = Offset;
            }
            //NOOP
            //new DamagePopup(Enemy.World, Enemy.Position + new Vector2(0, 10), "Slowed", 1, Util.HSVA2RGBA(Duration % 360, 1, 1, 255));
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }
    }
}

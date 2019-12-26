using Microsoft.Xna.Framework;
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

        public Poison(Enemy enemy, float duration = float.PositiveInfinity) : base(enemy, duration)
        {
            Offset = Enemy.Position + new Vector2(0, 16);
        }

        protected override void OnAdd()
        {
            PoisonFX = new StatusPoisonEffect(Enemy.World, Offset, 0, DurationMax);
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
                PoisonFX.Position = Enemy.Position + new Vector2(0, 16);

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
        public Stun(Enemy enemy, float duration = float.PositiveInfinity) : base(enemy, duration)
        {
        }

        protected override void OnAdd()
        {
            //You're stunned!
        }

        protected override void OnRemove()
        {
            //You're no longer stunned
        }

        protected override void UpdateDelta(float delta)
        {
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

        public Slow(Enemy enemy, float speedModifier, float duration = float.PositiveInfinity) : base(enemy, duration)
        {
            SpeedModifier = speedModifier;
            Offset = Enemy.Position - new Vector2(0, 20);
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
            SlowFX = new StatusSlowEffect(Enemy.World, Offset, 0, DurationMax);
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
                SlowFX.Position = Enemy.Position - new Vector2(0, 20);

            //NOOP
            //new DamagePopup(Enemy.World, Enemy.Position + new Vector2(0, 10), "Slowed", 1, Util.HSVA2RGBA(Duration % 360, 1, 1, 255));
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }
    }
}

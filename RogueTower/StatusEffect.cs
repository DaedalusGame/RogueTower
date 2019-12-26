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

        public Poison(Enemy enemy, float duration = float.PositiveInfinity) : base(enemy, duration)
        {
        }

        protected override void OnAdd()
        {
            //You're poisoned!
        }

        protected override void OnRemove()
        {
            //You're no longer poisoned
        }

        protected override void UpdateDelta(float delta)
        {
            PoisonTick += delta;
        }

        protected override void UpdateDiscrete()
        {
            if(PoisonTick > 120)
            {
                if(Enemy.Health > 1)
                    Enemy.Health = Math.Max(Enemy.Health - 5, 1);

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
            //NOOP
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }
    }

    class Slow : StatusEffect
    {
        public float SpeedModifier;

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
            //You slow down!
        }

        protected override void OnRemove()
        {
            //You're no longer slow
        }

        protected override void UpdateDelta(float delta)
        {
            //NOOP
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }
    }
}

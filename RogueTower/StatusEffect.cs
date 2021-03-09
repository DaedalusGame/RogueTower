using Microsoft.Xna.Framework;
using RogueTower.Effects;
using RogueTower.Effects.Particles;
using RogueTower.Effects.StatusEffectVisuals;
using RogueTower.Enemies;
using System;
using static RogueTower.Game;
using static RogueTower.Util;

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
            if(Enemy is Player player)
                PlaySFX(sfx_generic_debuff, 1);
            PoisonFX = new StatusPoisonEffect(Enemy.World, this);
            Message(Enemy, new MessageText("You have been poisoned!"));
            //if (Enemy is Player player)
            //    player.PlayerInput.SubActions.Add(new MessageBox("Oops! You just got poison'd!", InputResult.ActionTaken));
            //You're poisoned!
        }

        protected override void OnRemove()
        {
            Message(Enemy, new MessageText("You are wracked by the last of the poison."));
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

        public Stun(Enemy enemy, float duration = float.PositiveInfinity) : base(enemy, duration)
        {
        }

        protected override void OnAdd()
        {
            StunFX = new StatusStunEffect(Enemy.World, this);
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
        public StatusSlowEffect SlowFX;
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
            if (Enemy is Player player)
                PlaySFX(sfx_generic_debuff, 1);
            SlowFX = new StatusSlowEffect(Enemy.World, this);
            Message(Enemy, new MessageText("You slow down!"));
            //You slow down!
        }

        protected override void OnRemove()
        {
            Message(Enemy, new MessageText("Your speed returns to normal."));
            //You're no longer slow
        }

        protected override void UpdateDelta(float delta)
        {
            //NOOP
            //new DamagePopup(Enemy.World, Enemy.Position + new Vector2(0, 10), "Slowed", 1, Util.HSVA2RGBA(Duration % 360, 1, 1, 255));
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }
    }

    class Doom : StatusEffect
    {
        public float BuildUp;
        public float Threshold => 200;
        public bool Triggered = false;
        public Slider TriggerSlide = new Slider(0, 30);
        public float Fill => MathHelper.Clamp(BuildUp / Threshold, 0, 1);

        public Doom(Enemy enemy, float buildup, float duration = float.PositiveInfinity) : base(enemy, duration)
        {
            BuildUp = buildup;
        }

        protected override void OnAdd()
        {
            new StatusDeathEffect(Enemy.World, this);
        }

        protected override void OnRemove()
        {

        }

        public override StatusEffect[] Combine(StatusEffect other)
        {
            if (other is Doom doom)
            {
                BuildUp += doom.BuildUp;
                Duration = Math.Min(Duration, doom.Duration);
                DurationMax = Math.Max(DurationMax, doom.DurationMax);
            }
            return new[] { this };
        }

        protected override void UpdateDelta(float delta)
        {
            if (Triggered)
            {
                Duration = 0;
                TriggerSlide += delta;
            }
        }

        protected override void UpdateDiscrete()
        {
            if (BuildUp >= Threshold && !Triggered)
            {

                Triggered = true;
            }

            if(Triggered && TriggerSlide.Done)
            {
                Enemy.World.Hitstop = 10;
                Enemy.World.Flash((slide) => {
                    float quadSlide = (float)LerpHelper.QuadraticOut(0, 1, slide);
                    float i = MathHelper.Lerp(5, 1, quadSlide);
                    float e = MathHelper.Lerp(i, i - 1, quadSlide);
                    return new ColorMatrix(new Matrix(
                        i, 0, 0, 0,
                        0, i, 0, 0,
                        0, 0, i, 0,
                        0, 0, 0, 1.0f),
                        new Vector4(-e, -e, -e, 0));
                }, 20);
                Enemy.Health = 0;
                Enemy.Death();
                Message(Enemy, new MessageText("Your time has ran out!"));
                Remove();
            }
        }
    }

    class Curse : StatusEffect
    {
        public float CurseTick;
        private ColorMatrix CurseSkin = new ColorMatrix(new Matrix(
              -0.33f, -0.59f, -0.11f, 0,
              0, 0, 0, 0,
              0, 0, 0, 0,
              0, 0, 0, 1),
              new Vector4(1, 0, 0, 0));
        public override ColorMatrix ColorMatrix => ColorMatrix.Lerp(ColorMatrix.Identity, CurseSkin, MathHelper.Lerp(0.0f, 1.0f, (float)Math.Sin(Duration / 10f) * 0.5f + 0.5f));

        public Curse(Enemy enemy, float duration = float.PositiveInfinity) : base(enemy, duration)
        {
        }

        protected override void OnAdd()
        {
            if (Enemy is Player player)
                PlaySFX(sfx_generic_debuff, 1);
            Message(Enemy, new MessageText("You have been cursed!"));
            //You're cursed!
        }

        protected override void OnRemove()
        {
            Message(Enemy, new MessageText("Your curse has been lifted."));
            //You're no longer cursed
        }

        protected override void UpdateDelta(float delta)
        {
            CurseTick += delta;
        }

        protected override void UpdateDiscrete()
        {
            if (CurseTick > 90)
            {
                if (Enemy.Health > 1)
                {
                    var HealthLoss = Math.Max(Enemy.Health - 1, 1);
                    Enemy.Health = HealthLoss;
                }
                //Enemy.Hitstop = 6;
                Enemy.VisualOffset = Enemy.OffsetHitStun(6);
                new ScreenShakeJerk(Enemy.World, Util.AngleToVector(Enemy.Random.NextFloat() * MathHelper.TwoPi) * 4, 3);
                CurseTick -= 90;
            }
        }
    }

    class Bomb : StatusEffect
    {
        public int Stacks = 1;
        public int StacksMax;
        public float InitialDuration;
        public Bomb(Enemy target, float duration, int stacksmax = 5) : base(target, duration)
        {
            InitialDuration = duration;
            StacksMax = stacksmax;
        }

        protected override void OnAdd()
        {
            new StatusBombEffect(Enemy.World, this);
            Message(Enemy, new MessageText("You feel an unnatural energy radiating within!"));
        }

        protected override void OnRemove()
        {
            Message(Enemy, new MessageText("The energy within dissipates!"));
        }
        public override StatusEffect[] Combine(StatusEffect other)
        {
            if (other is Bomb bomb)
            {
                Stacks = Math.Min(Stacks + bomb.Stacks, StacksMax);
                Duration = Math.Min(Duration, bomb.Duration);
                DurationMax = Math.Max(DurationMax, bomb.DurationMax);
            }
            return new[] { this };
        }

        protected override void UpdateDelta(float delta)
        {
            if (Duration >= DurationMax && Stacks > 0)
            {
                Stacks--;
                Duration = 0;
            }
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }
    }
}

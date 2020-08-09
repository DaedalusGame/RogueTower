using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RogueTower.Game;
using static RogueTower.Util;

namespace RogueTower.Enemies
{
    abstract class Enemy : GameObject, IParryGiver, IParryReceiver
    {
        public abstract bool Incorporeal
        {
            get;
        }
        public abstract Vector2 HomingTarget
        {
            get;
        }
        public abstract Vector2 PopupPosition
        {
            get;
        }
        public abstract bool Dead
        {
            get;
        }
        public virtual bool CanParry => false;
        public virtual bool CanHit => true;
        public virtual bool CanDamage => false;

        public float Hitstop;
        public Func<Vector2> VisualOffset = () => Vector2.Zero;
        public Func<ColorMatrix> VisualFlash = () => ColorMatrix.Identity;
        public virtual ColorMatrix VisualBaseColor => ColorMatrix.Identity;
        public virtual bool VisualInvisible => false;

        public override RectangleF ActivityZone => new RectangleF(Position - new Vector2(1000, 600) / 2, new Vector2(1000, 600));

        public float Lifetime;
        public double Health;
        public double HealthMax;
        public List<StatusEffect> StatusEffects = new List<StatusEffect>();

        public virtual bool Stunned => StatusEffects.Any(effect => effect is Stun);

        public Enemy(GameWorld world, Vector2 position) : base(world)
        {
            Create(position.X, position.Y);
        }

        public virtual Vector2 Position
        {
            get;
            set;
        }

        public virtual void Create(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public void InitHealth(int health)
        {
            Health = health;
            HealthMax = health;
        }

        public void Resurrect()
        {
            Health = HealthMax;
            ClearStatusEffects();
        }

        public void AddStatusEffect(StatusEffect effect)
        {
            if (StatusEffects.Any(x => x.CanCombine(effect)))
                CombineStatusEffect(effect);
            else
                StatusEffects.Add(effect);
        }

        public void ClearStatusEffects()
        {
            foreach(var effect in StatusEffects)
                effect.Remove();
        }

        private void CombineStatusEffect(StatusEffect effect)
        {
            var combineable = StatusEffects.Where(x => x.CanCombine(effect)).ToList();
            var combined = combineable.SelectMany(x => x.Combine(effect)).Distinct().ToList();
            foreach (var added in combined.Except(combineable))
                StatusEffects.Add(effect);
            foreach (var removed in combineable.Except(combined))
                removed.Remove();
        }

        public override void Update(float delta)
        {
            Hitstop -= delta;
            float adjustedDelta = delta;
            if (StatusEffects.Any(effect => effect is Slow))
                adjustedDelta *= StatusEffects.OfType<Slow>().Min(effect => effect.SpeedModifier);
            if (Hitstop > 0)
                adjustedDelta = 0f;
            base.Update(adjustedDelta);
            HandleStatusEffects(delta);
        }

        private void HandleStatusEffects(float delta)
        {
            foreach (StatusEffect effect in StatusEffects)
                effect.Update(delta);
            StatusEffects.RemoveAll(effect => effect.Removed);
        }

        public virtual void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            HandleDamage(damageIn);
        }

        public bool Parry(RectangleF hitmask)
        {
            var affectedHitboxes = World.FindBoxes(hitmask);
            foreach (Box Box in affectedHitboxes)
            {
                if (Box.Data is IParryReceiver receiver && Util.Parry(this, receiver, hitmask))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void ParryGive(IParryReceiver receiver, RectangleF box)
        {
            //NOOP
        }

        public virtual void ParryReceive(IParryGiver giver, RectangleF box)
        {
            //NOOP
        }

        public virtual void HandleDamage(double damageIn)
        {
            if (CanDamage == false)
                return;
            Health = Math.Min(Math.Max(Health-damageIn, 0), HealthMax);
            if(Math.Abs(damageIn) >= 0.1)
                new DamagePopup(World, PopupPosition + new Vector2(0, -16), damageIn.ToString(), 30);
            if (Health <= 0 && !Dead)
            {
                Death();
            }
        }

        public virtual void Heal(double heal)
        {
            Health = Math.Min(Math.Max(Health + heal, 0), HealthMax);
            if (Math.Abs(heal) >= 0.1)
                new DamagePopup(World, PopupPosition + new Vector2(0, -16), heal.ToString(), 30, Color.Lime);
        }

        public virtual void Death()
        {
            Vector2 deathPosition = Position;
            Scheduler.Instance.RunTimer(() => DropItems(deathPosition), new WaitDelta(World, 10));
            ClearStatusEffects();
            //NOOP
        }

        public virtual void DropItems(Vector2 position)
        {
            //NOOP
        }

        public Func<Vector2> OffsetHitStun(float time)
        {
            float startTime = Lifetime;
            float angle = Random.NextFloat() * MathHelper.TwoPi;
            float dist = 4;
            Vector2 stunOffset = AngleToVector(angle) * dist;
            return () =>
            {
                float slide = (Lifetime - startTime) / time;
                if (slide < 0.2f)
                    return -stunOffset;
                else if (slide < 1f)
                    return stunOffset * (1-slide);
                else
                    return Vector2.Zero;
            };
        }

        public Func<Vector2> OffsetShudder(float time)
        {
            float startTime = World.Frame;
            Vector2 leftOffset = new Vector2(-2, 0);
            Vector2 rightOffset = new Vector2(2, 0);
            return () =>
            {
                float slide = (World.Frame - startTime) / time;
                if (slide < 1f)
                    return (World.Frame % 2) < 1 ? leftOffset : rightOffset;
                else
                    return Vector2.Zero;
            };
        }

        public Func<ColorMatrix> Flash(Color color, float time)
        {
            float startTime = Lifetime;
            ColorMatrix flash = new ColorMatrix(new Matrix(
              0, 0, 0, 0,
              0, 0, 0, 0,
              0, 0, 0, 0,
              0, 0, 0, color.A / 255f),
              new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 0));
            return () =>
            {
                float slide = (Lifetime - startTime) / time;
                if (slide < 1f)
                    return flash;
                else
                    return ColorMatrix.Identity;
            };
        }

        public override IEnumerable<Vector2> GetDrawPoints()
        {
            yield return Position;
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            if(VisualInvisible)
                yield return DrawPass.Invisible;
            else
                yield return DrawPass.Foreground;
        }
    }
}

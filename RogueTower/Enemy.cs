using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RogueTower.Game;

namespace RogueTower
{
    abstract class Enemy : GameObject
    {
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
    }

    class BallAndChain : Enemy
    {
        public IBox Box;
        public float Angle = 0;
        public float Speed = 0;
        public float Distance = 0;
        public bool Swings = false;
        public double Damage = 10;

        public Vector2 OffsetUnit => Swings ? AngleToVector(MathHelper.Pi + MathHelper.PiOver2 * (float)Math.Sin(Angle / MathHelper.PiOver2)) : AngleToVector(Angle);
        public Vector2 Offset => OffsetUnit * Distance;
        public Vector2 LastOffset;

        public BallAndChain(GameWorld world, Vector2 position, float angle, float speed, float distance) : base(world,position)
        {
            Angle = angle;
            Speed = speed;
            Distance = distance;
            CanDamage = false;
            Health = 240.00; //If we ever do want to make these destroyable, this is the value I propose for health.
        }

        public override void Create(float x, float y)
        {
            base.Create(x, y);
            Box = World.Create(x-8, y-8, 16, 16);
            Box.AddTags(CollisionTag.NoCollision);
            Box.Data = this;
        }

        private Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }

        protected override void UpdateDelta(float delta)
        {
            Angle += Speed * delta;
        }

        protected override void UpdateDiscrete()
        {
            //Damage here
            var size = Box.Bounds.Size;
            var move = Position + Offset - size / 2;
            var response = Box.Move(move.X,move.Y,collision =>
            {
                return new CrossResponse(collision);
            });
            foreach(var hit in response.Hits)
            {
                OnCollision(hit);
            }
            LastOffset = Offset;
        }

        protected void OnCollision(IHit hit)
        {
            if (hit.Box.HasTag(CollisionTag.NoCollision))
                return;
            if (hit.Box.Data is Player player)
            {
                var hitVelocity = (Offset - LastOffset);
                hitVelocity.Normalize();
                player.Hit(hitVelocity * 5, 40, 30, Damage);
            }
        }

        public override void ShowDamage(double damage)
        {
            new DamagePopup(World, Position + Offset, damage.ToString(), 30);
        }
    }
}

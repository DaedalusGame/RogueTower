using Humper.Base;
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

        public GameWorld World;
        public virtual Vector2 Position
        {
            get;
            set;
        }

        public virtual void Create(GameWorld world, float x, float y)
        {
            World = world;
            Position = new Vector2(x, y);
        }
    }

    class BallAndChain : Enemy
    {
        public float Angle = 0;
        public float Speed = 0;
        public float Distance = 0;
        public bool Swings = false;
        public double Damage = 10;

        public Vector2 OffsetUnit => Swings ? AngleToVector(MathHelper.Pi + MathHelper.PiOver2 * (float)Math.Sin(Angle / MathHelper.PiOver2)) : AngleToVector(Angle);
        public Vector2 Offset => OffsetUnit * Distance;
        public Vector2 LastOffset;

        public BallAndChain(float angle, float speed, float distance)
        {
            Angle = angle;
            Speed = speed;
            Distance = distance;
            CanDamage = false;
            Health = 240.00; //If we ever do want to make these destroyable, this is the value I propose for health.
            
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
            Vector2 size = new Vector2(16, 16);
            var hitPlayers = World.FindBoxes(new RectangleF(Position + Offset - size / 2, size)).Where(box => box.Data is Player).Select(box => box.Data);
            foreach (Player player in hitPlayers)
            {
                var hitVelocity = (Offset - LastOffset);
                hitVelocity.Normalize();
                player.Hit(hitVelocity * 5, 40, 30, Damage);
            }
            LastOffset = Offset;
        }
    }
}

using Humper.Base;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    abstract class Enemy : GameObject
    {
        double Health;

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

        public Vector2 OffsetUnit => new Vector2((float)Math.Sin(Angle), -(float)Math.Cos(Angle));
        public Vector2 Offset => OffsetUnit * Distance;
        public Vector2 LastOffset;

        public BallAndChain(float angle, float speed, float distance)
        {
            Angle = angle;
            Speed = speed;
            Distance = distance;
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
            foreach(Player player in hitPlayers)
            {
                var hitVelocity = (Offset - LastOffset);
                hitVelocity.Normalize();
                player.Hit(hitVelocity * 5, 40, 30);
            }
            LastOffset = Offset;
        }
    }
}

using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    public class SlideAdvancedResponse : ICollisionResponse
    {
        public SlideAdvancedResponse(ICollision collision)
        {
            var hitbox = collision.Hit.Box;
            var velocity = (collision.Goal.Center - collision.Origin.Center);

            var box1 = new RectangleF(collision.Hit.Position, collision.Origin.Size);
            var box2 = collision.Hit.Box.Bounds;

            var trueHit = collision.Hit.Position;

            var normal = collision.Hit.Normal;

            if (normal.X != 0 && box1.Bottom - box2.Top <= 0)
            {
                collision.Hit.Normal = Vector2.Zero;
            }
            if (normal.X != 0 && box2.Bottom - box1.Top <= 0)
            {
                collision.Hit.Normal = Vector2.Zero;
            }
            if (normal.Y != 0 && box2.Right - box1.Left <= 0)
            {
                collision.Hit.Normal = Vector2.Zero;
            }
            if (normal.Y != 0 && box1.Right - box2.Left <= 0)
            {
                collision.Hit.Normal = Vector2.Zero;
            }

            //normal = collision.Hit.Normal;

            var dot = collision.Hit.Remaining * (velocity.X * normal.Y + velocity.Y * normal.X);
            var slide = new Vector2(normal.Y, normal.X) * dot;

            slide = new Vector2((int)slide.X, (int)slide.Y);

            this.Destination = new RectangleF(trueHit + slide, collision.Goal.Size);
        }

        public RectangleF Destination { get; private set; }
    }
}

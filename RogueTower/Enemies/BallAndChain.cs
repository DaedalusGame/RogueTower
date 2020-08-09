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

        public override bool CanParry => true;
        public override bool Incorporeal => false;
        public override Vector2 HomingTarget => Position;
        public override Vector2 PopupPosition => Position + Offset;
        public override bool Dead => false;

        public BallAndChain(GameWorld world, Vector2 position, float angle, float speed, float distance) : base(world, position)
        {
            Angle = angle;
            Speed = speed;
            Distance = distance;
            InitHealth(240); //If we ever do want to make these destroyable, this is the value I propose for health.
        }

        public override void Create(float x, float y)
        {
            base.Create(x, y);
            Box = World.Create(x - 8, y - 8, 16, 16);
            Box.AddTags(CollisionTag.NoCollision);
            Box.Data = this;
        }

        public override void Destroy()
        {
            base.Destroy();
            World.Remove(Box);
        }

        private Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }

        protected override void UpdateDelta(float delta)
        {
            Lifetime += delta;

            Angle += Speed * delta;
        }

        protected override void UpdateDiscrete()
        {
            if (Active) //Antilag
            {
                var size = Box.Bounds.Size;
                var move = Position + Offset - size / 2;
                var response = Box.Move(move.X, move.Y, collision =>
                {
                    return null;
                });
                foreach (var hit in response.Hits)
                {
                    OnCollision(hit);
                }
            }
            LastOffset = Offset;
        }

        protected void OnCollision(IHit hit)
        {
            if (hit.Box.HasTag(CollisionTag.NoCollision))
                return;
            if (hit.Box.Data is Enemy enemy)
            {
                var hitVelocity = (Offset - LastOffset);
                hitVelocity.Normalize();
                enemy.Hit(hitVelocity * 5, 40, 30, Damage);
            }
        }

        public override IEnumerable<Vector2> GetDrawPoints()
        {
            yield return Position;
            yield return Position + Offset;
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var spikeball = SpriteLoader.Instance.AddSprite("content/spikeball");
            var chain = SpriteLoader.Instance.AddSprite("content/chain");
            for (float i = 0; i < Distance; i += 6f)
            {
                scene.DrawSprite(chain, 0, Position + OffsetUnit * i - chain.Middle, SpriteEffects.None, 1);
            }
            scene.DrawSprite(spikeball, 0, Position + Offset - spikeball.Middle + VisualOffset(), SpriteEffects.None, 1);
        }
    }
}

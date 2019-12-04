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
using static RogueTower.Util;

namespace RogueTower
{
    abstract class Enemy : GameObject
    {
        public override RectangleF ActivityZone => new RectangleF(Position - new Vector2(1000, 600) / 2,new Vector2(1000,600));

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

    class MoaiMan : Enemy
    {
        public IBox Box;
        public override Vector2 Position
        {
            get
            {
                return Box.Bounds.Center;
            }
            set
            {
                var pos = value + Box.Bounds.Center - Box.Bounds.Location;
                Box.Teleport(pos.X, pos.Y);
            }
        }
        public Vector2 Velocity;
        private Vector2 VelocityLeftover;

        public float Gravity = 0.2f;
        public float GravityLimit = 10f;
        public float SpeedLimit = 2;
        public bool OnGround;
        public bool InAir => !OnGround;
        public bool OnWall;
        public bool OnCeiling;
        public float GroundFriction = 1.0f;
        public float AppliedFriction;

        public HorizontalFacing Facing;

        public Player Target;

        public MoaiMan(GameWorld world, Vector2 position) : base(world, position)
        {
            CanDamage = true;
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x, y, 12, 14);
            Box.Data = this;
        }

        private Vector2 CalculateMovement(float delta)
        {
            var velocity = Velocity * delta + VelocityLeftover;
            var movement = new Vector2((int)velocity.X, (int)velocity.Y);

            VelocityLeftover = velocity - movement;

            return movement;
        }

        private IMovement Move(Vector2 movement)
        {
            return Box.Move(Box.X + movement.X, Box.Y + movement.Y, collision =>
            {
                if (collision.Hit.Box.HasTag(CollisionTag.NoCollision))
                    return null;
                return new SlideAdvancedResponse(collision);
            });
        }

        private void UpdateAI()
        {
            var viewSize = new Vector2(200, 50);
            RectangleF viewArea = new RectangleF(Position - viewSize/2, viewSize);

            if (viewArea.Contains(World.Player.Position))
                Target = World.Player;

            if(Target != null) //Engaged
            {
                if (Target.Position.X < Position.X)
                    Facing = HorizontalFacing.Left;
                else if (Target.Position.X > Position.X)
                    Facing = HorizontalFacing.Right;
            }
            else //Idle
            {

            }
        }

        protected override void UpdateDelta(float delta)
        {
            var movement = CalculateMovement(delta);

            bool IsMovingVertically = Math.Abs(movement.Y) > 0.1;
            bool IsMovingHorizontally = Math.Abs(movement.X) > 0.1;

            IMovement move = Move(movement);

            var hits = move.Hits.Where(c => c.Normal != Vector2.Zero && !c.Box.HasTag(CollisionTag.NoCollision));

            if (move.Hits.Any() && !hits.Any())
            {
                IsMovingHorizontally = false;
                IsMovingVertically = false;
            }

            if (IsMovingVertically)
            {
                if (hits.Any((c) => c.Normal.Y < 0))
                {
                    OnGround = true;
                }
                else
                {
                    OnGround = false;
                }

                if (hits.Any((c) => c.Normal.Y > 0))
                {
                    OnCeiling = true;
                }
                else
                {
                    OnCeiling = false;
                }
            }

            if (IsMovingHorizontally)
            {
                if (hits.Any((c) => c.Normal.X != 0))
                {
                    OnWall = true;
                }
                else
                {
                    OnWall = false;
                }
            }
        }

        private void UpdateGroundFriction()
        {
            var tiles = World.FindTiles(Box.Bounds.Offset(0, 1));
            if (tiles.Any())
                GroundFriction = tiles.Max(tile => tile.Friction);
            else
                GroundFriction = 1.0f;
        }

        protected override void UpdateDiscrete()
        {
            if (OnCeiling)
            {
                Velocity.Y = 1;
                AppliedFriction = 1;
            }
            else if (OnGround) //Friction
            {
                UpdateGroundFriction();
                Velocity.Y = 0;
                AppliedFriction = 1 - (1 - 0.85f) * GroundFriction;
            }
            else //Drag
            {
                AppliedFriction = 0.85f;
            }

            if (OnWall)
            {
                var wallTiles = World.FindTiles(Box.Bounds.Offset(GetFacingVector(Facing)));
                if (wallTiles.Any())
                {
                    Velocity.X = 0;
                }
                else
                {
                    OnWall = false;
                }
            }

            Velocity.X *= AppliedFriction;

            UpdateAI();

            if (Velocity.Y < GravityLimit)
                Velocity.Y = Math.Min(GravityLimit, Velocity.Y + Gravity); //Gravity
        }

        public override void ShowDamage(double damage)
        {
            new DamagePopup(World, Position, damage.ToString(), 30);
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
            if (Active) //Antilag
            {
                var size = Box.Bounds.Size;
                var move = Position + Offset - size / 2;
                var response = Box.Move(move.X, move.Y, collision =>
                  {
                      return new CrossResponse(collision);
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

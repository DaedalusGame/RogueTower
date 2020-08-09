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
    abstract class EnemyGravity : Enemy
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
                var pos = value + Box.Bounds.Location - Box.Bounds.Center;
                Box.Teleport(pos.X, pos.Y);
            }
        }
        public Vector2 Velocity;
        protected Vector2 VelocityLeftover;

        public virtual float Friction => 1 - (1 - 0.85f) * GroundFriction;
        public virtual float Drag => 0.85f;
        public virtual float Gravity => 0.2f;
        public virtual float GravityLimit => 10f;
        public bool OnGround;
        public bool InAir => !OnGround;
        public bool OnWall;
        public bool OnCeiling;
        public float GroundFriction = 1.0f;
        public float AppliedFriction;

        public override Vector2 HomingTarget => Position;
        public override Vector2 PopupPosition => Position;

        public EnemyGravity(GameWorld world, Vector2 position) : base(world, position)
        {
        }

        protected Vector2 CalculateMovement(float delta)
        {
            var velocity = Velocity * delta + VelocityLeftover;
            var movement = new Vector2((int)velocity.X, (int)velocity.Y);

            VelocityLeftover = velocity - movement;

            return movement;
        }

        protected IMovement Move(Vector2 movement)
        {
            return Box.Move(Box.X + movement.X, Box.Y + movement.Y, collision =>
            {
                if (IgnoresCollision(collision.Hit.Box))
                    return null;
                return new SlideAdvancedResponse(collision);
            });
        }

        protected bool HandlePanicBox(IMovement move)
        {
            RectangleF panicBox = new RectangleF(move.Destination.X + 2, move.Destination.Y + 2, move.Destination.Width - 4, move.Destination.Height - 4);
            var found = World.FindBoxes(panicBox);
            if (found.Any() && found.Any(x => x != Box && !IgnoresCollision(x)))
            {
                Box.Teleport(move.Origin.X, move.Origin.Y);
                return true;
            }
            return false;
        }

        protected bool IgnoresCollision(IBox box)
        {
            return Incorporeal || box.HasTag(CollisionTag.NoCollision) || box.HasTag(CollisionTag.Character);
        }

        protected void UpdateGroundFriction()
        {
            var tiles = World.FindTiles(Box.Bounds.Offset(0, 1));
            if (tiles.Any())
                GroundFriction = tiles.Max(tile => tile.Friction);
            else
                GroundFriction = 1.0f;
        }

        protected void HandleMovement(float delta)
        {
            var movement = CalculateMovement(delta);

            bool IsMovingVertically = Math.Abs(movement.Y) >= 1;
            bool IsMovingHorizontally = Math.Abs(movement.X) >= 1;

            IMovement move = Move(movement);

            var hits = move.Hits.Where(c => c.Normal != Vector2.Zero && !IgnoresCollision(c.Box));
            var cornerOnly = !move.Hits.Any(c => c.Normal != Vector2.Zero) && move.Hits.Any();

            /*if (move.Hits.Any() && !hits.Any())
            {
                IsMovingHorizontally = false;
                IsMovingVertically = false;
            }*/

            bool panic = false;
            if (!Incorporeal)
                panic = HandlePanicBox(move);
            if (!panic)
            {

                if (IsMovingVertically && !cornerOnly)
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

                if (IsMovingHorizontally && !cornerOnly)
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
        }

        protected void HandlePhysicsEarly()
        {
            if (OnCeiling && !Incorporeal)
            {
                HandleCeiling();
            }
            else if (OnGround && !Incorporeal) //Friction
            {
                UpdateGroundFriction();
                Velocity.Y = 0;
                AppliedFriction = Friction;
            }
            else //Drag
            {
                AppliedFriction = Drag;
            }
        }

        protected void HandlePhysicsLate()
        {
            Velocity.X *= AppliedFriction;

            if (Velocity.Y < GravityLimit)
                Velocity.Y = Math.Min(GravityLimit, Velocity.Y + Gravity); //Gravity
        }

        protected virtual void HandleGround()
        {

        }

        protected virtual void HandleCeiling()
        {
            Velocity.Y = 1;
            AppliedFriction = 1;
        }
    }
}

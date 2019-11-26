using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    enum HorizontalFacing
    {
        Left,
        Right,
    }

    class Player : GameObject
    {
        public enum Action
        {
            Idle,
            Move,
            Brake,
            Jet,
            JumpUp,
            JumpDown,
            Slash,
            SlashUp,
            SlashKnife,
            SlashForward,
            SlashDownward,
            Shoot,
            Hit,
        }

        public enum SwordAction
        {
            StartSwing,
            UpSwing,
            DownSwing,
            FinishSwing,
        }

        public GameWorld World;
        public IBox Box;
        public Vector2 Position
        {
            get
            {
                return Box.Bounds.Center;
            }
            set
            {
                var pos = value + Box.Bounds.Center - Box.Bounds.Location;
                Box.Teleport(pos.X,pos.Y);
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
        public bool Walking;
        public float GroundFriction = 1.0f;

        public HorizontalFacing Facing;
        public Action CurrentAction;

        public float WalkFrame;

        public SwordAction SlashAction;
        public float SlashStartTime;
        public float SlashUpTime;
        public float SlashDownTime;
        public float SlashFinishTime;
        public SlashEffect SlashEffect;

        public float Lifetime;

        KeyboardState LastState;
        SceneGame Game;
        

        public void Create(GameWorld world, float x, float y)
        {
            World = world;
            Box = World.Create(x, y, 12, 14);
            Box.Data = this;
        }

        public void SetControl(SceneGame game)
        {
            Game = game;
        }

        private Vector2 CalculateMovement(float delta)
        {
            var velocity = Velocity * delta + VelocityLeftover;
            var movement = new Vector2((int)velocity.X, (int)velocity.Y);

            VelocityLeftover = velocity - movement;

            return movement;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            SlashEffect?.Update(delta);
            if(SlashEffect != null && SlashEffect.Dead)
            {
                SlashEffect = null;
            }
        }

        protected override void UpdateDelta(float delta)
        {
            Lifetime += delta;

            if (CurrentAction == Action.SlashDownward)
            {
                switch (SlashAction)
                {
                    case (SwordAction.StartSwing):
                    case (SwordAction.UpSwing):
                        SlashStartTime -= delta;
                        if (SlashStartTime < 0)
                            SlashAction = SwordAction.DownSwing;
                        break;
                    case (SwordAction.FinishSwing):
                        SlashFinishTime -= delta;
                        break;
                }
                SlashStartTime -= delta;
            }
            else if ((CurrentAction == Action.Slash || CurrentAction == Action.SlashUp || CurrentAction == Action.SlashKnife) && SlashFinishTime > 0)
            {
                switch (SlashAction)
                {
                    case (SwordAction.StartSwing):
                        SlashStartTime -= delta;
                        if (SlashStartTime < 0)
                            SlashAction = SwordAction.UpSwing;
                        break;
                    case (SwordAction.UpSwing):
                        SlashUpTime -= delta;
                        if (SlashUpTime < 0)
                        {
                            SlashAction = SwordAction.DownSwing;
                            switch(CurrentAction)
                            {
                                case (Action.Slash):
                                    SlashEffect = new SlashEffect(0, false, 4);
                                    break;
                                case (Action.SlashUp):
                                    SlashEffect = new SlashEffect(MathHelper.ToRadians(45), true, 4);
                                    break;
                            }
                            
                            if (CurrentAction == Action.SlashKnife)
                            {
                                /*Bullet bullet = new Bullet();
                                bullet.Create(World, Position.X - 4, Position.Y - 4);
                                bullet.Velocity = new Vector2(Facing == HorizontalFacing.Left ? -8 : 8, 0);
                                bullet.LifeTime = 20;
                                World.Bullets.Add(bullet);*/
                            }
                        }
                        break;
                    case (SwordAction.DownSwing):
                        SlashDownTime -= delta;
                        if (SlashDownTime < 0)
                            SlashAction = SwordAction.FinishSwing;
                        break;
                    case (SwordAction.FinishSwing):
                        SlashFinishTime -= delta;
                        break;
                }
            }
            else if (OnGround)
            {
                if (Math.Abs(Velocity.X) < 0.01)
                    CurrentAction = Action.Idle;
                else if (Walking)
                    CurrentAction = Action.Move;
                else
                    CurrentAction = Action.Brake;
            }
            else
            {
                if (Velocity.Y < 0)
                    CurrentAction = Action.JumpUp;
                else
                    CurrentAction = Action.JumpDown;
            }

            if (CurrentAction == Action.Move)
            {
                if (Velocity.X > 0)
                {
                    Facing = HorizontalFacing.Right;
                }
                else if (Velocity.X < 0)
                {
                    Facing = HorizontalFacing.Left;
                }

                WalkFrame += Math.Abs(Velocity.X * delta * 0.125f) / (float)Math.Sqrt(GroundFriction);
            }

            if (CurrentAction == Action.JumpUp || CurrentAction == Action.JumpDown)
            {
                if (Velocity.X > 0)
                {
                    Facing = HorizontalFacing.Right;
                }
                else if (Velocity.X < 0)
                {
                    Facing = HorizontalFacing.Left;
                }
            }

            var movement = CalculateMovement(delta);

            bool IsMovingVertically = Math.Abs(movement.Y) > 0.1;
            bool IsMovingHorizontally = Math.Abs(movement.X) > 0.1;

            IMovement move = Move(movement);

            if (move.Hits.Any() && move.Hits.All(c => c.Normal == Vector2.Zero))
            {
                IsMovingHorizontally = false;
                IsMovingVertically = false;
            }

            if (IsMovingVertically)
            {
                if (move.Hits.Any((c) => c.Normal.Y < 0))
                {
                    OnGround = true;
                }
                else
                {
                    OnGround = false;
                }

                if (move.Hits.Any((c) => c.Normal.Y > 0))
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
                if (move.Hits.Any((c) => c.Normal.X != 0))
                {
                    OnWall = true;
                }
                else
                {
                    OnWall = false;
                }
            }

            RectangleF panicBox = new RectangleF(move.Destination.X + 2, move.Destination.Y + 2, move.Destination.Width - 4, move.Destination.Height - 4);
            var found = World.Find(panicBox);
            if (found.Any(x => x != Box && x.Bounds.Intersects(Box.Bounds)))
            {
                Box.Teleport(move.Origin.X, move.Origin.Y);
            }
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

        protected override void UpdateDiscrete()
        {
            float appliedFriction;
            if(OnCeiling)
            {
                Velocity.Y = 1;
                appliedFriction = 1;
            }
            else if (OnGround) //Friction
            {
                var tiles = World.FindTiles(Box.Bounds.Offset(0, 1));
                if (tiles.Any())
                    GroundFriction = tiles.Max(tile => tile.Friction);
                else
                    GroundFriction = 1.0f;
                Velocity.Y = 0;
                appliedFriction = 0.85f;
                appliedFriction = 1 - (1 - appliedFriction) * GroundFriction;
            }
            else //Drag
            {
                appliedFriction = 0.85f;
                if (CurrentAction == Action.Slash)
                    appliedFriction = 1 - (1 - appliedFriction) * 0.1f;
            }

            if(OnWall)
            {
                Velocity.X = 0;
            }

            Velocity.X *= appliedFriction;

            bool slashKey = Game.KeyState.IsKeyDown(Keys.Space) && LastState.IsKeyUp(Keys.Space);

            bool leftKey = Game.KeyState.IsKeyDown(Keys.A);
            bool rightKey = Game.KeyState.IsKeyDown(Keys.D);
            bool upKey = Game.KeyState.IsKeyDown(Keys.W);
            bool jumpKey = Game.KeyState.IsKeyDown(Keys.LeftShift) && LastState.IsKeyUp(Keys.LeftShift);
            bool downKey = Game.KeyState.IsKeyDown(Keys.S);
            if (CurrentAction == Action.SlashDownward)
            {
               
            }
            else if (CurrentAction == Action.SlashKnife)
            {

            }
            else if (CurrentAction == Action.SlashUp)
            {
                if (slashKey && (SlashAction == SwordAction.DownSwing || SlashAction == SwordAction.FinishSwing))
                {
                    if (downKey && InAir)
                        SlashDown();
                    else if (downKey)
                        SlashKnife();
                    else
                        Slash();
                }
            }
            else if (CurrentAction == Action.Slash)
            {
                if (slashKey && (SlashAction == SwordAction.DownSwing || SlashAction == SwordAction.FinishSwing))
                {
                    if (downKey && InAir)
                        SlashDown();
                    else if (downKey)
                        SlashKnife();
                    else
                        SlashUp();
                }
            }
            else
            {
                float adjustedSpeedLimit = SpeedLimit / appliedFriction;
                float acceleration = 0.5f / appliedFriction;
                if (OnGround)
                    acceleration *= GroundFriction;

                if (leftKey && Velocity.X > -adjustedSpeedLimit)
                    Velocity.X = Math.Max(Velocity.X - acceleration, -adjustedSpeedLimit);
                if (rightKey && Velocity.X < adjustedSpeedLimit)
                    Velocity.X = Math.Min(Velocity.X + acceleration, adjustedSpeedLimit);
                if (jumpKey && OnGround)
                    Velocity.Y -= GetJumpVelocity(60);
                if (slashKey)
                {
                    if (downKey && InAir)
                        SlashDown();
                    else if (downKey)
                        SlashKnife();
                    else
                        Slash();
                }
            }

            if (leftKey || rightKey)
                Walking = true;
            else
                Walking = false;

            if(CurrentAction == Action.SlashDownward)
            {
                if(SlashStartTime <= 0)
                    Velocity.Y = 5;
                if (OnGround)
                {
                    Velocity.Y = -4;
                    CurrentAction = Action.JumpUp;
                    //SlashAction = SwordAction.FinishSwing;
                    foreach(var tile in World.FindTiles(Box.Bounds.Offset(0, 1)).Where(tile => tile is WallIce))
                    {
                        tile.Replace(new EmptySpace(tile.Map,tile.X,tile.Y));
                    }
                }
                if (SlashAction == SwordAction.FinishSwing && SlashFinishTime <= 0)
                    CurrentAction = Action.Idle;
            }
            else if(Velocity.Y < GravityLimit)
                Velocity.Y = Math.Min(GravityLimit, Velocity.Y + Gravity); //Gravity

            LastState = Game.KeyState;
        }

        public void Slash()
        {
            CurrentAction = Action.Slash;
            SlashStartTime = SlashAction == SwordAction.FinishSwing ? 2 : 0;
            SlashUpTime = 4;
            SlashDownTime = 8;
            SlashFinishTime = 2;
            SlashAction = SwordAction.StartSwing;
            Velocity.Y *= 0.3f;
        }

        public void SlashKnife()
        {
            CurrentAction = Action.SlashKnife;
            SlashStartTime = SlashAction == SwordAction.FinishSwing ? 2 : 0;
            SlashUpTime = 4;
            SlashDownTime = 8;
            SlashFinishTime = 2;
            SlashAction = SwordAction.StartSwing;
            Velocity.Y *= 0.3f;
        }

        public void SlashUp()
        {
            CurrentAction = Action.SlashUp;
            SlashStartTime = SlashAction == SwordAction.FinishSwing ? 2 : 0;
            SlashUpTime = 4;
            SlashDownTime = 8;
            SlashFinishTime = 2;
            SlashAction = SwordAction.StartSwing;
            Velocity.Y *= 0.3f;
        }

        public void SlashDown()
        {
            CurrentAction = Action.SlashDownward;
            Velocity.X = 0;
            Velocity.Y = 0;
            SlashStartTime = 5;
            SlashFinishTime = 8;
            SlashAction = SwordAction.StartSwing;
        }

        public float GetJumpVelocity(float height)
        {
            return (float)Math.Sqrt(2 * Gravity * height);
        }
    }
}

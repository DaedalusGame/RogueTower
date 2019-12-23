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

namespace RogueTower
{
    abstract class Enemy : GameObject
    {
        public abstract bool Attacking
        {
            get;
        }
        public abstract bool Incorporeal
        {
            get;
        }
        public abstract Vector2 HomingTarget
        {
            get;
        }
        public abstract bool Dead
        {
            get;
        }
        public virtual bool CanHit => true;
        public virtual bool CanDamage => false;

        public override RectangleF ActivityZone => new RectangleF(Position - new Vector2(1000, 600) / 2, new Vector2(1000, 600));
        public double Health;
        public double HealthMax;

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

        public virtual void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            HandleDamage(damageIn);
        }

        public abstract void ShowDamage(double damage);

        public virtual void HandleDamage(double damageIn)
        {
            if (CanDamage == false)
                return;
            Health = Math.Min(Math.Max(Health-damageIn, 0), HealthMax);
            if(Math.Abs(damageIn) >= 0.1)
                ShowDamage(damageIn);
            if (Health <= 0)
            {
                Death();
            }
        }

        public virtual void Death()
        {
            //NOOP
        }
    }
    
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
                var pos = value + Box.Bounds.Center - Box.Bounds.Location;
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
            if (OnCeiling)
            {
                HandleCeiling();
            }
            else if (OnGround) //Friction
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

    abstract class EnemyHuman : EnemyGravity
    {
        public HorizontalFacing Facing;

        public override float Gravity => CurrentAction.HasGravity ? base.Gravity : 0;
        public override float Friction => CurrentAction.Friction;
        public override float Drag => CurrentAction.Drag;
        public virtual float Acceleration => 0.25f;
        public virtual float SpeedLimit => 2;
        public int ExtraJumps = 0;
        public int Invincibility = 0;
        public float Lifetime;
        public override bool CanDamage => true;

        public Weapon Weapon;

        public virtual bool Strafing => false;
        public override bool Attacking => CurrentAction.Attacking;
        public override bool Incorporeal => CurrentAction.Incorporeal;
        public override bool Dead => CurrentAction is ActionEnemyDeath;

        public Action CurrentAction;

        public EnemyHuman(GameWorld world, Vector2 position) : base(world, position)
        {
            CurrentAction = new ActionIdle(this);
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x, y, 12, 14);
            Box.AddTags(CollisionTag.Character);
            Box.Data = this;
        }

        public override void Destroy()
        {
            base.Destroy();
            World.Remove(Box);
        }

        public void ResetState()
        {
            if (OnGround)
            {
                CurrentAction = new ActionIdle(this);
            }
            else
            {
                CurrentAction = new ActionJump(this, true, true);
            }
        }

        public override void Update(float delta)
        {
            float modifier = 1.0f;
            if (CurrentAction is ActionEnemyDeath)
                modifier = 0.5f;
            base.Update(delta * modifier);
        }

        protected override void UpdateDelta(float delta)
        {
            Lifetime += delta;

            HandleMovement(delta);

            CurrentAction.UpdateDelta(delta);
        }

        protected override void UpdateDiscrete()
        {
            if (OnGround)
            {
                ExtraJumps = 0;
            }

            HandlePhysicsEarly();

            var wallTiles = World.FindTiles(Box.Bounds.Offset(GetFacingVector(Facing)));
            if (wallTiles.Any())
            {
                Velocity.X = 0;
            }
            else
            {
                OnWall = false;
            }

            if (!Incorporeal)
            {
                var nearbies = World.FindBoxes(Box.Bounds).Where(x => x.Data != this);
                foreach (var nearby in nearbies)
                {
                    if (nearby.Data is Enemy enemy && !enemy.Incorporeal)
                    {
                        float dx = enemy.Position.X - Position.X;
                        if (Math.Abs(dx) < 3)
                            dx = -Velocity.X;
                        if (dx == 0)
                            dx = Random.NextDouble() < 0.5 ? 1 : -1;
                        if (dx > 0 && Velocity.X > -1)
                            Velocity.X = -1;
                        else if (dx < 0 && Velocity.X < 1)
                            Velocity.X = 1;
                    }
                }
            }

            HandleDamage();

            CurrentAction.UpdateDiscrete();
            HandleInput(); //For implementors

            HandlePhysicsLate();

            if (OnGround) //Damage
            {
                var tiles = World.FindTiles(Box.Bounds.Offset(0, 1));
                if (tiles.Any())
                {
                    Tile steppedTile = tiles.WithMin(tile => Math.Abs(tile.X * 16 + 8 - Position.X));
                    steppedTile.StepOn(this);
                }
            }
        }

        public abstract PlayerState GetBasePose();

        public abstract void SetPhenoType(PlayerState pose);

        protected virtual void HandleInput()
        {
            //NOOP
        }

        public bool Parry(RectangleF hitmask)
        {
            //new RectangleDebug(World, hitmask, Color.Orange, 20);
            var affectedHitboxes = World.FindBoxes(hitmask);
            foreach (Box Box in affectedHitboxes)
            {
                if (Box.Data is Enemy enemy && enemy.Attacking)
                {
                    if (Box.Data == this)
                        continue;
                    PlaySFX(sfx_sword_bink, 1.0f, -0.3f, -0.5f);
                    World.Hitstop = 15;
                    Invincibility = 10;
                    if (OnGround)
                    {
                        Velocity += GetFacingVector(Facing) * -2;
                    }
                    else
                    {
                        ExtraJumps = Math.Max(ExtraJumps, 1);
                        Velocity.Y = 0;
                    }
                    new ParryEffect(World, Vector2.Lerp(Box.Bounds.Center, Position, 0.5f), 0, 10);
                    return true;
                }
            }

            return false;
        }

        public void SwingWeapon(RectangleF hitmask, double damageIn = 0)
        {
            //new RectangleDebug(World, hitmask, Color.Lime, 20);
            var affectedHitboxes = World.FindBoxes(hitmask);
            foreach (Box Box in affectedHitboxes)
            {
                if (Box.Data == this)
                    continue;
                if (Box.Data is Enemy enemy && enemy.CanHit)
                {
                    enemy.Hit(Util.GetFacingVector(Facing) + new Vector2(0, -2), 20, 50, damageIn);
                }
                if (Box.Data is Tile tile)
                {
                    tile.HandleTileDamage(damageIn);
                }
            }

        }

        public float GetJumpVelocity(float height)
        {
            return (float)Math.Sqrt(2 * Gravity * height);
        }

        protected void HandleDamage()
        {
            if (!(CurrentAction is ActionHit))
                Invincibility--;
        }

        public override void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            if (CurrentAction is ActionSlash slash && slash.IsUpSwing)
            {
                //Parry
                slash.Swing();
                return;
            }
            if (Invincibility > 0 || Dead)
                return;
            if (CurrentAction is ActionClimb)
                Velocity = GetFacingVector(Facing) * -1 + new Vector2(0, 1);
            else
                Velocity = velocity;
            OnWall = false;
            OnGround = false;
            Invincibility = invincibility;
            CurrentAction = new ActionHit(this, hurttime);
            PlaySFX(sfx_player_hurt, 1.0f, 0.1f, 0.3f);
            HandleDamage(damageIn);
            World.Hitstop = 6;
        }

        public override void ShowDamage(double damage)
        {
            new DamagePopup(World, Position + new Vector2(0, -16), damage.ToString(), 30);
        }
    }

    class MoaiMan : EnemyHuman
    {
        enum IdleState
        {
            Wait,
            MoveLeft,
            MoveRight,
        }

        public override float Acceleration => 0.25f;
        public override float SpeedLimit => InCombat ? 3.0f : 1.0f;
        public override bool Strafing => true;

        //public override bool Attacking => CurrentAction is ActionAttack;

        public int AttackCooldown = 0;
        public int RangedCooldown = 0;

        public bool InCombat => Target != null;

        public Player Target;
        public int TargetTime;

        IdleState Idle;
        int IdleTime;

        public MoaiMan(GameWorld world, Vector2 position) : base(world, position)
        {
            //Weapon = new WeaponWandOrange(10, 16, new Vector2(8, 32));
            //Weapon = new WeaponUnarmed(10, 14, new Vector2(7, 28));

            Weapon = Weapon.PresetWeaponList[Random.Next(0, Weapon.PresetWeaponList.Length - 1)];
            CurrentAction = new ActionIdle(this);
            InitHealth(80);
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x, y, 12, 14);
            Box.Data = this;
            Box.AddTags(CollisionTag.Character);
        }

        private void Walk(float dx)
        {
            float adjustedSpeedLimit = SpeedLimit;
            float baseAcceleraton = Acceleration;
            if (OnGround)
                baseAcceleraton *= GroundFriction;
            float acceleration = baseAcceleraton;

            if (dx < 0 && Velocity.X > -adjustedSpeedLimit)
                Velocity.X = Math.Max(Velocity.X - acceleration, -adjustedSpeedLimit);
            if (dx > 0 && Velocity.X < adjustedSpeedLimit)
                Velocity.X = Math.Min(Velocity.X + acceleration, adjustedSpeedLimit);
            if (Math.Sign(dx) == Math.Sign(Velocity.X))
                AppliedFriction = 1;
            if (CurrentAction is ActionMove move)
            {
                move.WalkingLeft = dx < 0;
                move.WalkingRight = dx > 0;
            }
        }

        private void WalkConstrained(float dx) //Same as walk but don't jump off cliffs
        {
            float offset = Math.Sign(dx);
            if (Math.Sign(dx) == Math.Sign(Velocity.X))
                offset *= Math.Max(1,Math.Abs(Velocity.X));
            var floor = World.FindTiles(Box.Bounds.Offset(new Vector2(16 * offset,1)));
            if (!floor.Any())
                return;
            Walk(dx);
        }

        private void UpdateAI()
        {
            var viewSize = new Vector2(200, 50);
            RectangleF viewArea = new RectangleF(Position - viewSize / 2, viewSize);

            if (viewArea.Contains(World.Player.Position))
            {
                Target = World.Player;
                TargetTime = 200;
            }
            else
            {
                TargetTime--;
                if (TargetTime <= 0)
                    Target = null;
            }

            if (Target != null) //Engaged
            {
                float dx = Target.Position.X - Position.X;
                float dy = Target.Position.Y - Position.Y;

                if (CurrentAction is ActionTwohandSlash)
                {
                    RangedCooldown--;
                }
                else if (CurrentAction is ActionHit)
                {

                }
                else if (CurrentAction is ActionEnemyDeath)
                {

                }
                else if (CurrentAction is ActionWandBlast)
                {
                    AttackCooldown--;
                }
                else if (CurrentAction is ActionJump)
                {

                }
                else if (CurrentAction is ActionClimb)
                {
                    OnWall = false;
                }
                else if (CurrentAction is ActionStealWeapon)
                {
                    AttackCooldown--;
                }
                else
                {
                    if (dx < 0)
                        Facing = HorizontalFacing.Left;
                    else if (dx > 0)
                        Facing = HorizontalFacing.Right;

                    float preferredDistanceMin = 20;
                    float preferredDistanceMax = 30;
                    if (Target.Invincibility > 0)
                    {
                        preferredDistanceMin = 30;
                        preferredDistanceMax = 40;
                    }
                    if(Target.InAir)
                    {
                        preferredDistanceMin = 40;
                        preferredDistanceMax = 50;
                    }
                    if (CurrentAction is ActionMove move)
                    {
                        move.WalkingLeft = false;
                        move.WalkingRight = false;
                    }
                    if (Math.Abs(dx) > preferredDistanceMax)
                    {
                        WalkConstrained(dx);
                    }
                    if (Math.Abs(dx) < preferredDistanceMin)
                    {
                        WalkConstrained(-dx);
                    }
                    var attackSize = new Vector2(40, 30);
                    var attackZone = new RectangleF(Position + GetFacingVector(Facing) * 20 - attackSize / 2, attackSize);
                    bool runningAway = Math.Abs(Target.Velocity.X) > 1 && Math.Abs(dx) > 30 && Math.Sign(Target.Velocity.X) == Math.Sign(dx);
                    if ((Math.Abs(dx) >= 50 || Target.InAir || runningAway) && Math.Abs(dx) <= 70 && RangedCooldown < 0 && Target.Invincibility < 3 && Weapon is WeaponWandOrange)
                    {
                        CurrentAction = new ActionWandBlast(this, Target, 24, 12, Weapon);
                        RangedCooldown = 60 + Random.Next(40);
                    }
                    else if (Math.Abs(dx) <= 30 && AttackCooldown < 0 && Target.Invincibility < 3 && Target.Box.Bounds.Intersects(attackZone) && !runningAway)
                    {
                        Velocity.X += Math.Sign(dx) * 2;
                        if (Weapon is WeaponUnarmed && !(Target.Weapon is WeaponUnarmed))
                        {
                            CurrentAction = new ActionStealWeapon(this, Target, 4, 8);
                        }
                        else 
                        {
                            CurrentAction = new ActionTwohandSlash(this, 3, 12, Weapon);
                        }
                        AttackCooldown = 30;
                    }
                }

                AttackCooldown--;
                RangedCooldown--;
            }
            else //Idle
            {
                IdleTime--;

                switch(Idle)
                {
                    case (IdleState.Wait):
                        break;
                    case (IdleState.MoveLeft):
                        Facing = HorizontalFacing.Left;
                        WalkConstrained(-1);
                        break;
                    case (IdleState.MoveRight):
                        Facing = HorizontalFacing.Right;
                        WalkConstrained(1);
                        break;
                }

                if(OnWall)
                {
                    if(Idle == IdleState.MoveLeft)
                        Idle = IdleState.MoveRight;
                    else if (Idle == IdleState.MoveRight)
                        Idle = IdleState.MoveLeft;
                    OnWall = false;
                    IdleTime = 70;
                }

                if(IdleTime <= 0)
                {
                    WeightedList<IdleState> nextState = new WeightedList<IdleState>();
                    nextState.Add(IdleState.Wait, 30);
                    nextState.Add(IdleState.MoveLeft, 70);
                    nextState.Add(IdleState.MoveRight, 70);
                    IdleTime = Random.Next(50) + 20;
                    Idle = nextState.GetWeighted(Random);
                }
            }
        }

        protected override void HandleInput()
        {
            UpdateAI();
        }

        protected override void UpdateDelta(float delta)
        {
            if (Active)
            {
                base.UpdateDelta(delta);
            }
        }

        protected override void UpdateDiscrete()
        {
            if (Active)
            {
                base.UpdateDiscrete();
            }
        }

        public override PlayerState GetBasePose()
        {
            PlayerState pose = new PlayerState(
                HeadState.Forward,
                BodyState.Stand,
                ArmState.Angular(5),
                ArmState.Angular(3),
                Weapon.GetWeaponState(MathHelper.ToRadians(270 - 20))
            );
            Weapon.GetPose(pose);
            return pose;
        }

        public override void SetPhenoType(PlayerState pose)
        {
            pose.Head.SetPhenoType("moai");
            pose.LeftArm.SetPhenoType("moai");
            pose.RightArm.SetPhenoType("moai");
        }

        public override void Death()
        {
            if (!(CurrentAction is ActionEnemyDeath))
                CurrentAction = new ActionEnemyDeath(this, 20);
        }

        public override void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            base.Hit(velocity, hurttime, invincibility / 10, damageIn);
        }
    }

    class Snake : Enemy
    {
        public enum SegmentRender
        {
            Invisible,
            Normal,
            Fat,
        }

        public class SnakeSegment
        {
            public Vector2 Offset => new Vector2((float)Math.Sin(Angle), (float)Math.Cos(Angle)) * Distance * ((float)(Index + 1) / Parent.Segments.Count);

            public Snake Parent;
            public int Index;

            public float Angle => MathHelper.Lerp(StartAngle, EndAngle, Parent.MoveDelta);
            public float Distance => MathHelper.Lerp(StartDistance, EndDistance, Parent.MoveDelta);

            public float StartAngle, EndAngle;
            public float StartDistance, EndDistance;

            public SnakeSegment(Snake parent, int index)
            {
                Parent = parent;
                Index = index;
            }

            public void MoveTowards(float angle, float distance, float speed)
            {
                EndAngle = Util.AngleLerp(EndAngle, angle, speed);
                EndDistance = MathHelper.Lerp(EndDistance, distance, speed);
            }

            public void UpdateDiscrete()
            {
                StartAngle = EndAngle;
                StartDistance = EndDistance;
            }
        }

        public abstract class Action
        {
            public Snake Snake;

            public virtual bool MouthOpen => false;
            public virtual float HeadIndex => Snake.Segments.Count - 1;
            public virtual bool Hidden => false;

            public Action(Snake snake)
            {
                Snake = snake;
            }

            public virtual SegmentRender GetRenderSegment(SnakeSegment segment)
            {
                return SegmentRender.Normal;
            }

            public abstract void UpdateDelta(float delta);

            public abstract void UpdateDiscrete();
        }

        public class ActionIdle : Action
        {
            public float Time;
            public float Offset;

            public override bool MouthOpen => true;

            public ActionIdle(Snake snake) : base(snake)
            {
                Offset = Snake.Random.Next(100);
            }

            public override void UpdateDelta(float delta)
            {
                Time += delta;
            }

            public override void UpdateDiscrete()
            {
                var frame = Time + Offset;
                Vector2 idleCircle = Snake.IdleCircle;
                Vector2 wantedPosition = Snake.IdleOffset + new Vector2((float)Math.Sin(frame / 20f) * idleCircle.X, (float)Math.Cos(frame / 20f) * idleCircle.Y);
                Snake.Move(wantedPosition, 0.2f);
            }
        }

        public class ActionHide : ActionIdle
        {
            public float HideTime;

            public override bool Hidden => true;
            public override float HeadIndex => MathHelper.Clamp(Snake.Segments.Count * (1 - Time / HideTime), 0, Snake.Segments.Count-1);

            public ActionHide(Snake snake, float hideTime) : base(snake)
            {
                HideTime = hideTime;
            }

            public override void UpdateDelta(float delta)
            {
                base.UpdateDelta(delta);

                if(Time > 1)
                {

                }

                if(Time >= HideTime)
                {
                    Snake.CurrentAction = new ActionHidden(Snake);
                }
            }
        }

        public class ActionHidden : Action
        {
            public float Time;
            public override bool Hidden => true;

            public ActionHidden(Snake snake) : base(snake)
            {
            }

            public override SegmentRender GetRenderSegment(SnakeSegment segment)
            {
                return SegmentRender.Invisible;
            }

            public override void UpdateDelta(float delta)
            {
                Time += delta;
            }

            public override void UpdateDiscrete()
            {
                //NOOP
            }
        }

        public class ActionUnhide : ActionIdle
        {
            public float HideTime;

            public override bool Hidden => true;
            public override float HeadIndex => MathHelper.Clamp(Snake.Segments.Count * (Time / HideTime), 0, Snake.Segments.Count - 1);

            public ActionUnhide(Snake snake, float hideTime) : base(snake)
            {
                HideTime = hideTime;
            }

            public override void UpdateDelta(float delta)
            {
                base.UpdateDelta(delta);

                if (Time >= HideTime)
                {
                    Snake.ResetState();
                }
            }
        }

        public class ActionBite : Action
        {
            public enum BiteState
            {
                Start,
                Bite,
                End,
            }

            public override bool MouthOpen => State == BiteState.Bite;

            public BiteState State;
            public float StartTime;
            public float BiteTime;
            public float EndTime;
            public Vector2 Target;

            public ActionBite(Snake snake, float startTime, float biteTime, float endTime) : base(snake)
            {
                StartTime = startTime;
                BiteTime = biteTime;
                EndTime = endTime;
            }

            public override void UpdateDelta(float delta)
            {
                switch (State)
                {
                    case (BiteState.Start):
                        StartTime -= delta;
                        if (StartTime < 0)
                            State = BiteState.Bite;
                        break;
                    case (BiteState.Bite):
                        BiteTime -= delta;
                        if (BiteTime < 0)
                            State = BiteState.End;
                        break;
                    case (BiteState.End):
                        EndTime -= delta;
                        if (EndTime < 0)
                            Snake.ResetState();
                        break;
                }
            }

            public override void UpdateDiscrete()
            {
                switch (State)
                {
                    case (BiteState.Start):
                        if (Snake.Target != null)
                        {
                            float dx = Snake.Target.Position.X - Snake.Position.X;
                            if (Math.Sign(dx) == GetFacingVector(Snake.Facing).X)
                            {
                                Target = Snake.Target.Position - Snake.Position;
                                Target = Math.Min(Target.Length(), 80) * Vector2.Normalize(Target);
                            }
                        }
                        Snake.Move(Target, 0.1f);
                        break;
                    case (BiteState.Bite):
                        Snake.Move(Target, 0.5f);
                        Snake.Move(Target, 0.5f);
                        Snake.Move(Target, 0.5f);
                        var maskSize = new Vector2(16, 16);
                        bool damaged = false;
                        foreach (var box in Snake.World.FindBoxes(new RectangleF(Snake.Position + Snake.Head.Offset - maskSize/2, maskSize)))
                        {
                            if (box == Snake.Box || Snake.NoFriendlyFire(box.Data))
                                continue;
                            if(box.Data is Enemy enemy)
                            {
                                enemy.Hit(new Vector2(GetFacingVector(Snake.Facing).X, -2), 20, 50, 20);
                                damaged = true;
                            }
                        }
                        if (damaged)
                            State = BiteState.End;
                        break;
                    case (BiteState.End):
                        Snake.Move(Target, 0.2f);
                        break;
                }
            }
        }

        public class ActionSpit : Action
        {
            public enum SpitState
            {
                Start,
                SpitStart,
                Spit,
                End,
            }

            public override bool MouthOpen => State == SpitState.End || State == SpitState.Spit;

            public Enemy Target;
            public SpitState State;
            public float StartTime;
            public float SpitStartTime;
            public float SpitTime;
            public float EndTime;
            public Vector2 Offset;

            public ActionSpit(Snake snake, Enemy target, Vector2 offset, float startTime, float spitStartTime, float spitTime, float endTime) : base(snake)
            {
                Target = target;
                Offset = offset;
                StartTime = startTime;
                SpitStartTime = spitStartTime;
                SpitTime = spitTime;
                EndTime = endTime;
            }

            public override SegmentRender GetRenderSegment(SnakeSegment segment)
            {
                int i = Snake.Segments.Count - (int)Math.Round(StartTime);
                if (segment.Index == i)
                    return SegmentRender.Fat;
                else
                    return SegmentRender.Normal;
            }

            public override void UpdateDelta(float delta)
            {
                switch (State)
                {
                    case (SpitState.Start):
                        StartTime -= delta;
                        if (StartTime < 0)
                            State = SpitState.SpitStart;
                        break;
                    case (SpitState.SpitStart):
                        SpitStartTime -= delta;
                        if (SpitStartTime < 0)
                        {
                            Fire();
                            State = SpitState.Spit;
                        }
                        break;
                    case (SpitState.Spit):
                        SpitTime -= delta;
                        if (SpitTime < 0)
                            State = SpitState.End;
                        break;
                    case (SpitState.End):
                        EndTime -= delta;
                        if (EndTime < 0)
                            Snake.ResetState();
                        break;
                }
            }

            private void Fire()
            {
                Vector2 firePosition = Snake.Position + Snake.Head.Offset + GetFacingVector(Snake.Facing) * 8;
                int spits = 8;
                for (int i = 0; i < spits; i++)
                {
                    var velocity = Target.HomingTarget + new Vector2(0,-32) - firePosition;
                    if (Snake.Facing == HorizontalFacing.Left && velocity.X > -3)
                        velocity.X = -3;
                    if (Snake.Facing == HorizontalFacing.Right && velocity.X < 3)
                        velocity.X = 3;
                    velocity = Vector2.Normalize(velocity) * (2 + i);
                    new SnakeSpit(Snake.World, firePosition)
                    {
                        Velocity = velocity,
                        Shooter = Snake,
                        FrameEnd = 80,
                    };
                }
            }

            public override void UpdateDiscrete()
            {
                switch (State)
                {
                    case (SpitState.Start):
                        Snake.Move(Offset - GetFacingVector(Snake.Facing) * 16, 0.3f);
                        break;
                    case (SpitState.SpitStart):
                        var spitOffset = Offset + new Vector2(0, 12) + GetFacingVector(Snake.Facing) * 20;
                        Snake.Move(spitOffset, 0.5f);
                        Snake.Move(spitOffset, 0.5f);
                        Snake.Move(spitOffset, 0.5f);
                        break;
                    case (SpitState.End):
                        Snake.Move(Offset, 0.1f);
                        break;
                }
            }
        }

        public class ActionBreath : Action
        {
            public enum BreathState
            {
                Start,
                Breath,
                End,
            }

            public override bool MouthOpen => State == BreathState.Breath || State == BreathState.End;

            public BreathState State;
            public float StartTime;
            public float BiteTime;
            public float EndTime;
            public Enemy Target;
            public Vector2 Offset;

            public ActionBreath(Snake snake, Enemy target, float startTime, float biteTime, float endTime) : base(snake)
            {
                StartTime = startTime;
                BiteTime = biteTime;
                EndTime = endTime;
                Target = target;
            }

            public override SegmentRender GetRenderSegment(SnakeSegment segment)
            {
                int bulgeLength = 8;
                float bulgeSpeed = 0.3f;
                switch (State)
                {
                    case (BreathState.Start):
                        int index = Snake.Segments.Count - (int)Math.Round(StartTime * bulgeSpeed);
                        return segment.Index < index && segment.Index > index - bulgeLength ? SegmentRender.Fat : SegmentRender.Normal;
                    case (BreathState.Breath):
                        return segment.Index > Snake.Segments.Count - (int)Math.Round(BiteTime * bulgeSpeed) && segment.Index > Snake.Segments.Count - bulgeLength ? SegmentRender.Fat : SegmentRender.Normal;
                    default:
                        return SegmentRender.Normal;
                }
            }


            public override void UpdateDelta(float delta)
            {
                switch (State)
                {
                    case (BreathState.Start):
                        StartTime -= delta;
                        if (StartTime < 0)
                            State = BreathState.Breath;
                        break;
                    case (BreathState.Breath):
                        BiteTime -= delta;
                        if (BiteTime < 0)
                            State = BreathState.End;
                        break;
                    case (BreathState.End):
                        EndTime -= delta;
                        if (EndTime < 0)
                            Snake.ResetState();
                        break;
                }
            }

            public override void UpdateDiscrete()
            {
                switch (State)
                {
                    case (BreathState.Start):
                        if (Math.Sign(Target.Position.X - Snake.Position.X) == GetFacingVector(Snake.Facing).X)
                        {
                            FindTarget();
                        }
                        else
                        {
                            Snake.ResetState();
                        }
                        Snake.Move(Offset, 0.5f);
                        break;
                    case (BreathState.Breath):
                        Snake.Move(Offset, 0.1f);
                        Snake.Move(Offset, 0.1f);
                        if (Math.Sign(Target.Position.X - Snake.Position.X) == GetFacingVector(Snake.Facing).X)
                        {
                            FindTarget();
                        }
                        var offset = GetFacingVector(Snake.Facing);
                        if ((int)BiteTime % 5 == 0)
                        {
                            float angle = Snake.Random.NextFloat() * MathHelper.TwoPi;
                            float distance = Snake.Random.NextFloat() * 3;
                            new PoisonBreath(Snake.World, Snake.Position + Snake.Head.Offset + offset * 8 + distance * new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle)))
                            {
                                Velocity = offset * 3.0f,
                                FrameEnd = 40,
                                Shooter = Snake,
                            };
                            /*new Fireball(Snake.World, Snake.Position + Snake.Head.Offset + offset * 8 + distance * new Vector2((float)Math.Sin(angle),(float)Math.Cos(angle)))
                            {
                                Velocity = offset * (Snake.Random.NextFloat() * 1.5f + 0.5f) + new Vector2(Snake.Velocity.X,0),
                                FrameEnd = 40,
                                Shooter = Snake,
                            };*/
                        }
                        break;
                    case (BreathState.End):
                        Snake.Move(Offset, 0.1f);
                        break;
                }
            }

            private void FindTarget()
            {
                var facing = GetFacingVector(Snake.Facing);
                Offset = new Vector2(Snake.Position.X + facing.X * 25, Target.Position.Y) - Snake.Position;
                Offset = Math.Min(Offset.Length(), 80) * Vector2.Normalize(Offset);
            }
        }

        public class ActionHit : Action
        {
            public int Time;
            public Vector2 Target;

            public ActionHit(Snake snake, Vector2 offset, int time) : base(snake)
            {
                Target = snake.Head.Offset + offset;
                Target = Math.Min(Target.Length(), 80) * Vector2.Normalize(Target);
                Time = time;
            }

            public override void UpdateDelta(float delta)
            {
                //NOOP
            }

            public override void UpdateDiscrete()
            {
                Snake.Move(Target, 0.3f);
                Snake.Move(Target, 0.3f);
                Snake.Move(Target, 0.3f);
                Time--;
                if (Time < 0)
                {
                    Snake.ResetState();
                }
            }
        }

        public class ActionDeath : Action
        {
            public int Time;
            public int TimeEnd;
            public Vector2 Target;
            public float SegmentCut;

            public ActionDeath(Snake snake, Vector2 offset, int time) : base(snake)
            {
                Target = snake.Head.Offset + offset;
                Time = time;
                TimeEnd = time;
                SegmentCut = 1;
            }

            public override SegmentRender GetRenderSegment(SnakeSegment segment)
            {
                return segment.Index < SegmentCut * Snake.Segments.Count ? SegmentRender.Normal : SegmentRender.Invisible;
            }

            public override void UpdateDelta(float delta)
            {
                SegmentCut -= delta / TimeEnd;
            }

            public override void UpdateDiscrete()
            {
                Snake.Move(Target, 0.3f);
                Snake.Move(Target, 0.3f);
                Snake.Move(Target, 0.3f);
                Time--;
                if (Time < 0)
                {
                    Snake.Destroy();
                }
                int index = MathHelper.Clamp((int)(SegmentCut * Snake.Segments.Count) - 1, 0, Snake.Segments.Count - 1);
                var segment = Snake.Segments[index];
                var size = new Vector2(8, 8);
                new FireEffect(Snake.World, Snake.Position + segment.Offset - size / 2 + new Vector2(Snake.Random.NextFloat() * size.X, Snake.Random.NextFloat() * size.Y), 0, 5);
            }
        }

        public IBox Box;
        public List<SnakeSegment> Segments = new List<SnakeSegment>();
        public SnakeSegment Head => Segments[(int)CurrentAction.HeadIndex];
        public Vector2 HeadOffset => GetHeadOffset();
        public Vector2 HeadPosition => Position + HeadOffset;
        public Vector2 PositionLast;
        public Vector2 Velocity => Position - PositionLast;

        public Action CurrentAction;

        public float Lifetime;
        public int Invincibility = 0;

        public override bool Attacking => false;
        public override bool Incorporeal => CurrentAction.Hidden;
        public override Vector2 HomingTarget => Position + Head.Offset;
        public override bool Dead => CurrentAction is ActionDeath;
        public override bool CanDamage => CurrentAction.Hidden;
        public override bool CanHit => CurrentAction.Hidden;

        public virtual Vector2 IdleOffset => -10 * GetFacingVector(Facing) + new Vector2(0, InCombat ? -30 : -15);
        public virtual Vector2 IdleCircle => InCombat ? new Vector2(20 * GetFacingVector(Facing).X, 10) : new Vector2(10 * GetFacingVector(Facing).X, 5);

        public Vector2 HomePosition;
        public HorizontalFacing HomeFacing;

        public HorizontalFacing Facing;
        public float MoveDelta;

        public bool InCombat => Target != null;
        public Player Target;
        public int TargetTime;

        public Snake(GameWorld world, Vector2 position) : base(world, position)
        {
            CurrentAction = new ActionIdle(this);
            InitHealth(80);
        }

        public override void Create(float x, float y)
        {
            base.Create(x, y);
            Box = World.Create(x - 8, y - 8, 16, 16);
            Box.AddTags(CollisionTag.NoCollision);
            Box.Data = this;

            for(int i = 0; i < 15; i++)
            {
                Segments.Add(new SnakeSegment(this,i));
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            World.Remove(Box);
        }

        public void ResetState()
        {
            CurrentAction = new ActionIdle(this);
        }

        private Vector2 GetHeadOffset()
        {
            int i = (int)Math.Floor(CurrentAction.HeadIndex);
            int e = (int)Math.Ceiling(CurrentAction.HeadIndex);
            float slide = CurrentAction.HeadIndex % 1;

            return Vector2.Lerp(Segments[i].Offset, Segments[e].Offset, slide);
        }

        public virtual void UpdateAI()
        {
            var viewSize = new Vector2(200, 100);
            RectangleF viewArea = new RectangleF(Position - viewSize / 2, viewSize);

            if (viewArea.Contains(World.Player.Position))
            {
                Target = World.Player;
                TargetTime = 200;
            }
            else
            {
                TargetTime--;
                if (TargetTime <= 0)
                    Target = null;
            }

            if (Target != null) //Engaged
            {
                float dx = Target.Position.X - Position.X;

                if (CurrentAction is ActionHidden)
                {
                    CurrentAction = new ActionUnhide(this, 300);
                }
                else if (CurrentAction is ActionHide)
                {

                }
                else if (CurrentAction is ActionUnhide)
                {

                }
                else if (CurrentAction is ActionIdle idle)
                {
                    if (dx < 0)
                    {
                        Facing = HorizontalFacing.Left;
                    }
                    else if (dx > 0)
                    {
                        Facing = HorizontalFacing.Right;
                    }

                    if (idle.Time > 180)
                    {
                        if (Random.NextDouble() < 0.4)
                            CurrentAction = new ActionSpit(this, Target, new Vector2(0, -70), 60, 20, 20, 20);
                        else
                            CurrentAction = new ActionBite(this, 40, 20, 30);
                    }
                }
            }
            else
            {
                if(CurrentAction is ActionHide)
                {

                }
                else if (CurrentAction is ActionUnhide)
                {

                }
                else if(CurrentAction is ActionIdle)
                {
                    CurrentAction = new ActionHide(this, 80);
                }
            }
        }

        protected override void UpdateDelta(float delta)
        {
            MoveDelta = Math.Min(MoveDelta + delta, 1.0f);
            Lifetime += delta;

            /*Vector2 wantedPosition = Position + new Vector2(-10, -30) + new Vector2((float)Math.Sin(Lifetime / 20f) * 40, (float)Math.Cos(Lifetime / 30f) * 20);

            wantedPosition = World.Player.Position;
            var wantedOffset = wantedPosition - Position;
            wantedOffset = Math.Min(wantedOffset.Length(), 80f) * Vector2.Normalize(wantedOffset);
            Move(wantedOffset);*/

            CurrentAction.UpdateDelta(delta);

            Box.Teleport(Position.X + Head.Offset.X - Box.Width / 2, Position.Y + Head.Offset.Y - Box.Height / 2);
        }

        private void Move(Vector2 offset, float speed)
        {
            float lastAngle = (float)Math.Atan2(offset.X, offset.Y);
            float lastDistance = offset.Length();
            foreach (SnakeSegment segment in Segments)
            {
                float angle = segment.EndAngle;
                float distance = segment.EndDistance;
                segment.MoveTowards(lastAngle, lastDistance, speed);
                lastAngle = angle;
                lastDistance = distance;
            }
        }

        protected override void UpdateDiscrete()
        {
            MoveDelta = 0;

            if (!(CurrentAction is ActionHit))
                Invincibility--;

            foreach (SnakeSegment segment in Segments)
            {
                segment.UpdateDiscrete();
            }

            CurrentAction.UpdateDiscrete();

            PositionLast = Position;

            UpdateAI();
        }

        public override bool NoFriendlyFire(object hit)
        {
            return hit is Snake;
        }

        public override void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            if (Invincibility > 0 || Dead)
                return;
            Invincibility = invincibility/10;
            CurrentAction = new ActionHit(this, velocity * 4, hurttime);
            PlaySFX(sfx_player_hurt, 1.0f, 0.1f, 0.3f);
            HandleDamage(damageIn);
            World.Hitstop = 6;
        }

        public override void ShowDamage(double damage)
        {
            new DamagePopup(World, Position + Head.Offset + new Vector2(0, -16), damage.ToString(), 30);
        }

        public override void Death()
        {
            if (!(CurrentAction is ActionDeath))
            {
                new SnakeHead(World, Position + Head.Offset, GetFacingVector(Facing)*2 + new Vector2(0, -4),Facing == HorizontalFacing.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, Facing == HorizontalFacing.Right ? 0.1f : -0.1f, 30);
                CurrentAction = new ActionDeath(this, GetFacingVector(Facing) * -24 + new Vector2(0, 0), 30);
            }
        }
    }

    class SnakeHydra : Snake
    {
        public Hydra Body;
        public int Index;

        public override Vector2 Position
        {
            get
            {
                return Body.NeckPosition;
            }
            set
            {
                //NOOP
            }
        }

        public override Vector2 IdleOffset => new Vector2(0,-20) + 15 * AngleToVector(MathHelper.PiOver2 + MathHelper.PiOver2 / Body.Heads.Count + MathHelper.Pi * Index / Body.Heads.Count);
        //public override Vector2 IdleCircle => new Vector2(15,5);

        public SnakeHydra(Hydra body, int index) : base(body.World, body.Position)
        {
            Body = body;
            Index = index;
        }

        public override void UpdateAI()
        {
            //NOOP
        }
    }

    class Hydra : EnemyGravity
    {
        public abstract class Action
        {
            public Hydra Hydra;

            public virtual bool MouthOpen => false;

            public Action(Hydra hydra)
            {
                Hydra = hydra;
            }

            public abstract void UpdateDelta(float delta);

            public abstract void UpdateDiscrete();
        }

        public class ActionIdle : Action
        {
            public float Time;

            public virtual float MinDistance => 160;
            public virtual float MaxDistance => 240;
            public override bool MouthOpen => true;

            public ActionIdle(Hydra hydra) : base(hydra)
            {

            }

            public override void UpdateDelta(float delta)
            {
                Time += delta;
            }

            public override void UpdateDiscrete()
            {
                //NOOP
            }
        }

        public class ActionAggressive : ActionIdle
        {
            public override float MinDistance => 60;
            public override float MaxDistance => 90;

            public ActionAggressive(Hydra hydra) : base(hydra)
            {
            }
        }

        public class ActionDeath : Action
        {
            public int Time;

            public ActionDeath(Hydra hydra, int time) : base(hydra)
            {
                Time = time;
            }

            public override void UpdateDelta(float delta)
            {
                //NOOP
            }

            public override void UpdateDiscrete()
            {
                Time--;
                if (Time < 0)
                {
                    Hydra.Destroy();
                }
                var size = Hydra.Box.Bounds.Size;
                if(Time % 3 == 0)
                    new BigFireEffect(Hydra.World, Hydra.Position - size / 2 + new Vector2(Hydra.Random.NextFloat() * size.X, Hydra.Random.NextFloat() * size.Y), 0, 10);
            }
        }

        public Vector2 NeckPosition => Position + GetFacingVector(Facing) * 8 + new Vector2(0, -8);

        public List<SnakeHydra> Heads = new List<SnakeHydra>();
        public Action CurrentAction;

        public float WalkFrame;
        public HorizontalFacing Facing;

        public override bool Attacking => false;
        public override bool Incorporeal => false;
        public override Vector2 HomingTarget => Position;
        public override bool Dead => false;

        public bool InCombat => Target != null;

        public Player Target;
        public int TargetTime;

        public Hydra(GameWorld world, Vector2 position) : base(world, position)
        {
            CurrentAction = new ActionIdle(this);
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x - 8, y - 8, 16, 16);
            Box.AddTags(CollisionTag.NoCollision);
            Box.Data = this;

            int heads = 3;
            for (int i = 0; i < heads; i++)
            {
                Heads.Add(new SnakeHydra(this,(i+1) % heads));
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            foreach (var head in Heads)
            {
                head.Destroy();
            }
            World.Remove(Box);
        }

        public override void ShowDamage(double damage)
        {
            //NOOP
        }

        private void Walk(float dx, float speedLimit)
        {
            float adjustedSpeedLimit = speedLimit;
            float baseAcceleraton = 0.03f;
            if (OnGround)
                baseAcceleraton *= GroundFriction;
            float acceleration = baseAcceleraton;

            if (dx < 0 && Velocity.X > -adjustedSpeedLimit)
                Velocity.X = Math.Max(Velocity.X - acceleration, -adjustedSpeedLimit);
            if (dx > 0 && Velocity.X < adjustedSpeedLimit)
                Velocity.X = Math.Min(Velocity.X + acceleration, adjustedSpeedLimit);
            if (Math.Sign(dx) == Math.Sign(Velocity.X))
                AppliedFriction = 1;

            WalkFrame += Velocity.X * 0.25f;
        }

        private void WalkConstrained(float dx, float speedLimit) //Same as walk but don't jump off cliffs
        {
            float offset = Math.Sign(dx);
            if (Math.Sign(dx) == Math.Sign(Velocity.X))
                offset *= Math.Max(1, Math.Abs(Velocity.X));
            var floor = World.FindTiles(Box.Bounds.Offset(new Vector2(16 * offset, 1)));
            if (!floor.Any())
                return;
            Walk(dx, speedLimit);
        }

        private void UpdateAI()
        {
            var viewSize = new Vector2(500, 50);
            RectangleF viewArea = new RectangleF(Position - viewSize / 2, viewSize);

            if (viewArea.Contains(World.Player.Position))
            {
                Target = World.Player;
                TargetTime = 200;
            }
            else
            {
                TargetTime--;
                if (TargetTime <= 0)
                    Target = null;
            }

            foreach (var head in Heads)
            {
                head.Target = Target;
            }

            if (Target != null) //Engaged
            {
                float dx = Target.Position.X - Position.X;
                float dy = Target.Position.Y - Position.Y;

                if(CurrentAction is ActionDeath)
                {

                }
                else if(CurrentAction is ActionIdle idle)
                {
                    if(idle.Time > 120)
                    {
                        if(Random.NextDouble() > 0.5)
                        {
                            CurrentAction = new ActionAggressive(this);
                        }
                        else
                        {
                            CurrentAction = new ActionIdle(this);
                        }
                    }

                    if (dx < 0)
                        Facing = HorizontalFacing.Left;
                    else if (dx > 0)
                        Facing = HorizontalFacing.Right;

                    foreach(var head in Heads)
                    {
                        if (head.CurrentAction is Snake.ActionIdle headIdle && headIdle.Time > 80)
                        {
                            head.Facing = Facing;
                            int attackingHeads = Heads.Count(x => !(x.CurrentAction is Snake.ActionIdle || x.CurrentAction is Snake.ActionDeath));
                            if (attackingHeads < 2)
                                SelectAttack(head);
                        }
                    }

                    float moveOffset = -Math.Sign(dx) * Target.Velocity.X * 32;
                    float preferredDistanceMin = idle.MinDistance + moveOffset;
                    float preferredDistanceMax = idle.MaxDistance + moveOffset;
                    if(Math.Abs(dx) > preferredDistanceMax * 1.5f)
                    {
                        WalkConstrained(dx, 1.0f);
                    }
                    else if (Math.Abs(dx) > preferredDistanceMax)
                    {
                        WalkConstrained(dx,0.5f);
                    }
                    else if (Math.Abs(dx) < preferredDistanceMin)
                    {
                        WalkConstrained(-dx,0.5f);
                    }
                }
            }
            else //Idle
            {
                
            }
        }

        private void SelectAttack(SnakeHydra head)
        {
            var weightedList = new WeightedList<Snake.Action>();
            weightedList.Add(new Snake.ActionIdle(head), 30);
            Vector2 dist = Target.Position - Position;
            if(Heads.Count(x => x.CurrentAction is Snake.ActionBite) < 2 && dist.LengthSquared() < 90*90)
                weightedList.Add(new Snake.ActionBite(head, 60 + Random.Next(60), 20, 60 + Random.Next(60)), 50);
            if (Heads.Count(x => x.CurrentAction is Snake.ActionSpit) < 1)
                weightedList.Add(new Snake.ActionSpit(head, Target, new Vector2(0, -70), 60, 20, 20, 20), 30);
            if (Heads.Count(x => x.CurrentAction is Snake.ActionBreath) < 1 && Math.Abs(dist.X) < 100)
                weightedList.Add(new Snake.ActionBreath(head, Target, 80, 120, 60), 30);
            head.CurrentAction = weightedList.GetWeighted(Random);
        }

        protected override void UpdateDelta(float delta)
        {
            if (Active)
            {
                HandleMovement(delta);

                CurrentAction.UpdateDelta(delta);
            }
        }

        protected override void UpdateDiscrete()
        {
            if (Active)
            {
                HandlePhysicsEarly();

                CurrentAction.UpdateDiscrete();

                UpdateAI();

                HandlePhysicsLate();

                if (Heads.All(x => x.Destroyed))
                {
                    Death();
                }
            }
        }

        public override void Death()
        {
            if(!(CurrentAction is ActionDeath))
                CurrentAction = new ActionDeath(this,50);
        }
    }

    abstract class Cannon : Enemy
    {
        public enum FireState
        {
            Idle,
            Charge,
            Fire,
        }

        public override bool Attacking => false;
        public override bool Incorporeal => false;
        public override Vector2 HomingTarget => Position;
        public override bool Dead => false;

        public FireState State;
        public float Angle = 0;
        public float DelayTime;
        public float IdleTime;
        public float ChargeTime;
        public float FireTime;

        public Vector2 FacingVector => new Vector2((float)Math.Sin(Angle), (float)Math.Cos(Angle));
        public Vector2 VarianceVector => new Vector2((float)Math.Sin(Angle + Math.PI / 2), (float)Math.Cos(Angle + Math.PI / 2));

        public Cannon(GameWorld world, Vector2 position, float angle) : base(world, position)
        {
            Angle = angle;
            Reset();
        }

        protected abstract void Reset();

        protected abstract void ShootStart();

        protected abstract void ShootTick();

        protected abstract void ShootEnd();

        protected override void UpdateDelta(float delta)
        {
            DelayTime -= delta;
            if (DelayTime <= 0)
            {
                switch (State)
                {
                    case (FireState.Idle):
                        IdleTime -= delta;
                        if (IdleTime <= 0)
                            State = FireState.Charge;
                        break;
                    case (FireState.Charge):
                        ChargeTime -= delta;
                        if (ChargeTime <= 0)
                        {
                            if (Active)
                                ShootStart();
                            State = FireState.Fire;
                        }
                        break;
                    case (FireState.Fire):
                        FireTime -= delta;
                        if (FireTime <= 0)
                        {
                            if (Active)
                                ShootEnd();
                            Reset();
                            State = FireState.Idle;
                        }
                        break;
                }
            }
        }

        protected override void UpdateDiscrete()
        {
            if(State == FireState.Fire && Active)
            {
                ShootTick();
            }
        }

        public override void ShowDamage(double damage)
        {
            //NOOP
        }
    }

    class CannonFire : Cannon
    {
        public CannonFire(GameWorld world, Vector2 position, float angle) : base(world, position, angle)
        {
        }

        protected override void Reset()
        {
            IdleTime = 50;
            ChargeTime = 30;
            FireTime = 60;
        }

        protected override void ShootStart()
        {
        }

        protected override void ShootTick()
        {
            if((int)FireTime % 5 == 0)
            new Fireball(World, Position + FacingVector * 8 + VarianceVector * (Random.NextFloat()-0.5f) * 12)
            {
                Velocity = FacingVector * (Random.NextFloat() * 0.5f + 0.5f),
                FrameEnd = 80,
                Shooter = this,
            };
        }

        protected override void ShootEnd()
        {

        }
    }

    class CannonPoisonBreath : Cannon
    {
        public CannonPoisonBreath(GameWorld world, Vector2 position, float angle) : base(world, position, angle)
        {
        }

        protected override void Reset()
        {
            IdleTime = 50;
            ChargeTime = 30;
            FireTime = 60;
        }

        protected override void ShootStart()
        {
        }

        protected override void ShootTick()
        {
            if ((int)FireTime % 5 == 0)
                new PoisonBreath(World, Position + FacingVector * 8)
                {
                    Velocity = FacingVector * 3.0f,
                    FrameEnd = 40,
                    Shooter = this,
                };
        }

        protected override void ShootEnd()
        {

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

        public override bool Attacking => true;
        public override bool Incorporeal => false;
        public override Vector2 HomingTarget => Position;
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
            if (hit.Box.Data is Enemy enemy)
            {
                var hitVelocity = (Offset - LastOffset);
                hitVelocity.Normalize();
                enemy.Hit(hitVelocity * 5, 40, 30, Damage);
            }
        }

        public override void ShowDamage(double damage)
        {
            new DamagePopup(World, Position + Offset + new Vector2(0,-16), damage.ToString(), 30);
        }
    }
}

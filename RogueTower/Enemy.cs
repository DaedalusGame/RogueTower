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

        public override RectangleF ActivityZone => new RectangleF(Position - new Vector2(1000, 600) / 2, new Vector2(1000, 600));

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

        public virtual void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            HandleDamage(damageIn);
        }
    }

    abstract class EnemyHuman : Enemy
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
        public HorizontalFacing Facing;

        public virtual float Gravity => 0.2f;
        public virtual float GravityLimit => 10f;
        public virtual float Acceleration => 0.25f;
        public virtual float SpeedLimit => 2;
        public bool OnGround;
        public bool InAir => !OnGround;
        public bool OnWall;
        public bool OnCeiling;
        public float GroundFriction = 1.0f;
        public float AppliedFriction;
        public int ExtraJumps = 0;
        public int Invincibility = 0;
        public float Lifetime;

        public virtual bool Strafing => false;
        public override bool Attacking => CurrentAction.Attacking;
        public override bool Incorporeal => CurrentAction.Incorporeal;

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

        protected override void UpdateDelta(float delta)
        {
            Lifetime += delta;

            CurrentAction.UpdateDelta(delta);

            var movement = CalculateMovement(delta);

            bool IsMovingVertically = Math.Abs(movement.Y) > 0.1;
            bool IsMovingHorizontally = Math.Abs(movement.X) > 0.1;

            IMovement move = Move(movement);

            var hits = move.Hits.Where(c => c.Normal != Vector2.Zero && !IgnoresCollision(c.Box));

            /*if (move.Hits.Any() && !hits.Any())
            {
                IsMovingHorizontally = false;
                IsMovingVertically = false;
            }*/

            if (!Incorporeal)
            {
                var nearbies = World.FindBoxes(Box.Bounds).Where(x => x.Data != this);
                foreach (var nearby in nearbies)
                {
                    if (nearby.Data is Enemy enemy && !enemy.Incorporeal)
                    {
                        float dx = enemy.Position.X - Position.X;
                        if (Math.Abs(dx) < 1)
                            dx = Random.NextFloat() - 0.5f;
                        if (dx > 0 && Velocity.X > -1)
                            Velocity.X = -1;
                        else if (dx < 0 && Velocity.X < 1)
                            Velocity.X = 1;
                    }
                }
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

            if(!Incorporeal)
                HandlePanicBox(move);
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
                if (Velocity.Y < 0)
                {

                }
                Velocity.Y = 0;
                AppliedFriction = CurrentAction.Friction;
                ExtraJumps = 0;
            }
            else //Drag
            {
                AppliedFriction = CurrentAction.Drag;
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

            HandleDamage();

            CurrentAction.UpdateDiscrete();
            HandleInput(); //For implementors

            Velocity.X *= AppliedFriction;

            if (CurrentAction.HasGravity && Velocity.Y < GravityLimit)
                Velocity.Y = Math.Min(GravityLimit, Velocity.Y + Gravity); //Gravity

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
                if (Box.Data is Enemy enemy)
                {
                    enemy.Hit(Util.GetFacingVector(Facing) + new Vector2(0, -2), 20, 50, damageIn);
                }
                if (Box.Data is Tile tile)
                {
                    tile.HandleTileDamage(damageIn);
                }
            }

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
                    return new CrossResponse(collision);
                return new SlideAdvancedResponse(collision);
            });
        }

        private void HandlePanicBox(IMovement move)
        {
            RectangleF panicBox = new RectangleF(move.Destination.X + 2, move.Destination.Y + 2, move.Destination.Width - 4, move.Destination.Height - 4);
            var found = World.FindBoxes(panicBox);
            if (found.Any() && found.Any(x => x != Box && !IgnoresCollision(x)))
            {
                Box.Teleport(move.Origin.X, move.Origin.Y);
            }
        }

        protected bool IgnoresCollision(IBox box)
        {
            return Incorporeal || box.HasTag(CollisionTag.NoCollision) || box.HasTag(CollisionTag.Character);
        }

        public float GetJumpVelocity(float height)
        {
            return (float)Math.Sqrt(2 * Gravity * height);
        }

        protected void UpdateGroundFriction()
        {
            var tiles = World.FindTiles(Box.Bounds.Offset(0, 1));
            if (tiles.Any())
                GroundFriction = tiles.Max(tile => tile.Friction);
            else
                GroundFriction = 1.0f;
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
            if (Invincibility > 0)
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

        class ActionTwohandSlash : Action
        {
            public enum SwingAction
            {
                UpSwing,
                DownSwing,
            }

            public SwingAction SlashAction;
            public float SlashUpTime;
            public float SlashDownTime;
            public bool Parried;

            public bool IsUpSwing => SlashAction == SwingAction.UpSwing;
            public bool IsDownSwing => SlashAction == SwingAction.DownSwing;

            public override float Friction => Parried ? 1 : base.Friction;
            public override float Drag => 1 - (1 - base.Drag) * 0.1f;

            public ActionTwohandSlash(EnemyHuman moaiMan, float upTime, float downTime) : base(moaiMan)
            {
                SlashUpTime = upTime;
                SlashDownTime = downTime;
            }

            public override void OnInput()
            {
                //NOOP
            }

            public override void GetPose(PlayerState basePose)
            {
                basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

                switch (SlashAction)
                {
                    default:
                    case (SwingAction.UpSwing):
                        basePose.LeftArm = ArmState.Angular(9);
                        basePose.RightArm = ArmState.Angular(11);
                        basePose.Weapon = WeaponState.WandOrange(MathHelper.ToRadians(-90 - 45));
                        break;
                    case (SwingAction.DownSwing):
                        basePose.Body = BodyState.Crouch(1);
                        basePose.LeftArm = ArmState.Angular(5);
                        basePose.RightArm = ArmState.Angular(3);
                        basePose.Weapon = WeaponState.WandOrange(MathHelper.ToRadians(45 + 22));
                        break;
                }
            }

            public override void UpdateDelta(float delta)
            {
                switch (SlashAction)
                {
                    case (SwingAction.UpSwing):
                        SlashUpTime -= delta;
                        if (SlashUpTime < 0)
                            Swing();
                        break;
                    case (SwingAction.DownSwing):
                        SlashDownTime -= delta;
                        if (SlashDownTime < 0)
                            Human.ResetState();
                        break;
                }
            }

            public override void UpdateDiscrete()
            {
                //NOOP
            }

            public virtual void Swing()
            {
                Vector2 Position = Human.Position;
                HorizontalFacing Facing = Human.Facing;
                Vector2 FacingVector = GetFacingVector(Facing);
                Vector2 PlayerWeaponOffset = Position + FacingVector * 14;
                Vector2 WeaponSize = new Vector2(14 / 2, 14 * 2);
                RectangleF weaponMask = new RectangleF(PlayerWeaponOffset - WeaponSize / 2, WeaponSize);
                if (true)
                {
                    Vector2 parrySize = new Vector2(22, 22);
                    bool success = Human.Parry(new RectangleF(Position + FacingVector * 8 - parrySize / 2, parrySize));
                    if (success)
                        Parried = true;
                }
                if(!Parried)
                    Human.SwingWeapon(weaponMask, 10);
                var effect = new SlashEffectRound(Human.World, () => Human.Position, 0.7f, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
                if (Parried)
                    effect.Frame = effect.FrameEnd / 2;
                SlashAction = SwingAction.DownSwing;
            }
        }

        class ActionWandBlast : Action
        {
            public enum SwingAction
            {
                UpSwing,
                DownSwing,
            }

            public Player Target;
            public SwingAction SlashAction;
            public float SlashUpTime;
            public float SlashDownTime;

            public ActionWandBlast(EnemyHuman moaiMan, Player target, float upTime, float downTime) : base(moaiMan)
            {
                Target = target;
                SlashUpTime = upTime;
                SlashDownTime = downTime;
                PlaySFX(sfx_wand_charge, 1.0f, 0.1f, 0.4f);
            }

            public override void OnInput()
            {
                //NOOP
            }

            public override void GetPose(PlayerState basePose)
            {
                basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

                switch (SlashAction)
                {
                    default:
                    case (SwingAction.UpSwing):
                        basePose.LeftArm = ArmState.Angular(9);
                        basePose.RightArm = ArmState.Angular(11);
                        basePose.Weapon = WeaponState.WandOrange(MathHelper.ToRadians(-90 - 45));
                        break;
                    case (SwingAction.DownSwing):
                        basePose.Body = BodyState.Crouch(1);
                        basePose.LeftArm = ArmState.Angular(0);
                        basePose.RightArm = ArmState.Angular(0);
                        basePose.Weapon = WeaponState.WandOrange(MathHelper.ToRadians(0));
                        break;
                }
            }

            public override void UpdateDelta(float delta)
            {
                switch (SlashAction)
                {
                    case (SwingAction.UpSwing):
                        SlashUpTime -= delta;
                        if (SlashUpTime < 0)
                            Fire();
                        break;
                    case (SwingAction.DownSwing):
                        SlashDownTime -= delta;
                        if (SlashDownTime < 0)
                            Human.ResetState();
                        break;
                }
            }

            public override void UpdateDiscrete()
            {
                //NOOP
            }

            public void Fire()
            {
                SlashAction = SwingAction.DownSwing;
                var facing = GetFacingVector(Human.Facing);
                var firePosition = Human.Position + facing * 10;
                var homing = Target.Position - firePosition;
                homing.Normalize();
                new SpellOrange(Human.World, firePosition)
                {
                    Velocity = homing * 3,
                    FrameEnd = 70,
                    Shooter = Human
                };
                PlaySFX(sfx_wand_orange_cast, 1.0f, 0.1f, 0.3f);
            }
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
            CurrentAction = new ActionIdle(this);
            Health = 80;
            CanDamage = true;
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
                    if ((Math.Abs(dx) >= 50 || Target.InAir || runningAway) && Math.Abs(dx) <= 70 && RangedCooldown < 0 && Target.Invincibility < 3)
                    {
                        CurrentAction = new ActionWandBlast(this, Target, 24, 12);
                        RangedCooldown = 60 + Random.Next(40);
                    }
                    else if (Math.Abs(dx) <= 30 && AttackCooldown < 0 && Target.Invincibility < 3 && Target.Box.Bounds.Intersects(attackZone) && !runningAway)
                    {
                        Velocity.X += Math.Sign(dx) * 2;
                        CurrentAction = new ActionTwohandSlash(this, 3, 12);
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

        public override void Update(float delta)
        {
            float modifier = 1.0f;
            if (CurrentAction is ActionEnemyDeath)
                modifier = 0.5f;
            base.Update(delta * modifier);
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
            return new PlayerState(
                HeadState.Forward,
                BodyState.Stand,
                ArmState.Angular(5),
                ArmState.Angular(3),
                WeaponState.WandOrange(MathHelper.ToRadians(270 - 20))
            );
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
    }

    class Snake : Enemy
    {
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

            public Action(Snake snake)
            {
                Snake = snake;
            }

            public virtual bool ShouldRenderSegment(SnakeSegment segment)
            {
                return true;
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
                Vector2 facingVector = GetFacingVector(Snake.Facing);
                Vector2 wantedPosition;
                if (Snake.InCombat)
                    wantedPosition = -10 * facingVector + new Vector2(0, -30) + new Vector2((float)Math.Sin(frame / 20f) * 20 * facingVector.X, (float)Math.Cos(frame / 20f) * 10);
                else
                    wantedPosition = -10 * facingVector + new Vector2(0, -15) + new Vector2((float)Math.Sin(frame / 20f) * 10 * facingVector.X, (float)Math.Cos(frame / 20f) * 5);
                Snake.Move(wantedPosition, 0.2f);
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
                            if (box == Snake.Box)
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

        public class ActionHit : Action
        {
            public int Time;
            public Vector2 Target;

            public ActionHit(Snake snake, Vector2 offset, int time) : base(snake)
            {
                Target = snake.Head.Offset + offset;
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

            public override bool ShouldRenderSegment(SnakeSegment segment)
            {
                return segment.Index < SegmentCut * Snake.Segments.Count;
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
        public SnakeSegment Head => Segments.Last();

        public Action CurrentAction;

        public float Lifetime;
        public int Invincibility = 0;

        public override bool Attacking => false;
        public override bool Incorporeal => false;

        public HorizontalFacing Facing;
        public float MoveDelta;

        public bool InCombat => Target != null;
        public Player Target;
        public int TargetTime;

        public Snake(GameWorld world, Vector2 position) : base(world, position)
        {
            CurrentAction = new ActionIdle(this);
            Health = 80;
            CanDamage = true;
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

        private void UpdateAI()
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

                if (CurrentAction is ActionIdle idle)
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
                        CurrentAction = new ActionBite(this, 40, 20, 30);
                }
            }
            else
            {

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

            UpdateAI();
        }

        public override void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            if (Invincibility > 0)
                return;
            Invincibility = invincibility;
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

        public BallAndChain(GameWorld world, Vector2 position, float angle, float speed, float distance) : base(world, position)
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

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

    class MoaiMan : Enemy
    {
        public abstract class Action
        {
            public MoaiMan MoaiMan;

            public virtual float Friction => 1 - (1 - 0.85f) * MoaiMan.GroundFriction;
            public virtual float Drag => 0.85f;

            public Action(MoaiMan moaiMan)
            {
                MoaiMan = moaiMan;
            }

            public abstract void GetPose(PlayerState basePose);

            public abstract void UpdateDelta(float delta);

            public abstract void UpdateDiscrete();
        }

        class ActionIdle : Action
        {
            public ActionIdle(MoaiMan moaiMan) : base(moaiMan)
            {

            }

            public override void GetPose(PlayerState basePose)
            {
                //NOOP
            }

            public override void UpdateDelta(float delta)
            {
                if (Math.Abs(MoaiMan.Velocity.X) >= 0.01)
                    MoaiMan.CurrentAction = new ActionMove(MoaiMan);
            }

            public override void UpdateDiscrete()
            {
                //NOOP
            }
        }

        class ActionMove : Action
        {
            public float WalkFrame;
            public bool Walking;

            public ActionMove(MoaiMan moaiMan) : base(moaiMan)
            {
            }

            public override void GetPose(PlayerState basePose)
            {
                basePose.Body = BodyState.Walk((int)WalkFrame);
            }

            public override void UpdateDelta(float delta)
            {
                if (Walking)
                {
                    var facingMod = MoaiMan.Facing == HorizontalFacing.Left ? -1 : 1;
                    WalkFrame += facingMod * MoaiMan.Velocity.X * delta * 0.125f / (float)Math.Sqrt(MoaiMan.GroundFriction);
                }
                if (Math.Abs(MoaiMan.Velocity.X) < 0.01)
                    MoaiMan.CurrentAction = new ActionIdle(MoaiMan);
            }

            public override void UpdateDiscrete()
            {
                //NOOP
            }
        }

        class ActionAttack : Action
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

            public ActionAttack(MoaiMan moaiMan, float upTime, float downTime) : base(moaiMan)
            {
                SlashUpTime = upTime;
                SlashDownTime = downTime;
            }

            public override void GetPose(PlayerState basePose)
            {
                basePose.Body = !MoaiMan.InAir ? BodyState.Stand : BodyState.Walk(1);

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
                            MoaiMan.ResetState();
                        break;
                }
            }

            public override void UpdateDiscrete()
            {
                //NOOP
            }

            public virtual void Swing()
            {
                Vector2 Position = MoaiMan.Position;
                HorizontalFacing Facing = MoaiMan.Facing;
                Vector2 FacingVector = GetFacingVector(Facing);
                Vector2 PlayerWeaponOffset = Position + FacingVector * 14;
                Vector2 WeaponSize = new Vector2(14 / 2, 14 * 2);
                RectangleF weaponMask = new RectangleF(PlayerWeaponOffset - WeaponSize / 2, WeaponSize);
                if (true)
                {
                    Vector2 parrySize = new Vector2(22, 22);
                    bool success = MoaiMan.Parry(new RectangleF(Position + FacingVector * 8 - parrySize / 2, parrySize));
                    if (success)
                        Parried = true;
                }
                if(!Parried)
                    MoaiMan.SwingWeapon(weaponMask, 10);
                var effect = new SlashEffectRound(MoaiMan.World, () => MoaiMan.Position, 0.7f, 0, MoaiMan.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
                if (Parried)
                    effect.Frame = effect.FrameEnd / 2;
                SlashAction = SwingAction.DownSwing;
            }
        }

        class ActionRanged : Action
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

            public ActionRanged(MoaiMan moaiMan, Player target, float upTime, float downTime) : base(moaiMan)
            {
                Target = target;
                SlashUpTime = upTime;
                SlashDownTime = downTime;
                PlaySFX(sfx_wand_charge, 1.0f, 0.1f, 0.4f);
            }

            public override void GetPose(PlayerState basePose)
            {
                basePose.Body = !MoaiMan.InAir ? BodyState.Stand : BodyState.Walk(1);

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
                            MoaiMan.ResetState();
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
                var facing = GetFacingVector(MoaiMan.Facing);
                var firePosition = MoaiMan.Position + facing * 10;
                var homing = Target.Position - firePosition;
                homing.Normalize();
                new SpellOrange(MoaiMan.World, firePosition)
                {
                    Velocity = homing * 3,
                    FrameEnd = 70,
                    Shooter = MoaiMan
                };
                PlaySFX(sfx_wand_orange_cast, 1.0f, 0.1f, 0.3f);
            }
        }

        class ActionHit : Action
        {
            int Time;

            public override float Drag => 1;

            public ActionHit(MoaiMan moaiMan, int time) : base(moaiMan)
            {
                Time = time;
            }

            public override void GetPose(PlayerState basePose)
            {
                basePose.Head = HeadState.Down;
                basePose.Body = BodyState.Hit;
                basePose.RightArm = ArmState.Angular(3);
            }

            public override void UpdateDelta(float delta)
            {
                //NOOP
            }

            public override void UpdateDiscrete()
            {
                Time--;
                if (Time <= 0)
                {
                    MoaiMan.ResetState();
                }
            }
        }

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

        public Action CurrentAction;

        public float Gravity = 0.2f;
        public float GravityLimit = 10f;
        public float Acceleration = 0.25f;
        public float SpeedLimit = 3.0f;
        public bool OnGround;
        public bool InAir => !OnGround;
        public bool OnWall;
        public bool OnCeiling;
        public float GroundFriction = 1.0f;
        public float AppliedFriction;

        public override bool Attacking => CurrentAction is ActionAttack;

        public float Lifetime;
        public int Invincibility = 0;
        public int AttackCooldown = 0;
        public int RangedCooldown = 0;

        public HorizontalFacing Facing;

        public Player Target;
        public int TargetTime;

        public MoaiMan(GameWorld world, Vector2 position) : base(world, position)
        {
            CurrentAction = new ActionIdle(this);
            CanDamage = true;
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x, y, 12, 14);
            Box.Data = this;
            Box.AddTags(CollisionTag.Character);
        }

        public void ResetState()
        {
            if (OnGround)
            {
                CurrentAction = new ActionIdle(this);
            }
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
                if (IgnoresCollision(collision.Hit.Box))
                    return new CrossResponse(collision);
                return new SlideAdvancedResponse(collision);
            });
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
                move.Walking = dx != 0;
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

        public bool Parry(RectangleF hitmask)
        {
            //new RectangleDebug(World, hitmask, Color.Orange, 20);
            var affectedHitboxes = World.FindBoxes(hitmask);
            foreach (Box Box in affectedHitboxes)
            {
                if (Box.Data is Player player && player.Attacking)
                {
                    PlaySFX(sfx_sword_bink, 1.0f, -0.3f, -0.5f);
                    World.Hitstop = 15;
                    Invincibility = 10;
                    if (OnGround)
                    {
                        Velocity += GetFacingVector(Facing) * -2;
                    }
                    else
                    {
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
            var affectedHitboxes = World.FindBoxes(hitmask).ToList();
            foreach (Box Box in affectedHitboxes)
            {
                if (Box.Data is Player player)
                {
                    player.Hit(Util.GetFacingVector(Facing) + new Vector2(0, -2), 20, 50, damageIn);
                }
                if (Box.Data is Tile tile)
                {
                    tile.HandleTileDamage(damageIn);
                }
            }

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

                if (CurrentAction is ActionAttack)
                {
                    RangedCooldown--;
                }
                else if (CurrentAction is ActionHit)
                {

                }
                else if (CurrentAction is ActionRanged)
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
                        move.Walking = false;
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
                        CurrentAction = new ActionRanged(this, Target, 24, 12);
                        RangedCooldown = 60 + Random.Next(40);
                    }
                    if (Math.Abs(dx) <= 30 && AttackCooldown < 0 && Target.Invincibility < 3 && Target.Box.Bounds.Intersects(attackZone) && !runningAway)
                    {
                        Velocity.X += Math.Sign(dx) * 2;
                        CurrentAction = new ActionAttack(this, 3, 12);
                        AttackCooldown = 30;
                    }
                }

                AttackCooldown--;
                RangedCooldown--;
            }
            else //Idle
            {

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

            RectangleF panicBox = new RectangleF(move.Destination.X + 2, move.Destination.Y + 2, move.Destination.Width - 4, move.Destination.Height - 4);
            var found = World.Find(panicBox);
            if (found.Any(x => x != Box && !IgnoresCollision(x) && x.Bounds.Intersects(Box.Bounds)))
            {
                Box.Teleport(move.Origin.X, move.Origin.Y);
            }
        }

        private bool IgnoresCollision(IBox box)
        {
            return box.HasTag(CollisionTag.NoCollision) || box.HasTag(CollisionTag.Character);
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
                AppliedFriction = CurrentAction.Friction;
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

            UpdateAI();
            CurrentAction.UpdateDiscrete();

            Velocity.X *= AppliedFriction;

            if (Velocity.Y < GravityLimit)
                Velocity.Y = Math.Min(GravityLimit, Velocity.Y + Gravity); //Gravity
        }

        private void HandleDamage()
        {
            if (!(CurrentAction is ActionHit))
                Invincibility--;
        }

        public override void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            if (CurrentAction is ActionAttack slash && slash.IsUpSwing)
            {
                //Parry
                slash.Swing();
                return;
            }
            if (Invincibility > 0)
                return;
            Velocity = velocity;
            OnGround = false;
            Invincibility = 1;
            CurrentAction = new ActionHit(this, hurttime);
            PlaySFX(sfx_player_hurt, 1.0f, 0.2f, 0.7f);
            HandleDamage(damageIn);
            World.Hitstop = 6;
        }

        public override void ShowDamage(double damage)
        {
            new DamagePopup(World, Position + new Vector2(0, -16), damage.ToString(), 30);
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
            new DamagePopup(World, Position + Offset + new Vector2(0,-16), damage.ToString(), 30);
        }
    }
}

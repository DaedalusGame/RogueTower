using Humper.Base;
using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Actions.Death;
using RogueTower.Actions.Hurt;
using RogueTower.Actions.Movement;
using RogueTower.Items;
using RogueTower.Items.Weapons;
using System;
using System.Linq;
using static RogueTower.Util;

namespace RogueTower.Enemies
{
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
            //Weapon = new WeaponKatana(15, new Vector2(10, 40));
            //Weapon = new WeaponRapier(15, new Vector2(10, 40));
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
                offset *= Math.Max(1, Math.Abs(Velocity.X));
            var floor = World.FindTiles(Box.Bounds.Offset(new Vector2(16 * offset, 1)));
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
                    if (Target.InAir)
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
                    if ((Math.Abs(dx) >= 50 || Target.InAir || runningAway) && Math.Abs(dx) <= 70 && RangedCooldown < 0 && Target.Invincibility < 3) //Ranged
                    {
                        //Begin Weapon Ranged Attack Checks
                        if (Weapon is WeaponWand wand)
                            CurrentAction = new ActionWandBlastHoming(this, Target, 24, 12, wand);
                        else if (Weapon is WeaponKnife)
                        {
                            if (Target.Position.Y < Position.Y)
                            {
                                Velocity.Y = -4;
                            }
                            if (Target.Position.Y <= Position.Y)
                                CurrentAction = new ActionKnifeThrow(this, 2, 4, 8, 2, Weapon);
                        }
                        else if (Weapon is WeaponRapier)
                        {
                            if (OnGround)
                            {
                                Velocity.Y = -2.5f;
                                OnGround = false;
                            }
                            CurrentAction = new ActionDashAttack(this, 2, 6, 2, 4, false, false, new ActionStab(this, 4, 2, Weapon));
                        }
                        else if (Weapon is WeaponBoomerang boomerang)
                            CurrentAction = new ActionBoomerangThrow(this, 10, boomerang, 40) { Angle = VectorToAngle(Target.Position - Position) };
                        else if (Weapon is WeaponWarhammer)
                        {
                            if (OnGround)
                            {
                                Velocity.Y = -5;
                                OnGround = false;
                            }
                            CurrentAction = new ActionShockwave(this, 4, 8, Weapon, 2);
                        }

                        RangedCooldown = 60 + Random.Next(40);
                    }
                    else if (Math.Abs(dx) <= 30 && AttackCooldown < 0 && Target.Invincibility < 3 && Target.Box.Bounds.Intersects(attackZone) && !runningAway) //Melee
                    {
                        Velocity.X += Math.Sign(dx) * 2;

                        //Begin Weapon Melee Attack Checks
                        if (Weapon is WeaponUnarmed && !(Target.Weapon is WeaponUnarmed))
                        {
                            CurrentAction = new ActionStealWeapon(this, Target, 4, 8);
                        }
                        else if (Weapon is WeaponSword)
                        {
                            ActionBase[] actionHolder =
                            {
                                new ActionSlash(this, 2, 4, 8, 2, Weapon),
                                new ActionSlashUp(this, 2, 4, 8, 2, Weapon)
                            };
                            CurrentAction = actionHolder[Random.Next(0, actionHolder.Length - 1)];
                        }
                        else if (Weapon is WeaponBoomerang boomerang)
                        {
                            if (boomerang.BoomerProjectile == null || boomerang.BoomerProjectile.Destroyed)
                            {
                                CurrentAction = new ActionSlash(this, 2, 4, 4, 2, Weapon);
                            }
                        }
                        else if (Weapon is WeaponKatana katana)
                        {
                            if (katana.Sheathed)
                            {
                                CurrentAction = new ActionKatanaSlash(this, 2, 4, 12, 4, katana);
                            }
                        }
                        else if (Weapon is WeaponKnife)
                        {
                            ActionBase[] actionHolder =
                            {
                                    new ActionStab(this, 4, 10, Weapon),
                                    new ActionDownStab(this, 4, 10, Weapon)
                            };
                            CurrentAction = actionHolder[Random.Next(0, actionHolder.Length - 1)];
                        }
                        else if (Weapon is WeaponRapier)
                        {
                            Velocity.X += OnGround ? GetFacingVector(Facing).X * 0.75f : GetFacingVector(Facing).X * 0.5f;
                            if (OnGround)
                            {
                                Velocity.Y = -GetJumpVelocity(8);
                                OnGround = false;
                            }
                            CurrentAction = new ActionRapierThrust(this, 4, 8, Weapon);
                        }
                        else if (Weapon is WeaponWandOrange)
                        {
                            CurrentAction = new ActionWandSwing(this, 10, 5, 20);
                        }
                        else if (Weapon is WeaponWarhammer)
                        {
                            if (OnGround)
                            {
                                CurrentAction = new ActionTwohandSlash(this, 3, 12, Weapon);
                            }
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

                switch (Idle)
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

                if (OnWall)
                {
                    if (Idle == IdleState.MoveLeft)
                        Idle = IdleState.MoveRight;
                    else if (Idle == IdleState.MoveRight)
                        Idle = IdleState.MoveLeft;
                    OnWall = false;
                    IdleTime = 70;
                }

                if (IdleTime <= 0)
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
                Weapon.GetWeaponState(this, MathHelper.ToRadians(270 - 20)),
                ShieldState.None
            );
            Weapon.GetPose(this, pose);
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
            base.Death();
            if (!(CurrentAction is ActionEnemyDeath))
                CurrentAction = new ActionEnemyDeath(this, 20);
        }

        public override void DropItems(Vector2 position)
        {
            new DroppedItem(World, position, Meat.Moai).Spread();
            new DroppedItem(World, position, new CurseMedal()).Spread();
            if (Random.NextDouble() > 0.75)
            {
                new DroppedItem(World, position, Weapon).Spread();
            }
        }

        public override void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            base.Hit(velocity, hurttime, invincibility / 10, damageIn);
        }
    }

}

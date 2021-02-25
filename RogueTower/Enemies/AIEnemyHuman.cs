using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Actions.Death;
using RogueTower.Actions.Hurt;
using RogueTower.Actions.Movement;
using RogueTower.Items.Weapons;
using System;
using System.Linq;
using static RogueTower.Util;

namespace RogueTower.Enemies
{
    class AIEnemyHuman
    {
        enum IdleState
        {
            Wait,
            MoveLeft,
            MoveRight,
        }

        public EnemyHuman Owner;
        public Vector2 Position => Owner.Position;
        public GameWorld World => Owner.World;
        public ActionBase CurrentAction { get { return Owner.CurrentAction; } set { Owner.CurrentAction = value; } }
        public Weapon Weapon => Owner.Weapon;

        public bool OnCeiling { get { return Owner.OnCeiling; } set { Owner.OnCeiling = value; } }
        public bool OnWall { get { return Owner.OnWall; } set { Owner.OnWall = value; } }
        public bool OnGround { get { return Owner.OnGround; } set { Owner.OnGround = value; } }

        public int AttackCooldown = 0;
        public int RangedCooldown = 0;

        public bool InCombat => Target != null;

        public Player Target;
        public int TargetTime;

        IdleState Idle;
        int IdleTime;

        public AIEnemyHuman(EnemyHuman owner)
        {
            Owner = owner;
        }

        public void UpdateAI()
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
                Vector2 DifferenceVector = Target.Position - Position;

                //float dx = Target.Position.X - Position.X;
                //float dy = Target.Position.Y - Position.Y;

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
                    if (DifferenceVector.X < 0)
                        Owner.Facing = HorizontalFacing.Left;
                    else if (DifferenceVector.X > 0)
                        Owner.Facing = HorizontalFacing.Right;

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
                    if (Math.Abs(DifferenceVector.X) > preferredDistanceMax)
                    {
                        Owner.WalkConstrained(DifferenceVector.X);
                    }
                    if (Math.Abs(DifferenceVector.X) < preferredDistanceMin)
                    {
                        Owner.WalkConstrained(-DifferenceVector.X);
                    }
                    var attackSize = new Vector2(40, 30);
                    var attackZone = new RectangleF(Position + GetFacingVector(Owner.Facing) * 20 - attackSize / 2, attackSize);
                    bool runningAway = Math.Abs(Target.Velocity.X) > 1 && Math.Abs(DifferenceVector.X) > 30 && Math.Sign(Target.Velocity.X) == Math.Sign(DifferenceVector.X);
                    if ((Math.Abs(DifferenceVector.X) >= 50 || Target.InAir || runningAway) && Math.Abs(DifferenceVector.X) <= 70 && RangedCooldown < 0 && Target.Invincibility < 3) //Ranged
                    {
                        //Begin Weapon Ranged Attack Checks
                        if (Weapon is WeaponWand wand)
                            CurrentAction = new ActionWandBlastHoming(Owner, Target, 24, 12, wand);
                        else if (Weapon is WeaponKnife)
                        {
                            if (Target.Position.Y < Position.Y)
                            {
                                Owner.Velocity.Y = -4;
                            }
                            if (Target.Position.Y <= Position.Y)
                                CurrentAction = new ActionKnifeThrow(Owner, 2, 4, 8, 2, Weapon);
                        }
                        else if (Weapon is WeaponRapier)
                        {
                            if (OnGround)
                            {
                                Owner.Velocity.Y = -2.5f;
                                OnGround = false;
                            }
                            CurrentAction = new ActionDashAttack(Owner, 2, 6, 2, 4, false, false, new ActionStab(Owner, 4, 2, Weapon));
                        }
                        else if (Weapon is WeaponBoomerang boomerang)
                            CurrentAction = new ActionBoomerangThrow(Owner, 10, boomerang, 40) { Angle = VectorToAngle(Target.Position - Position) };
                        else if (Weapon is WeaponWarhammer)
                        {
                            if (OnGround)
                            {
                                Owner.Velocity.Y = -5;
                                OnGround = false;
                            }
                            CurrentAction = new ActionShockwave(Owner, 4, 8, Weapon, 2);
                        }

                        RangedCooldown = 60 + Owner.Random.Next(40);
                    }
                    else if (Math.Abs(DifferenceVector.X) <= 30 && AttackCooldown < 0 && Target.Invincibility < 3 && Target.Box.Bounds.Intersects(attackZone) && !runningAway) //Melee
                    {
                        Owner.Velocity.X += Math.Sign(DifferenceVector.X) * 2;

                        //Begin Weapon Melee Attack Checks
                        if (Weapon is WeaponUnarmed && !(Target.Weapon is WeaponUnarmed))
                        {
                            CurrentAction = new ActionStealWeapon(Owner, Target, 4, 8);
                        }
                        else if (Weapon is WeaponSword)
                        {
                            ActionBase[] actionHolder =
                            {
                                new ActionSlash(Owner, 2, 4, 8, 2, Weapon),
                                new ActionSlashUp(Owner, 2, 4, 8, 2, Weapon)
                            };
                            CurrentAction = actionHolder[Owner.Random.Next(0, actionHolder.Length - 1)];
                        }
                        else if (Weapon is WeaponBoomerang boomerang)
                        {
                            if (boomerang.BoomerProjectile == null || boomerang.BoomerProjectile.Destroyed)
                            {
                                CurrentAction = new ActionSlash(Owner, 2, 4, 4, 2, Weapon);
                            }
                        }
                        else if (Weapon is WeaponKatana katana)
                        {
                            if (katana.Sheathed)
                            {
                                CurrentAction = new ActionKatanaSlash(Owner, 2, 4, 12, 4, katana);
                            }
                        }
                        else if (Weapon is WeaponKnife)
                        {
                            ActionBase[] actionHolder =
                            {
                                    new ActionStab(Owner, 4, 10, Weapon),
                                    new ActionDownStab(Owner, 4, 10, Weapon)
                            };
                            CurrentAction = actionHolder[Owner.Random.Next(0, actionHolder.Length - 1)];
                        }
                        else if (Weapon is WeaponRapier)
                        {
                            Owner.Velocity.X += OnGround ? GetFacingVector(Owner.Facing).X * 0.75f : GetFacingVector(Owner.Facing).X * 0.5f;
                            if (OnGround)
                            {
                                Owner.Velocity.Y = -Owner.GetJumpVelocity(8);
                                OnGround = false;
                            }
                            CurrentAction = new ActionRapierThrust(Owner, 4, 8, Weapon);
                        }
                        else if (Weapon is WeaponWandOrange)
                        {
                            CurrentAction = new ActionWandSwing(Owner, 10, 5, 20);
                        }
                        else if (Weapon is WeaponWarhammer)
                        {
                            if (OnGround)
                            {
                                CurrentAction = new ActionTwohandSlash(Owner, 3, 12, Weapon);
                            }
                        }
                        else
                        {
                            CurrentAction = new ActionTwohandSlash(Owner, 3, 12, Weapon);
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
                        Owner.Facing = HorizontalFacing.Left;
                        Owner.WalkConstrained(-1);
                        break;
                    case (IdleState.MoveRight):
                        Owner.Facing = HorizontalFacing.Right;
                        Owner.WalkConstrained(1);
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
                    IdleTime = Owner.Random.Next(50) + 20;
                    Idle = nextState.GetWeighted(Owner.Random);
                }
            }
        }
    }
}

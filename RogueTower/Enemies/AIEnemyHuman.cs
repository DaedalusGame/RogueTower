using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Actions.Death;
using RogueTower.Actions.Hurt;
using RogueTower.Actions.Movement;
using RogueTower.Enemies.WeaponAIs;
using RogueTower.Items.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using static RogueTower.Util;

namespace RogueTower.Enemies
{
    class AIEnemyHuman
    {
        static List<WeaponAI> WeaponAIs = new List<WeaponAI>();

        static AIEnemyHuman()
        {
            WeaponAIs.Add(new WeaponAISword());
            WeaponAIs.Add(new WeaponAIWandSwing());
            WeaponAIs.Add(new WeaponAIWandBlast());
            WeaponAIs.Add(new WeaponAIHammerSwing());
            WeaponAIs.Add(new WeaponAIHammerSlam());
            WeaponAIs.Add(new WeaponAIBoomerangSwing());
            WeaponAIs.Add(new WeaponAIBoomerangThrow());
            WeaponAIs.Add(new WeaponAIKnifeStab());
            WeaponAIs.Add(new WeaponAIKnifeThrow());
            WeaponAIs.Add(new WeaponAIRapierThrust());
            WeaponAIs.Add(new WeaponAIRapierDash());
            WeaponAIs.Add(new WeaponAIKatanaUnsheathe());
            WeaponAIs.Add(new WeaponAIStealing());
            WeaponAIs.Add(new WeaponAIDefault());
        }

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
        public Random Random => Owner.Random;
        public Vector2 DifferenceVector => Target.Position - Owner.Position;

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

        public bool RangedWhenFleeing = true;
        public bool RangedWhenAerial = true;

        public AIEnemyHuman(EnemyHuman owner)
        {
            Owner = owner;
        }

        public bool IsAttacking()
        {
            return GetWeaponAIs(Weapon).Any(ai => ai.IsAttackState(CurrentAction));
        }

        private IEnumerable<WeaponAI> GetWeaponAIs(Weapon weapon)
        {
            return WeaponAIs.Where(ai => ai.IsValid(weapon));
        }

        public RectangleF GetAttackZone(Vector2 offset, Vector2 size)
        {
            return RectangleF.Centered(Position + GetFacingVector(Owner.Facing) * offset.X + new Vector2(0, offset.Y), size);
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
                if (CurrentAction is ActionHit)
                {

                }
                else if (CurrentAction is ActionEnemyDeath)
                {

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
                    foreach (var weaponAI in GetWeaponAIs(Weapon))
                    {
                        weaponAI.Update(this);
                    }

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

        public bool IsTargetRunningAway(Player target)
        {
            var differenceVector = target.Position - Position;
            return Math.Abs(target.Velocity.X) > 1 && Math.Abs(differenceVector.X) > 30 && Math.Sign(target.Velocity.X) == Math.Sign(differenceVector.X);
        }

        public bool ShouldUseRangedAttack(Player target)
        {
            if (RangedWhenAerial && target.InAir)
                return true;
            if (RangedWhenFleeing && IsTargetRunningAway(target))
                return true;
            return false;
        }

        public bool ShouldUseMeleeAttack(Player target)
        {
            return !IsTargetRunningAway(target);
        }
    }
}

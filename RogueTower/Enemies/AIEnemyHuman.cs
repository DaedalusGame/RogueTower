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
using System.Collections.Generic;
using System.Linq;
using static RogueTower.Util;

namespace RogueTower.Enemies
{
    abstract class WeaponAI
    {
        public int RangedMinRange = 50;
        public int RangedMaxRange = 70;

        public abstract bool IsAttackState(ActionBase action);

        public abstract bool IsValid(Weapon weapon);

        protected bool IsInRangedRange(AIEnemyHuman ai)
        {
            var differenceVector = ai.Target.Position - ai.Position;
            return (Math.Abs(differenceVector.X) >= RangedMinRange || ai.ShouldUseRangedAttack(ai.Target)) && Math.Abs(differenceVector.X) <= RangedMaxRange;
        }

        protected bool IsInMeleeRange(AIEnemyHuman ai, RectangleF attackZone)
        {
            return ai.Target.Box.Bounds.Intersects(attackZone) && ai.ShouldUseMeleeAttack(ai.Target);
        }

        public abstract void Update(AIEnemyHuman ai);
    }

    class WeaponAISword : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionSlash;

        public override bool IsValid(Weapon weapon) => weapon is WeaponSword;

        public override void Update(AIEnemyHuman ai)
        {
            var attackZone = ai.GetAttackZone(new Vector2(20, 0), new Vector2(40, 30));

            if (!ai.IsAttacking() && IsInMeleeRange(ai, attackZone) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                ai.Owner.Velocity.X += Math.Sign(ai.DifferenceVector.X) * 2;

                ActionBase[] actionHolder =
                {
                    new ActionSlash(ai.Owner, 2, 4, 8, 2, ai.Weapon),
                    new ActionSlashUp(ai.Owner, 2, 4, 8, 2, ai.Weapon)
                };
                ai.CurrentAction = actionHolder[ai.Random.Next(0, actionHolder.Length - 1)];
                ai.AttackCooldown = 30;
            }
        }
    }  

    class WeaponAIWandBlast : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionWandBlast;

        public override bool IsValid(Weapon weapon) => weapon is WeaponWand;

        public override void Update(AIEnemyHuman ai)
        {
            if (ai.CurrentAction is ActionWandBlast)
            {
                ai.AttackCooldown--;
            }

            var wand = ai.Weapon as WeaponWand;

            if (!ai.IsAttacking() && IsInRangedRange(ai) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                ai.CurrentAction = new ActionWandBlastHoming(ai.Owner, ai.Target, 24, 12, wand);
                ai.RangedCooldown = 60 + ai.Random.Next(40);
            }
        }
    }

    class WeaponAIWand : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionWandSwing;

        public override bool IsValid(Weapon weapon) => weapon is WeaponWand;

        public override void Update(AIEnemyHuman ai)
        {
            var attackZone = ai.GetAttackZone(new Vector2(20, 0), new Vector2(40, 30));

            if (!ai.IsAttacking() && IsInMeleeRange(ai, attackZone) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                ai.Owner.Velocity.X += Math.Sign(ai.DifferenceVector.X) * 2;
                ai.CurrentAction = new ActionWandSwing(ai.Owner, 10, 5, 20);
                ai.AttackCooldown = 30;
            }
        }
    }

    class WeaponAIHammer : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionTwohandSlash;

        public override bool IsValid(Weapon weapon) => weapon is WeaponWarhammer;

        public override void Update(AIEnemyHuman ai)
        {
            var attackZone = ai.GetAttackZone(new Vector2(20, 0), new Vector2(40, 30));

            if (!ai.IsAttacking() && ai.OnGround && IsInMeleeRange(ai, attackZone) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                ai.Owner.Velocity.X += Math.Sign(ai.DifferenceVector.X) * 2;
                ai.CurrentAction = new ActionTwohandSlash(ai.Owner, 3, 12, ai.Weapon);
                ai.AttackCooldown = 30;
            }
        }
    }

    class WeaponAIHammerSlam : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionPlunge;

        public override bool IsValid(Weapon weapon) => weapon is WeaponWarhammer;

        public override void Update(AIEnemyHuman ai)
        {
            if (!ai.IsAttacking() && IsInRangedRange(ai) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                if (ai.OnGround)
                {
                    ai.Owner.Velocity.Y = -5;
                    ai.OnGround = false;
                }
                ai.CurrentAction = new ActionShockwave(ai.Owner, 4, 8, ai.Weapon, 2);
                ai.RangedCooldown = 60 + ai.Random.Next(40);
            }
        }
    }

    class WeaponAIRapier : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionRapierThrust;

        public override bool IsValid(Weapon weapon) => weapon is WeaponRapier;

        public override void Update(AIEnemyHuman ai)
        {
            var attackZone = ai.GetAttackZone(new Vector2(20, 0), new Vector2(40, 30));

            if (!ai.IsAttacking() && IsInMeleeRange(ai, attackZone) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                
                if (ai.OnGround)
                {
                    ai.Owner.Velocity.X += GetFacingVector(ai.Owner.Facing).X * 0.75f;
                    ai.Owner.Velocity.Y = -ai.Owner.GetJumpVelocity(8);
                    ai.OnGround = false;
                }
                else
                {
                    ai.Owner.Velocity.X += GetFacingVector(ai.Owner.Facing).X * 0.5f;
                }
                ai.CurrentAction = new ActionRapierThrust(ai.Owner, 4, 8, ai.Weapon);
                ai.AttackCooldown = 30;
            }
        }
    }

    class WeaponAIRapierDash : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionDashAttack || action is ActionStab;

        public override bool IsValid(Weapon weapon) => weapon is WeaponRapier;

        public override void Update(AIEnemyHuman ai)
        {
            if (!ai.IsAttacking() && IsInRangedRange(ai) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {

                if (ai.OnGround)
                {
                    ai.Owner.Velocity.Y = -2.5f;
                    ai.OnGround = false;
                }
                ai.CurrentAction = new ActionDashAttack(ai.Owner, 2, 6, 2, 4, false, false, new ActionStab(ai.Owner, 4, 2, ai.Weapon));
                ai.AttackCooldown = 30;
            }
        }
    }

    class WeaponAIKnife : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionStab;

        public override bool IsValid(Weapon weapon) => weapon is WeaponKnife;

        public override void Update(AIEnemyHuman ai)
        {
            var attackZone = ai.GetAttackZone(new Vector2(20, 0), new Vector2(40, 30));

            if (!ai.IsAttacking() && IsInMeleeRange(ai, attackZone) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                ai.Owner.Velocity.X += Math.Sign(ai.DifferenceVector.X) * 2;

                ActionBase[] actionHolder =
                {
                    new ActionStab(ai.Owner, 4, 10, ai.Weapon),
                    new ActionDownStab(ai.Owner, 4, 10, ai.Weapon)
                };
                ai.CurrentAction = actionHolder.Pick(ai.Random);
                ai.AttackCooldown = 30;
            }
        }
    }

    class WeaponAIKnifeThrow : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionKnifeThrow;

        public override bool IsValid(Weapon weapon) => weapon is WeaponKnife;

        public override void Update(AIEnemyHuman ai)
        {
            if (!ai.IsAttacking() && IsInRangedRange(ai) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {

                if (ai.DifferenceVector.Y < 0)
                {
                    ai.Owner.Velocity.Y = -4;
                }
                //if (ai.DifferenceVector.Y > 0)
                ai.CurrentAction = new ActionKnifeThrow(ai.Owner, 2, 4, 8, 2, ai.Weapon);
                ai.RangedCooldown = 60 + ai.Random.Next(40);
            }
        }
    }

    class WeaponAIBoomerang : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionSlash;

        public override bool IsValid(Weapon weapon) => weapon is WeaponBoomerang;

        public override void Update(AIEnemyHuman ai)
        {
            var boomerang = ai.Weapon as WeaponBoomerang;
            var attackZone = ai.GetAttackZone(new Vector2(20, 0), new Vector2(40, 30));

            if (!boomerang.Thrown && !ai.IsAttacking() && IsInMeleeRange(ai, attackZone) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                ai.Owner.Velocity.X += Math.Sign(ai.DifferenceVector.X) * 2;
                ai.CurrentAction = new ActionSlash(ai.Owner, 2, 4, 4, 2, ai.Weapon);
                ai.AttackCooldown = 30;
            }
        }
    }

    class WeaponAIBoomerangThrow : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionBoomerangThrow;

        public override bool IsValid(Weapon weapon) => weapon is WeaponBoomerang;

        public override void Update(AIEnemyHuman ai)
        {
            if (!ai.IsAttacking() && IsInRangedRange(ai) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                var boomerang = ai.Weapon as WeaponBoomerang;
                ai.CurrentAction = new ActionBoomerangThrow(ai.Owner, 10, boomerang, 40) { Angle = VectorToAngle(ai.DifferenceVector) };
                ai.RangedCooldown = 60 + ai.Random.Next(40);
            }
        }
    }
    
    class WeaponAIKatanaUnsheathe : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionKatanaSlash;

        public override bool IsValid(Weapon weapon) => weapon is WeaponKatana;

        public override void Update(AIEnemyHuman ai)
        {
            var katana = ai.Weapon as WeaponKatana;
            var attackZone = ai.GetAttackZone(new Vector2(20, 0), new Vector2(40, 30));

            if (katana.Sheathed && !ai.IsAttacking() && IsInMeleeRange(ai, attackZone) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                ai.Owner.Velocity.X += Math.Sign(ai.DifferenceVector.X) * 2;
                ai.CurrentAction = new ActionKatanaSlash(ai.Owner, 2, 4, 12, 4, katana);
                ai.AttackCooldown = 30;
            }
        }
    }

    class WeaponAIStealing : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionStealWeapon;

        public override bool IsValid(Weapon weapon) => weapon is WeaponUnarmed;

        public override void Update(AIEnemyHuman ai)
        {
            var attackZone = ai.GetAttackZone(new Vector2(20, 0), new Vector2(40, 30));

            if (!(ai.Target.Weapon is WeaponUnarmed) && !ai.IsAttacking() && IsInMeleeRange(ai, attackZone) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                ai.Owner.Velocity.X += Math.Sign(ai.DifferenceVector.X) * 2;
                ai.CurrentAction = new ActionStealWeapon(ai.Owner, ai.Target, 4, 8);
                ai.AttackCooldown = 15;
            }
        }
    }

    //TODO: why
    class WeaponAIDefault : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionTwohandSlash;

        public override bool IsValid(Weapon weapon) => !(weapon is WeaponUnarmed);

        public override void Update(AIEnemyHuman ai)
        {
            var attackZone = ai.GetAttackZone(new Vector2(20, 0), new Vector2(40, 30));

            if (!ai.IsAttacking() && IsInMeleeRange(ai, attackZone) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                ai.Owner.Velocity.X += Math.Sign(ai.DifferenceVector.X) * 2;
                ai.CurrentAction = new ActionTwohandSlash(ai.Owner, 3, 12, ai.Weapon);
                ai.AttackCooldown = 30;
            }
        }
    }

    class AIEnemyHuman
    {
        static List<WeaponAI> WeaponAIs = new List<WeaponAI>();

        static AIEnemyHuman()
        {
            WeaponAIs.Add(new WeaponAISword());
            WeaponAIs.Add(new WeaponAIWand());
            WeaponAIs.Add(new WeaponAIWandBlast());
            WeaponAIs.Add(new WeaponAIHammer());
            WeaponAIs.Add(new WeaponAIHammerSlam());
            WeaponAIs.Add(new WeaponAIBoomerang());
            WeaponAIs.Add(new WeaponAIBoomerangThrow());
            WeaponAIs.Add(new WeaponAIKnife());
            WeaponAIs.Add(new WeaponAIKnifeThrow());
            WeaponAIs.Add(new WeaponAIRapier());
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

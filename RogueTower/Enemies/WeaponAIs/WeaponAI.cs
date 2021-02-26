using Humper.Base;
using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Items.Weapons;
using System;

namespace RogueTower.Enemies.WeaponAIs
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
}

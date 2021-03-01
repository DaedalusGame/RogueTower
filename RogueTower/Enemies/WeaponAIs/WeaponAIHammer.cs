using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Items.Weapons;
using System;

namespace RogueTower.Enemies.WeaponAIs
{
    class WeaponAIHammerSwing : WeaponAI
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
}

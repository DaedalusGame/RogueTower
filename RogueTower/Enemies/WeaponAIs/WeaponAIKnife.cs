using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Items.Weapons;
using System;

namespace RogueTower.Enemies.WeaponAIs
{
    class WeaponAIKnifeStab : WeaponAI
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
}

using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Items.Weapons;
using System;

namespace RogueTower.Enemies.WeaponAIs
{
    class WeaponAIWandSwing : WeaponAI
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
}

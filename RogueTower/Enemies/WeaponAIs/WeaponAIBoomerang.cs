using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Items.Weapons;
using System;
using static RogueTower.Util;

namespace RogueTower.Enemies.WeaponAIs
{
    class WeaponAIBoomerangSwing : WeaponAI
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
}

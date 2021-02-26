using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Items.Weapons;
using System;

namespace RogueTower.Enemies.WeaponAIs
{
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
}

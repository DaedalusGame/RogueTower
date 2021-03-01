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

namespace RogueTower.Enemies.WeaponAIs
{
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
}

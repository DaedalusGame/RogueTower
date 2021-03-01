using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Items.Weapons;
using System;

namespace RogueTower.Enemies.WeaponAIs
{
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
}

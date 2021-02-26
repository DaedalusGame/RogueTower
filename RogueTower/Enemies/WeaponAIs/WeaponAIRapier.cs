using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Items.Weapons;
using static RogueTower.Util;

namespace RogueTower.Enemies.WeaponAIs
{
    class WeaponAIRapierThrust : WeaponAI
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
}

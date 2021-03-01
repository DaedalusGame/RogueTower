using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack.AlchemicalOrbs.Orange;
using RogueTower.Items.AlchemicalOrbs;
using RogueTower.Items.Weapons;

namespace RogueTower.Enemies.WeaponAIs
{
    class WeaponAIAlchemicalGauntletOrangeOrbTransplant : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionTransplantPunch;

        public override bool IsValid(Weapon weapon) => weapon is WeaponAlchemicalGauntlet;

        public override void Update(AIEnemyHuman ai)
        {
            var gauntlet = ai.Weapon as WeaponAlchemicalGauntlet;
            var attackZone = ai.GetAttackZone(new Vector2(20, 0), new Vector2(20, 30));

            if(gauntlet.Orb is OrangeOrb && !ai.IsAttacking() && IsInMeleeRange(ai, attackZone) && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                Bomb bomb = (Bomb)ai.Target.StatusEffects.Find(status => status is Bomb);
                if ((bomb != null && bomb.Stacks < bomb.StacksMax ) || bomb == null)
                {
                    ai.CurrentAction = new ActionTransplantPunch(ai.Owner, 4, 6, ai.Weapon);
                    ai.AttackCooldown = 20;
                }
            }
        }
    }

    class WeaponAIAlchemicalGauntletOrangeOrbActivate : WeaponAI
    {
        public override bool IsAttackState(ActionBase action) => action is ActionTransplantActivate;

        public override bool IsValid(Weapon weapon) => weapon is WeaponAlchemicalGauntlet;

        public override void Update(AIEnemyHuman ai)
        {
            var gauntlet = ai.Weapon as WeaponAlchemicalGauntlet;

            if (gauntlet.Orb is OrangeOrb && !ai.IsAttacking() && ai.AttackCooldown <= 0 && ai.Target.Invincibility < 3)
            {
                Bomb bomb = (Bomb)ai.Target.StatusEffects.Find(status => status is Bomb);
                if (bomb != null && bomb.Stacks >= bomb.StacksMax)
                {
                    ai.CurrentAction = new ActionTransplantActivate(ai.Owner, 4, 6);
                    ai.AttackCooldown = 40;
                }
            }
        }
    }
}

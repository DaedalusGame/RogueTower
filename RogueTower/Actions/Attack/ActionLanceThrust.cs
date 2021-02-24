using Microsoft.Xna.Framework;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using static RogueTower.Util;

namespace RogueTower.Actions.Attack
{
    class ActionLanceThrust : ActionStab
    {
        public ActionLanceThrust(EnemyHuman human, float upTime, float downTime, Weapon weapon) : base(human, upTime, downTime, weapon)
        {
            new ScreenShakeRandom(Human.World, 5, 5);
            Human.Velocity.X += GetFacingVector(Human.Facing).X * 1.5f;
            if (!Human.OnGround)
                Human.Velocity.Y += 1;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);
            switch (SlashAction)
            {
                default:
                case (SwingAction.UpSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(4);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-135f));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(2);
                    basePose.LeftArm = ArmState.Angular(7);
                    basePose.RightArm = ArmState.Angular(0);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(0));
                    break;
            }
        }
    }
}

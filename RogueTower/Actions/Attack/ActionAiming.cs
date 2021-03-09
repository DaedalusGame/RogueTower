using Microsoft.Xna.Framework;
using RogueTower.Actions.Interfaces;
using RogueTower.Effects.Particles;
using static RogueTower.Util;

namespace RogueTower.Actions.Attack
{
    class ActionAiming : ActionBase
    {
        public float AimAngle;
        public ActionBase PostAction;
        public AimingReticule AimFX;
        public ActionAiming(Player player, ActionBase action) : base(player)
        {
            PostAction = action;
            AimFX = new AimingReticule(player.World, Vector2.Zero, this);
        }

        public override void OnInput()
        {
            var player = (Player)Human;
            AimAngle = player.Controls.AimAngle;
            AimFX.Position = player.Position + (AngleToVector(AimAngle) * 100);
            if (player.Controls.AimFire)
            {
                if (PostAction is IActionAimable aimable)
                {
                    aimable.SetAngle(AimAngle);
                }
                player.CurrentAction = PostAction;
            }
        }

        public override void GetPose(PlayerState basePose)
        {
            var armAngle = AimAngle;
            var aimVector = AngleToVector(AimAngle);
            Human.Facing = (aimVector.X < 0) ? HorizontalFacing.Left : HorizontalFacing.Right;
            if (Human.Facing == HorizontalFacing.Left)
                armAngle = -armAngle;
            switch (basePose.WeaponHold)
            {
                case (WeaponHold.Left):
                    basePose.LeftArm = ArmState.Angular(armAngle);
                    break;
                case (WeaponHold.Right):
                    basePose.RightArm = ArmState.Angular(armAngle);
                    break;
                case (WeaponHold.TwoHand):
                    basePose.LeftArm = ArmState.Angular(armAngle);
                    basePose.RightArm = ArmState.Angular(armAngle);
                    break;
            }
            basePose.Weapon = Human.Weapon.GetWeaponState(Human, armAngle);
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
        public override void UpdateDelta(float delta)
        {
            //NOOP
        }
    }
}

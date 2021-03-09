using Microsoft.Xna.Framework;
using RogueTower.Effects.Particles;
using RogueTower.Items.Weapons;
using static RogueTower.Util;

namespace RogueTower.Actions.Attack
{
    class ActionWandBlastAim : ActionWandBlast
    {
        public AimingReticule AimFX;
        public float AimAngle;

        public override Vector2 TargetOffset => AngleToVector(AimAngle);

        public ActionWandBlastAim(Player player, float upTime, float downTime, WeaponWand weapon) : base(player, upTime, downTime, weapon)
        {
            FireReady = false;
            AimFX = new AimingReticule(player.World, Vector2.Zero, this);
        }

        public override void UpdateDelta(float delta)
        {
            if (Human is Player player)
            {
                if (!FireReady)
                    AimAngle = player.Controls.AimAngle;
                AimFX.Position = Human.Position + (AngleToVector(AimAngle) * 100);
            }

            base.UpdateDelta(delta);
        }

        public override void OnInput()
        {
            var player = (Player)Human;
            if (!FireReady)
            {
                var aimVector = AngleToVector(AimAngle);
                if (aimVector.X < -0.1)
                    Human.Facing = HorizontalFacing.Left;
                if (aimVector.X > 0.1)
                    Human.Facing = HorizontalFacing.Right;
                if (player.Controls.AimFire)
                {
                    FireReady = true;
                }
            }

        }
    }
}

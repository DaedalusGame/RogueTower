using Microsoft.Xna.Framework;
using RogueTower.Actions.Interfaces;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using static RogueTower.Util;

namespace RogueTower.Actions.Attack
{
    class ActionBoomerangThrow : ActionBase, IActionAimable
    {
        public enum BoomerangState
        {
            Prethrow,
            Throw
        }

        public void SetAngle(float angle)
        {
            Angle = angle;
        }

        public BoomerangState BoomerangAction;
        public BoomerangProjectile BoomerangProjectile;
        public float PrethrowTime;
        public float Lifetime;
        public float Angle = 0;
        public WeaponBoomerang Weapon;

        public ActionBoomerangThrow(EnemyHuman player, float prethrowTime, WeaponBoomerang weapon, float lifetime = 20) : base(player)
        {
            PrethrowTime = prethrowTime;
            Weapon = weapon;
            Lifetime = lifetime;
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (BoomerangAction)
            {
                case (BoomerangState.Prethrow):
                    basePose.RightArm = ArmState.Up;
                    break;
                case (BoomerangState.Throw):
                    basePose.RightArm = ArmState.Forward;
                    basePose.Weapon = WeaponState.None;
                    break;
            }
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void UpdateDelta(float delta)
        {
            switch (BoomerangAction)
            {
                case (BoomerangState.Prethrow):
                    PrethrowTime -= delta;
                    if (PrethrowTime < 0)
                    {
                        BoomerangProjectile = new BoomerangProjectile(Human.World, new Vector2(Human.Box.Bounds.X + 8 * GetFacingVector(Human.Facing).X, Human.Box.Y + Human.Box.Height / 2), Lifetime, Weapon)
                        {
                            Shooter = Human,
                            Velocity = AngleToVector(Angle) * 5
                        };
                        Weapon.BoomerProjectile = BoomerangProjectile;
                        BoomerangAction = BoomerangState.Throw;
                    }
                    break;
                case (BoomerangState.Throw):
                    if (BoomerangProjectile.Destroyed)
                    {
                        Human.ResetState();
                    }
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChaiFoxes.FMODAudio;
using static RogueTower.Game;
using static RogueTower.Util;
using RogueTower.Enemies;

namespace RogueTower.Actions.Attack
{
    class ActionWandBlast : ActionBase
    {
        public enum SwingAction
        {
            UpSwing,
            DownSwing,
        }

        public Vector2 FirePosition => Human.Position - new Vector2(8, 8) + Human.Pose.GetWeaponOffset(Human.Facing.ToMirror()) + Human.Pose.Weapon.GetOffset(Human.Facing.ToMirror(), 1.0f);
        public virtual Vector2 TargetOffset => GetFacingVector(Human.Facing);
        public SwingAction SlashAction;
        public float SlashUpTime;
        public float SlashDownTime;
        public WeaponWand Weapon;
        public bool FireReady = true;

        public ActionWandBlast(EnemyHuman human, float upTime, float downTime, WeaponWand weapon) : base(human)
        {
            SlashUpTime = upTime;
            SlashDownTime = downTime;
            Weapon = weapon;
            PlaySFX(sfx_wand_charge, 1.0f, 0.1f, 0.4f);
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch (SlashAction)
            {
                default:
                case (SwingAction.UpSwing):
                    basePose.LeftArm = ArmState.Angular(9);
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-90 - 45));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.LeftArm = ArmState.Angular(0);
                    basePose.RightArm = ArmState.Angular(0);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(0));
                    break;
            }
        }

        public override void UpdateDelta(float delta)
        {
            switch (SlashAction)
            {
                case (SwingAction.UpSwing):
                    SlashUpTime -= delta;
                    if (SlashUpTime < 0 && FireReady)
                        Fire();
                    break;
                case (SwingAction.DownSwing):
                    SlashDownTime -= delta;
                    if (SlashDownTime < 0)
                        Human.ResetState();
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }

        public void Fire()
        {
            SlashAction = SwingAction.DownSwing;
            Human.UpdatePose();
            var direction = TargetOffset;
            direction.Normalize();
            Weapon.Shoot(Human, FirePosition, direction);
        }
    }
}

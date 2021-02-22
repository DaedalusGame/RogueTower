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
    class ActionKnifeThrow : ActionSlash
    {
        public override bool CanParry => false;

        public ActionKnifeThrow(EnemyHuman player, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime, Weapon weapon) : base(player, slashStartTime, slashUpTime, slashDownTime, slashFinishTime, weapon)
        {

        }

        public override void GetPose(PlayerState basePose)
        {
            switch (SlashAction)
            {
                default:
                case (SwingAction.StartSwing):

                    basePose.RightArm = ArmState.Angular(5);
                    basePose.Weapon = WeaponState.Knife(MathHelper.ToRadians(90 + 45));
                    break;
                case (SwingAction.UpSwing):

                    basePose.RightArm = ArmState.Angular(6);
                    basePose.Weapon = WeaponState.Knife(MathHelper.ToRadians(90 + 45 + 22));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(0);
                    basePose.Weapon = WeaponState.None;
                    break;
                case (SwingAction.FinishSwing):
                    basePose.Body = BodyState.Crouch(2);
                    basePose.RightArm = ArmState.Angular(0);
                    basePose.Weapon = WeaponState.None;
                    break;
            }
        }

        public override void Swing()
        {
            Vector2 facing = GetFacingVector(Human.Facing);
            new Knife(Human.World, Human.Position + facing * 5)
            {
                Velocity = facing * 8,
                FrameEnd = 20,
                Shooter = Human
            };
            PlaySFX(sfx_knife_throw, 1.0f, 0.4f, 0.7f);
            SlashAction = SwingAction.DownSwing;
        }
    }
}

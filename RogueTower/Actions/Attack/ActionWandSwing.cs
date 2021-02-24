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
    class ActionWandSwing : ActionAttack
    {
        enum SwingAction
        {
            Start,
            Swing,
            End,
        }

        SwingAction State;
        Slider StartTime;
        Slider SwingTime;
        Slider EndTime;

        public override bool Done => State == SwingAction.End;
        public override bool CanParry => State == SwingAction.Start;

        public ActionWandSwing(EnemyHuman player, float startTime, float swingTime, float endTime) : base(player)
        {
            StartTime = new Slider(startTime, startTime);
            SwingTime = new Slider(swingTime, swingTime);
            EndTime = new Slider(endTime, endTime);
        }

        public override void GetPose(PlayerState basePose)
        {
            float startAngle = -45 / 2;
            float endAngle = 180 + 45 / 2;
            switch (State)
            {
                case (SwingAction.Start):
                    basePose.LeftArm = ArmState.Angular(MathHelper.ToRadians(startAngle));
                    break;
                case (SwingAction.Swing):
                    basePose.LeftArm = ArmState.Angular(MathHelper.Lerp(MathHelper.ToRadians(startAngle), MathHelper.ToRadians(endAngle), 1 - SwingTime.Slide));
                    break;
                case (SwingAction.End):
                    basePose.LeftArm = ArmState.Angular(MathHelper.ToRadians(endAngle));
                    break;
            }
            basePose.WeaponHold = WeaponHold.Left;
            basePose.Weapon.Angle = basePose.LeftArm.GetHoldAngle(ArmState.Type.Left) - MathHelper.PiOver2;
        }

        public override void UpdateDelta(float delta)
        {
            switch (State)
            {
                case (SwingAction.Start):
                    if (StartTime - delta <= 0)
                        State = SwingAction.Swing;
                    break;
                case (SwingAction.Swing):
                    if (SwingTime - delta <= 0)
                        State = SwingAction.End;
                    break;
                case (SwingAction.End):
                    if (EndTime - delta <= 0)
                        Human.ResetState();
                    break;
            }
        }

        public override void ParryGive(IParryReceiver receiver)
        {

        }

        public override void ParryReceive(IParryGiver giver)
        {

        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
    }
}

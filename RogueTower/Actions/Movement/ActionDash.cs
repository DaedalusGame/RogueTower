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

namespace RogueTower.Actions.Movement
{
    class ActionDash : ActionBase
    {
        public DashState DashAction;
        public float DashStartTime;
        public float DashTime;
        public float DashEndTime;
        public float DashFactor;
        public bool Phasing;
        public bool Reversed;
        public override float Friction => 1;

        public enum DashState
        {
            DashStart,
            Dash,
            DashEnd,
        }

        public ActionDash(EnemyHuman player, float dashStartTime, float dashTime, float dashEndTime, float dashFactor, bool phasing, bool reversed) : base(player)
        {
            DashStartTime = dashStartTime;
            DashTime = dashTime;
            DashEndTime = dashEndTime;
            DashFactor = dashFactor;
            Phasing = phasing;
            Reversed = reversed;
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (DashAction)
            {
                default:
                case (DashState.DashStart):
                    basePose.Head = HeadState.Down;
                    break;
                case (DashState.Dash):
                    basePose.Head = HeadState.Forward;
                    basePose.Body = BodyState.Crouch(1);
                    break;
                case (DashState.DashEnd):
                    basePose.Head = HeadState.Down;
                    basePose.Body = BodyState.Crouch(1);
                    break;
            }
        }

        public override void OnInput()
        {

        }

        public override void UpdateDelta(float delta)
        {
            switch (DashAction)
            {
                case (DashState.DashStart):
                    DashStartTime -= delta;
                    if (DashStartTime < 0)
                        DashAction = DashState.Dash;
                    break;
                case (DashState.Dash):
                    DashTime -= delta;
                    Human.Velocity.X = MathHelper.Clamp((GetFacingVector(Human.Facing).X * (Reversed ? -1 : 1)) * DashFactor, -DashFactor, DashFactor);
                    if (DashTime < 0)
                        DashAction = DashState.DashEnd;
                    break;
                case (DashState.DashEnd):
                    DashEndTime -= delta;
                    if (DashEndTime < 0)
                        Human.ResetState();
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
        }
    }
}

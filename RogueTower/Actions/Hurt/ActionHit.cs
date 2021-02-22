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

namespace RogueTower.Actions.Hurt
{
    class ActionHit : ActionBase
    {
        float Time;

        public override float Drag => 1;

        public ActionHit(EnemyHuman player, float time) : base(player)
        {
            Time = time;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Head = HeadState.Down;
            basePose.Body = BodyState.Hit;
            basePose.RightArm = ArmState.Angular(3);
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void UpdateDelta(float delta)
        {
            Time -= delta;
        }

        public override void UpdateDiscrete()
        {
            if (Time <= 0)
            {
                Human.ResetState();
            }
        }
    }
}

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
    class ActionIdle : ActionBase
    {
        public ActionIdle(EnemyHuman player) : base(player)
        {

        }

        public override void GetPose(PlayerState basePose)
        {
            //NOOP
        }

        public override void OnInput()
        {
            var player = (Player)Human;
            HandleMoveInput(player);
            HandleJumpInput(player);
            HandleSlashInput(player);
            HandleItemInput(player);
        }

        public override void UpdateDelta(float delta)
        {
            if (!Human.OnGround)
                Human.CurrentAction = new ActionJump(Human, true, true);
            else if (Math.Abs(Human.Velocity.X) >= 0.01)
                Human.CurrentAction = new ActionMove(Human);
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
    }
}

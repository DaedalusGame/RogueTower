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
    class ActionMove : ActionBase
    {
        public float WalkFrame;
        public bool WalkingLeft;
        public bool WalkingRight;

        public ActionMove(EnemyHuman player) : base(player)
        {

        }

        public override void OnInput()
        {
            var player = (Player)Human;
            HandleMoveInput(player);
            WalkingLeft = player.Controls.MoveLeft;
            WalkingRight = player.Controls.MoveRight;
            HandleJumpInput(player);
            HandleSlashInput(player);
            HandleItemInput(player);
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = BodyState.Walk((int)WalkFrame);
        }

        public override void UpdateDelta(float delta)
        {
            if (WalkingLeft || WalkingRight)
            {
                if (!Human.Strafing)
                {
                    if (Human.Velocity.X > 0 && WalkingRight)
                    {
                        Human.Facing = HorizontalFacing.Right;
                    }
                    else if (Human.Velocity.X < 0 && WalkingLeft)
                    {
                        Human.Facing = HorizontalFacing.Left;
                    }
                }
                WalkFrame += Math.Abs(Human.Velocity.X * delta * 0.125f) / (float)Math.Sqrt(Human.GroundFriction);
            }
            if (!Human.OnGround)
                Human.CurrentAction = new ActionJump(Human, true, true);
            else if (Math.Abs(Human.Velocity.X) < 0.01)
                Human.CurrentAction = new ActionIdle(Human);
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
    }
}

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
    class ActionJump : ActionBase
    {
        public enum State
        {
            Up,
            Down,
        }

        public State CurrentState;
        public bool JumpingLeft;
        public bool JumpingRight;
        public bool AllowAirControl;
        public bool AllowJumpControl;

        public override float Drag => AllowAirControl ? base.Drag : 1;

        public ActionJump(EnemyHuman player, bool airControl, bool jumpControl) : base(player)
        {
            AllowAirControl = airControl;
            AllowJumpControl = jumpControl;
        }

        public override void OnInput()
        {
            var player = (Player)Human;
            HandleMoveInput(player);
            if (AllowJumpControl && !player.Controls.JumpHeld && Human.Velocity.Y < 0)
                Human.Velocity.Y *= 0.7f;
            JumpingLeft = player.Controls.MoveLeft;
            JumpingRight = player.Controls.MoveRight;
            HandleExtraJump(player);
            HandleSlashInput(player);
            HandleItemInput(player);
        }

        public override void UpdateDelta(float delta)
        {
            if (JumpingLeft || JumpingRight)
            {
                if (!Human.Strafing)
                {
                    if (Human.Velocity.X > 0 && JumpingRight)
                        Human.Facing = HorizontalFacing.Right;
                    else if (Human.Velocity.X < 0 && JumpingLeft)
                        Human.Facing = HorizontalFacing.Left;
                }
            }

            if (Human.Velocity.Y < 0)
                CurrentState = State.Up;
            else
                CurrentState = State.Down;

            if (Human.OnGround)
                Human.CurrentAction = new ActionIdle(Human);
        }

        public override void UpdateDiscrete()
        {
            if (Human.OnWall)
            {
                var wallTiles = Human.World.FindTiles(Human.Box.Bounds.Offset(GetFacingVector(Human.Facing)));
                var climbTiles = wallTiles.Where(tile => tile.CanClimb(Human.Facing.Mirror()));
                if (Human.InAir && climbTiles.Any() && CurrentState == State.Down)
                {
                    Human.Velocity.Y = 0;
                    Human.CurrentAction = new ActionClimb(Human);
                }
            }
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (CurrentState)
            {
                default:
                case (State.Up):
                    if (Human.Velocity.Y < -0.5)
                        basePose.Body = BodyState.Walk(1);
                    else
                        basePose.Body = BodyState.Walk(0);
                    break;
                case (State.Down):
                    basePose.Body = BodyState.Walk(2);
                    break;
            }
        }
    }
}

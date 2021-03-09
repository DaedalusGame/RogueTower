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
    class ActionClimb : ActionBase
    {
        public float ClimbFrame;

        public override bool HasGravity => false;

        public ActionClimb(EnemyHuman player) : base(player)
        {
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = BodyState.Climb;
            basePose.LeftArm = ArmState.Angular(11 + Util.PositiveMod(3 + (int)-ClimbFrame, 7));
            basePose.RightArm = ArmState.Angular(11 + Util.PositiveMod((int)-ClimbFrame, 7));
            basePose.Weapon = WeaponState.None;
        }

        public override void OnInput()
        {
            var player = (Player)Human;
            if (player.Controls.ClimbUp)
                Human.Velocity.Y = -0.5f;
            if (player.Controls.ClimbDown)
            {
                Human.Velocity.Y = player.Controls.AltAttackHeld ? 2 : 0.5f;
            }
            if (!player.Controls.ClimbUp && !player.Controls.ClimbDown)
                Human.Velocity.Y = 0;
            if (player.Controls.Jump)
            {
                Human.OnWall = false;
                Human.CurrentAction = new ActionJump(Human, false, true);
                Human.Velocity = GetFacingVector(Human.Facing) * -Human.GetJumpVelocity(30) * 0.5f + new Vector2(0, -Human.GetJumpVelocity(30));
                //Player.DisableAirControl = true;
                Human.Facing = Human.Facing.Mirror();
                PlaySFX(sfx_player_jump, 0.5f, 0.1f, 0.5f);
            }
            if (Human.OnGround && ((player.Controls.MoveLeft && Human.Facing == HorizontalFacing.Right) || (player.Controls.MoveRight && Human.Facing == HorizontalFacing.Left)))
            {
                Human.OnWall = false;
                Human.ResetState();
                Human.Facing = Human.Facing.Mirror();
            }
        }

        public override void UpdateDelta(float delta)
        {
            if(Human is Player player)
            {
                if (player.Controls.ClimbDown && player.Controls.AltAttackHeld)
                {
                    ClimbFrame = 4;
                    return;
                }
            }

            ClimbFrame += Human.Velocity.Y * delta * 0.5f;
            
        }

        public override void UpdateDiscrete()
        {
            var climbTiles = Human.World.FindTiles(Human.Box.Bounds.Offset(GetFacingVector(Human.Facing))).Where(tile => tile.CanClimb(Human.Facing.Mirror()));
            if (!climbTiles.Any())
            {
                Human.OnWall = false;
                Human.CurrentAction = new ActionJump(Human, true, true);
                Human.Velocity.X = GetFacingVector(Human.Facing).X; //Tiny nudge to make the player stand on the ladder
            }
        }
    }
}

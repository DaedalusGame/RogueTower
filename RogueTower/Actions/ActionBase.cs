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

namespace RogueTower.Actions
{
    abstract class ActionBase
    {
        public EnemyHuman Human;
        protected ActionBase(EnemyHuman player)
        {
            Human = player;
        }

        public virtual bool HasGravity => true;
        public virtual float Friction => 1 - (1 - 0.85f) * Human.GroundFriction;
        public virtual float Drag => 0.85f;
        public virtual bool CanParry => false;
        public virtual bool Incorporeal => false;

        abstract public void OnInput();

        abstract public void UpdateDelta(float delta);

        abstract public void UpdateDiscrete();

        protected bool HandleJumpInput(Player player)
        {
            if (player.Controls.Jump)
            {
                Human.Velocity.Y -= Human.GetJumpVelocity(60);
                Human.OnGround = false;
                PlaySFX(sfx_player_jump, 0.5f, 0.1f, 0.5f);
                return true;
            }
            return false;
        }

        protected void HandleExtraJump(Player player)
        {
            if (Human.ExtraJumps > 0 && HandleJumpInput(player))
                Human.ExtraJumps--;
        }

        protected void HandleMoveInput(Player player)
        {
            float adjustedSpeedLimit = Human.SpeedLimit;
            float baseAcceleraton = Human.Acceleration;
            if (Human.OnGround)
                baseAcceleraton *= Human.GroundFriction;
            float acceleration = baseAcceleraton;

            if (player.Controls.MoveLeft && Human.Velocity.X > -adjustedSpeedLimit)
                Human.Velocity.X = Math.Max(Human.Velocity.X - acceleration, -adjustedSpeedLimit);
            if (player.Controls.MoveRight && Human.Velocity.X < adjustedSpeedLimit)
                Human.Velocity.X = Math.Min(Human.Velocity.X + acceleration, adjustedSpeedLimit);
            if ((player.Controls.MoveLeft && Human.Velocity.X < 0) || (player.Controls.MoveRight && Human.Velocity.X > 0))
                Human.AppliedFriction = 1;
        }

        protected void HandleSlashInput(Player player)
        {
            player.Weapon.HandleAttack(player);
        }

        protected void HandleItemInput(Player player)
        {
            if (player.Controls.Pickup && player.NearbyItems.Any())
            {
                var nearbyItem = player.GetNearestItem();
                player.Pickup(nearbyItem);
            }
        }

        abstract public void GetPose(PlayerState basePose);
    }
}

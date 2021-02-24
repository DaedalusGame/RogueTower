using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Humper.Base;
using static RogueTower.Game;
using static RogueTower.Util;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Enemies;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Actions.Movement;

namespace RogueTower.Items.Weapons
{
    class WeaponRapier : Weapon
    {
        protected WeaponRapier() : base()
        {

        }

        public WeaponRapier(double damage, Vector2 weaponSize) : base("Rapier", "", damage, weaponSize, 1.0f, 1.2f)
        {
            CanParry = true;
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            pose.WeaponHold = WeaponHold.Left;
            pose.LeftArm = ArmState.Angular(1);
            pose.RightArm = ArmState.Angular(9);
            pose.Weapon = GetWeaponState(human, MathHelper.ToRadians(-22.5f));

        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.Rapier(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack && !(player.CurrentAction is ActionRapierThrust))
            {
                player.Velocity.X += player.OnGround ? GetFacingVector(player.Facing).X * 0.75f : GetFacingVector(player.Facing).X * 0.5f;
                if (player.OnGround && !player.Controls.ClimbDown)
                {
                    player.Velocity.Y = -player.GetJumpVelocity(8);
                    player.OnGround = false;
                }
                player.CurrentAction = new ActionRapierThrust(player, 4, 8, this);
            }
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/rapier"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponRapier();
        }
    }
}

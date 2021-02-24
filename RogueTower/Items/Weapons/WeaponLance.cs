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
    class WeaponLance : Weapon
    {
        protected WeaponLance() : base()
        {

        }

        public WeaponLance(double damage, Vector2 weaponSize) : base("Lance", "", damage, weaponSize, 1.5f, 1.5f)
        {
            CanParry = true;
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            pose.RightArm = ArmState.Angular(7);
            pose.Shield = ShieldState.ShieldForward;
            pose.Weapon = GetWeaponState(human, MathHelper.ToRadians(-90));
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.Lance(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                player.CurrentAction = new ActionLanceThrust(player, 2, 12, this);
            }
            else if (player.Controls.AltAttack)
            {
                player.CurrentAction = new ActionCharge(player, 180, new ActionDashAttack(player, 2, 4, 4, 6, false, false, new ActionLanceThrust(player, 2, 6, this)), this, false, 0) { CanJump = true, CanMove = true };
            }
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/lance"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponLance();
        }
    }
}

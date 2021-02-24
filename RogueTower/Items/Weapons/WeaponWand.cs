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
    abstract class WeaponWand : Weapon
    {
        protected WeaponWand() : base()
        {
        }

        public WeaponWand(string name, string description, double damage, Vector2 weaponSize, float width, float length) : base(name, description, damage, weaponSize, width, length)
        {
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            pose.Weapon = GetWeaponState(human, MathHelper.ToRadians(-45));
            pose.WeaponHold = WeaponHold.Left;
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                player.CurrentAction = new ActionWandSwing(player, 10, 5, 20);
            }
            else if (player.Controls.AltAttack)
            {
                player.CurrentAction = new ActionWandBlast(player, 24, 12, this);
            }
            else if (player.Controls.IsAiming)
            {
                player.CurrentAction = new ActionWandBlastAim(player, 24, 12, this);
            }
        }

        public abstract void Shoot(EnemyHuman shooter, Vector2 position, Vector2 direction);
    }
}

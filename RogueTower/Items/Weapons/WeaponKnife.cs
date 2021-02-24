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
    class WeaponKnife : Weapon
    {
        protected WeaponKnife() : base()
        {

        }

        public WeaponKnife(double damage, Vector2 weaponSize) : base("Knife", "", damage, weaponSize, 1.0f, 0.8f)
        {
            CanParry = true;
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.Knife(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                Stab(player);
            }
            if (player.Controls.DownAttack)
            {
                StabDown(player);
            }
            if (player.Controls.AltAttack)
            {
                player.CurrentAction = new ActionDash(player, 2, 4, 2, 3, false, true);
            }
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/knife"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponKnife();
        }
    }
}

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
    class WeaponWarhammer : Weapon
    {
        protected WeaponWarhammer() : base()
        {

        }

        public WeaponWarhammer(double damage, Vector2 weaponSize) : base("Warhammer", "", damage, weaponSize, 2.0f, 2.0f)
        {
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.Warhammer(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                TwoHandSlash(player, 8, 16);
            }
            else if (player.Controls.AltAttack && player.OnGround)
            {
                SlashUp(player, 2, 8, 16, 2);
                player.Velocity.Y = -5;
                player.OnGround = false;
            }
            else if (player.Controls.AltAttack && player.InAir)
            {
                player.CurrentAction = new ActionShockwave(player, 4, 8, this, 2);
            }
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/warhammer"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponWarhammer();
        }
    }
}

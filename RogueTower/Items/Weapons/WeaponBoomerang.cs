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
    class WeaponBoomerang : Weapon
    {
        public BoomerangProjectile BoomerProjectile;

        public bool Thrown => BoomerProjectile != null && !BoomerProjectile.Destroyed;

        protected WeaponBoomerang() : base()
        {

        }

        public WeaponBoomerang(float damage, Vector2 weaponSize) : base("Boomerang", "", damage, weaponSize, 0.8f, 0.8f)
        {
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.Boomerang(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack && (BoomerProjectile == null || BoomerProjectile.Destroyed))
            {
                player.CurrentAction = new ActionSlash(player, 2, 4, 8, 2, this);
            }
            else if (player.Controls.IsAiming && (BoomerProjectile == null || BoomerProjectile.Destroyed))
            {
                player.CurrentAction = new ActionAiming(player, new ActionBoomerangThrow(player, 10, this, 40));
            }
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/boomerang"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponBoomerang();
        }
    }
}

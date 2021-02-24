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
    class WeaponUnarmed : Weapon
    {
        protected WeaponUnarmed() : base()
        {

        }

        public WeaponUnarmed(double damage, Vector2 weaponSize) : base("Unarmed", "", damage, weaponSize, 1.0f, 1.0f)
        {
            CanParry = false;
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            //NOOP
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.None;
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                if (player.CurrentAction.GetType() == typeof(ActionPunch))
                    player.CurrentAction = new ActionLeftPunch(player, 4, 8, this);
                else
                    player.CurrentAction = new ActionPunch(player, 4, 8, this);
            }
            else if (player.Controls.AltAttack)
            {

                RectangleF searchBox = new RectangleF(player.Position + GetFacingVector(player.Facing) * 8 + GetFacingVector(player.Facing) * (WeaponSize.X / 2) + new Vector2(0, 1) - WeaponSize / 2f, WeaponSize);
                new RectangleDebug(player.World, searchBox, Color.Pink, 10);
                foreach (var box in player.World.FindBoxes(searchBox))
                {
                    if (box.Data is EnemyHuman human && !(human.Weapon is WeaponUnarmed) && !(human is Player))
                    {
                        player.CurrentAction = new ActionStealWeapon(player, human, 4, 8);
                        break;
                    }
                }
            }
        }

        protected override Item MakeCopy()
        {
            return new WeaponUnarmed();
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            //NOOP
        }
    }
}

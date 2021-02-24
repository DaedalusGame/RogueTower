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
using RogueTower.Items.AlchemicalOrbs;

namespace RogueTower.Items.Weapons
{
    class WeaponAlchemicalGauntlet : Weapon
    {
        public AlchemicalOrb Orb;
        public string GauntletSprite = "alchemical_gauntlet";
        public Color GauntletColor = Color.Silver;

        protected WeaponAlchemicalGauntlet() : base()
        {

        }


        public WeaponAlchemicalGauntlet(double damage, Vector2 weaponSize) : base("Alchemical Gauntlet", "", damage, weaponSize, 1.0f, 1.0f)
        {
            Orb = new OrangeOrb(this);
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            pose.Shield = ShieldState.None;
            pose.Weapon = GetWeaponState(human, 0);
        }
        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.AlchemicalGauntlet(angle, GauntletSprite, Orb.OrbSprite, GauntletColor, Orb.OrbColor);
        }

        public override void HandleAttack(Player player)
        {
            if (Orb != null)
                Orb.HandleAttack(player);
            else
            {
                if (player.Controls.Attack || player.Controls.AltAttack)
                {
                    PlaySFX(sfx_player_disappointed, 1, 0.1f, 0.15f);
                    player.Hit(Vector2.Zero, 1, 0, 1);
                    Util.Message(player, new MessageText("The empty alchemical gauntlet drains your strength upon activation!"));
                }
            }
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/alchemical_gauntlet"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponAlchemicalGauntlet();
        }
    }
}

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
    class WeaponSword : Weapon
    {
        protected WeaponSword() : base()
        {

        }

        public WeaponSword(double damage, Vector2 weaponSize) : base("Sword", "", damage, weaponSize, 1.0f, 1.0f)
        {
            CanParry = true;
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            pose.LeftArm = ArmState.Shield;
            pose.Shield = ShieldState.ShieldForward;
            pose.Weapon = GetWeaponState(human, MathHelper.ToRadians(-90 - 22));
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.Sword(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.DownAttack && player.InAir)
            {
                SlashDown(player);
            }
            else if (player.Controls.DownAttack && player.OnGround && !(player.CurrentAction is ActionKnifeThrow))
            {
                SlashKnife(player);
            }
            else if (player.Controls.Attack && !player.Controls.DownAttack && !(player.CurrentAction is ActionKnifeThrow))
            {
                if (player.CurrentAction.GetType() == typeof(ActionSlash))
                    SlashUp(player);
                else
                    Slash(player);
            }

        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/sword"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponSword();
        }
    }
}

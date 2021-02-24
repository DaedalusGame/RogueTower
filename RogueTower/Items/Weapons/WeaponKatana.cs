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
    class WeaponKatana : Weapon
    {
        public bool Sheathed = true;
        protected WeaponKatana() : base()
        {

        }

        public WeaponKatana(double damage, Vector2 weaponSize) : base("Katana", "", damage, weaponSize, 1.0f, 1.5f)
        {
            CanParry = true;
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            Sheathed = true;
            pose.LeftArm = ArmState.Angular(5);
            pose.RightArm = ArmState.Angular(5);
            pose.Shield = ShieldState.KatanaSheath(MathHelper.ToRadians(-20));
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return Sheathed ? WeaponState.None : WeaponState.Katana(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.DownAttack && player.OnGround)
            {
                DashAttack(player, new ActionTwohandSlash(player, 6, 4, this), dashFactor: 4);
            }
            else if (player.Controls.Attack && Sheathed)
            {
                player.CurrentAction = new ActionKatanaSlash(player, 2, 4, 12, 4, this);
                if (player.OnGround)
                    player.Velocity.X += GetFacingVector(player.Facing).X * 2;
            }
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/katana"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponKatana();
        }
    }
}

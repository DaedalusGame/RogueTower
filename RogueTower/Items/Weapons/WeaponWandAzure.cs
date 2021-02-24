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
    class WeaponWandAzure : WeaponWand
    {
        protected WeaponWandAzure() : base()
        {

        }

        public WeaponWandAzure(double damage, Vector2 weaponSize) : base("Azure Wand", "", damage, weaponSize, 1.0f, 1.0f)
        {
            CanParry = true;
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.WandAzure(angle);
        }

        public override void Shoot(EnemyHuman shooter, Vector2 position, Vector2 direction)
        {
            new SpellAzure(shooter.World, position)
            {
                Velocity = direction * 3,
                FrameEnd = 70,
                Shooter = shooter
            };
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/wand_azure"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponWandAzure();
        }
    }
}

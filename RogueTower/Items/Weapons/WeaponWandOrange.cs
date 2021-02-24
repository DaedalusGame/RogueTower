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
    class WeaponWandOrange : WeaponWand
    {
        protected WeaponWandOrange() : base()
        {

        }

        public WeaponWandOrange(double damage, Vector2 weaponSize) : base("Orange Wand", "", damage, weaponSize, 1.0f, 1.0f)
        {
            CanParry = true;
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.WandOrange(angle);
        }

        public override void Shoot(EnemyHuman shooter, Vector2 position, Vector2 direction)
        {
            new SpellOrange(shooter.World, position)
            {
                Velocity = direction * 3,
                FrameEnd = 70,
                Shooter = shooter
            };
            PlaySFX(sfx_wand_orange_cast, 1.0f, 0.1f, 0.3f);
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/wand_orange"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponWandOrange();
        }
    }
}

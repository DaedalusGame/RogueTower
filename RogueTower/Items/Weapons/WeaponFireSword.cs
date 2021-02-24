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
    class WeaponFireSword : WeaponSword
    {
        bool FireballReady;
        Bullet LastFireball;

        public WeaponFireSword() : base()
        {
        }

        public WeaponFireSword(double damage, Vector2 weaponSize) : base(damage, weaponSize)
        {
        }

        public override void OnAttack(ActionBase action, RectangleF hitmask)
        {
            if (FireballReady)
            {
                EnemyHuman attacker = action.Human;
                for (int i = -1; i <= 1; i++)
                {
                    var time = Random.NextFloat() * 10;
                    LastFireball = new FireballBig(attacker.World, hitmask.Center + new Vector2(0, i * hitmask.Height / 2))
                    {
                        Velocity = GetFacingVector(attacker.Facing) * 3,
                        Shooter = attacker,
                        FrameEnd = 30 + time,
                        Frame = time,
                    };
                }
                FireballReady = false;
            }
        }

        public override void UpdateDiscrete(EnemyHuman holder)
        {
            if (!FireballReady && !(holder.CurrentAction is ActionAttack) && (LastFireball == null || LastFireball.Destroyed))
            {
                new WeaponFlash(holder, 20);
                FireballReady = true;
            }
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.FireSword(angle, (int)(human.Lifetime * 0.5f));
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/sword_flame"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponFireSword();
        }
    }
}

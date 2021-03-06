﻿using Humper.Base;
using Microsoft.Xna.Framework;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using static RogueTower.Util;

namespace RogueTower.Actions.Attack.AlchemicalOrbs.Orange
{
    class ActionTransplantPunch : ActionPunch
    {
        public ActionTransplantPunch(EnemyHuman human, float punchStartTime, float punchFinishTime, Weapon weapon) : base(human, punchStartTime, punchFinishTime, weapon)
        {

        }

        public override void Punch()
        {
            Vector2 weaponSize = Weapon.WeaponSize;
            RectangleF weaponMask = RectangleF.Centered(Human.Position
                + GetFacingVector(Human.Facing) * 8
                + GetFacingVector(Human.Facing) * (weaponSize.X / 2)
                + new Vector2(0, 2), weaponSize);

            foreach (var box in Human.World.FindBoxes(weaponMask))
            {
                if (box.Data is Enemy enemy && enemy.CanDamage)
                {
                    enemy.AddStatusEffect(new Bomb(enemy, 180));
                }
            }
            Human.SwingWeapon(weaponMask, Weapon.Damage);
            PunchVisual();
            PunchAction = PunchState.PunchEnd;
        }
    }
}

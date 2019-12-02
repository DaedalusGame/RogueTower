using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace RogueTower
{
    abstract class Weapon
    {
        public bool CanParry = false;
        public double Damage;
        public float WeaponSizeMult = 0;
        public Vector2 WeaponSize;

        public Weapon(Double damage, float weaponSizeMult, Vector2 weaponSize)
        {
            Damage = damage;
            WeaponSizeMult = weaponSizeMult;
            WeaponSize = weaponSize;
        }

    }

    class Sword : Weapon
    {
        public Sword(Double damage, float weaponSizeMult, Vector2 weaponSize) : base(damage, weaponSizeMult, weaponSize)
        {
            CanParry = true;
        }
    }

}

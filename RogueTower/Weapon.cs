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
        public float SwingSize;

        public Weapon(double damage, float weaponSizeMult, Vector2 weaponSize, float swingSize)
        {
            Damage = damage;
            WeaponSizeMult = weaponSizeMult;
            WeaponSize = weaponSize;
            SwingSize = swingSize;
        }

        public abstract WeaponState GetWeaponState(float angle);
    }

    class WeaponSword : Weapon
    {
        public WeaponSword(double damage, float weaponSizeMult, Vector2 weaponSize) : base(damage, weaponSizeMult, weaponSize, 0.7f)
        {
            CanParry = true;
        }

        public override WeaponState GetWeaponState(float angle)
        {
            return WeaponState.Sword(angle);
        }
    }

    class WeaponKnife : Weapon
    {
        public WeaponKnife(double damage, float weaponSizeMult, Vector2 weaponSize) : base(damage, weaponSizeMult, weaponSize, 0.5f)
        {
            CanParry = true;
        }

        public override WeaponState GetWeaponState(float angle)
        {
            return WeaponState.Knife(angle);
        }
    }

    class WeaponLance : Weapon
    {
        public WeaponLance(double damage, float weaponSizeMult, Vector2 weaponSize) : base(damage, weaponSizeMult, weaponSize, 1.5f)
        {
            CanParry = true;
        }

        public override WeaponState GetWeaponState(float angle)
        {
            return WeaponState.Lance(angle);
        }
    }
}

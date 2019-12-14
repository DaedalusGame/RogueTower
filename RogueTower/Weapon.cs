using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using static RogueTower.Util;

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

        public abstract void HandleAttack(Player player);

        public void Slash(Player player, float slashStartTime = 2, float slashUpTime = 4, float slashDownTime = 8, float slashFinishTime = 2)
        {
            player.CurrentAction = new ActionSlash(player, slashStartTime, slashUpTime, slashDownTime, slashFinishTime, this);
            player.Velocity.Y *= 0.3f;
        }

        public void Stab(Player player, float upTime = 4, float downTime = 10)
        {
            player.CurrentAction = new ActionStab(player, upTime, downTime, this);
            player.Velocity.Y *= 0.3f;
        }

        public void StabDown(Player player, float upTime = 4, float downTime = 10)
        {
            player.CurrentAction = new ActionDownStab(player, upTime, downTime, this);
            player.Velocity.Y *= 0.3f;
        }

        public void SlashKnife(Player player, float slashStartTime = 2, float slashUpTime = 4, float slashDownTime = 8, float slashFinishTime = 2)
        {
            player.CurrentAction = new ActionKnifeThrow(player, slashStartTime, slashUpTime, slashDownTime, slashFinishTime, this);
            player.Velocity.Y *= 0.3f;
        }

        public void SlashUp(Player player, float slashStartTime = 2, float slashUpTime = 4, float slashDownTime = 8, float slashFinishTime = 2)
        {
            player.CurrentAction = new ActionSlashUp(player, slashStartTime, slashUpTime, slashDownTime, slashFinishTime, this);
            player.Velocity.Y *= 0.3f;
        }

        public void SlashDown(Player player, float plungeStartTime = 5, float plungeFinishTime = 8)
        {
            player.CurrentAction = new ActionPlunge(player, plungeStartTime, plungeFinishTime, this);
            player.Velocity.X = 0;
            player.Velocity.Y = 0;
        }

        public void DashAttack(Player player, Action dashAttackAction, float dashStartTime = 2, float dashTime = 4, float dashEndTime = 2, float dashFactor = 1, bool phasing = false, bool reversed = false)
        {
            player.CurrentAction = new ActionDashAttack(player, dashStartTime, dashTime, dashEndTime, dashFactor, phasing, reversed, dashAttackAction);
        }
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

        public override void HandleAttack(Player player)
        {
            if (player.Controls.DownAttack && player.InAir)
            {
                SlashDown(player);
            }
            else if (player.Controls.DownAttack)
            {
                SlashKnife(player);
            }
            else if (player.Controls.Attack)
            {
                if(player.CurrentAction.GetType() == typeof(ActionSlash))
                    SlashUp(player);
                else
                    Slash(player);
            }
            
        }
    }

    class WeaponKatana : Weapon
    {
        public WeaponKatana(double damage, float weaponSizeMult, Vector2 weaponSize) : base(damage, weaponSizeMult, weaponSize, 1f)
        {
            CanParry = true;
        }
        public override WeaponState GetWeaponState(float angle)
        {
            return WeaponState.Katana(angle * 45);
        }
        public override void HandleAttack(Player player)
        {
            if (player.Controls.DownAttack && player.OnGround)
            {
                DashAttack(player, new ActionSlashUp(player, 2, 4, 8, 2, this), dashFactor: 4);
            }
            else if (player.Controls.Attack)
            {
                if (player.CurrentAction.GetType() == typeof(ActionSlashUp))
                    Slash(player);
                else
                {
                    SlashUp(player);
                }
            }
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

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                Stab(player);
            }
            if(player.Controls.DownAttack)
            {
                StabDown(player);
            }
            if (player.Controls.AltAttack)
            {
                player.CurrentAction = new ActionDash(player, 2, 4, 2, 3, false, true);
            }
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

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                SlashDown(player);
            }
        }
    }

    class WeaponRapier : Weapon
    {
        public int FinesseCounter = 0;
        public int FinesseLimit;
        public WeaponRapier(double damage, float weaponSizeMult, Vector2 weaponSize, int finesseLimit = 3) : base(damage, weaponSizeMult, weaponSize, 0.7f)
        {
            CanParry = true;
            FinesseLimit = finesseLimit;
        }

        public override WeaponState GetWeaponState(float angle)
        {
            return WeaponState.Rapier(angle);
        }

        public override void HandleAttack(Player player)
        {   
            if (player.Controls.Attack && FinesseCounter < FinesseLimit)
                {
                    Slash(player);
                    FinesseCounter++;
                }
            else if (player.Controls.Attack && FinesseCounter >= FinesseLimit)
                {
                    player.Velocity.X *= -5;
                    StabDown(player);
                    FinesseCounter = 0;
                }
            if (player.Controls.AltAttack)
                {
                    player.CurrentAction = new ActionDash(player, 2, 4, 2, 3, false, true);
                }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Humper.Base;
using static RogueTower.Game;
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

        public virtual void GetPose(PlayerState pose)
        {
            pose.LeftArm = ArmState.Shield;
            pose.Weapon = GetWeaponState(MathHelper.ToRadians(0));
        }

        public static Weapon[] PresetWeaponList =
        {
            new WeaponKnife(15, 14, new Vector2(14 / 2, 14 * 2)),
            new WeaponKatana(15, 20, new Vector2(10, 40)),
            new WeaponRapier(15, 20, new Vector2(10, 40)),
            new WeaponWandOrange(10, 16, new Vector2(8, 32)),
            new WeaponLance(20, 38, new Vector2(19, 76)),
            new WeaponWarhammer(30, 36, new Vector2(18, 72)),
            new WeaponBoomerang(10, 8, new Vector2(8, 8)),
            new WeaponUnarmed(10, 14, new Vector2(14, 10))
        };
        public Vector2 Input2Direction(Player player)
        {
            int up = player.Controls.ClimbUp ? -1 : 0;
            int down = player.Controls.ClimbDown ? 1 : 0;
            int left = player.Controls.MoveLeft ? -1 : 0;
            int right = player.Controls.MoveRight ? 1 : 0;

            return new Vector2(left + right, up + down);
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

        public void TwoHandSlash(Player player, float upTime, float downTime)
        {
            player.CurrentAction = new ActionTwohandSlash(player, upTime, downTime, this);
        }

        public void WandBlast(Player player, Enemy target, float upTime, float downTime)
        {
            player.CurrentAction = new ActionWandBlast(player, target, upTime, downTime, this);
        }

        public void WandBlastUntargeted(Player player, Vector2 direction, float upTime, float downTime)
        {
            player.CurrentAction = new ActionWandBlastUntargeted(player, direction, upTime, downTime, this);
        }

        public void ChargeAttack(Player player, float chargeTime, Action chargeAction, bool slowDown = true, float slowDownAmount = 0.6f)
        {
            player.CurrentAction = new ActionCharge(player, 60 * chargeTime, chargeAction, this, slowDown, slowDownAmount);
        }
    }

    class WeaponUnarmed : Weapon
    {
        public EnemyHuman Target;
        public WeaponUnarmed(double damage, float weaponSizeMult, Vector2 weaponSize) : base(damage, weaponSizeMult, weaponSize, 0.5f)
        {
            CanParry = false;
        }

        public override void GetPose(PlayerState pose)
        {
            //NOOP
        }

        public override WeaponState GetWeaponState(float angle)
        {
            return WeaponState.None;
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                if (player.CurrentAction.GetType() == typeof(ActionPunch))
                    player.CurrentAction = new ActionLeftPunch(player, 4, 8, this);
                else
                    player.CurrentAction = new ActionPunch(player, 4, 8, this);
            }
            else if(player.Controls.AltAttack){

                RectangleF searchBox = new RectangleF(player.Position + GetFacingVector(player.Facing) * 8 + GetFacingVector(player.Facing) * (WeaponSize.X / 2) + new Vector2(0, 1) - WeaponSize / 2f, WeaponSize);
                new RectangleDebug(player.World, searchBox, Color.Pink, 10);
                foreach (var box in player.World.FindBoxes(searchBox))
                {
                    if(box.Data is EnemyHuman human && !(human.Weapon is WeaponUnarmed) && !(human is Player))
                    {
                        player.CurrentAction = new ActionStealWeapon(player, human, 4, 8);
                        break;
                    }
                }
            }
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
                DashAttack(player, new ActionTwohandSlash(player, 6, 4, this), dashFactor: 4);
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
            if (player.Controls.AltAttack)
            {
                player.CurrentAction = new ActionCharge(player, 180, new ActionDashAttack(player, 2, 4, 4, 6, false, false, new ActionDownStab(player, 2, 4, this)), this, false, 0) { CanJump = true, CanMove = true };
            }
        }
    }

    class WeaponRapier : Weapon
    {
        public int FinesseCounter = 0;
        public float LastCombo;
        public int FinesseLimit;
        public WeaponRapier(double damage, float weaponSizeMult, Vector2 weaponSize, int finesseLimit = 2) : base(damage, weaponSizeMult, weaponSize, 0.7f)
        {
            CanParry = true;
            FinesseLimit = finesseLimit;
        }

        public override WeaponState GetWeaponState(float angle)
        {
            return WeaponState.Rapier(angle);
        }

        public void IncrementFinesse(Player player)
        {
            LastCombo = player.Lifetime;
            FinesseCounter++;
        }
        public override void HandleAttack(Player player)
        {
            if (player.Lifetime - LastCombo > 45)
                FinesseCounter = 0;
            if (player.Controls.Attack && FinesseCounter < FinesseLimit)
            {
                IncrementFinesse(player);

                if (player.CurrentAction.GetType() == typeof(ActionSlash))
                    SlashUp(player);
                else
                {
                    Slash(player);
                }
            }
            else if (player.Controls.Attack && FinesseCounter >= FinesseLimit)
            {
                if (FinesseCounter == FinesseLimit)
                {
                    DashAttack(player, new ActionDownStab(player, 4, 2, this), dashFactor: 4, reversed: true);
                    IncrementFinesse(player);
                }
                else if (FinesseCounter == FinesseLimit + 1)
                {
                    if (player.OnGround)
                        player.Velocity.Y = -2.5f;
                        player.OnGround = false;
                    DashAttack(player, new ActionStab(player, 4, 2, this), dashTime: 6, dashFactor: 4);
                    FinesseCounter = 0;
                }
            }
            if (player.Controls.AltAttack)
            {
                player.CurrentAction = new ActionDash(player, 2, 4, 2, 3, false, true);
            }
        }
    }

    class WeaponWandOrange : Weapon
    {

        public WeaponWandOrange(double damage, float weaponSizeMult, Vector2 weaponSize) : base(damage, weaponSizeMult, weaponSize, 0.7f)
        {
            CanParry = true;
        }

        public override void GetPose(PlayerState pose)
        {
            pose.Weapon = GetWeaponState(MathHelper.ToRadians(-45));
            pose.WeaponHold = WeaponHold.Left;
        }

        public override WeaponState GetWeaponState(float angle)
        {
            return WeaponState.WandOrange(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                //TwoHandSlash(player, 3, 12);
                player.CurrentAction = new ActionWandSwing(player, 10, 5, 20);
            }
            else if (player.Controls.AltAttack)
            {
                var SuccessOrFail = false;
                Vector2 ScanBox = new Vector2(96, 96);
                //new RectangleDebug(player.World, new RectangleF(player.Position + GetFacingVector(player.Facing) * 8 + GetFacingVector(player.Facing) * (ScanBox.X / 2) + new Vector2(0, 1) - ScanBox / 2f, ScanBox), Color.Red, 10);
                foreach (var Box in player.World.FindBoxes(new RectangleF(player.Position + GetFacingVector(player.Facing) * 8 + GetFacingVector(player.Facing) * (ScanBox.X / 2) + new Vector2(0, 1) - ScanBox / 2f, ScanBox)))
                {
                    if(Box.Data is Enemy enemy && Box.Data != player && enemy.CanDamage)
                    {
                        WandBlast(player, enemy, 24, 12);
                        SuccessOrFail = true;
                        break;
                        //With my ability to control the projectile's direciton, it's up to you if they should still have homing.
                    }

                }
                if (!SuccessOrFail)
                {
                    Vector2 Direction = Input2Direction(player);
                    bool NoDirection = Direction.Equals(new Vector2(0, 0));
                    WandBlastUntargeted(player, NoDirection ? GetFacingVector(player.Facing) * new Vector2(1,0) : Direction, 24, 12);
                }
            }
        }
    }

    class WeaponWarhammer : Weapon
    {
        public WeaponWarhammer(double damage, float weaponSizeMult, Vector2 weaponSize) : base(damage, weaponSizeMult, weaponSize, 1.5f)
        {
        }

        public override WeaponState GetWeaponState(float angle)
        {
            return WeaponState.Warhammer(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                TwoHandSlash(player, 8, 16);
            }
            else if(player.Controls.AltAttack && player.OnGround)
            {
                SlashUp(player, 2, 8, 16, 2);
                player.Velocity.Y = -5;
                player.OnGround = false;
            }
            else if(player.Controls.AltAttack && player.InAir)
            {
                player.CurrentAction = new ActionShockwave(player, 4, 8, this, 2);
            }
        }
    }

    class WeaponBoomerang : Weapon
    {
        public BoomerangProjectile BoomerProjectile;
        public WeaponBoomerang(float damage, float weaponSizeMult, Vector2 weaponSize) : base(damage, weaponSizeMult, weaponSize, 0.1f)
        {
        }

        public override WeaponState GetWeaponState(float angle)
        {
            return WeaponState.Boomerang(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack && (BoomerProjectile == null || BoomerProjectile.Destroyed))
            {
                player.CurrentAction = new ActionSlash(player, 2, 4, 8, 2, this);
            }
            else if(player.Controls.IsAiming && (BoomerProjectile == null || BoomerProjectile.Destroyed))
            {
                player.CurrentAction = new ActionAiming(player, new ActionBoomerangThrow(player, 10, this, 40));
            }
        }
    }
}
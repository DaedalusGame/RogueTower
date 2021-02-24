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
    abstract class Weapon : Item
    {
        public static float Sqrt2 = (float)Math.Sqrt(2);

        public bool CanParry = false;
        public double Damage;
        public Vector2 WeaponSize;
        public float LengthModifier = 1;
        public float WidthModifier = 1;

        protected Weapon() : base()
        {

        }

        public Weapon(string name, string description, double damage, Vector2 weaponSize, float width, float length) : base(name, description)
        {
            Damage = damage;
            WeaponSize = weaponSize;
            LengthModifier = length;
            WidthModifier = width;
        }

        public virtual void GetPose(EnemyHuman human, PlayerState pose)
        {
            pose.LeftArm = ArmState.Shield;
            pose.Shield = ShieldState.ShieldForward;
            pose.Weapon = GetWeaponState(human, MathHelper.ToRadians(0));
        }

        public static Weapon[] PresetWeaponList =
        {
            new WeaponSword(15, new Vector2(10, 40)),
            new WeaponFireSword(20, new Vector2(10, 40)),
            new WeaponKnife(15, new Vector2(14 / 2, 14 * 2)),
            new WeaponKatana(15, new Vector2(10, 40)),
            new WeaponRapier(15, new Vector2(10, 40)),
            new WeaponWandOrange(10, new Vector2(8, 32)),
            new WeaponWandAzure(10, new Vector2(8, 32)),
            new WeaponLance(20, new Vector2(19, 76)),
            new WeaponWarhammer(30, new Vector2(18, 72)),
            new WeaponBoomerang(10, new Vector2(8, 8)),
            new WeaponAlchemicalGauntlet(10, new Vector2(6,4)),
            new WeaponUnarmed(10, new Vector2(14, 10)),
        };
        public Vector2 Input2Direction(Player player)
        {
            int up = player.Controls.ClimbUp ? -1 : 0;
            int down = player.Controls.ClimbDown ? 1 : 0;
            int left = player.Controls.MoveLeft ? -1 : 0;
            int right = player.Controls.MoveRight ? 1 : 0;

            return new Vector2(left + right, up + down);
        }

        public abstract WeaponState GetWeaponState(EnemyHuman human, float angle);

        public abstract void HandleAttack(Player player);

        public virtual void OnAttack(ActionBase action, RectangleF hitmask)
        {
            //NOOP
        }

        public virtual void OnHit(ActionBase action, Enemy target)
        {
            EnemyHuman attacker = action.Human;
            target.Hit(Util.GetFacingVector(attacker.Facing) + new Vector2(0, -2), 20, 20, Damage);
        }

        public virtual void UpdateDelta(EnemyHuman holder, float delta)
        {
            //NOOP
        }

        public virtual void UpdateDiscrete(EnemyHuman holder)
        {
            //NOOP
        }

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

        public void DashAttack(Player player, ActionBase dashAttackAction, float dashStartTime = 2, float dashTime = 4, float dashEndTime = 2, float dashFactor = 1, bool phasing = false, bool reversed = false)
        {
            player.CurrentAction = new ActionDashAttack(player, dashStartTime, dashTime, dashEndTime, dashFactor, phasing, reversed, dashAttackAction);
        }

        public void TwoHandSlash(Player player, float upTime, float downTime)
        {
            player.CurrentAction = new ActionTwohandSlash(player, upTime, downTime, this);
        }

        public void ChargeAttack(Player player, float chargeTime, ActionBase chargeAction, bool slowDown = true, float slowDownAmount = 0.6f)
        {
            player.CurrentAction = new ActionCharge(player, 60 * chargeTime, chargeAction, this, slowDown, slowDownAmount);
        }

        protected void DrawWeaponAsIcon(SceneGame scene, SpriteReference sprite, int frame, Vector2 position)
        {
            Vector2 scale;
            if (sprite.Width >= 16)
                scale = new Vector2(14 * Sqrt2 / sprite.Width);
            else
                scale = Vector2.One;
            scene.DrawSpriteExt(sprite, frame, position - sprite.Middle, sprite.Middle, MathHelper.ToRadians(-45), scale, SpriteEffects.None, Color.White, 0);
        }

        protected override void CopyTo(Item item)
        {
            base.CopyTo(item);
            if (item is Weapon weapon)
            {
                weapon.CanParry = CanParry;
                weapon.Damage = Damage;
                weapon.WeaponSize = WeaponSize;
                weapon.WidthModifier = WidthModifier;
                weapon.LengthModifier = LengthModifier;
            }
        }
    }
}

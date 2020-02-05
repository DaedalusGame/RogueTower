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

namespace RogueTower
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
            pose.Weapon = GetWeaponState(human,MathHelper.ToRadians(0));
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

        public virtual void OnAttack(Action action, RectangleF hitmask)
        {
            //NOOP
        }

        public virtual void OnHit(Action action, Enemy target)
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

        public void DashAttack(Player player, Action dashAttackAction, float dashStartTime = 2, float dashTime = 4, float dashEndTime = 2, float dashFactor = 1, bool phasing = false, bool reversed = false)
        {
            player.CurrentAction = new ActionDashAttack(player, dashStartTime, dashTime, dashEndTime, dashFactor, phasing, reversed, dashAttackAction);
        }

        public void TwoHandSlash(Player player, float upTime, float downTime)
        {
            player.CurrentAction = new ActionTwohandSlash(player, upTime, downTime, this);
        }

        public void ChargeAttack(Player player, float chargeTime, Action chargeAction, bool slowDown = true, float slowDownAmount = 0.6f)
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
            if (item is Weapon weapon) {
                weapon.CanParry = CanParry;
                weapon.Damage = Damage;
                weapon.WeaponSize = WeaponSize;
                weapon.WidthModifier = WidthModifier;
                weapon.LengthModifier = LengthModifier;
            }
        }
    }

    class WeaponUnarmed : Weapon
    {
        protected WeaponUnarmed() : base()
        {

        }

        public WeaponUnarmed(double damage, Vector2 weaponSize) : base("Unarmed", "", damage, weaponSize, 1.0f, 1.0f)
        {
            CanParry = false;
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            //NOOP
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
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

        protected override Item MakeCopy()
        {
            return new WeaponUnarmed();
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            //NOOP
        }
    }

    class WeaponSword : Weapon
    {
        protected WeaponSword() : base()
        {

        }

        public WeaponSword(double damage, Vector2 weaponSize) : base("Sword", "", damage, weaponSize, 1.0f, 1.0f)
        {
            CanParry = true;
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            pose.LeftArm = ArmState.Shield;
            pose.Shield = ShieldState.ShieldForward;
            pose.Weapon = GetWeaponState(human, MathHelper.ToRadians(-90-22));
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.Sword(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.DownAttack && player.InAir)
            {
                SlashDown(player);
            }
            else if (player.Controls.DownAttack && player.OnGround && !(player.CurrentAction is ActionKnifeThrow))
            {
                SlashKnife(player);
            }
            else if (player.Controls.Attack && !player.Controls.DownAttack && !(player.CurrentAction is ActionKnifeThrow))
            {
                if(player.CurrentAction.GetType() == typeof(ActionSlash))
                    SlashUp(player);
                else
                    Slash(player);
            }

        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/sword"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponSword();
        }
    }

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

        public override void OnAttack(Action action, RectangleF hitmask)
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
            if(!FireballReady && !(holder.CurrentAction is ActionAttack) && (LastFireball == null || LastFireball.Destroyed))
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

    class WeaponKnife : Weapon
    {
        protected WeaponKnife() : base()
        {

        }

        public WeaponKnife(double damage, Vector2 weaponSize) : base("Knife", "", damage, weaponSize, 1.0f, 0.8f)
        {
            CanParry = true;
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
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

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/knife"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponKnife();
        }
    }

    class WeaponLance : Weapon
    {
        protected WeaponLance() : base()
        {

        }

        public WeaponLance(double damage, Vector2 weaponSize) : base("Lance", "", damage, weaponSize, 1.5f, 1.5f)
        {
            CanParry = true;
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            pose.RightArm = ArmState.Angular(7);
            pose.Shield = ShieldState.ShieldForward;
            pose.Weapon = GetWeaponState(human, MathHelper.ToRadians(-90));
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.Lance(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                player.CurrentAction = new ActionLanceThrust(player, 2, 12, this);
            }
            else if (player.Controls.AltAttack)
            {
                player.CurrentAction = new ActionCharge(player, 180, new ActionDashAttack(player, 2, 4, 4, 6, false, false, new ActionLanceThrust(player, 2, 6, this)), this, false, 0) { CanJump = true, CanMove = true };
            }
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/lance"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponLance();
        }
    }

    class WeaponRapier : Weapon
    {
        protected WeaponRapier() : base()
        {

        }

        public WeaponRapier(double damage, Vector2 weaponSize) : base("Rapier", "", damage, weaponSize, 1.0f, 1.2f)
        {
            CanParry = true;
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            pose.WeaponHold = WeaponHold.Left;
            pose.LeftArm = ArmState.Angular(1);
            pose.RightArm = ArmState.Angular(9);
            pose.Weapon = GetWeaponState(human, MathHelper.ToRadians(-22.5f));

        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.Rapier(angle);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack && !(player.CurrentAction is ActionRapierThrust))
            {
                player.Velocity.X += player.OnGround ? GetFacingVector(player.Facing).X * 0.75f : GetFacingVector(player.Facing).X * 0.5f;
                if (player.OnGround && !player.Controls.ClimbDown)
                {
                    player.Velocity.Y = -player.GetJumpVelocity(8);
                    player.OnGround = false;
                }
                player.CurrentAction = new ActionRapierThrust(player, 4, 8, this);                    
            }
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/rapier"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponRapier();
        }
    }

    abstract class WeaponWand : Weapon
    {
        protected WeaponWand() : base()
        {
        }

        public WeaponWand(string name, string description, double damage, Vector2 weaponSize, float width, float length) : base(name, description, damage, weaponSize, width, length)
        {
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            pose.Weapon = GetWeaponState(human, MathHelper.ToRadians(-45));
            pose.WeaponHold = WeaponHold.Left;
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                player.CurrentAction = new ActionWandSwing(player, 10, 5, 20);
            }
            else if (player.Controls.AltAttack)
            {
                player.CurrentAction = new ActionWandBlast(player, 24, 12, this);
            }
            else if (player.Controls.IsAiming)
            {
                player.CurrentAction = new ActionWandBlastAim(player, 24, 12, this);
            }
        }

        public abstract void Shoot(EnemyHuman shooter, Vector2 position, Vector2 direction);
    }

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

    class WeaponWarhammer : Weapon
    {
        protected WeaponWarhammer() : base()
        {

        }

        public WeaponWarhammer(double damage, Vector2 weaponSize) : base("Warhammer", "", damage, weaponSize, 2.0f, 2.0f)
        {
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
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

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/warhammer"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponWarhammer();
        }
    }

    class WeaponBoomerang : Weapon
    {
        public BoomerangProjectile BoomerProjectile;

        protected WeaponBoomerang() : base()
        {

        }

        public WeaponBoomerang(float damage, Vector2 weaponSize) : base("Boomerang", "", damage, weaponSize, 0.8f, 0.8f)
        {
        }

        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
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

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/boomerang"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponBoomerang();
        }
    }

    class WeaponAlchemicalGauntlet : Weapon
    {
        public AlchemicalOrbs Orb;
        public string GauntletSprite = "alchemical_gauntlet";
        public Color GauntletColor = Color.Silver;

        protected WeaponAlchemicalGauntlet() : base()
        {

        }


        public WeaponAlchemicalGauntlet(double damage, Vector2 weaponSize) : base("Alchemical Gauntlet", "", damage, weaponSize, 1.0f, 1.0f)
        {
            Orb = new OrangeOrb(this);
        }

        public override void GetPose(EnemyHuman human, PlayerState pose)
        {
            pose.Shield = ShieldState.None;
            pose.Weapon = GetWeaponState(human, 0);
        }
        public override WeaponState GetWeaponState(EnemyHuman human, float angle)
        {
            return WeaponState.AlchemicalGauntlet(angle, GauntletSprite, Orb.OrbSprite, GauntletColor, Orb.OrbColor);
        }

        public override void HandleAttack(Player player)
        {
            if(Orb != null)
                Orb.HandleAttack(player);
            else
            {
                if (player.Controls.Attack || player.Controls.AltAttack)
                {
                    PlaySFX(sfx_player_disappointed, 1, 0.1f, 0.15f);
                    player.Hit(Vector2.Zero, 1, 0, 1);
                    Util.Message(player, new MessageText("The empty alchemical gauntlet drains your strength upon activation!"));
                }
            }
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            DrawWeaponAsIcon(scene, SpriteLoader.Instance.AddSprite("content/alchemical_gauntlet"), 0, position);
        }

        protected override Item MakeCopy()
        {
            return new WeaponAlchemicalGauntlet();
        }
    }
}
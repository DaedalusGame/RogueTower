using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChaiFoxes.FMODAudio;
using static RogueTower.Game;
using static RogueTower.Util;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;

namespace RogueTower.Actions.Attack
{
    class ActionCharge : ActionBase
    {
        public float ChargeTime;
        public ActionBase ChargeAction;
        public Weapon Weapon;
        public bool SlowDown;
        public float SlowDownAmount;
        public bool CanJump = false;
        public bool CanMove = true;
        public float WalkFrame;
        public ChargeEffect chargingFX;
        public bool ChargeFinished = false;
        public ActionCharge(EnemyHuman human, float chargeTime, ActionBase chargeAction, Weapon weapon, bool slowDown, float slowDownAmount) : base(human)
        {
            ChargeTime = chargeTime;
            ChargeAction = chargeAction;
            Weapon = weapon;
            SlowDown = slowDown;
            SlowDownAmount = slowDownAmount;
            chargingFX = new ChargeEffect(human.World, human.Position, 0, chargeTime, human);
        }

        public override void OnInput()
        {
            var player = (Player)Human;
            if (SlowDown)
                player.Velocity.X *= SlowDownAmount;
            if (CanJump && (player.OnGround || player.ExtraJumps > 0))
                HandleJumpInput(player);
            if (CanMove)
                HandleMoveInput(player);

            if (!player.Controls.AltAttackHeld)
            {
                if (ChargeTime > 0)
                {
                    PlaySFX(sfx_player_disappointed, 1, 0.1f, 0.15f);
                    player.ResetState();
                }
                else
                {
                    player.CurrentAction = ChargeAction;
                }
            }
        }

        public override void UpdateDelta(float delta)
        {
            ChargeTime -= delta;
            if (ChargeTime < 0 && !ChargeFinished)
            {
                PlaySFX(sfx_sword_bink, 1.0f, 0.1f, 0.1f);
                new ParryEffect(Human.World, Human.Position, 0, 10);
                ChargeFinished = true;
            }
            WalkFrame += Math.Abs(Human.Velocity.X * delta * 0.125f) / (float)Math.Sqrt(Human.GroundFriction);
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = BodyState.Walk((int)WalkFrame);
            basePose.LeftArm = ArmState.Up;
            basePose.RightArm = ArmState.Up;
            basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-90));

        }
    }
}

using RogueTower.Actions.Interfaces;
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

namespace RogueTower.Actions.Attack.AlchemicalOrbs.Red
{
    class ActionCrimsonSaw : ActionBase
    {
        public enum SawState
        {
            WindUp,
            Spinning,
            WindDown
        }
        public SawState SawAction;
        public CrimsonSawEffect SawEffect;
        public ParryEffect CreationEffect;

        public Slider WindUpTime;
        public Slider WindDownTime;

        public float Pitch;
        public float Angle;
        public Vector2 Position;
        public float WalkFrame;

        public int DamageCounter;

        public ActionCrimsonSaw(EnemyHuman human, float windUpTime, float windDownTime) : base(human)
        {
            WindUpTime = new Slider(windUpTime);
            WindDownTime = new Slider(windDownTime);
            SawEffect = new CrimsonSawEffect(Human.World, Angle, this);

        }

        public override void OnInput()
        {
            var player = (Player)Human;
            if (SawAction == SawState.Spinning)
            {
                HandleMoveInput(player);
                if (player.OnGround)
                    HandleJumpInput(player);
            }
            if (!player.Controls.AttackHeld)
            {
                switch (SawAction)
                {
                    case (SawState.WindUp):
                        SawAction = SawState.WindDown;
                        break;
                    case (SawState.Spinning):
                        SawAction = SawState.WindDown;
                        break;
                }
            }
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (SawAction)
            {
                case (SawState.WindUp):
                    basePose.LeftArm = ArmState.Up;
                    basePose.RightArm = ArmState.Up;
                    basePose.Weapon = Human.Weapon.GetWeaponState(Human, MathHelper.ToRadians(-90));
                    break;
                case (SawState.Spinning):
                    basePose.Body = BodyState.Walk((int)WalkFrame);
                    basePose.RightArm = ArmState.Angular(2);
                    basePose.LeftArm = ArmState.Angular(1);
                    break;
                case (SawState.WindDown):
                    basePose.RightArm = ArmState.Angular(7);
                    basePose.Weapon = Human.Weapon.GetWeaponState(Human, MathHelper.ToRadians(-180));
                    break;
            }
        }
        public override void UpdateDiscrete()
        {
            /*if (SawAction == SawState.Spinning)
            {
                DamageCounter++;
                if(DamageCounter % 10 == 0)
                {
                    var hitmask = RectangleF.Centered(Position, new Vector2(16, 16));
                    //new RectangleDebug(Human.World, hitmask, Color.Red, 10);
                    Human.SwingWeapon(hitmask, 10);
                }
            }*/
        }
        public override void UpdateDelta(float delta)
        {
            if (Human.InAir && Human.Velocity.Y > 0)
                Human.Velocity.Y = 0.25f;
            switch (SawAction)
            {
                case (SawState.WindUp):
                    WindUpTime += delta * 0.25f;
                    Pitch = WindUpTime.Slide;
                    SawEffect.Angle += GetFacingVector(Human.Facing).X * WindUpTime.Slide;
                    if (CreationEffect is null)
                        CreationEffect = new ParryEffect(Human.World, Position = Human.Position - new Vector2(8, 8) + Human.Pose.GetWeaponOffset(Human.Facing.ToMirror()) + AngleToVector(Human.Pose.Weapon.GetAngle(Human.Facing.ToMirror())) * 24, 0, 20);
                    CreationEffect.Angle = SawEffect.Angle;
                    if (WindUpTime.Done)
                    {
                        SawAction = SawState.Spinning;
                    }
                    break;
                case (SawState.Spinning):
                    Pitch = 1.0f;
                    Position = Human.Position - new Vector2(8, 8) + Human.Pose.GetWeaponOffset(Human.Facing.ToMirror()) + AngleToVector(Human.Pose.Weapon.GetAngle(Human.Facing.ToMirror())) * 24;

                    if (!Human.InAir)
                        WalkFrame += Math.Abs(Human.Velocity.X * delta * 0.125f) / (float)Math.Sqrt(Human.GroundFriction);

                    SawEffect.Angle += GetFacingVector(Human.Facing).X * 10;

                    var hitmask = RectangleF.Centered(Position, new Vector2(16, 16));
                    //new RectangleDebug(Human.World, hitmask, Color.Red, 10);
                    Human.SwingWeapon(hitmask, 10);
                    Human.Velocity.X *= 0.75f;
                    break;
                case (SawState.WindDown):
                    WindDownTime += delta * 0.25f;
                    Pitch = 1 - WindDownTime.Slide;
                    SawEffect.Angle += (GetFacingVector(Human.Facing).X * 10) - WindDownTime.Slide;
                    if (WindDownTime.Done)
                    {
                        Human.ResetState();
                    }
                    break;
            }
        }
    }
}

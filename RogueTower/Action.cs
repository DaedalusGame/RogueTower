using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static RogueTower.Game;
using static RogueTower.Util;

namespace RogueTower
{
    abstract class Action
    {
        protected Player Player;
        protected Action(Player player)
        {
            Player = player;
        }

        public virtual bool HasGravity => true;
        public virtual float Friction => 1 - (1 - 0.85f) * Player.GroundFriction;
        public virtual float Drag => 0.85f;
        public virtual bool Attacking => false;

        abstract public void OnInput();

        abstract public void UpdateDelta(float delta);

        abstract public void UpdateDiscreet();

        protected bool HandleJumpInput()
        {
            if (Player.Controls.Jump)
            {
                Player.Velocity.Y -= Player.GetJumpVelocity(60);
                PlaySFX(sfx_player_jump, 0.7f, 0.1f, 0.5f);
                return true;
            }
            return false;
        }

        protected void HandleExtraJump()
        {
            if (/*Player.ExtraJumps > 0 &&*/ HandleJumpInput())
                Player.ExtraJumps--;
        }

        protected bool HandleMoveInput()
        {
            float adjustedSpeedLimit = Player.SpeedLimit / Player.AppliedFriction;
            float baseAcceleraton = 0.25f;
            if (Player.OnGround)
                baseAcceleraton *= Player.GroundFriction;
            float acceleration = 0.25f / Player.AppliedFriction;

            if (Player.Controls.MoveLeft && Player.Velocity.X > -adjustedSpeedLimit)
                Player.Velocity.X = Math.Max(Player.Velocity.X - acceleration, -adjustedSpeedLimit);
            if (Player.Controls.MoveRight && Player.Velocity.X < adjustedSpeedLimit)
                Player.Velocity.X = Math.Min(Player.Velocity.X + acceleration, adjustedSpeedLimit);

            return Player.Controls.MoveLeft || Player.Controls.MoveRight;
        }

        protected void HandleSlashInput()
        {
            if (Player.Controls.Attack)
            {
                if (Player.Controls.DownAttack && Player.InAir)
                    Player.SlashDown();
                else if (Player.Controls.DownAttack)
                    Player.SlashKnife();
                else
                    Player.Slash();
            }
        }

        abstract public void GetPose(PlayerState basePose);
    }

    class ActionIdle : Action
    {
        public ActionIdle(Player player) : base(player)
        {

        }

        public override void GetPose(PlayerState basePose)
        {
            //NOOP
        }

        public override void OnInput()
        {
            HandleMoveInput();
            HandleJumpInput();
            HandleSlashInput();
        }

        public override void UpdateDelta(float delta)
        {
            if (!Player.OnGround)
                Player.CurrentAction = new ActionJump(Player,true,true);
            else if(Math.Abs(Player.Velocity.X) >= 0.01)
                Player.CurrentAction = new ActionMove(Player);
        }

        public override void UpdateDiscreet()
        {
            //NOOP
        }
    }

    class ActionMove : Action
    {
        public float WalkFrame;
        public bool Walking;

        public ActionMove(Player player) : base(player)
        {

        }

        public override void OnInput()
        {
            Walking = HandleMoveInput();
            HandleJumpInput();
            HandleSlashInput();
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = BodyState.Walk((int)WalkFrame);
        }

        public override void UpdateDelta(float delta)
        {
            if (Walking)
            {
                if (Player.Velocity.X > 0)
                {
                    Player.Facing = HorizontalFacing.Right;
                }
                else if (Player.Velocity.X < 0)
                {
                    Player.Facing = HorizontalFacing.Left;
                }
                WalkFrame += Math.Abs(Player.Velocity.X * delta * 0.125f) / (float)Math.Sqrt(Player.GroundFriction);
            }
            if (!Player.OnGround)
                Player.CurrentAction = new ActionJump(Player, true, true);
            else if (Math.Abs(Player.Velocity.X) < 0.01)
                Player.CurrentAction = new ActionIdle(Player);
        }

        public override void UpdateDiscreet()
        {
            //NOOP
        }
    }

    class ActionJump : Action
    {
        public enum State
        {
            Up,
            Down,
        }

        public State CurrentState;
        public bool Control;
        public bool AllowAirControl;
        public bool AllowJumpControl;

        public override float Drag => AllowAirControl ? base.Drag : 1;

        public ActionJump(Player player, bool airControl, bool jumpControl) : base(player)
        {
            AllowAirControl = airControl;
            AllowJumpControl = jumpControl;
        }

        public override void OnInput()
        {
            Control = HandleMoveInput();
            if (AllowJumpControl && !Player.Controls.JumpHeld && Player.Velocity.Y < 0)
                Player.Velocity.Y *= 0.7f;
            HandleExtraJump();
            HandleSlashInput();
        }

        public override void UpdateDelta(float delta)
        {
            if (Control)
            {
                if (Player.Velocity.X > 0)
                    Player.Facing = HorizontalFacing.Right;
                else if (Player.Velocity.X < 0)
                    Player.Facing = HorizontalFacing.Left;
            }

            if (Player.Velocity.Y < 0)
                CurrentState = State.Up;
            else
                CurrentState = State.Down;

            if (Player.OnGround)
                Player.CurrentAction = new ActionIdle(Player);
        }

        public override void UpdateDiscreet()
        {
            if (Player.OnWall)
            {
                var wallTiles = Player.World.FindTiles(Player.Box.Bounds.Offset(GetFacingVector(Player.Facing)));
                var climbTiles = wallTiles.Where(tile => tile.CanClimb(Player.Facing.Mirror()));
                if (Player.InAir && climbTiles.Any() && CurrentState == State.Down)
                {
                    Player.Velocity.Y = 0;
                    Player.CurrentAction = new ActionClimb(Player);
                }
            }
        }

        public override void GetPose(PlayerState basePose)
        {
            switch(CurrentState)
            {
                default:
                case (State.Up):
                    if (Player.Velocity.Y < -0.5)
                        basePose.Body = BodyState.Walk(1);
                    else
                        basePose.Body = BodyState.Walk(0);
                    break;
                case (State.Down):
                    basePose.Body = BodyState.Walk(2);
                    break;
            }
        }
    }

    class ActionHit : Action
    {
        int Time;

        public override float Drag => 1;

        public ActionHit(Player player, int time) : base(player)
        {
            Time = time;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Head = HeadState.Down;
            basePose.Body = BodyState.Hit;
            basePose.RightArm = ArmState.Angular(3);
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void UpdateDelta(float delta)
        {
            //NOOP
        }

        public override void UpdateDiscreet()
        {
            Time--;
            if (Time <= 0)
            {
                Player.ResetState();
            }
        }
    }

    class ActionClimb : Action
    {
        public float ClimbFrame;

        public override bool HasGravity => false;

        public ActionClimb(Player player) : base(player)
        {
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = BodyState.Climb;
            basePose.LeftArm = ArmState.Angular(11 + Util.PositiveMod(3 + (int)-ClimbFrame, 7));
            basePose.RightArm = ArmState.Angular(11 + Util.PositiveMod((int)-ClimbFrame, 7));
            basePose.Weapon = WeaponState.None;
        }

        public override void OnInput()
        {
            if (Player.Controls.ClimbUp)
                Player.Velocity.Y = -0.5f;
            if (Player.Controls.ClimbDown)
                Player.Velocity.Y = 0.5f;
            if (!Player.Controls.ClimbUp && !Player.Controls.ClimbDown)
                Player.Velocity.Y = 0;
            if (Player.Controls.Jump)
            {
                Player.OnWall = false;
                Player.CurrentAction = new ActionJump(Player, false, true);
                Player.Velocity = GetFacingVector(Player.Facing) * -Player.GetJumpVelocity(30) * 0.5f + new Vector2(0, -Player.GetJumpVelocity(30));
                //Player.DisableAirControl = true;
                Player.Facing = Player.Facing.Mirror();
                PlaySFX(sfx_player_jump, 0.7f, 0.1f, 0.5f);
            }
            if (Player.OnGround && ((Player.Controls.MoveLeft && Player.Facing == HorizontalFacing.Right) || (Player.Controls.MoveRight && Player.Facing == HorizontalFacing.Left)))
            {
                Player.OnWall = false;
                Player.ResetState();
                Player.Facing = Player.Facing.Mirror();
            }
        }

        public override void UpdateDelta(float delta)
        {
            ClimbFrame += Player.Velocity.Y * delta * 0.5f;
        }

        public override void UpdateDiscreet()
        {
            var climbTiles = Player.World.FindTiles(Player.Box.Bounds.Offset(GetFacingVector(Player.Facing))).Where(tile => tile.CanClimb(Player.Facing.Mirror()));
            if (!climbTiles.Any())
            {
                Player.OnWall = false;
                Player.CurrentAction = new ActionJump(Player, true, true);
                Player.Velocity.X = GetFacingVector(Player.Facing).X; //Tiny nudge to make the player stand on the ladder
            }
        }
    }

    class ActionSlash : Action
    {
        public SwingAction SlashAction;
        public float SlashStartTime;
        public float SlashUpTime;
        public float SlashDownTime;
        public float SlashFinishTime;

        public bool Parried;
        public bool IsUpSwing => SlashAction == SwingAction.UpSwing || SlashAction == SwingAction.StartSwing;
        public bool IsDownSwing => SlashAction == SwingAction.DownSwing || SlashAction == SwingAction.FinishSwing;

        public override float Friction => Parried ? 1 : base.Friction;
        public override float Drag => 1 - (1 - base.Drag) * 0.1f;
        public override bool Attacking => true;

        public enum SwingAction
        {
            StartSwing,
            UpSwing,
            DownSwing,
            FinishSwing,
        }

        public ActionSlash(Player player, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime) : base(player)
        {
            SlashStartTime = slashStartTime;
            SlashUpTime = slashUpTime;
            SlashDownTime = slashDownTime;
            SlashFinishTime = slashFinishTime;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Player.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch(SlashAction)
            {
                default:
                case (SwingAction.StartSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Player.Weapon.GetWeaponState(MathHelper.ToRadians(-90 - 22));
                    break;
                case (SwingAction.UpSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Player.Weapon.GetWeaponState(MathHelper.ToRadians(-90 - 45));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(4);
                    basePose.Weapon = Player.Weapon.GetWeaponState(MathHelper.ToRadians(45 + 22));
                    break;
                case (SwingAction.FinishSwing):
                    basePose.RightArm = ArmState.Angular(4);
                    basePose.Weapon = Player.Weapon.GetWeaponState(MathHelper.ToRadians(45 + 22));
                    break;
            }
            
        }

        public override void OnInput()
        {
            if (Player.Controls.Attack && IsDownSwing)
            {
                if (Player.Controls.DownAttack && Player.InAir)
                    Player.SlashDown();
                else if (Player.Controls.DownAttack)
                    Player.SlashKnife();
                else
                    Player.SlashUp();
            }
            if (Parried)
                HandleExtraJump();
        }

        public override void UpdateDelta(float delta)
        {
            switch (SlashAction)
            {
                case (SwingAction.StartSwing):
                    SlashStartTime -= delta;
                    if (SlashStartTime < 0)
                        SlashAction = SwingAction.UpSwing;
                    break;
                case (SwingAction.UpSwing):
                    SlashUpTime -= delta;
                    if (SlashUpTime < 0)
                        Swing();
                    break;
                case (SwingAction.DownSwing):
                    SlashDownTime -= delta;
                    if (SlashDownTime < 0)
                        SlashAction = SwingAction.FinishSwing;
                    break;
                case (SwingAction.FinishSwing):
                    SlashFinishTime -= delta;
                    if (SlashFinishTime < 0)
                        Player.ResetState();
                    break;
            }
        }

        public virtual void Swing()
        {
            Vector2 Position = Player.Position;
            HorizontalFacing Facing = Player.Facing;
            Vector2 FacingVector = GetFacingVector(Facing);
            Vector2 PlayerWeaponOffset = Position + FacingVector * Player.Weapon.WeaponSizeMult;
            Vector2 WeaponSize = Player.Weapon.WeaponSize;
            RectangleF weaponMask = new RectangleF(PlayerWeaponOffset - WeaponSize / 2, WeaponSize);
            if (Player.Weapon.CanParry == true)
            {
                Vector2 parrySize = new Vector2(22, 22);
                bool success = Player.Parry(new RectangleF(Position + FacingVector * 8 - parrySize / 2, parrySize));
                if(success)
                    Parried = true;
            }
            if (!Parried)
                Player.SwingWeapon(weaponMask, 10);
            SwingVisual(Parried);
            SlashAction = SwingAction.DownSwing;
        }

        public virtual void SwingVisual(bool parry)
        {
            var effect = new SlashEffect(Player.World, () => Player.Position, Player.Weapon.SwingSize, 0, Player.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
            if (parry)
                effect.Frame = effect.FrameEnd / 2;
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
        }

        public override void UpdateDiscreet()
        {
            //NOOP
        }
    }
    class ActionSlashUp : ActionSlash
    {
        public ActionSlashUp(Player player, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime) : base(player, slashStartTime, slashUpTime, slashDownTime, slashFinishTime)
        {
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Player.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch (SlashAction)
            {
                default:
                case (SwingAction.StartSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(6);
                    basePose.Weapon = Player.Weapon.GetWeaponState(MathHelper.ToRadians(100));
                    break;
                case (SwingAction.UpSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(6);
                    basePose.Weapon = Player.Weapon.GetWeaponState(MathHelper.ToRadians(125));
                    break;
                case (SwingAction.DownSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Player.Weapon.GetWeaponState(MathHelper.ToRadians(-75));
                    break;
                case (SwingAction.FinishSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Player.Weapon.GetWeaponState(MathHelper.ToRadians(-75));
                    break;
            }
            if (Parried)
                HandleExtraJump();
        }

        public override void OnInput()
        {
            if (IsDownSwing)
            {
                HandleSlashInput();
            }
        }

        public override void SwingVisual(bool parry)
        {
            var effect = new SlashEffect(Player.World, () => Player.Position, Player.Weapon.SwingSize, MathHelper.ToRadians(45), SpriteEffects.FlipVertically | (Player.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 4);
            if (parry)
                effect.Frame = effect.FrameEnd / 2;
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
        }
    }

    class ActionKnifeThrow : ActionSlash
    {
        public override bool Attacking => false;

        public ActionKnifeThrow(Player player, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime) : base(player, slashStartTime, slashUpTime, slashDownTime, slashFinishTime)
        {

        }

        public override void GetPose(PlayerState basePose)
        {
            switch (SlashAction)
            {
                default:
                case (SwingAction.StartSwing):
                    
                    basePose.RightArm = ArmState.Angular(5);
                    basePose.Weapon = WeaponState.Knife(MathHelper.ToRadians(90 + 45));
                    break;
                case (SwingAction.UpSwing):
                    
                    basePose.RightArm = ArmState.Angular(6);
                    basePose.Weapon = WeaponState.Knife(MathHelper.ToRadians(90 + 45 + 22));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(0);
                    basePose.Weapon = WeaponState.None;
                    break;
                case (SwingAction.FinishSwing):
                    basePose.Body = BodyState.Crouch(2);
                    basePose.RightArm = ArmState.Angular(0);
                    basePose.Weapon = WeaponState.None;
                    break;
            }
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void Swing()
        {
            Vector2 facing = GetFacingVector(Player.Facing);
            new Knife(Player.World, Player.Position + facing * 5)
            {
                Velocity = facing * 8,
                LifeTime = 20,
                Shooter = Player
            };
            PlaySFX(sfx_knife_throw, 1.0f, 0.4f, 0.7f);
            SlashAction = SwingAction.DownSwing;
        }
    }

    class ActionPlunge : Action
    {
        public float PlungeStartTime;
        public float PlungeFinishTime;
        public bool PlungeFinished = false;

        public override bool HasGravity => false;

        public ActionPlunge(Player player, float plungeStartTime, float plungeFinishTime) : base(player)
        {
            PlungeStartTime = plungeStartTime;
            PlungeFinishTime = plungeFinishTime;
        }

        public override void OnInput()
        {
        }

        public override void UpdateDiscreet()
        {
            if (PlungeStartTime <= 0)
                Player.Velocity.Y = 5;
            if (Player.OnGround)
            {
                Player.Velocity.Y = -4;
                Player.OnGround = false;
                Player.World.Hitstop = 4;
                PlaySFX(sfx_sword_bink, 1.0f, 0.1f, 0.4f);
                Player.CurrentAction = new ActionJump(Player, true, false);
                PlungeFinished = true;

                double damageIn = Player.Weapon.Damage * 1.5;
                foreach (var box in Player.World.FindBoxes(Player.Box.Bounds.Offset(0, 1)))
                {
                    if(box.Data is Tile tile)
                        tile.HandleTileDamage(damageIn);
                    if (box.Data is Enemy enemy)
                        enemy.Hit(new Vector2(0, 2), 20, 50, damageIn);
                }
            }
            if (PlungeFinished && PlungeFinishTime <= 0)
                Player.CurrentAction = new ActionIdle(Player);
        }

        public override void UpdateDelta(float delta)
        {
            if (PlungeFinished)
                PlungeFinishTime -= delta;
            else
                PlungeStartTime -= delta;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Head = HeadState.Down;
            basePose.Body = BodyState.Crouch(1);
            basePose.LeftArm = ArmState.Angular(4);
            basePose.RightArm = ArmState.Angular(2);
            basePose.Weapon = Player.Weapon.GetWeaponState(MathHelper.ToRadians(90));
        }
    }



}

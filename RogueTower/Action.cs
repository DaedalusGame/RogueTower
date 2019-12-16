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
        protected EnemyHuman Human;
        protected Action(EnemyHuman player)
        {
            Human = player;
        }

        public virtual bool HasGravity => true;
        public virtual float Friction => 1 - (1 - 0.85f) * Human.GroundFriction;
        public virtual float Drag => 0.85f;
        public virtual bool Attacking => false;
        public virtual bool Incorporeal => false;

        abstract public void OnInput();

        abstract public void UpdateDelta(float delta);

        abstract public void UpdateDiscrete();

        protected bool HandleJumpInput(Player player)
        {
            if (player.Controls.Jump)
            {
                Human.Velocity.Y -= Human.GetJumpVelocity(60);
                PlaySFX(sfx_player_jump, 0.5f, 0.1f, 0.5f);
                return true;
            }
            return false;
        }

        protected void HandleExtraJump(Player player)
        {
            if (Human.ExtraJumps > 0 && HandleJumpInput(player))
                Human.ExtraJumps--;
        }

        protected void HandleMoveInput(Player player)
        {
            float adjustedSpeedLimit = Human.SpeedLimit;
            float baseAcceleraton = Human.Acceleration;
            if (Human.OnGround)
                baseAcceleraton *= Human.GroundFriction;
            float acceleration = baseAcceleraton;

            if (player.Controls.MoveLeft && Human.Velocity.X > -adjustedSpeedLimit)
                Human.Velocity.X = Math.Max(Human.Velocity.X - acceleration, -adjustedSpeedLimit);
            if (player.Controls.MoveRight && Human.Velocity.X < adjustedSpeedLimit)
                Human.Velocity.X = Math.Min(Human.Velocity.X + acceleration, adjustedSpeedLimit);
            if ((player.Controls.MoveLeft && Human.Velocity.X < 0) || (player.Controls.MoveRight && Human.Velocity.X > 0))
                Human.AppliedFriction = 1;
        }

        protected void HandleSlashInput(Player player)
        {
            player.Weapon.HandleAttack(player);
        }

        abstract public void GetPose(PlayerState basePose);
    }

    class ActionIdle : Action
    {
        public ActionIdle(EnemyHuman player) : base(player)
        {

        }

        public override void GetPose(PlayerState basePose)
        {
            //NOOP
        }

        public override void OnInput()
        {
            var player = (Player)Human;
            HandleMoveInput(player);
            HandleJumpInput(player);
            HandleSlashInput(player);
        }

        public override void UpdateDelta(float delta)
        {
            if (!Human.OnGround)
                Human.CurrentAction = new ActionJump(Human,true,true);
            else if(Math.Abs(Human.Velocity.X) >= 0.01)
                Human.CurrentAction = new ActionMove(Human);
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
    }

    class ActionMove : Action
    {
        public float WalkFrame;
        public bool WalkingLeft;
        public bool WalkingRight;

        public ActionMove(EnemyHuman player) : base(player)
        {

        }

        public override void OnInput()
        {
            var player = (Player)Human;
            HandleMoveInput(player);
            WalkingLeft = player.Controls.MoveLeft;
            WalkingRight = player.Controls.MoveRight;
            HandleJumpInput(player);
            HandleSlashInput(player);
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = BodyState.Walk((int)WalkFrame);
        }

        public override void UpdateDelta(float delta)
        {
            if (WalkingLeft || WalkingRight)
            {
                if (!Human.Strafing)
                {
                    if (Human.Velocity.X > 0 && WalkingRight)
                    {
                        Human.Facing = HorizontalFacing.Right;
                    }
                    else if (Human.Velocity.X < 0 && WalkingLeft)
                    {
                        Human.Facing = HorizontalFacing.Left;
                    }
                }
                WalkFrame += Math.Abs(Human.Velocity.X * delta * 0.125f) / (float)Math.Sqrt(Human.GroundFriction);
            }
            if (!Human.OnGround)
                Human.CurrentAction = new ActionJump(Human, true, true);
            else if (Math.Abs(Human.Velocity.X) < 0.01)
                Human.CurrentAction = new ActionIdle(Human);
        }

        public override void UpdateDiscrete()
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
        public bool JumpingLeft;
        public bool JumpingRight;
        public bool AllowAirControl;
        public bool AllowJumpControl;

        public override float Drag => AllowAirControl ? base.Drag : 1;

        public ActionJump(EnemyHuman player, bool airControl, bool jumpControl) : base(player)
        {
            AllowAirControl = airControl;
            AllowJumpControl = jumpControl;
        }

        public override void OnInput()
        {
            var player = (Player)Human;
            HandleMoveInput(player);
            if (AllowJumpControl && !player.Controls.JumpHeld && Human.Velocity.Y < 0)
                Human.Velocity.Y *= 0.7f;
            JumpingLeft = player.Controls.MoveLeft;
            JumpingRight = player.Controls.MoveRight;
            HandleExtraJump(player);
            HandleSlashInput(player);
        }

        public override void UpdateDelta(float delta)
        {
            if (JumpingLeft || JumpingRight)
            {
                if (!Human.Strafing)
                {
                    if (Human.Velocity.X > 0 && JumpingRight)
                        Human.Facing = HorizontalFacing.Right;
                    else if (Human.Velocity.X < 0 && JumpingLeft)
                        Human.Facing = HorizontalFacing.Left;
                }
            }

            if (Human.Velocity.Y < 0)
                CurrentState = State.Up;
            else
                CurrentState = State.Down;

            if (Human.OnGround)
                Human.CurrentAction = new ActionIdle(Human);
        }

        public override void UpdateDiscrete()
        {
            if (Human.OnWall)
            {
                var wallTiles = Human.World.FindTiles(Human.Box.Bounds.Offset(GetFacingVector(Human.Facing)));
                var climbTiles = wallTiles.Where(tile => tile.CanClimb(Human.Facing.Mirror()));
                if (Human.InAir && climbTiles.Any() && CurrentState == State.Down)
                {
                    Human.Velocity.Y = 0;
                    Human.CurrentAction = new ActionClimb(Human);
                }
            }
        }

        public override void GetPose(PlayerState basePose)
        {
            switch(CurrentState)
            {
                default:
                case (State.Up):
                    if (Human.Velocity.Y < -0.5)
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

        public ActionHit(EnemyHuman player, int time) : base(player)
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

        public override void UpdateDiscrete()
        {
            Time--;
            if (Time <= 0)
            {
                Human.ResetState();
            }
        }
    }

    class ActionEnemyDeath : Action
    {
        int Time;

        public override float Drag => 1;
        public override bool Incorporeal => true;

        public ActionEnemyDeath(EnemyHuman player, int time) : base(player)
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

        public override void UpdateDiscrete()
        {
            Time--;
            if (Time <= 0)
            {
                Human.Destroy();
            }
            Vector2 pos = new Vector2(Human.Box.X + Human.Random.NextFloat() * Human.Box.Width, Human.Box.Y + Human.Random.NextFloat() * Human.Box.Height);
            new FireEffect(Human.World, pos, 0, 5);
        }
    }

    class ActionClimb : Action
    {
        public float ClimbFrame;

        public override bool HasGravity => false;

        public ActionClimb(EnemyHuman player) : base(player)
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
            var player = (Player)Human;
            if (player.Controls.ClimbUp)
                Human.Velocity.Y = -0.5f;
            if (player.Controls.ClimbDown)
                Human.Velocity.Y = 0.5f;
            if (!player.Controls.ClimbUp && !player.Controls.ClimbDown)
                Human.Velocity.Y = 0;
            if (player.Controls.Jump)
            {
                Human.OnWall = false;
                Human.CurrentAction = new ActionJump(Human, false, true);
                Human.Velocity = GetFacingVector(Human.Facing) * -Human.GetJumpVelocity(30) * 0.5f + new Vector2(0, -Human.GetJumpVelocity(30));
                //Player.DisableAirControl = true;
                Human.Facing = Human.Facing.Mirror();
                PlaySFX(sfx_player_jump, 0.5f, 0.1f, 0.5f);
            }
            if (Human.OnGround && ((player.Controls.MoveLeft && Human.Facing == HorizontalFacing.Right) || (player.Controls.MoveRight && Human.Facing == HorizontalFacing.Left)))
            {
                Human.OnWall = false;
                Human.ResetState();
                Human.Facing = Human.Facing.Mirror();
            }
        }

        public override void UpdateDelta(float delta)
        {
            ClimbFrame += Human.Velocity.Y * delta * 0.5f;
        }

        public override void UpdateDiscrete()
        {
            var climbTiles = Human.World.FindTiles(Human.Box.Bounds.Offset(GetFacingVector(Human.Facing))).Where(tile => tile.CanClimb(Human.Facing.Mirror()));
            if (!climbTiles.Any())
            {
                Human.OnWall = false;
                Human.CurrentAction = new ActionJump(Human, true, true);
                Human.Velocity.X = GetFacingVector(Human.Facing).X; //Tiny nudge to make the player stand on the ladder
            }
        }
    }

    class ActionDash : Action
    {
        public DashState DashAction;
        public float DashStartTime;
        public float DashTime;
        public float DashEndTime;
        public float DashFactor;
        public bool Phasing;
        public bool Reversed;
        public override float Friction => 1;

        public enum DashState
        {
            DashStart,
            Dash,
            DashEnd,
        }

        public ActionDash(EnemyHuman player, float dashStartTime, float dashTime, float dashEndTime, float dashFactor, bool phasing, bool reversed) : base(player)
        {
            DashStartTime = dashStartTime;
            DashTime = dashTime;
            DashEndTime = dashEndTime;
            DashFactor = dashFactor;
            Phasing = phasing;
            Reversed = reversed;
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (DashAction)
            {
                default:
                case (DashState.DashStart):
                    basePose.Head = HeadState.Down;
                    break;
                case (DashState.Dash):
                    basePose.Head = HeadState.Forward;
                    basePose.Body = BodyState.Crouch(1);
                    break;
                case (DashState.DashEnd):
                    basePose.Head = HeadState.Down;
                    basePose.Body = BodyState.Crouch(1);
                    break;
            }
        }

        public override void OnInput()
        {

        }

        public override void UpdateDelta(float delta)
        {
            switch (DashAction)
            {
                case (DashState.DashStart):
                    DashStartTime -= delta;
                    if (DashStartTime < 0)
                        DashAction = DashState.Dash;
                    break;
                case (DashState.Dash):
                    DashTime -= delta;
                    Human.Velocity.X = MathHelper.Clamp((GetFacingVector(Human.Facing).X * (Reversed ? -1 : 1)) * DashFactor, -DashFactor, DashFactor);
                    if (DashTime < 0)
                        DashAction = DashState.DashEnd;
                    break;
                case (DashState.DashEnd):
                    DashEndTime -= delta;
                    if (DashEndTime < 0)
                        Human.ResetState();
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
        }
    }

    class ActionDashAttack : ActionDash
    {
        public Action DashAttack;
        public ActionDashAttack(EnemyHuman player, float dashStartTime, float dashTime, float dashEndTime, float dashFactor, bool phasing, bool reversed, Action actionDashAttack) : base(player, dashStartTime, dashTime, dashEndTime, dashFactor, phasing, reversed)
        {
            DashStartTime = dashStartTime;
            DashTime = dashTime;
            DashEndTime = dashEndTime;
            DashFactor = dashFactor;
            Phasing = phasing;
            Reversed = reversed;
            DashAttack = actionDashAttack;
        }

        public override void UpdateDelta(float delta)
        {
            switch (DashAction)
            {
                case (DashState.DashStart):
                    DashStartTime -= delta;
                    if (DashStartTime < 0)
                        DashAction = DashState.Dash;
                    break;
                case (DashState.Dash):
                    DashTime -= delta;
                    Human.Velocity.X = MathHelper.Clamp((GetFacingVector(Human.Facing).X * (Reversed ? -1 : 1)) * DashFactor, -DashFactor, DashFactor);
                    if (DashTime < 0)
                        DashAction = DashState.DashEnd;
                    break;
                case (DashState.DashEnd):
                    DashEndTime -= delta;
                    if (DashEndTime < 0)
                        Human.CurrentAction = DashAttack;
                    break;
            }
        }
    }
    class ActionSlash : Action
    {
        public Weapon Weapon;
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

        public ActionSlash(EnemyHuman player, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime, Weapon weapon) : base(player)
        {
            SlashStartTime = slashStartTime;
            SlashUpTime = slashUpTime;
            SlashDownTime = slashDownTime;
            SlashFinishTime = slashFinishTime;
            Weapon = weapon;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch(SlashAction)
            {
                default:
                case (SwingAction.StartSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(-90 - 22));
                    break;
                case (SwingAction.UpSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(-90 - 45));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(4);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(45 + 22));
                    break;
                case (SwingAction.FinishSwing):
                    basePose.RightArm = ArmState.Angular(4);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(45 + 22));
                    break;
            }
            
        }

        public override void OnInput()
        {
            var player = (Player)Human;
            if (IsDownSwing)
            {
                HandleSlashInput(player);
            }
            if (Parried)
                HandleExtraJump(player);
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
                        Human.ResetState();
                    break;
            }
        }

        public virtual void Swing()
        {
            Vector2 Position = Human.Position;
            HorizontalFacing Facing = Human.Facing;
            Vector2 FacingVector = GetFacingVector(Facing);
            Vector2 PlayerWeaponOffset = Position + FacingVector * Weapon.WeaponSizeMult;
            Vector2 WeaponSize = Weapon.WeaponSize;
            RectangleF weaponMask = new RectangleF(PlayerWeaponOffset - WeaponSize / 2, WeaponSize);
            if (Weapon.CanParry == true)
            {
                Vector2 parrySize = new Vector2(22, 22);
                bool success = Human.Parry(new RectangleF(Position + FacingVector * 8 - parrySize / 2, parrySize));
                if(success)
                    Parried = true;
            }
            if (!Parried)
                Human.SwingWeapon(weaponMask, 10);
            SwingVisual(Parried);
            SlashAction = SwingAction.DownSwing;
        }

        public virtual void SwingVisual(bool parry)
        {
            var effect = new SlashEffectRound(Human.World, () => Human.Position, Weapon.SwingSize, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
            if (parry)
                effect.Frame = effect.FrameEnd / 2;
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
    }
    class ActionSlashUp : ActionSlash
    {
        public ActionSlashUp(EnemyHuman player, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime, Weapon weapon) : base(player, slashStartTime, slashUpTime, slashDownTime, slashFinishTime, weapon)
        {
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch (SlashAction)
            {
                default:
                case (SwingAction.StartSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(6);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(100));
                    break;
                case (SwingAction.UpSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(6);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(125));
                    break;
                case (SwingAction.DownSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(-75));
                    break;
                case (SwingAction.FinishSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(-75));
                    break;
            }
        }

        public override void OnInput()
        {
            var player = (Player)Human;
            if (IsDownSwing)
            {
                HandleSlashInput(player);
            }

            if (Parried)
                HandleExtraJump(player);
        }

        public override void SwingVisual(bool parry)
        {
            var effect = new SlashEffectRound(Human.World, () => Human.Position, Weapon.SwingSize, MathHelper.ToRadians(45), SpriteEffects.FlipVertically | (Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 4);
            if (parry)
                effect.Frame = effect.FrameEnd / 2;
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
        }
    }

    class ActionStab : Action
    {
        public enum SwingAction
        {
            UpSwing,
            DownSwing,
        }

        public Weapon Weapon;
        public SwingAction SlashAction;
        public float SlashUpTime;
        public float SlashDownTime;
        public bool Parried;

        public bool IsUpSwing => SlashAction == SwingAction.UpSwing;
        public bool IsDownSwing => SlashAction == SwingAction.DownSwing;

        public override float Friction => Parried ? 1 : base.Friction;
        public override float Drag => 1 - (1 - base.Drag) * 0.1f;

        public ActionStab(EnemyHuman player, float upTime, float downTime, Weapon weapon) : base(player)
        {
            SlashUpTime = upTime;
            SlashDownTime = downTime;
            Weapon = weapon;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch (SlashAction)
            {
                default:
                case (SwingAction.UpSwing):
                    basePose.RightArm = ArmState.Angular(8);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(-90 + 22.5f));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(0);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(0));
                    break;
            }
        }

        public override void OnInput()
        {

        }

        public override void UpdateDelta(float delta)
        {
            switch (SlashAction)
            {
                case (SwingAction.UpSwing):
                    SlashUpTime -= delta;
                    if (SlashUpTime < 0)
                        Swing();
                    break;
                case (SwingAction.DownSwing):
                    SlashDownTime -= delta;
                    if (SlashDownTime < 0)
                        Human.ResetState();
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }

        public virtual void Swing()
        {
            float sizeMult = (Human.Random.NextFloat() * 3 - 0.5f) + 1;
            Vector2 weaponSize = new Vector2(12, 4) * sizeMult;
            RectangleF weaponMask = new RectangleF(Human.Position
                + GetFacingVector(Human.Facing) * 8
                + GetFacingVector(Human.Facing) * (weaponSize.X / 2)
                + new Vector2(0, 1)
                - weaponSize / 2f,
                weaponSize);
            new RectangleDebug(Human.World, weaponMask, Color.Red, 10);
            /*
            Vector2 PlayerWeaponOffset = Position + FacingVector * 14;
            Vector2 WeaponSize = new Vector2(14 / 2, 14 * 2);
            RectangleF weaponMask = new RectangleF(PlayerWeaponOffset - WeaponSize / 2, WeaponSize);*/
            if (Weapon.CanParry)
            {
                HorizontalFacing Facing = Human.Facing;
                Vector2 Position = Human.Position;
                Vector2 FacingVector = GetFacingVector(Facing);
                Vector2 parrySize = new Vector2(22, 22);
                bool success = Human.Parry(new RectangleF(Position + FacingVector * 8 - parrySize / 2, parrySize));
                if (success)
                    Parried = true;
            }
            if (!Parried)
                Human.SwingWeapon(weaponMask, 10);
            SwingVisual(Parried);
            SlashAction = SwingAction.DownSwing;
        }

        public virtual void SwingVisual(bool parry)
        {
            Vector2 FacingVector = GetFacingVector(Human.Facing);
            var effect = new SlashEffectStraight(Human.World, () => Human.Position + FacingVector * 6, Weapon.SwingSize, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
            if (parry)
                effect.Frame = effect.FrameEnd / 2;
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
        }
    }

    class ActionDownStab : ActionStab
    {
        public ActionDownStab(EnemyHuman player, float upTime, float downTime, Weapon weapon) : base(player, upTime, downTime, weapon)
        {
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (SlashAction)
            {
                default:
                case (SwingAction.UpSwing):
                    basePose.Body = BodyState.Crouch(2);
                    basePose.RightArm = ArmState.Angular(6);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(0));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(1);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(0));
                    break;
            }
        }

        public override void SwingVisual(bool parry)
        {
            Vector2 FacingVector = GetFacingVector(Human.Facing);
            var effect = new SlashEffectStraight(Human.World, () => Human.Position + new Vector2(0,2) + FacingVector * 6, Weapon.SwingSize, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
            if (parry)
                effect.Frame = effect.FrameEnd / 2;
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
        }
    }

    class ActionKnifeThrow : ActionSlash
    {
        public override bool Attacking => false;

        public ActionKnifeThrow(EnemyHuman player, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime, Weapon weapon) : base(player, slashStartTime, slashUpTime, slashDownTime, slashFinishTime, weapon)
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
            Vector2 facing = GetFacingVector(Human.Facing);
            new Knife(Human.World, Human.Position + facing * 5)
            {
                Velocity = facing * 8,
                FrameEnd = 20,
                Shooter = Human
            };
            PlaySFX(sfx_knife_throw, 1.0f, 0.4f, 0.7f);
            SlashAction = SwingAction.DownSwing;
        }
    }

    class ActionPlunge : Action
    {
        public Weapon Weapon;
        public float PlungeStartTime;
        public float PlungeFinishTime;
        public bool PlungeFinished = false;

        public override bool HasGravity => false;

        public ActionPlunge(EnemyHuman player, float plungeStartTime, float plungeFinishTime, Weapon weapon) : base(player)
        {
            PlungeStartTime = plungeStartTime;
            PlungeFinishTime = plungeFinishTime;
            Weapon = weapon;
        }

        public override void OnInput()
        {
        }

        public override void UpdateDiscrete()
        {
            if (PlungeStartTime <= 0)
                Human.Velocity.Y = 5;
            if (Human.OnGround)
            {
                Human.Velocity.Y = -4;
                Human.OnGround = false;
                Human.World.Hitstop = 4;
                PlaySFX(sfx_sword_bink, 1.0f, 0.1f, 0.4f);
                Human.CurrentAction = new ActionJump(Human, true, false);
                PlungeFinished = true;

                double damageIn = Weapon.Damage * 1.5;
                foreach (var box in Human.World.FindBoxes(Human.Box.Bounds.Offset(0, 1)))
                {
                    if(box.Data is Tile tile)
                        tile.HandleTileDamage(damageIn);
                    if (box.Data is Enemy enemy)
                        enemy.Hit(new Vector2(0, 2), 20, 50, damageIn);
                }
            }
            if (PlungeFinished && PlungeFinishTime <= 0)
                Human.CurrentAction = new ActionIdle(Human);
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
            basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(90));
        }
    }

    class ActionTwohandSlash : Action
    {
        public enum SwingAction
        {
            UpSwing,
            DownSwing,
        }

        public SwingAction SlashAction;
        public float SlashUpTime;
        public float SlashDownTime;
        public bool Parried;
        public Weapon Weapon;

        public bool IsUpSwing => SlashAction == SwingAction.UpSwing;
        public bool IsDownSwing => SlashAction == SwingAction.DownSwing;

        public override float Friction => Parried ? 1 : base.Friction;
        public override float Drag => 1 - (1 - base.Drag) * 0.1f;

        public ActionTwohandSlash(EnemyHuman human, float upTime, float downTime, Weapon weapon) : base(human)
        {
            SlashUpTime = upTime;
            SlashDownTime = downTime;
            Weapon = weapon;
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch (SlashAction)
            {
                default:
                case (SwingAction.UpSwing):
                    basePose.LeftArm = ArmState.Angular(9);
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(-90 - 45));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.LeftArm = ArmState.Angular(5);
                    basePose.RightArm = ArmState.Angular(3);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(45 + 22));
                    break;
            }
        }

        public override void UpdateDelta(float delta)
        {
            switch (SlashAction)
            {
                case (SwingAction.UpSwing):
                    SlashUpTime -= delta;
                    if (SlashUpTime < 0)
                        Swing();
                    break;
                case (SwingAction.DownSwing):
                    SlashDownTime -= delta;
                    if (SlashDownTime < 0)
                        Human.ResetState();
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }

        public virtual void Swing()
        {
            Vector2 Position = Human.Position;
            HorizontalFacing Facing = Human.Facing;
            Vector2 FacingVector = GetFacingVector(Facing);
            Vector2 PlayerWeaponOffset = Position + FacingVector * 14;
            Vector2 WeaponSize = new Vector2(14 / 2, 14 * 2);
            RectangleF weaponMask = new RectangleF(PlayerWeaponOffset - WeaponSize / 2, WeaponSize);
            if (true)
            {
                Vector2 parrySize = new Vector2(22, 22);
                bool success = Human.Parry(new RectangleF(Position + FacingVector * 8 - parrySize / 2, parrySize));
                if (success)
                    Parried = true;
            }
            if (!Parried)
                Human.SwingWeapon(weaponMask, 10);
            var effect = new SlashEffectRound(Human.World, () => Human.Position, 0.7f, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
            if (Parried)
                effect.Frame = effect.FrameEnd / 2;
            SlashAction = SwingAction.DownSwing;
        }
    }

    class ActionWandBlast : Action
    {
        public enum SwingAction
        {
            UpSwing,
            DownSwing,
        }

        public Enemy Target;
        public SwingAction SlashAction;
        public float SlashUpTime;
        public float SlashDownTime;
        public Weapon Weapon;

        public ActionWandBlast(EnemyHuman human, Enemy target, float upTime, float downTime, Weapon weapon) : base(human)
        {
            Target = target;
            SlashUpTime = upTime;
            SlashDownTime = downTime;
            Weapon = weapon;
            PlaySFX(sfx_wand_charge, 1.0f, 0.1f, 0.4f);
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch (SlashAction)
            {
                default:
                case (SwingAction.UpSwing):
                    basePose.LeftArm = ArmState.Angular(9);
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(-90 - 45));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.LeftArm = ArmState.Angular(0);
                    basePose.RightArm = ArmState.Angular(0);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(0));
                    break;
            }
        }

        public override void UpdateDelta(float delta)
        {
            switch (SlashAction)
            {
                case (SwingAction.UpSwing):
                    SlashUpTime -= delta;
                    if (SlashUpTime < 0)
                        Fire();
                    break;
                case (SwingAction.DownSwing):
                    SlashDownTime -= delta;
                    if (SlashDownTime < 0)
                        Human.ResetState();
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }

        public void Fire()
        {
            SlashAction = SwingAction.DownSwing;
            var facing = GetFacingVector(Human.Facing);
            var firePosition = Human.Position + facing * 10;
            var homing = Target.Position - firePosition;
            homing.Normalize();
            new SpellOrange(Human.World, firePosition)
            {
                Velocity = homing * 3,
                FrameEnd = 70,
                Shooter = Human
            };
            PlaySFX(sfx_wand_orange_cast, 1.0f, 0.1f, 0.3f);
        }
    }

    class ActionWandBlastUntargeted : Action //I had to do this sin to allow untargeted wand shots.
    {
        public enum SwingAction
        {
            UpSwing,
            DownSwing,
        }

        public Vector2 Direction;
        public SwingAction SlashAction;
        public float SlashUpTime;
        public float SlashDownTime;
        public Weapon Weapon;

        public ActionWandBlastUntargeted(EnemyHuman human, Vector2 direction, float upTime, float downTime, Weapon weapon) : base(human)
        {
            Direction = direction;
            SlashUpTime = upTime;
            SlashDownTime = downTime;
            Weapon = weapon;
            PlaySFX(sfx_wand_charge, 1.0f, 0.1f, 0.4f);
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch (SlashAction)
            {
                default:
                case (SwingAction.UpSwing):
                    basePose.LeftArm = ArmState.Angular(9);
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(-90 - 45));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.LeftArm = ArmState.Angular(0);
                    basePose.RightArm = ArmState.Angular(0);
                    basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(0));
                    break;
            }
        }

        public override void UpdateDelta(float delta)
        {
            switch (SlashAction)
            {
                case (SwingAction.UpSwing):
                    SlashUpTime -= delta;
                    if (SlashUpTime < 0)
                        Fire();
                    break;
                case (SwingAction.DownSwing):
                    SlashDownTime -= delta;
                    if (SlashDownTime < 0)
                        Human.ResetState();
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }

        public void Fire()
        {
            SlashAction = SwingAction.DownSwing;
            var facing = GetFacingVector(Human.Facing);
            var firePosition = Human.Position + facing * 10;
            new SpellOrange(Human.World, firePosition)
            {
                Velocity = Direction * 3,
                FrameEnd = 70,
                Shooter = Human
            };
            PlaySFX(sfx_wand_orange_cast, 1.0f, 0.1f, 0.3f);
        }
    }

}

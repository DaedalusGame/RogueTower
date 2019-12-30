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
                Human.OnGround = false;
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

        protected void HandleItemInput(Player player)
        {
            if (player.Controls.Pickup && player.NearbyItems.Any())
            {
                var nearbyItem = player.NearbyItems.First();
                player.Pickup(nearbyItem);
            }
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
            HandleItemInput(player);
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
            HandleItemInput(player);
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
            HandleItemInput(player);
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
        float Time;

        public override float Drag => 1;

        public ActionHit(EnemyHuman player, float time) : base(player)
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
            Time -= delta;
        }

        public override void UpdateDiscrete()
        {
            if (Time <= 0)
            {
                Human.ResetState();
            }
        }
    }

    class ActionEnemyDeath : Action
    {
        int Time;

        public override float Friction => 1;
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
                Cleanup();
            }
            Vector2 pos = GetRandomPosition(Human.Box.Bounds,Human.Random);
            new FireEffect(Human.World, pos, 0, 5);
        }

        protected virtual void Cleanup()
        {
            Human.Destroy();
        }
    }

    class ActionPlayerDeath : ActionEnemyDeath
    {
        public ActionPlayerDeath(EnemyHuman player, int time) : base(player, time)
        {
        }

        protected override void Cleanup()
        {
            Human.Position = new Vector2(50, Human.World.Height - 50);
            Human.Velocity = Vector2.Zero;
            Human.ResetState();
            Human.Resurrect();
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

    class ActionShockwave : ActionPlunge
    {
        public float TaggedVelocity;
        public bool ShockwaveFinished = false;
        public int ShockwaveCount;
        public ActionShockwave(EnemyHuman player, float plungeStartTime, float plungeFinishTime, Weapon weapon, int shockwaveCount = 2) : base(player, plungeStartTime, plungeFinishTime, weapon)
        {
            PlungeStartTime = plungeStartTime;
            PlungeFinishTime = plungeFinishTime;
            Weapon = weapon;
            ShockwaveCount = shockwaveCount;
        }

        public override void UpdateDiscrete()
        {
            if (PlungeStartTime <= 0)
                Human.Velocity.Y += 0.5f;
            if (Human.OnGround)
            {
                PlungeFinished = true;
                double damageIn = Math.Floor(Weapon.Damage * 1.5);
                float? floorY = null;
                foreach (var box in Human.World.FindBoxes(Human.Box.Bounds.Offset(0, 1)))
                {
                    if (box.Data is Tile tile)
                    {
                        if (!floorY.HasValue || box.Bounds.Top < floorY)
                            floorY = box.Bounds.Top;
                        tile.HandleTileDamage(damageIn);
                    }
                    if (box.Data is Enemy enemy)
                        if (enemy != Human)
                        {
                            enemy.Hit(new Vector2(0, 2), 20, 50, damageIn);
                        }
                }
                //Console.WriteLine(TaggedVelocity);
                if (!ShockwaveFinished && floorY.HasValue)
                {
                    for (int i = 0; i < ShockwaveCount; i++)
                    {
                        var speed = 3 + (i >> 1) * 1.25f;
                        var shockwave = new Shockwave(Human.World, new Vector2(Human.Position.X, floorY.Value - (int)(16 * TaggedVelocity / 5) / 2f), TaggedVelocity)
                        {
                            Velocity = new Vector2((-1 + (i & 1) * 2) * speed, 0),
                            FrameEnd = 70,
                            Shooter = Human
                        };
                    }
                    new ScreenShakeRandom(Human.World, 15, 5);
                    PlaySFX(sfx_explosion1, 1f, 0.01f, 0.2f);
                    new ParryEffect(Human.World, Human.Position, 0, 5);
                    ShockwaveFinished = true;
                }
            }
            else
            {
                if(Human.Velocity.Y > TaggedVelocity)
                    TaggedVelocity = Human.Velocity.Y;
            }
            if (PlungeFinished && PlungeFinishTime <= 0)
                Human.CurrentAction = new ActionIdle(Human);
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
            Vector2 PlayerWeaponOffset = Position + FacingVector * Weapon.WeaponSizeMult;
            Vector2 WeaponSize = Weapon.WeaponSize;
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
            var effect = new SlashEffectRound(Human.World, () => Human.Position, Weapon.SwingSize, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
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
            var homing = Target.HomingTarget - firePosition;
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

    class ActionWandSwing : Action
    {
        enum SwingAction
        {
            Start,
            Swing,
            End,
        }

        SwingAction State;
        Slider StartTime;
        Slider SwingTime;
        Slider EndTime;

        public ActionWandSwing(EnemyHuman player, float startTime, float swingTime, float endTime) : base(player)
        {
            StartTime = new Slider(startTime,startTime);
            SwingTime = new Slider(swingTime,swingTime);
            EndTime = new Slider(endTime,endTime);
        }

        public override void GetPose(PlayerState basePose)
        {
            float startAngle = -45 / 2;
            float endAngle = 180 + 45 / 2;
            switch (State)
            {
                case (SwingAction.Start):
                    basePose.LeftArm = ArmState.Angular(MathHelper.ToRadians(startAngle));
                    break;
                case (SwingAction.Swing):
                    basePose.LeftArm = ArmState.Angular(MathHelper.Lerp(MathHelper.ToRadians(startAngle), MathHelper.ToRadians(endAngle), 1-SwingTime.Slide));
                    break;
                case (SwingAction.End):
                    basePose.LeftArm = ArmState.Angular(MathHelper.ToRadians(endAngle));
                    break;
            }
            basePose.WeaponHold = WeaponHold.Left;
            basePose.Weapon.Angle = basePose.LeftArm.GetHoldAngle(ArmState.Type.Left) - MathHelper.PiOver2;
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void UpdateDelta(float delta)
        {
            switch(State)
            {
                case (SwingAction.Start):
                    if (StartTime - delta <= 0)
                        State = SwingAction.Swing;
                    break;
                case (SwingAction.Swing):
                    if (SwingTime - delta <= 0)
                        State = SwingAction.End;
                    break;
                case (SwingAction.End):
                    if (EndTime - delta <= 0)
                        Human.ResetState();
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
    }

    class ActionCharge : Action
    {
        public float ChargeTime;
        public Action ChargeAction;
        public Weapon Weapon;
        public bool SlowDown;
        public float SlowDownAmount;
        public bool CanJump = false;
        public bool CanMove = true;
        public float WalkFrame;
        public ChargeEffect chargingFX;
        public bool ChargeFinished = false;
        public ActionCharge(EnemyHuman human, float chargeTime, Action chargeAction, Weapon weapon, bool slowDown, float slowDownAmount) : base(human)
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
            if(SlowDown)
                player.Velocity.X *= SlowDownAmount;
            if (CanJump && (player.OnGround || player.ExtraJumps > 0))
                HandleJumpInput(player);
            if(CanMove)
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
            if(ChargeTime < 0 && !ChargeFinished)
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
            basePose.Weapon = Weapon.GetWeaponState(MathHelper.ToRadians(-90));

        }
    }

    class ActionPunch : Action
    {
        public enum PunchState
        {
            PunchStart,
            PunchEnd
        }

        public PunchState PunchAction;
        public float PunchStartTime;
        public float PunchFinishTime;
        public Weapon Weapon;
        public ActionPunch(EnemyHuman human, float punchStartTime, float punchFinishTime, Weapon weapon) : base(human)
        {
            PunchStartTime = punchStartTime;
            PunchFinishTime = punchFinishTime;
            Weapon = weapon;
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (PunchAction)
            {
                case (PunchState.PunchStart):
                    basePose.Head = HeadState.Down;
                    basePose.RightArm = ArmState.Angular(MathHelper.ToRadians(180));
                    basePose.Body = BodyState.Kneel;
                    break;
                case (PunchState.PunchEnd):
                    basePose.RightArm = ArmState.Forward;
                    basePose.Body = BodyState.Stand;
                    break;
            }
            
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void UpdateDelta(float delta)
        {
            switch (PunchAction)
            {
                case (PunchState.PunchStart):
                    PunchStartTime -= delta;
                    if(PunchStartTime < 0)
                    {
                        Punch();
                        PunchAction = PunchState.PunchEnd;
                    }
                    break;
                case (PunchState.PunchEnd):
                    PunchFinishTime -= delta;
                    if(PunchFinishTime < 0)
                    {
                        Human.ResetState();
                    }
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
        }

        public virtual void Punch()
        {
            Vector2 weaponSize = Weapon.WeaponSize;
            RectangleF weaponMask = new RectangleF(Human.Position
                + GetFacingVector(Human.Facing) * 8
                + GetFacingVector(Human.Facing) * (weaponSize.X / 2)
                + new Vector2(0, 1)
                - weaponSize / 2f,
                weaponSize);
            Human.SwingWeapon(weaponMask, Weapon.Damage);
            PunchVisual();
            //new RectangleDebug(Human.World, weaponMask, Color.Red, 10);
        }

        public virtual void PunchVisual()
        {
            new PunchEffectStraight(Human.World, () => Human.Position, Weapon.SwingSize, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
        }
    }

    class ActionLeftPunch : ActionPunch
    {
        public ActionLeftPunch(EnemyHuman human, float punchStartTime, float punchEndTime, Weapon weapon) : base(human, punchStartTime, punchEndTime, weapon)
        {
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (PunchAction)
            {
                case (PunchState.PunchStart):
                    basePose.Head = HeadState.Down;
                    basePose.LeftArm = ArmState.Angular(MathHelper.ToRadians(180));
                    basePose.Body = BodyState.Kneel;
                    break;
                case (PunchState.PunchEnd):
                    basePose.LeftArm = ArmState.Forward;
                    basePose.Body = BodyState.Stand;
                    break;
            }

        }

        public override void Punch()
        {
            Vector2 weaponSize = Weapon.WeaponSize;
            RectangleF weaponMask = new RectangleF(Human.Position
                + GetFacingVector(Human.Facing) * 8
                + GetFacingVector(Human.Facing) * (weaponSize.X / 2)
                + new Vector2(0, 2)
                - weaponSize / 2f,
                weaponSize);
            Human.SwingWeapon(weaponMask, Weapon.Damage);
            PunchVisual();
            //new RectangleDebug(Human.World, weaponMask, Color.Red, 10);
        }

        public override void PunchVisual()
        {
            new PunchEffectStraight(Human.World, () => Human.Position + new Vector2(0, 2), Weapon.SwingSize, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
        }
    }

    class ActionStealWeapon : Action
    {
        public enum StealState
        {
            StealStart,
            StealFinish
        }

        public StealState StealAction;
        public EnemyHuman Victim;
        public float StealStartTime;
        public float StealEndTime;
        public ActionStealWeapon(EnemyHuman thief, EnemyHuman victim, float stealStartTime, float stealEndTime) : base(thief)
        {
            Victim = victim;
            StealStartTime = stealStartTime;
            StealEndTime = stealEndTime;
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (StealAction)
            {
                case (StealState.StealStart):
                    basePose.Body = BodyState.Kneel;
                    basePose.LeftArm = ArmState.Forward;
                    basePose.Head = HeadState.Down;
                    break;
                case (StealState.StealFinish):
                    basePose.Body = BodyState.Stand;
                    basePose.LeftArm = ArmState.Up;
                    basePose.Head = HeadState.Backward;
                    break;
            }
        }

        public override void UpdateDelta(float delta)
        {
            switch (StealAction)
            {
                case (StealState.StealStart):
                    StealStartTime -= delta;
                    if (StealStartTime < 0)
                    {
                        if (!(Victim.Weapon is WeaponUnarmed) && !(Victim.Weapon is null) && Victim.Invincibility <= 0)
                        {
                            Human.Weapon = Victim.Weapon;
                            Victim.Weapon = new WeaponUnarmed(10, 14, new Vector2(14, 10));
                            Victim.Hit(GetFacingVector(Human.Facing)*0.5f, 30, 30, 0);
                            new ParryEffect(Human.World, Victim.Position, 0, 10);
                            PlaySFX(sfx_player_disappointed, 1.0f);
                        }
                        StealAction = StealState.StealFinish;
                    }
                    break;

                case (StealState.StealFinish):
                    StealEndTime -= delta;
                    if(StealEndTime < 0)
                    {
                        Human.ResetState();
                    }
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
    }

    class ActionBoomerangThrow : Action, IActionAimable
    {
        public enum BoomerangState
        {
            Prethrow,
            Throw
        }

        public void SetAngle(float angle)
        {
            Angle = angle;
        }

        public BoomerangState BoomerangAction;
        public BoomerangProjectile BoomerangProjectile;
        public float PrethrowTime;
        public float Lifetime;
        public float Angle = 0;
        public WeaponBoomerang Weapon;

        public ActionBoomerangThrow(EnemyHuman player, float prethrowTime, WeaponBoomerang weapon, float lifetime = 20) : base(player)
        {
            PrethrowTime = prethrowTime;
            Weapon = weapon;
            Lifetime = lifetime;
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (BoomerangAction)
            {
                case (BoomerangState.Prethrow):
                    basePose.RightArm = ArmState.Up;
                    break;
                case (BoomerangState.Throw):
                    basePose.RightArm = ArmState.Forward;
                    basePose.Weapon = WeaponState.None;
                    break;
            }
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void UpdateDelta(float delta)
        {
            switch (BoomerangAction)
            {
                case (BoomerangState.Prethrow):
                    PrethrowTime -= delta;
                    if(PrethrowTime < 0)
                    {
                        BoomerangProjectile = new BoomerangProjectile(Human.World, new Vector2(Human.Box.Bounds.X + (Human.Box.Bounds.Width + 8) * GetFacingVector(Human.Facing).X, Human.Box.Y + Human.Box.Height / 2), Lifetime, Weapon)
                        {
                            Shooter = Human,
                            Velocity = AngleToVector(Angle)
                        };
                        Weapon.BoomerProjectile = BoomerangProjectile;
                        BoomerangAction = BoomerangState.Throw;
                    }
                    break;
                case (BoomerangState.Throw):
                    if(BoomerangProjectile.Destroyed)
                    {
                        Human.ResetState();
                    }
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
    }

    interface IActionAimable 
    {
        void SetAngle(float angle);
    }

    class ActionAiming : Action
    {
        public float AimAngle;
        public Action PostAction;
        public ActionAiming(Player player, Action action) : base(player)
        {
            PostAction = action;
        }

        public override void OnInput()
        {
            var player = (Player)Human;
            AimAngle = player.Controls.AimAngle;
            if (player.Controls.AimFire)
            {
                if(PostAction is IActionAimable aimable)
                {
                    aimable.SetAngle(AimAngle);
                }
                player.CurrentAction = PostAction;
            }
        }

        public override void GetPose(PlayerState basePose)
        {
            var armAngle = AimAngle;
            if (Human.Facing == HorizontalFacing.Left)
                armAngle = -armAngle;
            switch (basePose.WeaponHold)
            {
                case (WeaponHold.Left):
                    basePose.LeftArm = ArmState.Angular(armAngle);
                    break;
                case (WeaponHold.Right):
                    basePose.RightArm = ArmState.Angular(armAngle);
                    break;
                case (WeaponHold.TwoHand):
                    basePose.LeftArm = ArmState.Angular(armAngle);
                    basePose.RightArm = ArmState.Angular(armAngle);
                    break;
            }
            basePose.Weapon = Human.Weapon.GetWeaponState(armAngle);
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
        public override void UpdateDelta(float delta)
        {
            //NOOP
        }
    }
}

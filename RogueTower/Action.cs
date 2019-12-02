using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using static RogueTower.Game;
using static RogueTower.Util;

namespace RogueTower
{
    abstract class Action
    {
        protected Player CurrentPlayer;
        protected Action(Player player)
        {
            CurrentPlayer = player;
        }
        abstract public void OnInput();

        abstract public void UpdateDelta(float delta);

        abstract public void UpdateDiscreet(float delta);
    }

    class ActionSlash : Action
    {
        public SwingAction SlashAction;
        public float SlashStartTime;
        public float SlashUpTime;
        public float SlashDownTime;
        public float SlashFinishTime;

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

        public override void OnInput()
        {
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
                    {
                        Swing();
                    }
                    break;
                case (SwingAction.DownSwing):
                    SlashDownTime -= delta;
                    if (SlashDownTime < 0)
                        SlashAction = SwingAction.FinishSwing;
                    break;
                case (SwingAction.FinishSwing):
                    SlashFinishTime -= delta;
                    break;
            }
        }

        public virtual void Swing()
        {
            Vector2 Position = CurrentPlayer.Position;
            HorizontalFacing Facing = CurrentPlayer.Facing;
            Vector2 FacingVector = GetFacingVector(Facing);
            Vector2 PlayerWeaponOffset = Position + FacingVector * CurrentPlayer.PlayerWeapon.WeaponSizeMult;
            Vector2 WeaponSize = CurrentPlayer.PlayerWeapon.WeaponSize;
            RectangleF weaponMask = new RectangleF(PlayerWeaponOffset - WeaponSize / 2, WeaponSize);
            if(CurrentPlayer.PlayerWeapon.CanParry == true)
            {
                Vector2 parrySize = new Vector2(22, 22);
                CurrentPlayer.Parry(new RectangleF(Position + FacingVector * 8 - parrySize / 2, parrySize));
            }
            CurrentPlayer.SwingWeapon(weaponMask, 10);
            SlashAction = SwingAction.DownSwing;
        }
        public virtual void SwingVisual()
        {
            CurrentPlayer.SlashEffect = new SlashEffect(CurrentPlayer.World, 0, false, 4);
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
        }
    }
    class ActionSlashUp : ActionSlash
    {
        public ActionSlashUp(Player player, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime) : base(player, slashStartTime, slashUpTime, slashDownTime, slashFinishTime)
        {
        }
        public override void SwingVisual()
        {
            CurrentPlayer.SlashEffect = new SlashEffect(CurrentPlayer.World, MathHelper.ToRadians(45), true, 4);
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
        }
    }

    class ActionKnifeThrow : ActionSlash
    {
        public ActionKnifeThrow(Player player, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime) : base(player, slashStartTime, slashUpTime, slashDownTime, slashFinishTime)
        {

        }

        public override void Swing()
        {
            Vector2 facing = GetFacingVector(CurrentPlayer.Facing);
            new Knife(CurrentPlayer.World, CurrentPlayer.Position + facing * 5)
            {
                Velocity = facing * 8,
                LifeTime = 20,
                Shooter = CurrentPlayer
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
        public ActionPlunge(Player player, float plungeStartTime, float plungeFinishTime) : base(player)
        {
            PlungeStartTime = plungeStartTime;
            PlungeFinishTime = plungeFinishTime;
        }
        public override void OnInput()
        {
        }
        public override void UpdateDiscreet(float delta)
        {
            
        }

        public virtual void Plunge()
        {
            if (PlungeStartTime <= 0)
                CurrentPlayer.Velocity.Y = 5;
            if (CurrentPlayer.OnGround)
            {
                CurrentPlayer.Velocity.Y = -4;
                CurrentPlayer.OnGround = false;
                CurrentPlayer.World.Hitstop = 4;
                PlaySFX(sfx_sword_bink, 1.0f, 0.1f, 0.4f);
                CurrentAction = Action.JumpUp;
                DisableJumpControl = true;
                //SlashAction = SwordAction.FinishSwing;
                foreach (var tile in CurrentPlayer.World.FindTiles(CurrentPlayer.Box.Bounds.Offset(0, 1)))
                {
                    tile.HandleTileDamage(CurrentPlayer.PlayerWeapon.Damage * 1.5);
                }
            }
            if (PlungeFinished && PlungeFinishTime <= 0)
                CurrentAction = Action.Idle;
        }
    }

    class ActionIdle : Action
    {
        public ActionIdle(Player player) : base(player)
        {

        }

        public override void OnInput()
        {

        }

        public override void UpdateDelta(float delta)
        {
            throw new NotImplementedException();
        }

        public override void UpdateDiscreet(float delta)
        {
            throw new NotImplementedException();
        }
    }

    class ActionMove : ActionIdle
    {
        public float WalkFrame;
        public ActionMove(Player player) : base(player)
        {

        }

        public override void OnInput()
        {
        }

        public override void UpdateDelta(float delta)
        {
            if (CurrentPlayer.Velocity.X > 0)
            {
                CurrentPlayer.Facing = HorizontalFacing.Right;
            }
            else if (CurrentPlayer.Velocity.X < 0)
            {
                CurrentPlayer.Facing = HorizontalFacing.Left;
            }
            WalkFrame += Math.Abs(CurrentPlayer.Velocity.X * delta * 0.125f) / (float)Math.Sqrt(CurrentPlayer.GroundFriction);
        }
    }

}

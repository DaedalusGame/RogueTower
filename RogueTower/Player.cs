using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChaiFoxes.FMODAudio;
using static RogueTower.Game;
using static RogueTower.Util;

namespace RogueTower
{
    enum HorizontalFacing
    {
        Left,
        Right,
    }

    enum CollisionType
    {
        Air,
        Floor,
        Wall,
        Ceiling,
    }

    class InputQueue
    {
        Player Player;

        public bool MoveLeft;
        public bool MoveRight;
        public bool Jump;
        public bool JumpHeld;

        public bool Attack;
        public bool ForwardAttack;
        public bool BackAttack;
        public bool UpAttack;
        public bool DownAttack;
        public bool AltAttack;

        public bool ClimbUp;
        public bool ClimbDown;

        KeyboardState LastState;
        GamePadState LastGPState;

        public InputQueue(Player player)
        {
            Player = player;
        }

        public void Update(SceneGame game)
        {
            bool left = game.KeyState.IsKeyDown(Keys.A) || (game.PadState.IsButtonDown(Buttons.LeftThumbstickLeft) || game.PadState.IsButtonDown(Buttons.DPadLeft));
            bool right = game.KeyState.IsKeyDown(Keys.D) || (game.PadState.IsButtonDown(Buttons.LeftThumbstickRight) || game.PadState.IsButtonDown(Buttons.DPadRight));
            bool up = game.KeyState.IsKeyDown(Keys.W) || (game.PadState.IsButtonDown(Buttons.LeftThumbstickUp) || game.PadState.IsButtonDown(Buttons.DPadUp));
            bool down = game.KeyState.IsKeyDown(Keys.S) || (game.PadState.IsButtonDown(Buttons.LeftThumbstickDown) || game.PadState.IsButtonDown(Buttons.DPadDown));
            bool attack = game.KeyState.IsKeyDown(Keys.Space) && LastState.IsKeyUp(Keys.Space) || (game.PadState.IsButtonDown(Buttons.X) && LastGPState.IsButtonUp(Buttons.X));
            bool altattack = game.KeyState.IsKeyDown(Keys.LeftAlt) && LastState.IsKeyUp(Keys.LeftAlt) || (game.PadState.IsButtonDown(Buttons.B) && LastGPState.IsButtonUp(Buttons.B));
            bool forward = (Player.Facing == HorizontalFacing.Left && left) || (Player.Facing == HorizontalFacing.Right && right);
            bool back = (Player.Facing == HorizontalFacing.Left && right) || (Player.Facing == HorizontalFacing.Right && left);

            MoveLeft = left;
            MoveRight = right;

            if((game.KeyState.IsKeyDown(Keys.LeftShift) && LastState.IsKeyUp(Keys.LeftShift)) || (game.PadState.IsButtonDown(Buttons.A) && LastGPState.IsButtonUp(Buttons.A)))
                Jump = true;
            JumpHeld = game.KeyState.IsKeyDown(Keys.LeftShift) || game.PadState.IsButtonDown(Buttons.A);
            
            ClimbUp = up;
            ClimbDown = down;

            if (attack)
                Attack = true;
            if (attack && up)
                UpAttack = true;
            if (attack && down)
                DownAttack = true;
            if (forward && attack)
                ForwardAttack = true;
            if (back && attack)
                BackAttack = true;

            if(altattack)
                AltAttack = true;
            LastState = game.KeyState;
            LastGPState = game.PadState;
        }

        public void Reset()
        {
            MoveLeft = false;
            MoveRight = false;
            Jump = false;
            JumpHeld = false;

            Attack = false;
            ForwardAttack = false;
            BackAttack = false;
            DownAttack = false;
            UpAttack = false;
            AltAttack = false;

            ClimbUp = false;
            ClimbDown = false;
        }
    }

    class Player : EnemyHuman
    {
        public InputQueue Controls;

        public override RectangleF ActivityZone => World.Bounds;

        //public Weapon Weapon = new WeaponKnife(15, 14, new Vector2(14 / 2, 14 * 2));
        public Weapon Weapon = new WeaponKatana(15, 20, new Vector2(10, 40));
        //public Weapon Weapon = new WeaponRapier(15, 20, new Vector2(10, 40));
        //public Weapon Weapon = new WeaponWandOrange(10, 16, new Vector2(8, 32));

        public double SwordSwingDamage = 15.0;
        public double SwordSwingDownDamage = 20.0;

        SceneGame SceneGame;

        public Player(GameWorld world, Vector2 position) : base(world, position)
        {
            Controls = new InputQueue(this);
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x, y, 12, 14);
            Box.AddTags(CollisionTag.Character);
            Box.Data = this;
        }

        public void SetControl(SceneGame game)
        {
            SceneGame = game;
        }

        protected override void UpdateDelta(float delta)
        {
            Controls.Update(SceneGame);

            base.UpdateDelta(delta);
        }

        protected override void UpdateDiscrete()
        {
            base.UpdateDiscrete();
        }

        protected override void HandleInput()
        {
            CurrentAction.OnInput();
            Controls.Reset();
        }

        public override PlayerState GetBasePose()
        {
            return new PlayerState(
                HeadState.Forward,
                BodyState.Stand,
                ArmState.Shield,
                ArmState.Neutral,
                Weapon.GetWeaponState(MathHelper.ToRadians(0))
            );
        }

        public override void SetPhenoType(PlayerState pose)
        {
            //NOOP
        }
    }
}

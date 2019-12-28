using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RogueTower.Util;

namespace RogueTower
{
    enum InputResult
    {
        NoResult,
        ActionTaken,
        Cancel,
    }

    abstract class InputAction
    {
        public List<InputAction> SubActions = new List<InputAction>();
        public InputResult Result;
        public int Ticks;

        public abstract bool Done
        {
            get;
        }
        public virtual float GameSpeed => 1.0f;

        public virtual void HandleInput(SceneGame scene)
        {
            Ticks++;
        }

        protected void HandleSubActions(SceneGame scene)
        {
            var first = SubActions.First();
            first.HandleInput(scene);
            if (first.Result == InputResult.ActionTaken)
                Result = first.Result;
            if (first.Done)
                SubActions.RemoveAt(0);
        }

        public abstract void Draw(SceneGame scene);
    }

    class PlayerInput : InputAction
    {
        Player Player;

        public override bool Done => false;
        public override float GameSpeed => SubActions.Count > 0 ? SubActions.First().GameSpeed : (GameSpeedToggle ? 0.1f : base.GameSpeed);
        bool GameSpeedToggle = false;
        int CurrentWeaponIndex = 0;

        public PlayerInput(Player player)
        {
            Player = player;
        }

        public override void HandleInput(SceneGame scene)
        {
            base.HandleInput(scene);

            if (SubActions.Count > 0)
            {
                HandleSubActions(scene);
                return;
            }

            if ((scene.KeyState.IsKeyDown(Keys.Tab) && scene.LastKeyState.IsKeyUp(Keys.Tab)) || (scene.PadState.IsButtonDown(Buttons.RightStick) && scene.LastPadState.IsButtonUp(Buttons.RightStick)))
                GameSpeedToggle = !GameSpeedToggle;

            if (scene.PadState.IsButtonDown(Buttons.RightTrigger) && scene.LastPadState.IsButtonUp(Buttons.RightTrigger))
            {
                CurrentWeaponIndex = PositiveMod(CurrentWeaponIndex + 1, Weapon.PresetWeaponList.Length);
                Player.Weapon = Weapon.PresetWeaponList[CurrentWeaponIndex];
            }
            else if (scene.PadState.IsButtonDown(Buttons.RightShoulder) && scene.LastPadState.IsButtonUp(Buttons.RightShoulder))
            {
                CurrentWeaponIndex = PositiveMod(CurrentWeaponIndex - 1, Weapon.PresetWeaponList.Length);
                Player.Weapon = Weapon.PresetWeaponList[CurrentWeaponIndex];
            }

            if ((scene.KeyState.IsKeyDown(Keys.Enter) && scene.LastKeyState.IsKeyUp(Keys.Enter)) || (scene.PadState.IsButtonDown(Buttons.Start) && scene.LastPadState.IsButtonUp(Buttons.Start)))
            {
                SubActions.Add(new Pause());
                return;
            }

            Player.Controls.Update(scene);
        }

        public override void Draw(SceneGame scene)
        {
            //NOOP
        }
    }

    class Pause : InputAction
    {
        public override bool Done => Result != InputResult.NoResult;
        public override float GameSpeed => 0.0f;

        public override void HandleInput(SceneGame scene)
        {
            base.HandleInput(scene);

            if ((scene.KeyState.IsKeyDown(Keys.Enter) && scene.LastKeyState.IsKeyUp(Keys.Enter)) || (scene.PadState.IsButtonDown(Buttons.Start) && scene.LastPadState.IsButtonUp(Buttons.Start)))
            {
                Result = InputResult.ActionTaken;
            }
        }

        public override void Draw(SceneGame scene)
        {
            scene.SpriteBatch.Draw(scene.Pixel, new Rectangle(scene.Viewport.X, scene.Viewport.Y, scene.Viewport.Width, scene.Viewport.Height), new Color(0, 0, 0, 128));
            scene.DrawText(Game.ConvertToPixelText("PAUSED"), new Vector2(scene.Viewport.Width / 2, scene.Viewport.Height / 2), Alignment.Center, new TextParameters().SetColor(Color.White, Color.Black));
        }
    }

    class MessageBox : InputAction
    {
        public string Text;
        public InputResult ConfirmResult;
        public InputResult CancelResult;

        public override bool Done => Result != InputResult.NoResult;
        public override float GameSpeed => 0.1f;

        public MessageBox(string text, InputResult confirmResult, InputResult cancelResult)
        {
            Text = text;
            ConfirmResult = confirmResult;
            CancelResult = cancelResult;
        }

        public MessageBox(string text, InputResult result) : this(text, result, result)
        {

        }

        public override void HandleInput(SceneGame scene)
        {
            base.HandleInput(scene);

            if (scene.KeyState.IsKeyDown(Keys.Enter) && scene.LastKeyState.IsKeyUp(Keys.Enter))
                Result = ConfirmResult;
        }

        public override void Draw(SceneGame scene)
        {
            SpriteReference textbox = SpriteLoader.Instance.AddSprite("content/ui_box");
            int edgeDistance = 75;
            int width = scene.Viewport.Width - edgeDistance * 2;
            int height = 16 * 4;
            int x = edgeDistance;
            int y = (scene.Viewport.Height - height) / 2;
            float openCoeff = Math.Min(Ticks / 7f, 1f);
            float openResize = MathHelper.Lerp(-0.5f, 0.0f, openCoeff);
            RectangleF rect = new RectangleF(x, y, width, height);
            rect.Inflate(rect.Width * openResize, rect.Height * openResize);
            if (openCoeff > 0)
                scene.DrawUI(textbox, rect.ToRectangle(), Color.White);
            if (openCoeff >= 1)
                scene.DrawText(Text, new Vector2(x, y), Alignment.Left, new TextParameters().SetConstraints(width, height).SetBold(true).SetColor(Color.White, Color.Black));
        }
    }
}

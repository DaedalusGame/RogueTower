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
                //SubActions.Add(new Pause());
                SubActions.Add(new Menu(Player));
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

            bool confirm = (scene.KeyState.IsKeyDown(Keys.Enter) && scene.LastKeyState.IsKeyUp(Keys.Enter)) || (scene.PadState.IsButtonDown(Buttons.A) && scene.LastPadState.IsButtonUp(Buttons.A));
            bool cancel = (scene.KeyState.IsKeyDown(Keys.Escape) && scene.LastKeyState.IsKeyUp(Keys.Escape)) || (scene.PadState.IsButtonDown(Buttons.B) && scene.LastPadState.IsButtonUp(Buttons.B));

            if (confirm)
                Result = ConfirmResult;
            if (cancel)
                Result = CancelResult;
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

    class ActAction
    {
        public string Name;
        public Func<InputResult> Action;

        public ActAction(string name, Func<InputResult> action)
        {
            Name = name;
            Action = action;
        }
    }

    class Act : InputAction
    {
        public List<ActAction> Actions = new List<ActAction>();
        public int Selection;

        public override bool Done
        {
            get
            {
                return Result != InputResult.NoResult;
            }
        }

        public ActAction SelectedAction
        {
            get
            {
                return Actions[PositiveMod(Selection, Actions.Count)];
            }
        }

        public void AddAction(ActAction action)
        {
            Actions.Add(action);
        }

        public override void HandleInput(SceneGame scene)
        {
            base.HandleInput(scene);

            if (SubActions.Count > 0)
            {
                HandleSubActions(scene);
                return;
            }

            bool up = (scene.KeyState.IsKeyDown(Keys.W) && scene.LastKeyState.IsKeyUp(Keys.W)) || (scene.PadState.IsButtonDown(Buttons.LeftThumbstickUp) && scene.LastPadState.IsButtonUp(Buttons.LeftThumbstickUp)) || (scene.PadState.IsButtonDown(Buttons.DPadUp) && scene.LastPadState.IsButtonUp(Buttons.DPadUp));
            bool down = (scene.KeyState.IsKeyDown(Keys.S) && scene.LastKeyState.IsKeyUp(Keys.S)) || (scene.PadState.IsButtonDown(Buttons.LeftThumbstickDown) && scene.LastPadState.IsButtonUp(Buttons.LeftThumbstickDown)) || (scene.PadState.IsButtonDown(Buttons.DPadDown) && scene.LastPadState.IsButtonUp(Buttons.DPadDown));
            bool confirm = (scene.KeyState.IsKeyDown(Keys.Enter) && scene.LastKeyState.IsKeyUp(Keys.Enter)) || (scene.PadState.IsButtonDown(Buttons.A) && scene.LastPadState.IsButtonUp(Buttons.A));
            bool cancel = (scene.KeyState.IsKeyDown(Keys.Escape) && scene.LastKeyState.IsKeyUp(Keys.Escape)) || (scene.PadState.IsButtonDown(Buttons.B) && scene.LastPadState.IsButtonUp(Buttons.B));

            if (up) //TODO: key repeat
            {
                Selection--;
            }
            else if (down) //TODO: key repeat
            {
                Selection++;
            }
            else if (confirm)
            {
                Result = SelectedAction.Action();
            }
            else if (cancel)
            {
                Result = InputResult.Cancel;
            }
        }

        public override void Draw(SceneGame scene)
        {
            SpriteReference cursor = SpriteLoader.Instance.AddSprite("content/cursor");
            SpriteReference textbox = SpriteLoader.Instance.AddSprite("content/ui_box");
            int edgeDistance = 50;
            int width = scene.Viewport.Width - edgeDistance * 2;
            int height = 16 * Actions.Count;
            int x = edgeDistance;
            int y = (scene.Viewport.Height - height) / 2;
            float openCoeff = Math.Min(Ticks / 7f, 1f);
            float openResize = MathHelper.Lerp(-0.5f, 0.0f, openCoeff);
            RectangleF rect = new RectangleF(x, y, width, height);
            rect.Inflate(rect.Width * openResize, rect.Height * openResize);
            if (openCoeff > 0)
                scene.DrawUI(textbox, rect.ToRectangle(), Color.White);
            int i = 0;
            if (openCoeff >= 1)
                foreach (var menupoint in Actions)
                {
                    if (menupoint == SelectedAction)
                        scene.SpriteBatch.Draw(cursor.Texture, new Vector2(x + 0, y + i * 16), cursor.GetFrameRect(0), Color.White);
                    scene.DrawText(menupoint.Name, new Vector2(x + 32, y + i * 16), Alignment.Left, new TextParameters().SetConstraints(width - 32, 16).SetBold(true).SetColor(Color.White, Color.Black));
                    i++;
                }
        }
    }

    class Menu : Act
    {
        public Player Player;

        public override float GameSpeed => 0.1f;

        public Menu(Player player)
        {
            Player = player;
            AddAction(new ActAction("Items", () => { SubActions.Add(new MessageBox("Not implemented.",InputResult.Cancel)); return InputResult.NoResult; }));
            AddAction(new ActAction("Stats", () => { SubActions.Add(new MessageBox("Not implemented.", InputResult.Cancel)); return InputResult.NoResult; }));
            AddAction(new ActAction("Fusion", () => { SubActions.Add(new MessageBox("Not implemented.", InputResult.Cancel)); return InputResult.NoResult; }));
        }
    }
}

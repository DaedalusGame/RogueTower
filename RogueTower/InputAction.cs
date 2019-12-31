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

        DroppedItem NearestItem = null;
        float NearestItemTicks = 0;

        public PlayerInput(Player player)
        {
            Player = player;
        }

        public override void HandleInput(SceneGame scene)
        {
            base.HandleInput(scene);

            NearestItemTicks++;

            if (SubActions.Count > 0)
            {
                HandleSubActions(scene);
                return;
            }

            if (scene.InputState.IsKeyPressed(Keys.Tab) || scene.InputState.IsButtonPressed(Buttons.RightStick))
                GameSpeedToggle = !GameSpeedToggle;

            if (scene.InputState.IsButtonPressed(Buttons.RightTrigger))
            {
                CurrentWeaponIndex = PositiveMod(CurrentWeaponIndex + 1, Weapon.PresetWeaponList.Length);
                Player.Weapon = Weapon.PresetWeaponList[CurrentWeaponIndex];
            }
            else if (scene.InputState.IsButtonPressed(Buttons.RightShoulder))
            {
                CurrentWeaponIndex = PositiveMod(CurrentWeaponIndex - 1, Weapon.PresetWeaponList.Length);
                Player.Weapon = Weapon.PresetWeaponList[CurrentWeaponIndex];
            }

            if ((scene.InputState.IsKeyPressed(Keys.Enter)) || (scene.InputState.IsButtonPressed(Buttons.Start)))
            {
                //SubActions.Add(new Pause());
                SubActions.Add(new Menu(Player));
                return;
            }

            Player.Controls.Update(scene);

            DroppedItem nearest = null;
            if (Player.NearbyItems.Any())
            {
                nearest = Player.NearbyItems.First();
            }

            if (nearest != NearestItem)
            {
                NearestItem = nearest;
                NearestItemTicks = 0;
            }
        }

        public override void Draw(SceneGame scene)
        {
            SpriteReference textbox = SpriteLoader.Instance.AddSprite("content/ui_box");

            if (NearestItem != null)
            {
                int width = 256;
                int height = 16;
                int x = (scene.Viewport.Width - width) / 2;
                int y = scene.Viewport.Height - height - 4;
                float openCoeff = Math.Min(NearestItemTicks / 7f, 1f);
                float openResize = MathHelper.Lerp(-0.5f, 0.0f, openCoeff);
                RectangleF rect = new RectangleF(x, y, width, height);
                rect.Inflate(rect.Width * openResize * 0.5f, rect.Height * openResize);
                if (openCoeff > 0)
                    scene.DrawUI(textbox, rect.ToRectangle(), Color.White);
                if (openCoeff >= 1)
                    scene.DrawText($"Pick up {NearestItem.Item.Name}", new Vector2(x, y), Alignment.Center, new TextParameters().SetConstraints(width, height).SetBold(true).SetColor(Color.White, Color.Black));
            }
        }
    }

    class Pause : InputAction
    {
        public override bool Done => Result != InputResult.NoResult;
        public override float GameSpeed => 0.0f;

        public override void HandleInput(SceneGame scene)
        {
            base.HandleInput(scene);

            if ((scene.InputState.IsKeyPressed(Keys.Enter)) || (scene.InputState.IsButtonPressed(Buttons.Start)))
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
        public override float GameSpeed => 0.0f;

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

            bool confirm = (scene.InputState.IsKeyPressed(Keys.Enter)) || (scene.InputState.IsButtonPressed(Buttons.A));
            bool cancel = (scene.InputState.IsKeyPressed(Keys.Escape)) || (scene.InputState.IsButtonPressed(Buttons.B));

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
                return Result != InputResult.NoResult && !SubActions.Any();
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

            bool up = (scene.InputState.IsKeyPressed(Keys.W, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.LeftThumbstickUp, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.DPadUp, 20, 5));
            bool down = (scene.InputState.IsKeyPressed(Keys.S, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.LeftThumbstickDown, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.DPadDown, 20, 5));
            bool confirm = (scene.InputState.IsKeyPressed(Keys.Enter)) || (scene.InputState.IsButtonPressed(Buttons.A));
            bool cancel = (scene.InputState.IsKeyPressed(Keys.Escape)) || (scene.InputState.IsButtonPressed(Buttons.B));

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
            AddAction(new ActAction("Items", () => { SubActions.Add(new ItemMenu(Player)); return InputResult.NoResult; }));
            AddAction(new ActAction("Stats", () => { SubActions.Add(new MessageBox("Not implemented.", InputResult.Cancel)); return InputResult.NoResult; }));
            AddAction(new ActAction("Fusion", () => { SubActions.Add(new MessageBox("Not implemented.", InputResult.Cancel)); return InputResult.NoResult; }));
        }
    }

    class ItemMenu : InputAction
    {
        public class Stack
        {
            public List<Item> Items;

            public Item this[int index]
            {
                get
                {
                    return Items[index];
                }
            }

            public Item Representative
            {
                get
                {
                    return Items.First();
                }
            }

            public int Count
            {
                get
                {
                    return Items.Count;
                }
            }

            public Stack(IEnumerable<Item> items)
            {
                Items = items.ToList();
            }
        }

        public Player Player;
        public int Selection;
        public int SubSelection;
        public List<Stack> Items;

        public override bool Done => Result != InputResult.NoResult && !SubActions.Any();
        public Stack SelectedStack => Items[Util.PositiveMod(Selection, Items.Count)];
        public Item SelectedItem => SelectedStack[Util.PositiveMod(SubSelection, SelectedStack.Count)];

        public virtual bool IsBlacklisted(Item item)
        {
            return false;
        }

        public ItemMenu(Player player)
        {
            Player = player;
            Populate();
        }

        public void Refresh()
        {
            if (Items.Count == 0)
                Populate();
            else
            {
                Item currentSelection = SelectedItem;
                Populate();
                int index = GetIndex(currentSelection);
                if (index >= 0)
                    Selection = index;
            }
        }

        private void Populate()
        {
            Items = Player.Inventory.GroupBy(x => x, Item.Stacker).Select(x => x.Where(item => !IsBlacklisted(item))).Where(x => x.Any()).Select(x => new Stack(x)).ToList();
        }

        public int GetIndex(Item item)
        {
            return Items.FindIndex(x => x.Items.Contains(item));
        }

        public override void HandleInput(SceneGame scene)
        {
            base.HandleInput(scene);

            if (SubActions.Count > 0)
            {
                HandleSubActions(scene);
                if (SubActions.Count == 0)
                    Refresh();
                return;
            }

            bool up = (scene.InputState.IsKeyPressed(Keys.W, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.LeftThumbstickUp, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.DPadUp, 20, 5));
            bool down = (scene.InputState.IsKeyPressed(Keys.S, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.LeftThumbstickDown, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.DPadDown, 20, 5));
            bool left = (scene.InputState.IsKeyPressed(Keys.A, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.LeftThumbstickLeft, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.DPadLeft, 20, 5));
            bool right = (scene.InputState.IsKeyPressed(Keys.D, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.LeftThumbstickRight, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.DPadRight, 20, 5));
            bool confirm = (scene.InputState.IsKeyPressed(Keys.Enter)) || (scene.InputState.IsButtonPressed(Buttons.A));
            bool cancel = (scene.InputState.IsKeyPressed(Keys.Escape)) || (scene.InputState.IsButtonPressed(Buttons.B));


            if (up)
            {
                Selection--;
            }
            else if (down)
            {
                Selection++;
            }
            else if (left)
            {
                SubSelection--;
            }
            else if (right)
            {
                SubSelection++;
            }
            else if (Items.Count > 0 && confirm)
            {
                var actionMenu = new Act();

                actionMenu.AddAction(new ActAction($"Examine {SelectedItem.Name}", () =>
                {
                    StringBuilder description = new StringBuilder(SelectedItem.Description);
                    actionMenu.SubActions.Add(new MessageBox(description.ToString(), InputResult.NoResult));
                    return InputResult.Cancel;
                }));
                if(SelectedItem is IEdible edible && edible.CanEat(Player))
                {
                    actionMenu.AddAction(new ActAction($"Eat {SelectedItem.Name}", () =>
                    {
                        edible.EatEffect(Player);
                        return InputResult.Cancel;
                    }));
                }
                actionMenu.AddAction(new ActAction($"Dispose {SelectedItem.Name}", () =>
                {
                    Player.Inventory.Remove(SelectedItem);
                    actionMenu.SubActions.Add(new MessageBox($"Threw {SelectedItem.Name} away.", InputResult.NoResult));
                    return InputResult.Cancel;
                }));

                SubActions.Add(actionMenu);
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

            int edgeDistance = 75;
            int width = scene.Viewport.Width - edgeDistance * 2;
            int height = 16 * Math.Max(Items.Count, 1);
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
            {
                if (Items.Count <= 0)
                    scene.DrawText("You haven't anything.", new Vector2(x, y), Alignment.Left, new TextParameters().SetConstraints(width - 32, 16).SetBold(true).SetColor(Color.LightGray, Color.Black));
                foreach (var menupoint in Items)
                {
                    var item = menupoint.Representative;
                    if (menupoint == SelectedStack)
                    {
                        scene.SpriteBatch.Draw(cursor.Texture, new Vector2(x + 0, y + i * 16), cursor.GetFrameRect(0), Color.White);
                        item = SelectedItem;
                    }

                    item.DrawIcon(scene, new Vector2(x + 16 + 8, y + i * 16 + 8));
                    scene.DrawText($"{item.Name} x{menupoint.Count}", new Vector2(x + 32, y + i * 16), Alignment.Left, new TextParameters().SetConstraints(width - 32, 16).SetBold(true).SetColor(Color.White, Color.Black));
                    i++;
                }
            }
        }
    }
}

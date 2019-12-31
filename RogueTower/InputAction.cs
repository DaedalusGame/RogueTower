﻿using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        None,
        Close,
        CloseAll,
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
            if (first.Result == InputResult.CloseAll)
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
                    scene.DrawText($"Pick up {NearestItem.Item.FakeName}", new Vector2(x, y), Alignment.Center, new TextParameters().SetConstraints(width, height).SetBold(true).SetColor(Color.White, Color.Black));
            }
        }
    }

    class Pause : InputAction
    {
        public override bool Done => Result != InputResult.None;
        public override float GameSpeed => 0.0f;

        public override void HandleInput(SceneGame scene)
        {
            base.HandleInput(scene);

            if ((scene.InputState.IsKeyPressed(Keys.Enter)) || (scene.InputState.IsButtonPressed(Buttons.Start)))
            {
                Result = InputResult.Close;
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

        public override bool Done => Result != InputResult.None;
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
                return Result != InputResult.None && !SubActions.Any();
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

            if (up)
            {
                Selection--;
            }
            else if (down)
            {
                Selection++;
            }
            else if (confirm)
            {
                Result = SelectedAction.Action();
            }
            else if (cancel)
            {
                Result = InputResult.Close;
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
            AddAction(new ActAction("Items", () => { SubActions.Add(new ItemMenu(Player)); return InputResult.None; }));
            AddAction(new ActAction("Stats", () => { SubActions.Add(new MessageBox("Not implemented.", InputResult.Close)); return InputResult.None; }));
            AddAction(new ActAction("Fusion", () => { SubActions.Add(new MessageBox("Not implemented.", InputResult.Close)); return InputResult.None; }));
        }
    }

    class ActItem : Act
    {
        public Player Player;
        public ItemSelection Selection;

        public Item Item => Selection.Item;
        public ItemMenu.Stack Stack => Selection.Stack;

        public ActItem(Player player, ItemSelection selection)
        {
            Player = player;
            Selection = selection;
            Populate();
        }

        private void Populate()
        {
            AddAction(new ActAction($"Examine {Item.FakeName}", () =>
            {
                StringBuilder description = new StringBuilder(Item.FakeDescription);
                SubActions.Add(new MessageBox(description.ToString(), InputResult.Close));
                return InputResult.None;
            }));
            if (Item is IEdible edible && edible.CanEat(Player))
            {
                AddAction(new ActAction($"Eat {Item.FakeName}", () =>
                {
                    edible.EatEffect(Player);
                    return Item.Destroyed ? InputResult.Close : InputResult.None;
                }));
            }
            if (Item is Weapon weapon)
            {
                AddAction(new ActAction($"Equip {Item.FakeName}", () =>
                {
                    Player.Weapon = weapon;
                    return InputResult.Close;
                }));
            }
            if (Item is Potion potion)
            {
                AddAction(new ActAction($"Quaff {Item.FakeName}", () =>
                {
                    potion.DrinkEffect(Player);
                    return Item.Destroyed ? InputResult.Close : InputResult.None;
                }));
            }
            AddAction(new ActAction($"Dispose {Item.FakeName}", () =>
            {
                Item.Destroy();
                SubActions.Add(new MessageBox($"Threw {Item.FakeName} away.", InputResult.Close));
                return Item.Destroyed ? InputResult.Close : InputResult.None;
            }));
        }

        public override void HandleInput(SceneGame scene)
        {
            if (Item.Destroyed)
                Result = InputResult.Close;
            base.HandleInput(scene);
        }
    }

    class ActCombine : Act
    {
        public Player Player;
        public List<ItemSelection> Selections;

        public IEnumerable<Item> Items => Selections.Select(x => x.Item);
        public IEnumerable<ItemMenu.Stack> Stacks => Selections.Select(x => x.Stack);

        public ActCombine(Player player, IEnumerable<ItemSelection> selections)
        {
            Player = player;
            Selections = selections.Distinct().ToList();
            Populate();
        }

        private void Populate()
        {
            string combineString;
            if (Selections.Count <= 3)
                combineString = EnglishJoin(", ", " and ", Selections.Select(x => x.Item.FakeName));
            else
                combineString = $"{Selections.Count} Items";
            if (Items.All(x => x is Potion)) //All items are potions -> mix
            {
                AddAction(new ActAction($"Mix {combineString}", () =>
                {
                    return InputResult.None;
                }));
            }
            else if (Items.Any(x => x is Potion) && Selections.Count == 2) //2 items, of which one is a potion -> dip
            {
                var potion = Items.First(x => x is Potion);
                var nonPotion = Items.First(x => !(x is Potion));
                AddAction(new ActAction($"Dip {nonPotion.FakeName} into {potion.FakeName}", () =>
                {
                    return InputResult.None;
                }));
            }
            else
            {
                AddAction(new ActAction($"Combine {combineString}", () =>
                {
                    return InputResult.None;
                }));
            }
            if (Selections.Count == 2)
            {
                AddAction(new ActAction($"Swap {combineString}", () =>
                {
                    return InputResult.None;
                }));
            }
            AddAction(new ActAction($"Dispose {combineString}", () =>
            {
                foreach(var item in Items)
                {
                    item.Destroy();
                }
                SubActions.Add(new MessageBox($"Threw {combineString} away.", InputResult.Close));
                return Items.Any(x => x.Destroyed) ? InputResult.Close : InputResult.None;
            }));
        }

        public override void HandleInput(SceneGame scene)
        {
            if (Items.Any(x => x.Destroyed))
                Result = InputResult.Close;
            base.HandleInput(scene);
        }
    }

    struct ItemSelection
    {
        ItemMenu Menu;
        public int Index;
        public int SubIndex;

        public ItemMenu.Stack Stack => Menu.Items[PositiveMod(Index, Menu.Items.Count)];
        public Item Item => Stack[PositiveMod(SubIndex, Stack.Count)];

        public ItemSelection(ItemMenu menu, int selection, int subSelection)
        {
            Menu = menu;
            Index = selection;
            SubIndex = subSelection;
        }

        public static bool operator ==(ItemSelection a, ItemSelection b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ItemSelection a, ItemSelection b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (obj is ItemSelection other)
                return PositiveMod(Index,Menu.Items.Count) == PositiveMod(other.Index, other.Menu.Items.Count) && PositiveMod(SubIndex,Stack.Count) == PositiveMod(other.SubIndex,other.Stack.Count);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return PositiveMod(Index, Menu.Items.Count).GetHashCode() ^ PositiveMod(SubIndex, Stack.Count).GetHashCode() * 13;
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
        public ItemSelection Selection;
        public List<ItemSelection> CombineSelections = new List<ItemSelection>();
        public List<Stack> Items;

        public override bool Done => Result != InputResult.None && !SubActions.Any();
        //public Stack SelectedStack => Items[PositiveMod(Selection, Items.Count)];
        //public Item SelectedItem => SelectedStack[PositiveMod(SubSelection, SelectedStack.Count)];

        public virtual bool IsBlacklisted(Item item)
        {
            if (item is WeaponUnarmed)
                return true;
            return false;
        }

        public ItemMenu(Player player)
        {
            Player = player;
            Populate();
            Selection = new ItemSelection(this, 0, 0);
        }

        public void Refresh()
        {
            if (Items.Count == 0)
                Populate();
            else
            {
                Item selectionItem = Selection.Item;
                List<Item> combineItems = CombineSelections.Select(x => x.Item).ToList();
                Populate();
                var newSelection = GetIndex(selectionItem);
                if (newSelection.HasValue)
                    Selection = newSelection.Value;
                CombineSelections.Clear();
                CombineSelections.AddRange(combineItems.Select(x => GetIndex(x)).Where(x => x.HasValue).Select(x => x.Value));
            }
        }

        private void Populate()
        {
            Items = Player.Inventory.GroupBy(x => x, Item.Stacker).Select(x => x.Where(item => !IsBlacklisted(item))).Where(x => x.Any()).Select(x => new Stack(x)).ToList();
        }

        public ItemSelection? GetIndex(Item item)
        {
            var stackIndex = Items.FindIndex(x => x.Items.Contains(item));
            if (stackIndex < 0)
                return null;
            var itemIndex = Items[stackIndex].Items.FindIndex(x => x == item);
            return new ItemSelection(this,stackIndex,itemIndex);
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

            if (Player.InventoryChanged)
            {
                Refresh();
            }

            bool up = (scene.InputState.IsKeyPressed(Keys.W, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.LeftThumbstickUp, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.DPadUp, 20, 5));
            bool down = (scene.InputState.IsKeyPressed(Keys.S, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.LeftThumbstickDown, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.DPadDown, 20, 5));
            bool left = (scene.InputState.IsKeyPressed(Keys.A, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.LeftThumbstickLeft, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.DPadLeft, 20, 5));
            bool right = (scene.InputState.IsKeyPressed(Keys.D, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.LeftThumbstickRight, 20, 5)) || (scene.InputState.IsButtonPressed(Buttons.DPadRight, 20, 5));
            bool confirm = (scene.InputState.IsKeyPressed(Keys.Enter)) || (scene.InputState.IsButtonPressed(Buttons.A));
            bool combine = (scene.InputState.IsKeyPressed(Keys.LeftShift)) || (scene.InputState.IsButtonPressed(Buttons.Y));
            bool cancel = (scene.InputState.IsKeyPressed(Keys.Escape)) || (scene.InputState.IsButtonPressed(Buttons.B));

            if (up)
            {
                Selection.Index--;
            }
            else if (down)
            {
                Selection.Index++;
            }
            else if (left)
            {
                Selection.SubIndex--;
            }
            else if (right)
            {
                Selection.SubIndex++;
            }
            else if (Items.Count > 0 && confirm)
            {
                var combinedSelections = CombineSelections.Concat(new[] { Selection }).Distinct();
                if (combinedSelections.Count() > 1)
                    SubActions.Add(new ActCombine(Player, combinedSelections));
                else
                    SubActions.Add(new ActItem(Player,Selection));
            }
            else if (Items.Count > 0 && combine)
            {
                if (!CombineSelections.Contains(Selection))
                    CombineSelections.Add(Selection);
            }
            else if (cancel)
            {
                if (CombineSelections.Any())
                    CombineSelections.RemoveAt(CombineSelections.Count-1);
                else
                    Result = InputResult.Close;
            }
        }

        public override void Draw(SceneGame scene)
        {
            SpriteReference cursor = SpriteLoader.Instance.AddSprite("content/cursor");
            SpriteReference textbox = SpriteLoader.Instance.AddSprite("content/ui_box");
            SpriteReference flagEquipped = SpriteLoader.Instance.AddSprite("content/flag_equip");

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
                    
                    if (CombineSelections.Any(selection => selection.Stack == menupoint))
                    {
                        var combineSelection = CombineSelections.Find(selection => selection.Stack == menupoint);
                        if (Ticks % 2 == 0)
                            scene.SpriteBatch.Draw(cursor.Texture, new Vector2(x + 6, y + i * 16), cursor.GetFrameRect(0), Color.White);
                        item = combineSelection.Item;
                    }
                    if (menupoint == Selection.Stack)
                    {
                        scene.SpriteBatch.Draw(cursor.Texture, new Vector2(x + 0, y + i * 16), cursor.GetFrameRect(0), Color.White);
                        item = Selection.Item;
                    }                

                    item.DrawIcon(scene, new Vector2(x + 16 + 8, y + i * 16 + 8));
                    if (item == Player.Weapon)
                        scene.DrawSprite(flagEquipped, 0, new Vector2(x + 16, y + i * 16), SpriteEffects.None, 0);
                    scene.DrawText($"{item.FakeName} x{menupoint.Count}", new Vector2(x + 32, y + i * 16), Alignment.Left, new TextParameters().SetConstraints(width - 32, 16).SetBold(true).SetColor(Color.White, Color.Black));
                    i++;
                }
            }
        }
    }
}
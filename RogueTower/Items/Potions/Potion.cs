using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueTower.Items.Potions
{
    abstract class Potion : Item
    {
        public PotionAppearance Appearance;

        public override string FakeName => Appearance.Randomized.Name;
        public override string FakeDescription => Appearance.Randomized.Description;

        public Potion(PotionAppearance appearance, string name, string description) : base(name, description)
        {
            Appearance = appearance;
        }

        public abstract void DrinkEffect(Enemy enemy);

        public abstract void DipEffect(Enemy enemy, Item item);

        protected override void CopyTo(Item item)
        {
            base.CopyTo(item);
            if (item is Potion potion)
            {
                potion.Appearance = Appearance;
            }
        }

        public void Empty(Enemy enemy)
        {
            Transform(enemy, new EmptyBottle());
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            var appearance = Appearance.Randomized;
            scene.DrawSprite(appearance.Sprite, 0, position - appearance.Sprite.Middle, SpriteEffects.None, 1.0f);
        }
    }

    class PotionAppearance
    {
        public SpriteReference Sprite;
        public string Name;
        public string Description;
        public PotionAppearance Randomized;

        public PotionAppearance(SpriteReference sprite, string name, string description)
        {
            Sprite = sprite;
            Name = name;
            Description = description;
            Randomized = this;
        }

        //Not random
        public static PotionAppearance Water = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_water"), "Clear Potion", "A clear potion.");
        public static PotionAppearance Blood = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_red"), "Red Potion", "A red potion.");
        public static PotionAppearance Lava = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_redange"), "Hot Potion", "A hot potion.");
        //Random
        public static PotionAppearance Red = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_red"), "Red Potion", "A red potion.");
        public static PotionAppearance Blue = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_blue"), "Blue Potion", "A blue potion.");
        public static PotionAppearance Green = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_green"), "Green Potion", "A green potion.");
        public static PotionAppearance Clear = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_water"), "Clear Potion", "A clear potion.");
        public static PotionAppearance Grey = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_grey"), "Grey Potion", "A grey potion.");
        public static PotionAppearance Mauve = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_mauve"), "Mauve Potion", "A mauve potion.");
        public static PotionAppearance Orange = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_orange"), "Orange Potion", "An orange potion.");
        public static PotionAppearance Septic = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_septic"), "Septic Potion", "A septic potion.");
        public static PotionAppearance Lime = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_lime"), "Lime Potion", "A lime potion.");
        public static PotionAppearance BrownPink = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_bink"), "Disgusting Potion", "A disgusting potion.");
        public static PotionAppearance BluePurple = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_blurple"), "Pleasant Potion", "A pleasant potion.");
        public static PotionAppearance BlueGrey = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_bray"), "Crystalline Potion", "A crystalline potion.");
        public static PotionAppearance OrangeRed = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_redange"), "Hot Potion", "A hot potion.");
        public static PotionAppearance YellowGreen = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_yeen"), "Bubbling Potion", "A bubbling potion.");

        public static IEnumerable<PotionAppearance> RandomAppearances
        {
            get
            {
                yield return Red;
                yield return Blue;
                yield return Green;
                yield return Clear;
                yield return Grey;
                yield return Mauve;
                yield return Orange;
                yield return Septic;
                yield return Lime;
                yield return BrownPink;
                yield return BluePurple;
                yield return BlueGrey;
                yield return OrangeRed;
                yield return YellowGreen;
            }
        }

        public static void Randomize(Random random)
        {
            var potionsA = RandomAppearances.ToList();
            var potionsB = RandomAppearances.Shuffle().ToList();

            foreach (var tuple in Enumerable.Zip(potionsA, potionsB, (a, b) => Tuple.Create(a, b)))
            {
                tuple.Item1.Randomized = tuple.Item2;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

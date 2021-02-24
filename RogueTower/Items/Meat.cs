using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Enemies;
using RogueTower.Items.Interfaces;

namespace RogueTower.Items
{
    class Meat : Item, IEdible
    {
        public enum Type
        {
            Moai,
            Snake,
        }

        SpriteReference Sprite;
        public Type MeatType;

        protected Meat() : base()
        {

        }

        public Meat(SpriteReference sprite, Type type, string name, string description) : base(name, description)
        {
            Sprite = sprite;
            MeatType = type;
        }

        public static Meat Moai => new Meat(SpriteLoader.Instance.AddSprite("content/item_meat_moai"), Type.Moai, "Moai Meat", "Tastes undescribable.");
        public static Meat Snake => new Meat(SpriteLoader.Instance.AddSprite("content/item_meat_snake"), Type.Snake, "Snake Meat", "Chewy.");

        public override int GetStackCode()
        {
            return base.GetStackCode() ^ (int)MeatType;
        }

        public override bool IsStackable(Item other)
        {
            if (other is Meat otherMeat)
                return otherMeat.MeatType == MeatType;
            else
                return base.IsStackable(other);
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            scene.DrawSprite(Sprite, 0, position - Sprite.Middle, SpriteEffects.None, 1.0f);
        }

        public bool CanEat(Enemy enemy)
        {
            return true;
        }

        public void EatEffect(Enemy enemy)
        {
            enemy.Heal(10);
            Destroy();
        }

        protected override Item MakeCopy()
        {
            return new Meat();
        }

        protected override void CopyTo(Item item)
        {
            base.CopyTo(item);
            if (item is Meat meat)
            {
                meat.Sprite = Sprite;
                meat.MeatType = MeatType;
            }
        }
    }
}

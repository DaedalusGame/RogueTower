using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueTower.Items
{
    class EmptyBottle : Item
    {
        public EmptyBottle() : base("Empty Bottle", "An empty bottle. The remainder of drinking a potion.")
        {
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            var potionEmpty = SpriteLoader.Instance.AddSprite("content/item_potion_empty");

            scene.DrawSprite(potionEmpty, 0, position - potionEmpty.Middle, SpriteEffects.None, 1.0f);
        }

        protected override Item MakeCopy()
        {
            return new EmptyBottle();
        }
    }
}

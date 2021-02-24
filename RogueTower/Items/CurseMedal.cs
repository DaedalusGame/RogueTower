using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Enemies;

namespace RogueTower.Items
{
    class CurseMedal : Item
    {
        public override bool AutoPickup => true;

        public CurseMedal() : base("Curse Medal", "This item will curse you.")
        {
        }

        public override void OnAdd(Enemy enemy)
        {
            enemy.VisualOffset = enemy.OffsetShudder(60);
            enemy.Hitstop = 60;
            enemy.AddStatusEffect(new Curse(enemy));
            Destroy();
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            var curse = SpriteLoader.Instance.AddSprite("content/item_curse");

            scene.DrawSprite(curse, 0, position - curse.Middle, SpriteEffects.None, 1.0f);
        }

        protected override Item MakeCopy()
        {
            return new CurseMedal();
        }
    }
}

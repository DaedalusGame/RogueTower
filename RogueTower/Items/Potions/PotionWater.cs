using RogueTower.Enemies;
using static RogueTower.Util;

namespace RogueTower.Items.Potions
{
    class PotionWater : Potion
    {
        public static ItemMemoryKey MemoryPotion = new ItemMemoryKey();

        public override ItemMemoryKey MemoryKey => MemoryPotion;

        public PotionWater() : base(PotionAppearance.Water, "Water", "A bottle of water.")
        {

        }

        public override void DipEffect(Enemy enemy, Item item)
        {
            //NOOP
        }

        public override void DrinkEffect(Enemy enemy)
        {
            Message(enemy, new MessageText("Tastes like water."));
            Identify(enemy);
            Empty(enemy);
        }

        protected override Item MakeCopy()
        {
            return new PotionWater();
        }
    }
}

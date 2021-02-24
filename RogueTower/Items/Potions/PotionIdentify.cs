using RogueTower.Enemies;
using static RogueTower.Util;

namespace RogueTower.Items.Potions
{
    class PotionIdentify : Potion
    {
        public static ItemMemoryKey MemoryPotion = new ItemMemoryKey();

        public override ItemMemoryKey MemoryKey => MemoryPotion;

        public PotionIdentify() : base(PotionAppearance.Clear, "Identify Potion", "An identify potion.")
        {

        }

        public override void DipEffect(Enemy enemy, Item item)
        {
            if (enemy is Player player && !player.Memory.IsKnown(item))
            {
                Message(enemy, new MessageItem(item.Copy(), $"Suddenly, {0}'s nature is much clearer to you."));
                item.Identify(player);
                Identify(player);
            }
            Empty(enemy);
        }

        public override void DrinkEffect(Enemy enemy)
        {
            Empty(enemy);
        }

        protected override Item MakeCopy()
        {
            return new PotionIdentify();
        }
    }
}

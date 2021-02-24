using RogueTower.Enemies;

namespace RogueTower.Items.Potions
{
    class PotionHealth : Potion
    {
        public static ItemMemoryKey MemoryPotion = new ItemMemoryKey();

        public override ItemMemoryKey MemoryKey => MemoryPotion;

        public PotionHealth() : base(PotionAppearance.Red, "Health Potion", "A health potion.")
        {

        }

        public override void DipEffect(Enemy enemy, Item item)
        {
            //NOOP
        }

        public override void DrinkEffect(Enemy enemy)
        {
            if (enemy.Health < enemy.HealthMax)
            {
                Identify(enemy);
                enemy.Heal(40);
            }
            Empty(enemy);
        }

        protected override Item MakeCopy()
        {
            return new PotionHealth();
        }
    }
}

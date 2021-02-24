using RogueTower.Enemies;
using System.Linq;

namespace RogueTower.Items.Potions
{
    class PotionHolyWater : Potion
    {
        public static ItemMemoryKey MemoryPotion = new ItemMemoryKey();

        public override ItemMemoryKey MemoryKey => MemoryPotion;

        public PotionHolyWater() : base(PotionAppearance.Water, "Holy Water", "A bottle of holy water.")
        {

        }

        public override void DipEffect(Enemy enemy, Item item)
        {
            //NOOP
        }

        public override void DrinkEffect(Enemy enemy)
        {
            if (enemy.StatusEffects.Any(x => x is Curse))
                Identify(enemy);
            foreach (var statusEffect in enemy.StatusEffects.Where(x => x is Curse))
                statusEffect.Remove();
            Empty(enemy);
        }

        protected override Item MakeCopy()
        {
            return new PotionHolyWater();
        }
    }
}

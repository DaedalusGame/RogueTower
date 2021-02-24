using RogueTower.Enemies;
using System.Linq;

namespace RogueTower.Items.Potions
{
    class PotionAntidote : Potion
    {
        public static ItemMemoryKey MemoryPotion = new ItemMemoryKey();

        public override ItemMemoryKey MemoryKey => MemoryPotion;

        public PotionAntidote() : base(PotionAppearance.Green, "Antidote Potion", "An antidote potion.")
        {

        }

        public override void DipEffect(Enemy enemy, Item item)
        {
            //NOOP
        }

        public override void DrinkEffect(Enemy enemy)
        {
            if (enemy.StatusEffects.Any(x => x is Poison))
                Identify(enemy);
            foreach (var statusEffect in enemy.StatusEffects.Where(x => x is Poison))
                statusEffect.Remove();
            Empty(enemy);
        }

        protected override Item MakeCopy()
        {
            return new PotionAntidote();
        }
    }
}

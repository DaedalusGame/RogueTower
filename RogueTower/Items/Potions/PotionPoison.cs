using RogueTower.Enemies;
using System.Linq;
using static RogueTower.Util;


namespace RogueTower.Items.Potions
{
    class PotionPoison : Potion
    {
        public static ItemMemoryKey MemoryPotion = new ItemMemoryKey();

        public override ItemMemoryKey MemoryKey => MemoryPotion;

        public PotionPoison() : base(PotionAppearance.Septic, "Poison Potion", "A poison potion.")
        {

        }

        public override void DipEffect(Enemy enemy, Item item)
        {
            //NOOP
        }

        public override void DrinkEffect(Enemy enemy)
        {
            Message(enemy, new MessageText("Blech! This is tainted!"));
            if (!enemy.StatusEffects.Any(x => x is Poison))
                Identify(enemy);
            enemy.AddStatusEffect(new Poison(enemy, 1000));
            Empty(enemy);
        }

        protected override Item MakeCopy()
        {
            return new PotionPoison();
        }
    }
}

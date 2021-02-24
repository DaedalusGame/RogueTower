using RogueTower.Enemies;
using static RogueTower.Util;

namespace RogueTower.Items.Potions
{
    class PotionBlood : Potion
    {
        public static ItemMemoryKey MemoryPotion = new ItemMemoryKey();

        public override ItemMemoryKey MemoryKey => MemoryPotion;

        public PotionBlood() : base(PotionAppearance.Blood, "Blood", "A bottle of blood.")
        {

        }

        public override void DipEffect(Enemy enemy, Item item)
        {
            //NOOP
        }

        public override void DrinkEffect(Enemy enemy)
        {
            Message(enemy, new MessageText("Tastes like blood."));
            Identify(enemy);
            Empty(enemy);
        }

        protected override Item MakeCopy()
        {
            return new PotionBlood();
        }
    }
}

using RogueTower.Enemies;
using RogueTower.Items.Devices;
using System;
using static RogueTower.Util;

namespace RogueTower.Items.Potions
{
    class PotionEnergy : Potion
    {
        public static ItemMemoryKey MemoryPotion = new ItemMemoryKey();

        public override ItemMemoryKey MemoryKey => MemoryPotion;

        public PotionEnergy() : base(PotionAppearance.Orange, "Energy Potion", "An energy potion.")
        {

        }

        public override void DipEffect(Enemy enemy, Item item)
        {
            if (item is Device device)
            {
                Message(enemy, new MessageItem(device.Copy(), $"{0} is imbued with energy!"));
                device.Charges = Math.Min(device.Charges + enemy.Random.Next(6) + 3, device.MaxCharges);
                if (device.Broken)
                    device.Transform<Device>(enemy, x => x.Broken = false);
                Identify(enemy);
            }
            Empty(enemy);
        }

        public override void DrinkEffect(Enemy enemy)
        {
            Empty(enemy);
        }

        protected override Item MakeCopy()
        {
            return new PotionEnergy();
        }
    }
}

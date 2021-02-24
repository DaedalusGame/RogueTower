using RogueTower.Enemies;
using RogueTower.Items.Potions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower.Items.Devices
{
    class DeviceBrew : Device
    {
        WeightedList<Func<Potion>> Potions = new WeightedList<Func<Potion>>()
            {
                { () => new PotionHealth(), 10 },
                { () => new PotionAntidote(), 10 },
                { () => new PotionPoison(), 10 },
            };

        protected DeviceBrew() : base()
        {

        }

        public DeviceBrew(bool broken) : base("Brewing Machine", "This machine can turn meat into potions.", broken, 10)
        {
        }

        public override bool CanUse(Enemy enemy, IEnumerable<Item> items)
        {
            return items.All(x => x is Meat);
        }

        public override void MachineEffect(Enemy enemy, IEnumerable<Item> items)
        {
            foreach (Item item in items)
            {
                if (Charges >= 1)
                {
                    Charges -= 1;
                    if (item is Meat meat)
                    {
                        meat.Transform(enemy, BrewPotion(meat));
                        Identify(enemy);
                    }
                    else
                    {
                        item.Destroy();
                    }
                }
            }
        }

        private Potion BrewPotion(Meat meat)
        {
            Random random = new Random(meat.MeatType.GetHashCode());
            return Potions.GetWeighted(random)();
        }

        protected override Item MakeCopy()
        {
            return new DeviceBrew();
        }
    }
}

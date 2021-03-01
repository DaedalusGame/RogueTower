using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using System.Collections.Generic;
using System.Linq;

namespace RogueTower.Items.Devices
{
    class DeviceFlamebringer : Device
    {
        protected DeviceFlamebringer() : base()
        {

        }

        public DeviceFlamebringer(bool broken) : base("Conflagration Machine", "A machine with a symbol representing a blade surrounded by a flame.", broken, 6)
        {

        }

        public override bool CanUse(Enemy enemy, IEnumerable<Item> items)
        {
            return items.All(x => x is WeaponSword);
        }

        public override void MachineEffect(Enemy enemy, IEnumerable<Item> items)
        {
            foreach (Item item in items)
            {
                if (Charges >= 1)
                {
                    Charges -= 1;
                    if (item is WeaponSword sword)
                    {
                        sword.Transform(enemy, new WeaponFireSword(sword.Damage, sword.WeaponSize));
                        Identify(enemy);
                    }
                    else
                    {
                        item.Destroy();
                        new MessageText($"The {item.GetName(enemy)} turns to embers.");
                    }
                }
            }
        }

        protected override Item MakeCopy()
        {
            return new DeviceFlamebringer();
        }
    }
}

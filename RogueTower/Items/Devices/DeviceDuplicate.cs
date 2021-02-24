using RogueTower.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower.Items.Devices
{
    class DeviceDuplicate : Device
    {
        protected DeviceDuplicate() : base()
        {

        }

        public DeviceDuplicate(bool broken) : base("Duplicate Machine", "Any item this machine is used with will be duplicated.", broken, 1)
        {
        }

        public override bool CanUse(Enemy enemy, IEnumerable<Item> items)
        {
            return items.Any();
        }

        public override void MachineEffect(Enemy enemy, IEnumerable<Item> items)
        {
            foreach (Item item in items)
            {
                if (Charges >= 1)
                {
                    Charges -= 1;
                    if (enemy is Player player)
                    {
                        player.Pickup(item.Copy());
                    }
                    Identify(enemy);
                }
            }
        }

        protected override Item MakeCopy()
        {
            return new DeviceDuplicate();
        }
    }
}

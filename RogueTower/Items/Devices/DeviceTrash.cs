using RogueTower.Enemies;
using System.Collections.Generic;
using System.Linq;

namespace RogueTower.Items.Devices
{
    class DeviceTrash : Device
    {
        protected DeviceTrash() : base()
        {

        }

        public DeviceTrash(bool broken) : base("Trash Machine", "Any item this machine is used with will disappear.", broken, 10)
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
                    item.Destroy();
                    Identify(enemy);
                }
            }
        }

        protected override Item MakeCopy()
        {
            return new DeviceTrash();
        }
    }
}

using RogueTower.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    class ItemMemoryKey
    {

    }

    class ItemMemory
    {
        HashSet<ItemMemoryKey> ItemKnowledge = new HashSet<ItemMemoryKey>();

        public bool IsKnown(Item item)
        {
            return ItemKnowledge.Contains(item.MemoryKey);
        }

        public void Identify(Item item)
        {
            ItemKnowledge.Add(item.MemoryKey);
        }

        public void Forget(Item item)
        {
            ItemKnowledge.Remove(item.MemoryKey);
        }

        public string GetName(Item item)
        {
            return IsKnown(item) ? item.TrueName : item.FakeName;
        }

        public string GetDescription(Item item)
        {
            return IsKnown(item) ? item.TrueDescription : item.FakeDescription;
        }
    }
}

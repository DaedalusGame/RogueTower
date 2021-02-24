using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Enemies;
using System.Collections.Generic;

namespace RogueTower.Items.Devices
{
    abstract class Device : Item
    {
        public bool Broken;
        public int Charges, MaxCharges;
        public ItemMemoryKey MemoryBroken = new ItemMemoryKey();

        public override ItemMemoryKey MemoryKey => MemoryBroken;

        public override string FakeName => Broken ? "Broken Machine" : "? Machine";
        public override string FakeDescription => Broken ? "This machine is broken." : "This machine is unknown.";
        public override string TrueName => Broken ? $"Broken Machine ({Name})" : $"{Name} [{Charges}]";

        protected Device() : base()
        {

        }

        public Device(string name, string description, bool broken, int maxCharges) : base(name, description)
        {
            Broken = broken;
            MaxCharges = maxCharges;
            Charges = maxCharges;
        }

        public override int GetStackCode()
        {
            return GetHashCode();
        }

        public override bool IsStackable(Item other)
        {
            return false;
        }

        public abstract bool CanUse(Enemy enemy, IEnumerable<Item> items);

        public abstract void MachineEffect(Enemy enemy, IEnumerable<Item> items);

        protected override void CopyTo(Item item)
        {
            base.CopyTo(item);

            if (item is Device device)
            {
                device.Broken = Broken;
                device.Charges = Charges;
                device.MaxCharges = MaxCharges;
            }
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            var caseBroken = SpriteLoader.Instance.AddSprite("content/item_machine_case_broken");
            var gemBroken = SpriteLoader.Instance.AddSprite("content/item_machine_gem_broken");
            var caseFixed = SpriteLoader.Instance.AddSprite("content/item_machine_case_iron");
            var gemFixed = SpriteLoader.Instance.AddSprite("content/item_machine_gem_blue");

            if (Broken)
            {
                scene.DrawSprite(gemBroken, 0, position - gemBroken.Middle, SpriteEffects.None, 1.0f);
                scene.DrawSprite(caseBroken, 0, position - caseBroken.Middle, SpriteEffects.None, 1.0f);
            }
            else
            {
                scene.DrawSprite(gemFixed, 0, position - gemFixed.Middle, SpriteEffects.None, 1.0f);
                scene.DrawSprite(caseFixed, 0, position - caseFixed.Middle, SpriteEffects.None, 1.0f);
            }
        }
    }
}

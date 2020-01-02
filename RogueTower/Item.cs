using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueTower
{
    interface IEdible
    {
        bool CanEat(Enemy enemy);

        void EatEffect(Enemy enemy);
    }

    class ItemStacker : IEqualityComparer<Item>
    {
        public bool Equals(Item x, Item y)
        {
            return x.IsStackable(y);
        }

        public int GetHashCode(Item obj)
        {
            return obj.GetStackCode();
        }
    }

    abstract class Item
    {
        public static ItemStacker Stacker = new ItemStacker();
        public static ItemMemoryKey MemoryKnown = new ItemMemoryKey();

        public string Name;
        public string Description;
        public bool Destroyed;

        public virtual ItemMemoryKey MemoryKey => MemoryKnown;
        public virtual string FakeName => Name;
        public virtual string FakeDescription => Description;
        public virtual string TrueName => Name;
        public virtual string TrueDescription => Description;

        protected Item()
        {

        }

        public Item(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void Destroy()
        {
            Destroyed = true;
        }

        public virtual int GetStackCode()
        {
            return GetType().GetHashCode();
        }

        public virtual bool IsStackable(Item other)
        {
            return GetType().Equals(GetType());
        }

        public void Identify(Enemy enemy)
        {
            if (enemy is Player player)
                player.Memory.Identify(this);
        }

        public Item Copy()
        {
            var item = MakeCopy();
            CopyTo(item);
            return item;
        }

        protected abstract Item MakeCopy();

        protected virtual void CopyTo(Item item)
        {
            item.Name = Name;
            item.Description = Description;
        }

        public abstract void DrawIcon(SceneGame scene, Vector2 position);
    }

    class Meat : Item, IEdible
    {
        public enum Type
        {
            Moai,
            Snake,
        }

        SpriteReference Sprite;
        public Type MeatType;

        protected Meat() : base()
        {

        }

        public Meat(SpriteReference sprite, Type type, string name, string description) : base(name, description)
        {
            Sprite = sprite;
            MeatType = type;
        }

        public static Meat Moai => new Meat(SpriteLoader.Instance.AddSprite("content/item_meat_moai"), Type.Moai, "Moai Meat", "Tastes undescribable.");
        public static Meat Snake => new Meat(SpriteLoader.Instance.AddSprite("content/item_meat_snake"), Type.Snake, "Snake Meat", "Chewy.");

        public override int GetStackCode()
        {
            return base.GetStackCode() ^ (int)MeatType;
        }

        public override bool IsStackable(Item other)
        {
            if(other is Meat otherMeat)
                return otherMeat.MeatType == MeatType;
            else
                return base.IsStackable(other);
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            scene.DrawSprite(Sprite, 0, position - Sprite.Middle, SpriteEffects.None, 1.0f);
        }

        public bool CanEat(Enemy enemy)
        {
            return true;
        }

        public void EatEffect(Enemy enemy)
        {
            enemy.Heal(10);
            Destroy();
        }

        protected override Item MakeCopy()
        {
            return new Meat();
        }

        protected override void CopyTo(Item item)
        {
            base.CopyTo(item);
            if (item is Meat meat) {
                meat.Sprite = Sprite;
                meat.MeatType = MeatType;
            }
        }
    }

    class PotionAppearance
    {
        public SpriteReference Sprite;
        public string Name;
        public string Description;
        public PotionAppearance Randomized;

        public PotionAppearance(SpriteReference sprite, string name, string description)
        {
            Sprite = sprite;
            Name = name;
            Description = description;
            Randomized = this;
        }

        //Not random
        public static PotionAppearance Water = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_water"), "Water Bottle", "A bottle of water.");
        public static PotionAppearance Blood = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_red"), "Blood Potion", "A blood potion.");
        //Random
        public static PotionAppearance Red = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_red"), "Red Potion", "A red potion.");
        public static PotionAppearance Blue = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_blue"), "Blue Potion", "A blue potion.");
        public static PotionAppearance Green = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_green"), "Green Potion", "A green potion.");
        public static PotionAppearance Clear = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_water"), "Clear Potion", "A clear potion.");
        public static PotionAppearance Grey = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_grey"), "Grey Potion", "A grey potion.");
        public static PotionAppearance Mauve = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_mauve"), "Mauve Potion", "A mauve potion.");
        public static PotionAppearance Orange = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_orange"), "Orange Potion", "An orange potion.");
        public static PotionAppearance Septic = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_septic"), "Septic Potion", "A septic potion.");
        public static PotionAppearance Lime = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_lime"), "Lime Potion", "A lime potion.");
        public static PotionAppearance BrownPink = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_bink"), "Disgusting Potion", "A disgusting potion.");
        public static PotionAppearance BluePurple = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_blurple"), "Pleasant Potion", "A pleasant potion.");
        public static PotionAppearance BlueGrey = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_bray"), "Crystalline Potion", "A crystalline potion.");
        public static PotionAppearance OrangeRed = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_redange"), "Hot Potion", "A hot potion.");
        public static PotionAppearance YellowGreen = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_yeen"), "Bubbling Potion", "A bubbling potion.");

        public static IEnumerable<PotionAppearance> RandomAppearances
        {
            get
            {
                yield return Red;
                yield return Blue;
                yield return Green;
                yield return Clear;
                yield return Grey;
                yield return Mauve;
                yield return Orange;
                yield return Septic;
                yield return Lime;
                yield return BrownPink;
                yield return BluePurple;
                yield return BlueGrey;
                yield return OrangeRed;
                yield return YellowGreen;
            }
        }

        public static void Randomize(Random random)
        {
            var potionsA = RandomAppearances.ToList();
            var potionsB = RandomAppearances.Shuffle().ToList();

            foreach(var tuple in Enumerable.Zip(potionsA,potionsB,(a,b) => Tuple.Create(a,b)))
            {
                tuple.Item1.Randomized = tuple.Item2;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    class EmptyBottle : Item
    {
        public EmptyBottle() : base("Empty Bottle", "An empty bottle. The remainder of drinking a potion.")
        {
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            var potionEmpty = SpriteLoader.Instance.AddSprite("content/item_potion_empty");

            scene.DrawSprite(potionEmpty, 0, position - potionEmpty.Middle, SpriteEffects.None, 1.0f);
        }

        protected override Item MakeCopy()
        {
            return new EmptyBottle();
        }
    }

    abstract class Potion : Item
    {
        public PotionAppearance Appearance;

        public override string FakeName => Appearance.Randomized.Name;
        public override string FakeDescription => Appearance.Randomized.Description;

        public Potion(PotionAppearance appearance, string name, string description) : base(name, description)
        {
            Appearance = appearance;
        }

        public abstract void DrinkEffect(Enemy enemy);

        public abstract void DipEffect(Enemy enemy, Item item);

        protected override void CopyTo(Item item)
        {
            base.CopyTo(item);
            if (item is Potion potion)
            {
                potion.Appearance = Appearance;
            }
        }

        public void Empty(Enemy enemy)
        {
            if(enemy is Player player)
            {
                player.Pickup(new EmptyBottle());
            }
            Destroy();
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            var appearance = Appearance.Randomized;
            scene.DrawSprite(appearance.Sprite, 0, position - appearance.Sprite.Middle, SpriteEffects.None, 1.0f);
        }
    }

    class PotionHealth : Potion
    {
        public static ItemMemoryKey MemoryPotion = new ItemMemoryKey();

        public override ItemMemoryKey MemoryKey => MemoryPotion;

        public PotionHealth() : base(PotionAppearance.Red, "Health Potion", "A health potion.")
        {

        }

        public override void DipEffect(Enemy enemy, Item item)
        {
            //NOOP
        }

        public override void DrinkEffect(Enemy enemy)
        {
            if (enemy.Health < enemy.HealthMax)
            {
                Identify(enemy);
                enemy.Heal(40);
            }
            Empty(enemy);
        }

        protected override Item MakeCopy()
        {
            return new PotionHealth();
        }
    }

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

    class PotionIdentify : Potion
    {
        public static ItemMemoryKey MemoryPotion = new ItemMemoryKey();

        public override ItemMemoryKey MemoryKey => MemoryPotion;

        public PotionIdentify() : base(PotionAppearance.Clear, "Identify Potion", "An identify potion.")
        {

        }

        public override void DipEffect(Enemy enemy, Item item)
        {
            if(enemy is Player player && !player.Memory.IsKnown(item))
            {
                item.Identify(player);
                Identify(player);
            }
            Empty(enemy);
        }

        public override void DrinkEffect(Enemy enemy)
        {
            Empty(enemy);
        }

        protected override Item MakeCopy()
        {
            return new PotionIdentify();
        }
    }

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

            if(item is Device device)
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
                    if(enemy is Player player)
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
                        if (enemy is Player player)
                        {
                            player.Pickup(BrewPotion(meat));
                        }
                        Identify(enemy);
                    }
                    item.Destroy();
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

    class DroppedItem : GameObject
    {
        public Item Item;

        public IBox Box;
        public Vector2 Position
        {
            get
            {
                return Box.Bounds.Center;
            }
            set
            {
                var pos = value + Box.Bounds.Center - Box.Bounds.Location;
                Box.Teleport(pos.X, pos.Y);
            }
        }
        public Vector2 Velocity;
        protected Vector2 VelocityLeftover;

        public virtual float Gravity => 0.2f;
        public virtual float GravityLimit => 10f;
        public bool OnGround;
        public bool InAir => !OnGround;
        public bool OnWall;
        public bool OnCeiling;

        public bool Incorporeal => false;

        public override RectangleF ActivityZone => new RectangleF(Position - new Vector2(1000, 600) / 2, new Vector2(1000, 600));

        public DroppedItem(GameWorld world, Vector2 position, Item item) : base(world)
        {
            Item = item;
            Create(position.X, position.Y);
        }

        public void Create(float x, float y)
        {
            Box = World.Create(x-4, y-4, 8, 8);
            Box.AddTags(CollisionTag.Character);
            Box.Data = this;
        }

        public override void Destroy()
        {
            base.Destroy();
            World.Remove(Box);
        }

        public void Spread()
        {
            Velocity = Util.AngleToVector(Random.NextFloat() * MathHelper.TwoPi) * 2;
            Velocity.Y = -2;
        }

        protected Vector2 CalculateMovement(float delta)
        {
            var velocity = Velocity * delta + VelocityLeftover;
            var movement = new Vector2((int)velocity.X, (int)velocity.Y);

            VelocityLeftover = velocity - movement;

            return movement;
        }

        protected IMovement Move(Vector2 movement)
        {
            return Box.Move(Box.X + movement.X, Box.Y + movement.Y, collision =>
            {
                if (IgnoresCollision(collision.Hit.Box))
                    return null;
                return new SlideAdvancedResponse(collision);
            });
        }

        protected bool HandlePanicBox(IMovement move)
        {
            RectangleF panicBox = new RectangleF(move.Destination.X + 2, move.Destination.Y + 2, move.Destination.Width - 4, move.Destination.Height - 4);
            var found = World.FindBoxes(panicBox);
            if (found.Any() && found.Any(x => x != Box && !IgnoresCollision(x)))
            {
                Box.Teleport(move.Origin.X, move.Origin.Y);
                return true;
            }
            return false;
        }

        protected bool IgnoresCollision(IBox box)
        {
            return Incorporeal || box.HasTag(CollisionTag.NoCollision) || box.HasTag(CollisionTag.Character);
        }

        protected override void UpdateDelta(float delta)
        {
            var movement = CalculateMovement(delta);

            bool IsMovingVertically = Math.Abs(movement.Y) >= 1;
            bool IsMovingHorizontally = Math.Abs(movement.X) >= 1;

            IMovement move = Move(movement);

            var hits = move.Hits.Where(c => c.Normal != Vector2.Zero && !IgnoresCollision(c.Box));
            var cornerOnly = !move.Hits.Any(c => c.Normal != Vector2.Zero) && move.Hits.Any();

            bool panic = false;
            if (!Incorporeal)
                panic = HandlePanicBox(move);
            if (!panic)
            {

                if (IsMovingVertically && !cornerOnly)
                {
                    if (hits.Any((c) => c.Normal.Y < 0))
                        OnGround = true;
                    else
                        OnGround = false;

                    if (hits.Any((c) => c.Normal.Y > 0))
                        OnCeiling = true;
                    else
                        OnCeiling = false;
                }

                if (IsMovingHorizontally && !cornerOnly)
                {
                    if (hits.Any((c) => c.Normal.X != 0))
                        OnWall = true;
                    else
                        OnWall = false;
                }
            }
        }

        protected override void UpdateDiscrete()
        {
            if (OnCeiling && !Incorporeal)
            {
                Velocity.Y = 1;
            }
            else if (OnGround && !Incorporeal)
            {
                Velocity.X = 0;
                Velocity.Y = 0;
            }

            if(OnWall)
            {
                Velocity.X *= -1;
                Velocity.Y = 0;
                OnWall = false;
            }

            if (Velocity.Y < GravityLimit)
                Velocity.Y = Math.Min(GravityLimit, Velocity.Y + Gravity); //Gravity
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Foreground;
        }

        public override void Draw(SceneGame scene)
        {
            Item.DrawIcon(scene, Position + new Vector2(0,-Box.Height/2));
        }
    }
}

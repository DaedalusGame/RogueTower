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

        public string Name;
        public string Description;
        public bool Destroyed;

        public virtual string FakeName => Name;
        public virtual string FakeDescription => Description;

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
        public static PotionAppearance Blood = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_blood"), "Blood Potion", "A blood potion.");
        //Random
        public static PotionAppearance Red = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_red"), "Red Potion", "A red potion.");
        public static PotionAppearance Blue = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_blue"), "Blue Potion", "A blue potion.");
        public static PotionAppearance Green = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_green"), "Green Potion", "A green potion.");
        public static PotionAppearance Clear = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_water"), "Clear Potion", "A clear potion.");
        public static PotionAppearance Grey = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_grey"), "Grey Potion", "A grey potion.");
        public static PotionAppearance Mauve = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_mauve"), "Mauve Potion", "A mauve potion.");
        public static PotionAppearance Orange = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_orange"), "Orange Potion", "An orange potion.");
        public static PotionAppearance Septic = new PotionAppearance(SpriteLoader.Instance.AddSprite("content/item_potion_septic"), "Septic Potion", "A septic potion.");

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

    abstract class Potion : Item
    {
        public PotionAppearance Appearance;

        public override string FakeName => Appearance.Randomized.Name;
        public override string FakeDescription => Appearance.Randomized.Description;

        public Potion(PotionAppearance appearance, string name, string description) : base(name, description)
        {
            Appearance = appearance;
        }

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            var appearance = Appearance.Randomized;
            scene.DrawSprite(appearance.Sprite, 0, position - appearance.Sprite.Middle, SpriteEffects.None, 1.0f);
        }

        public abstract void DrinkEffect(Enemy enemy);

        public abstract void DipEffect(Item item);
    }

    class PotionHealth : Potion
    {
        public PotionHealth() : base(PotionAppearance.Red, "Health Potion", "A health potion.")
        {

        }

        public override void DipEffect(Item item)
        {
            //NOOP
        }

        public override void DrinkEffect(Enemy enemy)
        {
            enemy.Heal(40);
            Destroy();
        }
    }

    class PotionAntidote : Potion
    {
        public PotionAntidote() : base(PotionAppearance.Green, "Antidote Potion", "An antidote potion.")
        {

        }

        public override void DipEffect(Item item)
        {
            //NOOP
        }

        public override void DrinkEffect(Enemy enemy)
        {
            foreach (var statusEffect in enemy.StatusEffects.Where(x => x is Poison))
                statusEffect.Remove();
            Destroy();
        }
    }

    class PotionPoison : Potion
    {
        public PotionPoison() : base(PotionAppearance.Septic, "Poison Potion", "A poison potion.")
        {

        }

        public override void DipEffect(Item item)
        {
            //NOOP
        }

        public override void DrinkEffect(Enemy enemy)
        {
            enemy.AddStatusEffect(new Poison(enemy, 1000));
            Destroy();
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

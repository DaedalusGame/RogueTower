using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using RogueTower.Enemies;
using RogueTower.Items.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static RogueTower.Util;

namespace RogueTower.Items
{
    abstract class Item
    {
        public static ItemStacker Stacker = new ItemStacker();
        public static ItemMemoryKey MemoryKnown = new ItemMemoryKey();

        public string Name;
        public string Description;
        public bool Destroyed;

        public Random Random = new Random();

        public virtual ItemMemoryKey MemoryKey => MemoryKnown;
        public virtual string FakeName => Name;
        public virtual string FakeDescription => Description;
        public virtual string TrueName => Name;
        public virtual string TrueDescription => Description;
        public virtual bool AutoPickup => false;

        protected Item()
        {

        }

        public Item(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public virtual void OnAdd(Enemy enemy)
        {
            //NOOP
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

        public bool IsKnown(Enemy enemy)
        {
            if (enemy is Player player)
            {
                return player.Memory.IsKnown(this);
            }
            return true;
        }

        public void Identify(Enemy enemy)
        {
            if (enemy is Player player)
            {
                if (!player.Memory.IsKnown(this) && FakeName != TrueName)
                    player.History.Add(new MessageItemIdentify(this));
                player.Memory.Identify(this);
            }
        }

        public string GetName(Enemy enemy)
        {
            if (enemy is Player player)
            {
                return player.Memory.GetName(this);
            }

            return TrueName;
        }

        /// <summary>
        /// Transforms an item in some way (without turning it into a different item).
        /// Prints a message if the name of the item was changed.
        /// </summary>
        /// <typeparam name="T">Purely for more convenient use of lambdas. Must be the type of this instance, or a supertype.</typeparam>
        /// <param name="enemy">The enemy performing the transformation.</param>
        /// <param name="transform">The transformation.</param>
        public void Transform<T>(Enemy enemy, Action<T> transform) where T : Item
        {
            if (!(this is T))
                throw new ArgumentException();
            string nameA = GetName(enemy);
            Item previous = Copy();
            transform((T)this);
            string nameB = GetName(enemy);
            Item current = Copy();
            if (nameA != nameB)
                Message(enemy, new MessageItemTransform(previous, current));
        }

        /// <summary>
        /// Transforms an item into a different item.
        /// Prints a message if the two items have different names.
        /// </summary>
        /// <param name="enemy">The enemy performing the transformation.</param>
        /// <param name="newItem">The new item.</param>
        public void Transform(Enemy enemy, Item newItem)
        {
            string nameA = GetName(enemy);
            string nameB = newItem.GetName(enemy);
            if (nameA != nameB)
                Message(enemy, new MessageItemTransform(this, newItem));
            if (enemy is Player player)
                player.Pickup(newItem);
            Destroy();
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
        public bool AutoPickup => Item.AutoPickup;

        public override RectangleF ActivityZone => new RectangleF(Position - new Vector2(1000, 600) / 2, new Vector2(1000, 600));

        public DroppedItem(GameWorld world, Vector2 position, Item item) : base(world)
        {
            Item = item;
            Create(position.X, position.Y);
        }

        public void Create(float x, float y)
        {
            Box = World.Create(x - 4, y - 4, 8, 8);
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

            if (OnWall)
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

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            Item.DrawIcon(scene, Position + new Vector2(0, -Box.Height / 2));
        }
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
}

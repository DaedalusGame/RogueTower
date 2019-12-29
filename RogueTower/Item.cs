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

        public Item(string name, string description)
        {
            Name = name;
            Description = description;
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

    class Meat : Item
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

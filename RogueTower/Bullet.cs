using Humper;
using Humper.Responses;
using Microsoft.Xna.Framework;
using System;
using static RogueTower.Game;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humper.Base;

namespace RogueTower
{
    abstract class Bullet : GameObject
    {
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
        public Vector2 BulletSize;
        public Vector2 Velocity;
        public float Frame, FrameEnd;
        public GameObject Shooter;

        public override RectangleF ActivityZone => World.Bounds;

        protected Bullet(GameWorld world, Vector2 position) : base(world)
        {
            Create(position.X, position.Y);
            BulletSize = new Vector2(8, 8);
        }

        public override void Destroy()
        {
            base.Destroy();
            World.Remove(Box);
        }

        public void Create(float x, float y)
        {
            Box = World.Create(x - BulletSize.X / 2, y - BulletSize.Y / 2, BulletSize.X, BulletSize.Y);
            Box.Data = this;
            Box.AddTags(CollisionTag.NoCollision);
        }

        protected override void UpdateDelta(float delta)
        {
            Frame += delta;
            var move = Box.Move(Box.X + Velocity.X * delta, Box.Y + Velocity.Y * delta, collision =>
            {
                return GetCollision(collision);
            });
            foreach (var hit in move.Hits)
                OnCollision(hit);
        }

        protected virtual ICollisionResponse GetCollision(ICollision collision)
        {
            return new CrossResponse(collision);
        }

        protected abstract void OnCollision(IHit hit);

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
                Destroy();
        }

        public override void ShowDamage(double damage)
        {
            //NOOP
        }
    }

    class SpellOrange : Bullet
    {
        public SpellOrange(GameWorld world, Vector2 position) : base(world, position)
        {
        }

        protected override void OnCollision(IHit hit)
        {
            if (Destroyed || hit.Box.Data == Shooter)
                return;
            bool explode = false;
            if (hit.Box.Data is Player player)
            {
                explode = true;
            }
            if(hit.Box.Data is Tile tile)
            {
                explode = true;
            }
            if (explode)
            {
                new ScreenShakeRandom(World, 5, 10);
                new Explosion(World, Position)
                {
                    Shooter = this.Shooter,
                    FrameEnd = 20,
                };
                Destroy();
            }
        }
    }

    class Explosion : Bullet
    {
        public Explosion(GameWorld world, Vector2 position) : base(world, position)
        {
            BulletSize = new Vector2(32, 32);
            PlaySFX(sfx_explosion1, 1.0f, 0.1f, 0.2f);
            HandleDamage();
        }

        protected override void OnCollision(IHit hit)
        {
            //NOOP
        }

        protected override void UpdateDiscrete()
        {
            base.UpdateDiscrete();
            if(Frame < FrameEnd / 2)
                HandleDamage();
        }

        private void HandleDamage()
        {
            foreach (var box in World.FindBoxes(Box.Bounds))
            {
                if (box.Data is Player player)
                {
                    player.Hit(new Vector2(Math.Sign(player.Position.X - Position.X), -2), 20, 50, 100);
                }
            }
        }
    }

    class Fireball : Bullet
    {
        public Fireball(GameWorld world, Vector2 position) : base(world, position)
        {
        }

        protected override void OnCollision(IHit hit)
        {
            if (Destroyed || hit.Box.Data == Shooter)
                return;
            if (hit.Box.Data is Player player)
            {
                player.Hit(new Vector2(Math.Sign(Velocity.X), -2), 20, 50, 20);
            }
        }
    }

    class Knife : Bullet
    {
        double knifeDamage = 15.0;

        public Knife(GameWorld world, Vector2 position) : base(world, position)
        {
        }

        protected override ICollisionResponse GetCollision(ICollision collision)
        {
            if(collision.Hit.Box.Data == Shooter)
                return new CrossResponse(collision);
            return new TouchResponse(collision);
        }

        protected override void OnCollision(IHit hit)
        {
            if (Destroyed || hit.Box.Data == Shooter)
                return;
            bool bounced = true;
            
            if (hit.Box.Data is Tile tile)
                tile.HandleTileDamage(knifeDamage);
            if (hit.Box.Data is Enemy enemy)
            {
                if (enemy.CanDamage)
                    bounced = false;
                enemy.Hit(new Vector2(Math.Sign(Velocity.X), -2), 20, 50, knifeDamage);
            }
            if (bounced)
            {
                new KnifeBounced(World, Position, new Vector2(Math.Sign(Velocity.X) * -1.5f, -3f), MathHelper.Pi * 0.3f, 24);
                PlaySFX(sfx_sword_bink, 1.0f, 0.1f, 0.3f);
            }
            Destroy();
        }
    }
}

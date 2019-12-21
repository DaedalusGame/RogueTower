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
using Microsoft.Xna.Framework.Graphics;

namespace RogueTower
{
    abstract class Bullet : GameObject
    {
        public virtual Vector2 Position
        {
            get;
            set;
        }
        public Vector2 BulletSize;
        public Vector2 Velocity;
        public float Frame, FrameEnd;
        public GameObject Shooter;

        public override RectangleF ActivityZone => World.Bounds;

        protected Bullet(GameWorld world, Vector2 position, Vector2 size) : base(world)
        {
            BulletSize = size;
            Create(position.X, position.Y);
        }

        public virtual void Create(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        protected override void UpdateDelta(float delta)
        {
            Frame += delta;
            Position += Velocity * delta;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
                Destroy();
        }

        protected void HandleDamage()
        {
            foreach (var box in World.FindBoxes(new RectangleF(Position - BulletSize / 2, BulletSize)))
            {
                if (Destroyed || Shooter.NoFriendlyFire(box.Data))
                    return;
                if (box.Data is Enemy enemy)
                {
                    ApplyEffect(enemy);
                }
            }
        }

        protected virtual void ApplyEffect(Enemy enemy)
        {
            //NOOP
        }

        public abstract void Draw(SceneGame scene);
    }

    abstract class BulletSolid : Bullet
    {
        public IBox Box;
        public override Vector2 Position
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

        protected BulletSolid(GameWorld world, Vector2 position, Vector2 size) : base(world, position, size)
        {
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x - BulletSize.X / 2, y - BulletSize.Y / 2, BulletSize.X, BulletSize.Y);
            Box.Data = this;
            Box.AddTags(CollisionTag.NoCollision);
        }

        public override void Destroy()
        {
            base.Destroy();
            World.Remove(Box);
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
            //return new CrossResponse(collision);
            return null;
        }

        protected abstract void OnCollision(IHit hit);
    }

    class SpellOrange : BulletSolid
    {
        public SpellOrange(GameWorld world, Vector2 position) : base(world, position, new Vector2(8, 8))
        {
        }

        protected override void OnCollision(IHit hit)
        {
            if (Destroyed || Shooter.NoFriendlyFire(hit.Box.Data))
                return;
            bool explode = false;
            if (hit.Box.Data is Enemy enemy)
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
                    Shooter = Shooter,
                    FrameEnd = 20,
                };
                Destroy();
            }
        }

        public override void Draw(SceneGame scene)
        {
            var magicOrange = SpriteLoader.Instance.AddSprite("content/magic_orange");
            scene.DrawSprite(magicOrange, (int)Frame, Position - magicOrange.Middle, SpriteEffects.None, 0);
        }
    }

    class SnakeSpit : BulletSolid
    {
        public SnakeSpit(GameWorld world, Vector2 position) : base(world, position, new Vector2(8, 8))
        {
        }

        protected override void UpdateDiscrete()
        {
            base.UpdateDiscrete();

            if (Velocity.Y < 10)
                Velocity.Y = Math.Min(Velocity.Y + 0.2f, 10);
        }

        protected override void OnCollision(IHit hit)
        {
            if (Destroyed || Shooter.NoFriendlyFire(hit.Box.Data))
                return;
            bool explode = false;
            if (hit.Box.Data is Enemy enemy)
            {
                explode = true;
            }
            if (hit.Box.Data is Tile tile)
            {
                explode = true;
            }
            if (explode)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene)
        {
            var snakeSpit = SpriteLoader.Instance.AddSprite("content/snake_poison");
            scene.DrawSprite(snakeSpit, 0, Position - snakeSpit.Middle, SpriteEffects.None, 0);
        }
    }

    class Explosion : Bullet
    {
        public override RectangleF ActivityZone => World.Bounds;

        public Explosion(GameWorld world, Vector2 position, Vector2 size) : base(world,position, size)
        {
            
        }

        public Explosion(GameWorld world, Vector2 position) : this(world, position, new Vector2(16,16))
        {
            PlaySFX(sfx_explosion1, 1.0f, 0.1f, 0.2f);
        }

        protected override void UpdateDiscrete()
        {
            base.UpdateDiscrete();

            if (Frame < FrameEnd / 2)
                HandleDamage();
        }

        protected override void ApplyEffect(Enemy enemy)
        {
            enemy.Hit(new Vector2(Math.Sign(enemy.Position.X - Position.X), -2), 20, 50, 45);
        }

        public override void Draw(SceneGame scene)
        {
            var spriteExplosion = SpriteLoader.Instance.AddSprite("content/explosion");
            scene.DrawSprite(spriteExplosion, scene.AnimationFrame(spriteExplosion, Frame, FrameEnd), Position - spriteExplosion.Middle, SpriteEffects.None, 0);
        }
    }

    class PoisonBreath : Explosion
    {
        public PoisonBreath(GameWorld world, Vector2 position) : base(world, position)
        {
        }

        protected override void ApplyEffect(Enemy enemy)
        {
            enemy.Health = Math.Max(enemy.Health - 1, 1);
        }

        public override void Draw(SceneGame scene)
        {
            var breathPoison = SpriteLoader.Instance.AddSprite("content/breath_poison");
            scene.DrawSpriteExt(breathPoison, scene.AnimationFrame(breathPoison, Frame, FrameEnd), Position - breathPoison.Middle, breathPoison.Middle, (float)Math.Atan2(Velocity.X, Velocity.Y), SpriteEffects.None, 0);
        }
    }

    class Fireball : Bullet
    {
        public Fireball(GameWorld world, Vector2 position) : base(world, position, new Vector2(8, 8))
        {
        }

        protected override void UpdateDiscrete()
        {
            base.UpdateDiscrete();

            HandleDamage();
        }

        protected override void ApplyEffect(Enemy enemy)
        {
            enemy.Hit(new Vector2(Math.Sign(Velocity.X), -2), 20, 50, 20);
        }

        public override void Draw(SceneGame scene)
        {
            var fireball = SpriteLoader.Instance.AddSprite("content/fireball");
            scene.DrawSpriteExt(fireball, 0, Position - fireball.Middle, fireball.Middle, -MathHelper.PiOver2 * (int)(Frame * 0.5), SpriteEffects.None, 0);
        }
    }

    class Knife : BulletSolid
    {
        double knifeDamage = 15.0;

        public Knife(GameWorld world, Vector2 position) : base(world, position, new Vector2(8, 8))
        {
        }

        protected override ICollisionResponse GetCollision(ICollision collision)
        {
            if (Shooter.NoFriendlyFire(collision.Hit.Box.Data))
                return null;
                //return new CrossResponse(collision);
            return new TouchResponse(collision);
        }

        protected override void OnCollision(IHit hit)
        {
            if (Destroyed || Shooter.NoFriendlyFire(hit.Box.Data))
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

        public override void Draw(SceneGame scene)
        {
            var knife = SpriteLoader.Instance.AddSprite("content/knife");
            scene.DrawSpriteExt(knife, 0, Position - knife.Middle, knife.Middle, (float)Math.Atan2(Velocity.Y, Velocity.X), SpriteEffects.None, 0);
        }
    }

    class Shockwave : BulletSolid
    {
        public float ShockwaveForce;
        public float ScalingFactor = 0;

        public Shockwave(GameWorld world, Vector2 position, float velocityDown) : base(world, position, new Vector2(8, (int)(16 * velocityDown / 5)))
        {
            ScalingFactor = velocityDown;
            ShockwaveForce = (velocityDown >= 1) ? 20 * velocityDown : 20;
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

        protected override void UpdateDiscrete()
        {
            base.UpdateDiscrete();
            var tilesLeft = World.FindTiles(new RectangleF(Box.Bounds.Left,Box.Bounds.Y+1,1, Box.Bounds.Height));
            var tilesRight = World.FindTiles(new RectangleF(Box.Bounds.Right, Box.Bounds.Y+1, 1, Box.Bounds.Height));
            if (((Velocity.X < 0 && !tilesLeft.Any()) || (Velocity.X > 0 && !tilesRight.Any())) && !Destroyed)
            {
                Destroy();
            }
        }

        protected override void OnCollision(IHit hit)
        {
            if (Destroyed || Shooter.NoFriendlyFire(hit.Box.Data) || hit.Box.Data is Bullet)
                return;
            bool hitwall = true;

            if (hit.Box.Data is Tile tile)
            {
                tile.HandleTileDamage(ShockwaveForce);
                if (tile.CanDamage == false && !Destroyed)
                {
                    Destroy();
                }
                ShockwaveForce -= 20;
            }
            if (hit.Box.Data is Enemy enemy)
            {
                if (enemy.CanDamage)
                    hitwall = false;
                enemy.Hit(new Vector2(Math.Sign(Velocity.X), -2), 20, 50, ShockwaveForce);
                ShockwaveForce -= 20;
            }
            if (hitwall)
            {
                PlaySFX(sfx_sword_bink, 1.0f, 0.1f, 0.3f);
            }
            if(ShockwaveForce <= 0 && !Destroyed)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene)
        {
            var spriteShockwave = SpriteLoader.Instance.AddSprite("content/shockwave");
            scene.DrawSpriteExt(spriteShockwave, (int)Frame, Position - new Vector2(spriteShockwave.Middle.X, spriteShockwave.Height) + new Vector2(0, Box.Height / 2), new Vector2(spriteShockwave.Middle.X, spriteShockwave.Height), 0, new Vector2(1, (ScalingFactor > 1) ? 1 + ScalingFactor / 5 : 1), SpriteEffects.None, Color.White, 0);
        }
    }
}

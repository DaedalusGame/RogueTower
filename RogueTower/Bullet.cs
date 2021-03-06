﻿using ChaiFoxes.FMODAudio;
using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Effects;
using RogueTower.Effects.Particles;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using static RogueTower.Game;

namespace RogueTower
{
    abstract class Bullet : GameObject, IParryGiver
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

        public bool CheckFriendlyFire(object hit)
        {
            return Shooter != null && Shooter.NoFriendlyFire(hit);
        }

        protected void HandleDamage()
        {
            foreach (var box in World.FindBoxes(new RectangleF(Position - BulletSize / 2, BulletSize)))
            {
                if (Destroyed || CheckFriendlyFire(box.Data))
                    return;
                if (box.Data is Enemy enemy && enemy.CanHit)
                {
                    ApplyEffect(enemy);
                }
            }
        }

        public bool Parry(RectangleF hitmask)
        {
            var affectedHitboxes = World.FindBoxes(hitmask);
            foreach (Box Box in affectedHitboxes)
            {
                if (Box.Data is IParryReceiver receiver && Util.Parry(this, receiver, hitmask))
                {
                    return true;
                }
            }

            return false;
        }

        public void ParryGive(IParryReceiver receiver, RectangleF box)
        {
            //NOOP
        }

        protected virtual void ApplyEffect(Enemy enemy)
        {
            //NOOP
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Bullet;
        }
    }

    abstract class BulletSolid : Bullet, IParryReceiver
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
        public virtual bool CanParry => false;

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

        public void ParryReceive(IParryGiver giver, RectangleF box)
        {
            //NOOP
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
            if (Destroyed || CheckFriendlyFire(hit.Box.Data))
                return;
            if (hit.Box.Data is Enemy enemy && enemy.CanHit)
            {
                Explode();
            }
            if(hit.Box.Data is Tile tile)
            {
                Explode();
            }
        }

        private void Explode()
        {
            new ScreenShakeRandom(World, 5, 10);
            new Explosion(World, Position)
            {
                Shooter = Shooter,
                FrameEnd = 20,
            };
            new Ring(World, Position, 0, 64, new Color(50, 50, 50), new Color(0, 0, 0), 15);
            Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var magicOrange = SpriteLoader.Instance.AddSprite("content/magic_orange");
            scene.DrawSprite(magicOrange, (int)Frame, Position - magicOrange.Middle, SpriteEffects.None, 0);
        }
    }
    
    class SpellAzure : BulletSolid
    {
        public SpellAzure(GameWorld world, Vector2 position) : base(world, position, new Vector2(12, 12))
        {
        }

        protected override void OnCollision(IHit hit)
        {
            if (Destroyed || CheckFriendlyFire(hit.Box.Data))
                return;
            if (hit.Box.Data is Enemy enemy && enemy.CanHit)
            {
                enemy.AddStatusEffect(new Slow(enemy, 0.5f, 1000));
                Destroy();
            }
            if (hit.Box.Data is Tile tile)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            scene.PushSpriteBatch(blendState: BlendState.Additive, shader: scene.Shader, shaderSetup: (matrix) =>
            {
                scene.SetupColorMatrix(ColorMatrix.TwoColor(new Color(0, 16, 0), new Color(16, 64, 255)), matrix);
            });
            var magicSparkle = SpriteLoader.Instance.AddSprite("content/magic_sparkle");
            int sparkles = 3;
            for (int i = 0; i < sparkles; i++)
            {
                scene.DrawSprite(magicSparkle, (int)(Frame + (float)i / sparkles), Position - magicSparkle.Middle + Util.AngleToVector(MathHelper.TwoPi * (Frame / 10f + (float)i / sparkles)) * 4, SpriteEffects.None, 0);
            }
            scene.PopSpriteBatch();
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
            if (Destroyed || CheckFriendlyFire(hit.Box.Data))
                return;
            bool explode = false;
            if (hit.Box.Data is Enemy enemy && enemy.CanHit)
            {
                enemy.Hit(Vector2.Zero, 1, 0, 0);
                enemy.AddStatusEffect(new Stun(enemy, 30));
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

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var snakeSpit = SpriteLoader.Instance.AddSprite("content/snake_poison");
            scene.DrawSprite(snakeSpit, 0, Position - snakeSpit.Middle, SpriteEffects.None, 0);
        }
    }

    class Explosion : Bullet
    {
        public override RectangleF ActivityZone => World.Bounds;
        public virtual Sound Sound { get; set; } = sfx_explosion1;
        public virtual float SoundVolume { get; set; } = 1;

        public SoundChannel SoundChannel;
        public int HurtTime;
        public int Invincibility;
        public double Damage;

        public Explosion(GameWorld world, Vector2 position, Vector2 size, int hurttime = 20, int invincibility = 50, double damage = 45) : base(world,position, size)
        {
            HurtTime = hurttime;
            Invincibility = invincibility;
            Damage = damage;
            PlaySoundOnStart();
        }

        public Explosion(GameWorld world, Vector2 position, int hurttime = 20, int invincibility = 50, double damage = 45) : this(world, position, new Vector2(16,16))
        {
            HurtTime = hurttime;
            Invincibility = invincibility;
            Damage = damage;
            PlaySoundOnStart();
        }

        public void PlaySoundOnStart()
        {
            SoundChannel = PlaySFX(Sound, SoundVolume);
            SoundChannel.Pitch = Random.NextFloat(1f, 1.25f);
        }

        protected override void UpdateDiscrete()
        {
            base.UpdateDiscrete();

            if (Frame < FrameEnd / 2)
                HandleDamage();
        }

        protected override void ApplyEffect(Enemy enemy)
        {
            enemy.Hit(new Vector2(Math.Sign(enemy.Position.X - Position.X), -2), HurtTime, Invincibility, Damage);
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var spriteExplosion = SpriteLoader.Instance.AddSprite("content/explosion");
            scene.DrawSprite(spriteExplosion, scene.AnimationFrame(spriteExplosion, Frame, FrameEnd), Position - spriteExplosion.Middle, SpriteEffects.None, 0);
        }
    }

    class PoisonBreath : Explosion
    {
        public override Sound Sound => sfx_breath;
        public PoisonBreath(GameWorld world, Vector2 position) : base(world, position)
        {
        }

        protected override void ApplyEffect(Enemy enemy)
        {
            enemy.AddStatusEffect(new Poison(enemy, 1000));
            //enemy.Health = Math.Max(enemy.Health - 1, 1);
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var breathPoison = SpriteLoader.Instance.AddSprite("content/breath_poison");
            scene.DrawSpriteExt(breathPoison, scene.AnimationFrame(breathPoison, Frame, FrameEnd), Position - breathPoison.Middle, breathPoison.Middle, (float)Math.Atan2(Velocity.X, Velocity.Y)+MathHelper.Pi, SpriteEffects.None, 0);
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

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var fireball = SpriteLoader.Instance.AddSprite("content/fireball");
            scene.DrawSpriteExt(fireball, 0, Position - fireball.Middle, fireball.Middle, -MathHelper.PiOver2 * (int)(Frame * 0.5), SpriteEffects.None, 0);
        }
    }

    class FireballBig : Bullet
    {
        public FireballBig(GameWorld world, Vector2 position) : base(world, position, new Vector2(12, 12))
        {
        }

        protected override void UpdateDiscrete()
        {
            base.UpdateDiscrete();

            HandleDamage();

            if ((int) Frame % 2 == 0)
            {
                float angle = Random.NextFloat() * MathHelper.TwoPi;
                float dist = Random.NextFloat() * 6;
                new FireEffect(World, Position + Util.AngleToVector(angle) * dist, Util.VectorToAngle(-Velocity), Random.NextFloat() * 15 + 3);
            }
        }

        protected override void ApplyEffect(Enemy enemy)
        {
            enemy.Hit(new Vector2(Math.Sign(Velocity.X), -2), 20, 50, 20);
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var fireball = SpriteLoader.Instance.AddSprite("content/fireball_big");
            var middle = new Vector2(8, 4);
            scene.DrawSpriteExt(fireball, (int)Frame, Position - middle, middle, Util.VectorToAngle(Velocity), SpriteEffects.None, 0);
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
            if (CheckFriendlyFire(collision.Hit.Box.Data))
                return null;
                //return new CrossResponse(collision);
            return new TouchResponse(collision);
        }

        protected override void OnCollision(IHit hit)
        {
            if (Destroyed || CheckFriendlyFire(hit.Box.Data) || hit.Box.Data is Bullet)
                return;
            bool bounced = true;
            
            if (hit.Box.Data is Tile tile)
                tile.HandleTileDamage(knifeDamage);
            if (hit.Box.Data is Enemy enemy && enemy.CanHit)
            {
                bool parried = Util.Parry(this, enemy, Box.Bounds);
                if (!parried)
                {
                    if (enemy.CanDamage)
                        bounced = false;
                    enemy.Hit(new Vector2(Math.Sign(Velocity.X), -2), 20, 50, knifeDamage);
                }
            }
            if (bounced)
            {
                new KnifeBounced(World, Position, new Vector2(Math.Sign(Velocity.X) * -1.5f, -3f), MathHelper.Pi * 0.3f, 24);
                PlaySFX(sfx_sword_bink, 1.0f, 0.1f, 0.3f);
            }
            Destroy();
        }

        public override void Draw(SceneGame scene, DrawPass pass)
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
            ShockwaveForce = (velocityDown >= 1) ? (float)Math.Floor(20 * velocityDown) : 20;
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
            if (Destroyed || CheckFriendlyFire(hit.Box.Data) || hit.Box.Data is Bullet)
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
            if (hit.Box.Data is Enemy enemy && enemy.CanHit)
            {
                if (enemy.CanDamage)
                    hitwall = false;
                enemy.Hit(new Vector2(Math.Sign(Velocity.X), -2), 20, 50, Math.Floor(ShockwaveForce * 0.20));
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

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var spriteShockwave = SpriteLoader.Instance.AddSprite("content/shockwave");
            scene.DrawSpriteExt(spriteShockwave, (int)Frame, Position - new Vector2(spriteShockwave.Middle.X, spriteShockwave.Height) + new Vector2(0, Box.Height / 2), new Vector2(spriteShockwave.Middle.X, spriteShockwave.Height), 0, new Vector2(1, (ScalingFactor > 1) ? 1 + ScalingFactor / 5 : 1), SpriteEffects.None, Color.White, 0);
        }
    }

    class BoomerangProjectile : BulletSolid
    {
        public float Angle;
        public float Lifetime; //How long before this returns?
        public bool Bounced = false;
        public WeaponBoomerang Boomerang;
        public BoomerangProjectile(GameWorld world, Vector2 position, float lifetime, WeaponBoomerang boomerang) : base(world, position, new Vector2(8, 8))
        {
            Lifetime = lifetime;
            Boomerang = boomerang;
            FrameEnd = float.PositiveInfinity;
        }

        protected override ICollisionResponse GetCollision(ICollision collision)
        {
            if(Bounced || Lifetime < 0)
            {
                if (collision.Box.Data is Tile tile)
                {
                    return null;
                }
            }
            return base.GetCollision(collision);
        }

        protected override void UpdateDiscrete()
        {
            base.UpdateDiscrete();
            if (Bounced || Lifetime < 0)
            {
                if (Shooter is Enemy human)
                {
                    var diffX = human.Position.X - Position.X;
                    var diffY = human.Position.Y - Position.Y;
                    var diffVector = Vector2.Normalize(new Vector2(diffX, diffY)) * 5;
                    Velocity = diffVector;
                    if (Math.Abs(diffX) < 4 && Math.Abs(diffY) < 4)
                    {
                        Destroy();
                    }
                }
                else
                {
                    Destroy();
                }
            }
        }
        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Lifetime -= delta;
            if(Velocity.X != 0)
                Angle += (Math.Sign(Velocity.X) * delta) * 0.5f;
            else
            {
                Angle -= (Math.Sign(Velocity.Y) * delta) * 0.5f;
            }
        }
        protected override void OnCollision(IHit hit)
        {
            if (Destroyed || CheckFriendlyFire(hit.Box.Data) || hit.Box.Data is Bullet)
                return;
            
            if(hit.Box.Data is Enemy enemy)
            {
                bool parried = Util.Parry(this, enemy, Box.Bounds);
                if (!parried)
                {
                    if (enemy.CanDamage)
                    {
                        enemy.AddStatusEffect(new Stun(enemy, 120));
                        enemy.Hit(Velocity, 0, 20, Boomerang.Damage);
                    }
                    Bounced = true;
                }
            }
            if(hit.Box.Data is Tile tile)
            {
                if (tile.CanDamage)
                {
                    tile.HandleTileDamage(Boomerang.Damage);
                }
                Bounced = true;
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var spriteBoomerang = SpriteLoader.Instance.AddSprite("content/boomerang");
            scene.DrawSpriteExt(spriteBoomerang, 0, Position - spriteBoomerang.Middle, spriteBoomerang.Middle, Angle, SpriteEffects.None, 0);
        }
    }
}

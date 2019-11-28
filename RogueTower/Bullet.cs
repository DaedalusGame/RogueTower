﻿using Humper;
using Humper.Responses;
using Microsoft.Xna.Framework;
using System;
using static RogueTower.Game;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    abstract class Bullet : GameObject
    {
        public GameWorld World;
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
        public float LifeTime;
        public GameObject Shooter;

        public override void Destroy()
        {
            base.Destroy();
            World.Remove(Box);
        }

        public void Create(GameWorld world, float x, float y)
        {
            World = world;
            Box = World.Create(x-4, y-4, 8, 8);
            Box.Data = this;
            Box.AddTags(CollisionTag.NoCollision);
        }

        protected override void UpdateDelta(float delta)
        {
            LifeTime -= delta;
            var move = Box.Move(Box.X + Velocity.X * delta, Box.Y + Velocity.Y * delta, collision =>
            {
                return GetCollision(collision);
            });
            foreach(var hit in move.Hits)
                OnCollision(hit);
        }

        protected virtual ICollisionResponse GetCollision(ICollision collision)
        {
            return new CrossResponse(collision);
        }

        protected abstract void OnCollision(IHit hit);

        protected override void UpdateDiscrete()
        {
            if (LifeTime <= 0)
                Destroy();
        }
    }

    class Knife : Bullet
    {
        double knifeDamage = 15.0;
        protected override ICollisionResponse GetCollision(ICollision collision)
        {
            if(collision.Hit.Box.HasTag(CollisionTag.NoCollision))
                return new CrossResponse(collision);
            return new TouchResponse(collision);
        }

        protected override void OnCollision(IHit hit)
        {
            if (hit.Box.HasTag(CollisionTag.NoCollision) || hit.Box.Data == Shooter)
                return;
            World.Objects.Add(new KnifeBounced(Position, new Vector2(Math.Sign(Velocity.X) * -1.5f, -3f), MathHelper.Pi * 0.3f, 24));
            if (hit.Box.Data is Tile tile)
                tile.HandleTileDamage(knifeDamage);
            PlaySFX(sfx_sword_bink, 1.0f, 0.1f, 0.3f);
            Destroy();
        }
    }
}

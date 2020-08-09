using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RogueTower.Game;
using static RogueTower.Util;


namespace RogueTower.Enemies
{
    class CannonFire : Cannon
    {
        public CannonFire(GameWorld world, Vector2 position, float angle) : base(world, position, angle)
        {
        }

        protected override void Reset()
        {
            IdleTime = 50;
            ChargeTime = 30;
            FireTime = 60;
        }

        protected override void ShootStart()
        {
        }

        protected override void ShootTick()
        {
            if ((int)FireTime % 5 == 0)
                new Fireball(World, Position + FacingVector * 8 + VarianceVector * (Random.NextFloat() - 0.5f) * 12)
                {
                    Velocity = FacingVector * (Random.NextFloat() * 0.5f + 0.5f),
                    FrameEnd = 80,
                    Shooter = this,
                };
        }

        protected override void ShootEnd()
        {

        }
    }
}

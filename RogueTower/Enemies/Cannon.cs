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
    abstract class Cannon : Enemy
    {
        public enum FireState
        {
            Idle,
            Charge,
            Fire,
        }

        public override bool CanParry => false;
        public override bool Incorporeal => false;
        public override Vector2 HomingTarget => Position;
        public override Vector2 PopupPosition => Position;
        public override bool Dead => false;

        public FireState State;
        public float Angle = 0;
        public float DelayTime;
        public float IdleTime;
        public float ChargeTime;
        public float FireTime;

        public Vector2 FacingVector => AngleToVector(Angle);
        public Vector2 VarianceVector => AngleToVector(Angle + MathHelper.PiOver2);

        public Cannon(GameWorld world, Vector2 position, float angle) : base(world, position)
        {
            Angle = angle;
            Reset();
        }

        protected abstract void Reset();

        protected abstract void ShootStart();

        protected abstract void ShootTick();

        protected abstract void ShootEnd();

        protected override void UpdateDelta(float delta)
        {
            Lifetime += delta;

            DelayTime -= delta;
            if (DelayTime <= 0)
            {
                switch (State)
                {
                    case (FireState.Idle):
                        IdleTime -= delta;
                        if (IdleTime <= 0)
                            State = FireState.Charge;
                        break;
                    case (FireState.Charge):
                        ChargeTime -= delta;
                        if (ChargeTime <= 0)
                        {
                            if (Active)
                                ShootStart();
                            State = FireState.Fire;
                        }
                        break;
                    case (FireState.Fire):
                        FireTime -= delta;
                        if (FireTime <= 0)
                        {
                            if (Active)
                                ShootEnd();
                            Reset();
                            State = FireState.Idle;
                        }
                        break;
                }
            }
        }

        protected override void UpdateDiscrete()
        {
            if (State == FireState.Fire && Active)
            {
                ShootTick();
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var wallGun = SpriteLoader.Instance.AddSprite("content/wall_gun");
            var wallGunBase = SpriteLoader.Instance.AddSprite("content/wall_gun_base");
            scene.DrawSpriteExt(wallGun, 0, Position - wallGun.Middle + VisualOffset(), wallGun.Middle, Angle, SpriteEffects.None, 1);
        }
    }

}

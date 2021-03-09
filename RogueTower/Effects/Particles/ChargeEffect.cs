using ChaiFoxes.FMODAudio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Actions.Attack;
using RogueTower.Enemies;

namespace RogueTower.Effects.Particles
{
    class ChargeEffect : Particle
    {
        public override Vector2 Position
        {
            get
            {
                return Human.Position;
            }
            set
            {
                //NOOP
            }
        }

        public float Angle;
        public float FrameEnd;
        public EnemyHuman Human;
        SoundChannel Sound;

        public ChargeEffect(GameWorld world, Vector2 position, float angle, float time, EnemyHuman human) : base(world, position)
        {
            Angle = angle;
            FrameEnd = time;
            Human = human;
            Sound = Game.sfx_player_charging.Play();
            Sound.Pitch = 0;
            Sound.Looping = true;
        }

        public override void Destroy()
        {
            base.Destroy();
            StopSound();
        }

        private void StopSound()
        {
            Sound.Looping = false;
            Sound.Stop();
        }

        public override void Update(float delta)
        {
            if (!(Human.CurrentAction is ActionCharge))
            {
                Destroy();
            }
            base.Update(1.0f);
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Sound.Pitch = (float)LerpHelper.CircularOut(0f, 2f, Frame / FrameEnd);
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var charge = SpriteLoader.Instance.AddSprite("content/charge");
            scene.DrawSpriteExt(charge, (int)-Frame, Position - charge.Middle, charge.Middle, Angle, SpriteEffects.None, 0);
        }
    }
}

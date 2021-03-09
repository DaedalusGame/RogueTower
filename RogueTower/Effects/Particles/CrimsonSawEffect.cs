using ChaiFoxes.FMODAudio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Actions.Attack.AlchemicalOrbs.Red;
using RogueTower.Enemies;
using static RogueTower.Util;

namespace RogueTower.Effects.Particles
{
    class CrimsonSawEffect : Particle
    {
        public float Angle;
        public ActionCrimsonSaw SawAction;
        public EnemyHuman Human;
        SoundChannel Sound;
        public override Vector2 Position
        {
            get
            {
                return Human.Position - new Vector2(8, 8) + Human.Pose.GetWeaponOffset(Human.Facing.ToMirror()) + AngleToVector(Human.Pose.Weapon.GetAngle(Human.Facing.ToMirror())) * 24;
            }
            set
            {
                //NOOP
            }
        }
        public CrimsonSawEffect(GameWorld world, float angle, ActionCrimsonSaw sawAction) : base(world, Vector2.Zero)
        {
            Angle = angle;
            SawAction = sawAction;
            Human = SawAction.Human;
            Sound = Game.sfx_sawblade.Play();
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
            if (!(Human.CurrentAction is ActionCrimsonSaw))
            {
                Destroy();
            }
            base.Update(1.0f);
            Sound.Pitch = SawAction.Pitch;
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var sawsprite = SpriteLoader.Instance.AddSprite("content/orb_red_saw");
            scene.DrawSpriteExt(sawsprite, 0, Position - sawsprite.Middle, sawsprite.Middle, Angle, SpriteEffects.None, 0);
        }
    }
}

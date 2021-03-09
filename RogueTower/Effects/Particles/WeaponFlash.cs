using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Enemies;

namespace RogueTower.Effects.Particles
{
    class WeaponFlash : Particle
    {
        public float FrameEnd;
        public EnemyHuman Human;

        public override Vector2 Position
        {
            get
            {
                return Human.Position - new Vector2(8, 8) + Human.Pose.GetWeaponOffset(Human.Facing.ToMirror()) + Human.Pose.Weapon.GetOffset(Human.Facing.ToMirror(), 1.0f);
            }
            set
            {
                //NOOP
            }
        }

        public WeaponFlash(EnemyHuman human, float time) : base(human.World, Vector2.Zero)
        {
            Human = human;
            FrameEnd = time;
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
            var flash = SpriteLoader.Instance.AddSprite("content/flash");
            float size = MathHelper.Lerp(1.0f, 0.0f, Frame / FrameEnd);
            scene.DrawSpriteExt(flash, 0, Position - flash.Middle, flash.Middle, Frame * 0.1f, new Vector2(size), SpriteEffects.None, Color.White, 0);
        }
    }
}

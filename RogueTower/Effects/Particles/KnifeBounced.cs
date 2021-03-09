using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueTower.Effects.Particles
{
    class KnifeBounced : Particle
    {
        public Vector2 Velocity;
        public float FrameEnd;
        public float Rotation;

        public KnifeBounced(GameWorld world, Vector2 position, Vector2 velocity, float rotation, float time) : base(world, position)
        {
            Velocity = velocity;
            Rotation = rotation;
            FrameEnd = time;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            Position += Velocity * delta;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
            Velocity += new Vector2(0, 0.4f);
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var knife = SpriteLoader.Instance.AddSprite("content/knife");
            scene.DrawSpriteExt(knife, 0, Position - knife.Middle, knife.Middle, Rotation * Frame, SpriteEffects.None, 0);
        }
    }
}

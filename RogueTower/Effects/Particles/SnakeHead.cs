using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueTower.Effects.Particles
{
    class SnakeHead : Particle
    {
        public Vector2 Velocity;
        public SpriteEffects Mirror;
        public float Rotation;
        public float FrameEnd;

        public SnakeHead(GameWorld world, Vector2 position, Vector2 velocity, SpriteEffects mirror, float rotation, float time) : base(world, position)
        {
            Velocity = velocity;
            FrameEnd = time;
            Mirror = mirror;
            Rotation = rotation;
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
            var snakeHeadOpen = SpriteLoader.Instance.AddSprite("content/snake_open");
            scene.DrawSpriteExt(snakeHeadOpen, 0, Position - snakeHeadOpen.Middle, snakeHeadOpen.Middle, Rotation * Frame, Mirror, 0);
        }
    }
}

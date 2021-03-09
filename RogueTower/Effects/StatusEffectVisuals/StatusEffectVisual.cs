using Microsoft.Xna.Framework;
using RogueTower.Enemies;

namespace RogueTower.Effects.StatusEffectVisuals
{
    abstract class StatusEffectVisual<T> : VisualEffect where T : StatusEffect
    {
        protected T Effect;
        protected Enemy Enemy => Effect.Enemy;
        protected Vector2 HeadPosition
        {
            get
            {
                if (Enemy is EnemyHuman enemy)
                    return new Vector2(enemy.Box.Bounds.Center.X, enemy.Box.Bounds.Top - 16);
                else
                    return Enemy.HomingTarget - new Vector2(0, 16);
            }
        }
        protected Vector2 Position => Enemy.HomingTarget;

        public StatusEffectVisual(GameWorld world, T effect) : base(world)
        {
            Effect = effect;
        }

        protected override void UpdateDiscrete()
        {
            if (Effect.Removed || Effect.Enemy.Destroyed)
            {
                Destroy();
            }
        }
    }
}

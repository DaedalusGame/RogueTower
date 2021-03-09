using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Actions;
using RogueTower.Enemies;

namespace RogueTower.Effects.Particles
{
    class AimingReticule : Particle
    {
        public float FrameEnd;
        public ActionBase Action;
        public EnemyHuman Player => Action.Human;
        public AimingReticule(GameWorld world, Vector2 position, ActionBase action) : base(world, position)
        {
            FrameEnd = float.PositiveInfinity;
            Action = action;
        }

        protected override void UpdateDelta(float delta)
        {
            base.UpdateDelta(delta);
            if (Player.CurrentAction != Action)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var aimingreticule = SpriteLoader.Instance.AddSprite("content/aim_reticule");
            scene.DrawSpriteExt(aimingreticule, 0, Position - aimingreticule.Middle, aimingreticule.Middle, 0, SpriteEffects.None, 0);
        }
    }
}

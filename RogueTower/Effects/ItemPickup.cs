using Microsoft.Xna.Framework;
using RogueTower.Items;
using System;

namespace RogueTower.Effects
{
    class ItemPickup : VisualEffect
    {
        Item Item;
        Vector2 PositionWorld;
        Vector2 PositionBag;
        float FrameEnd;
        Vector2 Offset;

        public ItemPickup(GameWorld world, Item item, Vector2 position, Vector2 positionBag, float time) : base(world)
        {
            Item = item;
            PositionWorld = position;
            PositionBag = positionBag;
            FrameEnd = time;
            Offset = Util.AngleToVector(Random.NextFloat() * MathHelper.TwoPi) * 40;
        }

        public override void Update(float delta)
        {
            base.Update(1.0f);
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
            var posBag = Vector2.Transform(PositionBag, Matrix.Invert(scene.WorldTransform));
            var offset = Offset * (float)Math.Sin(Frame / FrameEnd * Math.PI);
            Item.DrawIcon(scene, Vector2.Lerp(PositionWorld, posBag, (float)LerpHelper.CircularIn(0, 1, Frame / FrameEnd)) + offset);
        }
    }
}

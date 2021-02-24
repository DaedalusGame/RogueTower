using Microsoft.Xna.Framework;
using RogueTower.Actions.Movement;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using System;
using static RogueTower.Game;

namespace RogueTower.Actions.Attack
{
    class ActionShockwave : ActionPlunge
    {
        public float TaggedVelocity;
        public bool ShockwaveFinished = false;
        public int ShockwaveCount;
        public ActionShockwave(EnemyHuman player, float plungeStartTime, float plungeFinishTime, Weapon weapon, int shockwaveCount = 2) : base(player, plungeStartTime, plungeFinishTime, weapon)
        {
            PlungeStartTime = plungeStartTime;
            PlungeFinishTime = plungeFinishTime;
            Weapon = weapon;
            ShockwaveCount = shockwaveCount;
        }

        public override void UpdateDiscrete()
        {
            if (PlungeStartTime <= 0)
                Human.Velocity.Y += 0.5f;
            double damageIn = Math.Floor(Weapon.Damage * 1.5);
            HandleDamage(damageIn);
            if (Human.OnGround)
            {
                PlungeFinished = true;
                float? floorY = null;
                foreach (var box in Human.World.FindBoxes(Human.Box.Bounds.Offset(0, 1)))
                {
                    if (box.Data is Tile tile)
                    {
                        if (!floorY.HasValue || box.Bounds.Top < floorY)
                            floorY = box.Bounds.Top;
                        tile.HandleTileDamage(damageIn);
                    }
                }
                //Console.WriteLine(TaggedVelocity);
                if (!ShockwaveFinished && floorY.HasValue)
                {
                    for (int i = 0; i < ShockwaveCount; i++)
                    {
                        var speed = 3 + (i >> 1) * 1.25f;
                        var shockwave = new Shockwave(Human.World, new Vector2(Human.Position.X, floorY.Value - (int)(16 * TaggedVelocity / 5) / 2f), TaggedVelocity)
                        {
                            Velocity = new Vector2((-1 + (i & 1) * 2) * speed, 0),
                            FrameEnd = 70,
                            Shooter = Human
                        };
                    }
                    new ScreenShakeRandom(Human.World, 15, 5);
                    PlaySFX(sfx_explosion1, 1f, 0.01f, 0.2f);
                    new ParryEffect(Human.World, Human.Position, 0, 5);
                    ShockwaveFinished = true;
                }
            }
            else
            {
                if (Human.Velocity.Y > TaggedVelocity)
                    TaggedVelocity = Human.Velocity.Y;
            }
            if (PlungeFinished && PlungeFinishTime <= 0)
                Human.CurrentAction = new ActionIdle(Human);
        }
    }
}

using Humper.Base;
using Microsoft.Xna.Framework;
using RogueTower.Actions.Movement;
using RogueTower.Effects;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using static RogueTower.Game;

namespace RogueTower.Actions.Attack
{
    class ActionPlunge : ActionAttack
    {
        public Weapon Weapon;
        public float PlungeStartTime;
        public float PlungeFinishTime;
        public bool PlungeFinished = false;

        public override bool HasGravity => false;
        public override bool Done => PlungeFinished;
        public override bool CanParry => !PlungeFinished;

        public ActionPlunge(EnemyHuman player, float plungeStartTime, float plungeFinishTime, Weapon weapon) : base(player)
        {
            PlungeStartTime = plungeStartTime;
            PlungeFinishTime = plungeFinishTime;
            Weapon = weapon;
        }

        public override void OnInput()
        {
        }

        public override void ParryGive(IParryReceiver receiver)
        {

        }

        public override void ParryReceive(IParryGiver giver)
        {

        }

        protected void HandleDamage(double damageIn)
        {
            var hitsize = new Vector2(8, 8);
            hitsize.X *= Weapon.WidthModifier;
            hitsize.Y *= Weapon.LengthModifier;
            var hitmask = RectangleF.Centered(Human.Position + new Vector2(0, 8 + hitsize.Y * 0.5f), hitsize);
            if (SceneGame.DebugMasks)
                new RectangleDebug(Human.World, hitmask, new Color(Color.Lime, 0.5f), 1);
            foreach (var box in Human.World.FindBoxes(hitmask))
            {
                if (box.Data is Enemy enemy && box.Data != Human)
                    enemy.Hit(new Vector2(0, 2), 20, 50, damageIn);
            }
        }

        public override void UpdateDiscrete()
        {
            double damageIn = Weapon.Damage * 1.5;
            if (PlungeStartTime <= 0)
                Human.Velocity.Y = 5;
            HandleDamage(damageIn);
            if (Human.OnGround)
            {
                Human.Velocity.Y = -4;
                Human.OnGround = false;
                Human.World.Hitstop = 4;
                PlaySFX(sfx_sword_bink, 1.0f, 0.1f, 0.4f);
                Human.CurrentAction = new ActionJump(Human, true, false);
                PlungeFinished = true;
                foreach (var box in Human.World.FindBoxes(Human.Box.Bounds.Offset(0, 1)))
                {
                    if (box.Data is Tile tile)
                        tile.HandleTileDamage(damageIn);
                }
            }
            if (PlungeFinished && PlungeFinishTime <= 0)
                Human.CurrentAction = new ActionIdle(Human);
        }

        public override void UpdateDelta(float delta)
        {
            if (PlungeFinished)
                PlungeFinishTime -= delta;
            else
                PlungeStartTime -= delta;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Head = HeadState.Down;
            basePose.Body = BodyState.Crouch(1);
            basePose.LeftArm = ArmState.Angular(4);
            basePose.RightArm = ArmState.Angular(2);
            basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(90));
        }
    }
}

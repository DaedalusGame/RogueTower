using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Effects.Particles;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using static RogueTower.Util;

namespace RogueTower.Actions.Attack
{
    class ActionPunch : ActionAttack
    {
        public enum PunchState
        {
            PunchStart,
            PunchEnd
        }

        public Weapon Weapon;
        public PunchState PunchAction;
        public float PunchStartTime;
        public float PunchFinishTime;

        public override bool Done => PunchAction == PunchState.PunchEnd;
        public override bool CanParry => PunchAction == PunchState.PunchStart;

        public ActionPunch(EnemyHuman human, float punchStartTime, float punchFinishTime, Weapon weapon) : base(human)
        {
            PunchStartTime = punchStartTime;
            PunchFinishTime = punchFinishTime;
            Weapon = weapon;
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (PunchAction)
            {
                case (PunchState.PunchStart):
                    basePose.Head = HeadState.Down;
                    basePose.RightArm = ArmState.Angular(MathHelper.ToRadians(180));
                    basePose.Body = BodyState.Kneel;
                    break;
                case (PunchState.PunchEnd):
                    basePose.RightArm = ArmState.Forward;
                    basePose.Body = BodyState.Stand;
                    break;
            }

        }

        public override void UpdateDelta(float delta)
        {
            switch (PunchAction)
            {
                case (PunchState.PunchStart):
                    PunchStartTime -= delta;
                    if (PunchStartTime < 0)
                    {
                        Punch();
                    }
                    break;
                case (PunchState.PunchEnd):
                    PunchFinishTime -= delta;
                    if (PunchFinishTime < 0)
                    {
                        Human.ResetState();
                    }
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
        }

        public override void ParryGive(IParryReceiver receiver)
        {
            PunchVisual();
            PunchAction = PunchState.PunchEnd;
        }

        public override void ParryReceive(IParryGiver giver)
        {
            PunchVisual();
            PunchAction = PunchState.PunchEnd;
        }

        public virtual void Punch()
        {
            Vector2 weaponSize = Weapon.WeaponSize;
            RectangleF weaponMask = new RectangleF(Human.Position
                + GetFacingVector(Human.Facing) * 8
                + GetFacingVector(Human.Facing) * (weaponSize.X / 2)
                + new Vector2(0, 1)
                - weaponSize / 2f,
                weaponSize);
            Human.SwingWeapon(weaponMask, Weapon.Damage);
            PunchVisual();
            PunchAction = PunchState.PunchEnd;
            //new RectangleDebug(Human.World, weaponMask, Color.Red, 10);
        }

        public virtual void PunchVisual()
        {
            new PunchEffectStraight(Human.World, () => Human.Position, 1, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
        }
    }
}

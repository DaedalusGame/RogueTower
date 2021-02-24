using Microsoft.Xna.Framework;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using static RogueTower.Game;
using static RogueTower.Util;

namespace RogueTower.Actions.Attack
{
    class ActionStealWeapon : ActionAttack
    {
        public enum StealState
        {
            StealStart,
            StealFinish
        }

        public StealState StealAction;
        public EnemyHuman Victim;
        public float StealStartTime;
        public float StealEndTime;

        public override bool Done => StealAction == StealState.StealFinish;
        public override bool CanParry => StealAction == StealState.StealStart;

        public ActionStealWeapon(EnemyHuman thief, EnemyHuman victim, float stealStartTime, float stealEndTime) : base(thief)
        {
            Victim = victim;
            StealStartTime = stealStartTime;
            StealEndTime = stealEndTime;
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (StealAction)
            {
                case (StealState.StealStart):
                    basePose.Body = BodyState.Kneel;
                    basePose.LeftArm = ArmState.Forward;
                    basePose.Head = HeadState.Down;
                    break;
                case (StealState.StealFinish):
                    basePose.Body = BodyState.Stand;
                    basePose.LeftArm = ArmState.Up;
                    basePose.Head = HeadState.Backward;
                    break;
            }
        }

        public override void UpdateDelta(float delta)
        {
            switch (StealAction)
            {
                case (StealState.StealStart):
                    StealStartTime -= delta;
                    if (StealStartTime < 0)
                    {
                        Steal();
                    }
                    break;

                case (StealState.StealFinish):
                    StealEndTime -= delta;
                    if (StealEndTime < 0)
                    {
                        Human.ResetState();
                    }
                    break;
            }
        }

        public override void ParryGive(IParryReceiver receiver)
        {
            Steal();
        }

        public override void ParryReceive(IParryGiver giver)
        {
            Steal();
        }

        private void Steal()
        {
            if (!(Victim.Weapon is WeaponUnarmed) && !(Victim.Weapon is null) && Victim.Invincibility <= 0)
            {
                Human.Weapon = Victim.Weapon;
                Victim.Weapon = new WeaponUnarmed(10, new Vector2(14, 10));
                Victim.Hit(GetFacingVector(Human.Facing) * 0.5f, 30, 30, 0);
                new ParryEffect(Human.World, Victim.Position, 0, 10);
                PlaySFX(sfx_player_disappointed, 1.0f);
            }
            StealAction = StealState.StealFinish;
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
    }
}

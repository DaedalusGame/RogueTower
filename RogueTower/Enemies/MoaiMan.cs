using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using RogueTower.Actions;
using RogueTower.Actions.Attack;
using RogueTower.Actions.Death;
using RogueTower.Actions.Hurt;
using RogueTower.Actions.Movement;
using RogueTower.Items;
using RogueTower.Items.Weapons;
using System;
using System.Linq;
using static RogueTower.Util;

namespace RogueTower.Enemies
{
    class MoaiMan : EnemyHuman
    {
        public AIEnemyHuman AI;
        public override float Acceleration => 0.25f;
        public override float SpeedLimit => AI.InCombat ? 3.0f : 1.0f;
        public override bool Strafing => true;

        //public override bool Attacking => CurrentAction is ActionAttack;



        public MoaiMan(GameWorld world, Vector2 position) : base(world, position)
        {
            //Weapon = new WeaponWandOrange(10, 16, new Vector2(8, 32));
            //Weapon = new WeaponUnarmed(10, 14, new Vector2(7, 28));
            AI = new AIEnemyHuman(this);
            Weapon = Weapon.PresetWeaponList[Random.Next(0, Weapon.PresetWeaponList.Length - 1)];
            //Weapon = new WeaponKatana(15, new Vector2(10, 40));
            //Weapon = new WeaponRapier(15, new Vector2(10, 40));
            //Weapon = new WeaponAlchemicalGauntlet(10, new Vector2(6, 4));
            CurrentAction = new ActionIdle(this);
            InitHealth(80);
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x, y, 12, 14);
            Box.Data = this;
            Box.AddTags(CollisionTag.Character);
        }



        protected override void HandleInput()
        {
            AI.UpdateAI();
        }

        protected override void UpdateDelta(float delta)
        {
            if (Active)
            {
                base.UpdateDelta(delta);
            }
        }

        protected override void UpdateDiscrete()
        {
            if (Active)
            {
                base.UpdateDiscrete();
            }
        }

        public override PlayerState GetBasePose()
        {
            PlayerState pose = new PlayerState(
                HeadState.Forward,
                BodyState.Stand,
                ArmState.Angular(5),
                ArmState.Angular(3),
                Weapon.GetWeaponState(this, MathHelper.ToRadians(270 - 20)),
                ShieldState.None
            );
            Weapon.GetPose(this, pose);
            return pose;
        }

        public override void SetPhenoType(PlayerState pose)
        {
            pose.Head.SetPhenoType("moai");
            pose.LeftArm.SetPhenoType("moai");
            pose.RightArm.SetPhenoType("moai");
        }

        public override void Death()
        {
            base.Death();
            if (!(CurrentAction is ActionEnemyDeath))
                CurrentAction = new ActionEnemyDeath(this, 20);
        }

        public override void DropItems(Vector2 position)
        {
            new DroppedItem(World, position, Meat.Moai).Spread();
            new DroppedItem(World, position, new CurseMedal()).Spread();
            if (Random.NextDouble() > 0.75)
            {
                new DroppedItem(World, position, Weapon).Spread();
            }
        }

        public override void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            base.Hit(velocity, hurttime, invincibility / 10, damageIn);
        }
    }

}

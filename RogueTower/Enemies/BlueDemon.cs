using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RogueTower.Game;
using static RogueTower.Util;


namespace RogueTower.Enemies
{
    class BlueDemon : EnemyHuman
    {
        public BlueDemon(GameWorld world, Vector2 position) : base(world, position)
        {
            Weapon = new WeaponFireSword(20, new Vector2(10, 40));
            CurrentAction = new ActionIdle(this);
            InitHealth(80);
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x, y, 12, 14);
            Box.Data = this;
            Box.AddTags(CollisionTag.Character);
        }

        public override PlayerState GetBasePose()
        {
            PlayerState pose = new PlayerState(
                HeadState.Forward,
                BodyState.Stand,
                ArmState.Neutral,
                ArmState.Neutral,
                WeaponState.None,
                ShieldState.None
            );
            Weapon.GetPose(this, pose);
            return pose;
        }

        public override void SetPhenoType(PlayerState pose)
        {
            pose.Head.SetPhenoType("demon_blue");
            pose.Body.SetPhenoType("armor");
            pose.Body.Color = new Color(255, 128, 75);
            pose.LeftArm.SetPhenoType("armor");
            pose.RightArm.SetPhenoType("armor");
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

        public override void Death()
        {
            base.Death();
            if (!(CurrentAction is ActionEnemyDeath))
                CurrentAction = new ActionEnemyDeath(this, 20);
        }

        public override void DropItems(Vector2 position)
        {
        }

        public override void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            base.Hit(velocity, hurttime, invincibility / 10, damageIn);
        }
    }

}

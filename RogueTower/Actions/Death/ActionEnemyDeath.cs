using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChaiFoxes.FMODAudio;
using static RogueTower.Game;
using static RogueTower.Util;
using RogueTower.Enemies;
using RogueTower.Effects.Particles;

namespace RogueTower.Actions.Death
{
    class ActionEnemyDeath : ActionBase
    {
        int Time;

        public override float Friction => 1;
        public override float Drag => 1;
        public override bool Incorporeal => true;

        public ActionEnemyDeath(EnemyHuman player, int time) : base(player)
        {
            Time = time;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Head = HeadState.Down;
            basePose.Body = BodyState.Hit;
            basePose.RightArm = ArmState.Angular(3);
        }

        public override void OnInput()
        {
            //NOOP
        }

        public override void UpdateDelta(float delta)
        {
            //NOOP
        }

        public override void UpdateDiscrete()
        {
            Time--;
            if (Time <= 0)
            {
                Cleanup();
            }
            Vector2 pos = GetRandomPosition(Human.Box.Bounds, Human.Random);
            new FireEffect(Human.World, pos, 0, 5);
        }

        protected virtual void Cleanup()
        {
            Human.Destroy();
        }
    }
}

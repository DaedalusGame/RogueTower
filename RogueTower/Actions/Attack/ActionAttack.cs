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
using RogueTower.Actions.Movement;

namespace RogueTower.Actions.Attack
{
    abstract class ActionAttack : ActionBase
    {
        public bool Parried;

        public abstract bool Done
        {
            get;
        }

        public override float Friction => Parried ? 1 : base.Friction;
        public override float Drag => 1 - (1 - base.Drag) * 0.1f;
        public override bool CanParry => true;

        public ActionAttack(EnemyHuman player) : base(player)
        {
        }

        public abstract void ParryGive(IParryReceiver receiver);

        public abstract void ParryReceive(IParryGiver giver);

        public override void OnInput()
        {
            var player = (Player)Human;
            if (Done)
                HandleSlashInput(player);
            if (Parried)
                HandleExtraJump(player);
        }
    }
}

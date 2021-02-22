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

namespace RogueTower.Actions.Death
{
    class ActionPlayerDeath : ActionEnemyDeath
    {
        public ActionPlayerDeath(EnemyHuman player, int time) : base(player, time)
        {
        }

        protected override void Cleanup()
        {
            Human.Position = new Vector2(50, Human.World.Height - 50);
            Human.Velocity = Vector2.Zero;
            Human.ResetState();
            Human.Resurrect();
        }
    }
}

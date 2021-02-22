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

namespace RogueTower.Actions.Attack
{
    class ActionWandBlastHoming : ActionWandBlast
    {
        Enemy Target;

        public override Vector2 TargetOffset => Target.Position - FirePosition;

        public ActionWandBlastHoming(EnemyHuman human, Enemy target, float upTime, float downTime, WeaponWand weapon) : base(human, upTime, downTime, weapon)
        {
            Target = target;
        }
    }
}

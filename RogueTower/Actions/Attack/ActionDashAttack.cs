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
    class ActionDashAttack : ActionDash
    {
        public ActionBase DashAttack;
        public ActionDashAttack(EnemyHuman player, float dashStartTime, float dashTime, float dashEndTime, float dashFactor, bool phasing, bool reversed, ActionBase actionDashAttack) : base(player, dashStartTime, dashTime, dashEndTime, dashFactor, phasing, reversed)
        {
            DashStartTime = dashStartTime;
            DashTime = dashTime;
            DashEndTime = dashEndTime;
            DashFactor = dashFactor;
            Phasing = phasing;
            Reversed = reversed;
            DashAttack = actionDashAttack;
        }

        public override void UpdateDelta(float delta)
        {
            switch (DashAction)
            {
                case (DashState.DashStart):
                    DashStartTime -= delta;
                    if (DashStartTime < 0)
                        DashAction = DashState.Dash;
                    break;
                case (DashState.Dash):
                    DashTime -= delta;
                    Human.Velocity.X = MathHelper.Clamp((GetFacingVector(Human.Facing).X * (Reversed ? -1 : 1)) * DashFactor, -DashFactor, DashFactor);
                    if (DashTime < 0)
                        DashAction = DashState.DashEnd;
                    break;
                case (DashState.DashEnd):
                    DashEndTime -= delta;
                    if (DashEndTime < 0)
                        Human.CurrentAction = DashAttack;
                    break;
            }
        }
    }
}

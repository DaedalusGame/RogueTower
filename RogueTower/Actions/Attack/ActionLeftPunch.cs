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
using RogueTower.Items.Weapons;

namespace RogueTower.Actions.Attack
{
    class ActionLeftPunch : ActionPunch
    {
        public ActionLeftPunch(EnemyHuman human, float punchStartTime, float punchEndTime, Weapon weapon) : base(human, punchStartTime, punchEndTime, weapon)
        {
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (PunchAction)
            {
                case (PunchState.PunchStart):
                    basePose.Head = HeadState.Down;
                    basePose.LeftArm = ArmState.Angular(MathHelper.ToRadians(180));
                    basePose.Body = BodyState.Kneel;
                    break;
                case (PunchState.PunchEnd):
                    basePose.LeftArm = ArmState.Forward;
                    basePose.Body = BodyState.Stand;
                    break;
            }

        }

        public override void Punch()
        {
            Vector2 weaponSize = Weapon.WeaponSize;
            RectangleF weaponMask = new RectangleF(Human.Position
                + GetFacingVector(Human.Facing) * 8
                + GetFacingVector(Human.Facing) * (weaponSize.X / 2)
                + new Vector2(0, 2)
                - weaponSize / 2f,
                weaponSize);
            Human.SwingWeapon(weaponMask, Weapon.Damage);
            PunchVisual();
            PunchAction = PunchState.PunchEnd;
            //new RectangleDebug(Human.World, weaponMask, Color.Red, 10);
        }

        public override void PunchVisual()
        {
            new PunchEffectStraight(Human.World, () => Human.Position + new Vector2(0, 2), 1, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
        }
    }
}

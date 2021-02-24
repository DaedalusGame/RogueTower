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
    class ActionDownStab : ActionStab
    {
        public ActionDownStab(EnemyHuman player, float upTime, float downTime, Weapon weapon) : base(player, upTime, downTime, weapon)
        {
        }

        public override void GetPose(PlayerState basePose)
        {
            switch (SlashAction)
            {
                default:
                case (SwingAction.UpSwing):
                    basePose.Body = BodyState.Crouch(2);
                    basePose.RightArm = ArmState.Angular(6);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(0));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(1);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(0));
                    break;
            }
        }

        public override void SwingVisual(bool parry)
        {
            Vector2 FacingVector = GetFacingVector(Human.Facing);
            float swingSize = 0.5f * Weapon.LengthModifier;
            var effect = new SlashEffectStraight(Human.World, () => Human.Position + new Vector2(0, 2) + FacingVector * 6, swingSize, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
            if (parry)
                effect.Frame = effect.FrameEnd / 2;
            PlaySFX(SwingSound, 1.0f, 0.1f, 0.5f);
        }
    }
}

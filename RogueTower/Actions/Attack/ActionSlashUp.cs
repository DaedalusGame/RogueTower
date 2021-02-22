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
    class ActionSlashUp : ActionSlash
    {
        public ActionSlashUp(EnemyHuman player, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime, Weapon weapon) : base(player, slashStartTime, slashUpTime, slashDownTime, slashFinishTime, weapon)
        {
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch (SlashAction)
            {
                default:
                case (SwingAction.StartSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(6);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(100));
                    break;
                case (SwingAction.UpSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(6);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(125));
                    break;
                case (SwingAction.DownSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-75));
                    break;
                case (SwingAction.FinishSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-75));
                    break;
            }
        }

        public override void SwingVisual(bool parry)
        {
            float swingSize = 0.7f * Weapon.LengthModifier;
            var effect = new SlashEffectRound(Human.World, () => Human.Position, swingSize, MathHelper.ToRadians(45), SpriteEffects.FlipVertically | (Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 4);
            if (parry)
                effect.Frame = effect.FrameEnd / 2;
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
        }
    }
}

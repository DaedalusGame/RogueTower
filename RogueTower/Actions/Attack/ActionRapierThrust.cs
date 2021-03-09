using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Effects.Particles;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using static RogueTower.Game;
using static RogueTower.Util;

namespace RogueTower.Actions.Attack
{
    class ActionRapierThrust : ActionStab
    {
        public int ArmAttackAngle;
        public ActionRapierThrust(EnemyHuman player, float upTime, float downTime, Weapon weapon) : base(player, upTime, downTime, weapon)
        {
            ArmAttackAngle = Human.Weapon.Random.Next(-3, 3);
        }
        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);
            switch (SlashAction)
            {
                default:
                case (SwingAction.UpSwing):
                    basePose.Body = BodyState.Crouch(2);
                    basePose.LeftArm = ArmState.Angular(0);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-45));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.LeftArm = ArmState.Angular(ArmAttackAngle);
                    basePose.RightArm = ArmState.Angular(8);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(0));
                    break;
            }
        }

        public override void SwingVisual(bool parry)
        {
            Vector2 FacingVector = GetFacingVector(Human.Facing);
            float swingSize = 0.5f * Weapon.LengthModifier;
            var effect = new SlashEffectStraight(Human.World, () => Human.Position + FacingVector * 9 + new Vector2(0, ArmAttackAngle * 2), swingSize, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
            if (parry)
                effect.Frame = effect.FrameEnd / 2;
            PlaySFX(SwingSound, 1.0f, 0.1f, 0.5f);
        }
    }
}

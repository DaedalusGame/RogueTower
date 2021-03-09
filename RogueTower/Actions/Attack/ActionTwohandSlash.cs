using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Effects.Particles;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using static RogueTower.Game;

namespace RogueTower.Actions.Attack
{
    class ActionTwohandSlash : ActionAttack
    {
        public enum SwingAction
        {
            UpSwing,
            DownSwing,
        }

        public Weapon Weapon;
        public SwingAction SlashAction;
        public float SlashUpTime;
        public float SlashDownTime;

        public bool IsUpSwing => SlashAction == SwingAction.UpSwing;
        public bool IsDownSwing => SlashAction == SwingAction.DownSwing;
        public override bool Done => IsDownSwing;
        public override bool CanParry => IsUpSwing;

        public ActionTwohandSlash(EnemyHuman human, float upTime, float downTime, Weapon weapon) : base(human)
        {
            SlashUpTime = upTime;
            SlashDownTime = downTime;
            Weapon = weapon;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);
            basePose.Shield = ShieldState.None;

            switch (SlashAction)
            {
                default:
                case (SwingAction.UpSwing):
                    basePose.LeftArm = ArmState.Angular(9);
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-90 - 45));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.LeftArm = ArmState.Angular(5);
                    basePose.RightArm = ArmState.Angular(3);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(45 + 22));
                    break;
            }
        }

        public override void UpdateDelta(float delta)
        {
            switch (SlashAction)
            {
                case (SwingAction.UpSwing):
                    SlashUpTime -= delta;
                    if (SlashUpTime < 0)
                        Swing();
                    break;
                case (SwingAction.DownSwing):
                    SlashDownTime -= delta;
                    if (SlashDownTime < 0)
                        Human.ResetState();
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }

        public override void ParryGive(IParryReceiver receiver)
        {
            SwingVisual(true);
            SlashAction = SwingAction.DownSwing;
        }

        public override void ParryReceive(IParryGiver giver)
        {
            SwingVisual(true);
            SlashAction = SwingAction.DownSwing;
        }

        public virtual void Swing()
        {
            Vector2 WeaponSize = new Vector2(16, 24) * Weapon.LengthModifier;
            RectangleF weaponMask = RectangleF.Centered(Human.Position + new Vector2(Human.Facing.GetX() * 0.5f * WeaponSize.X, 0), WeaponSize);
            if (Human.Weapon.CanParry)
            {
                Vector2 parrySize = new Vector2(22, 22);
                bool success = Human.Parry(weaponMask);
                if (success)
                    Parried = true;
            }
            if (!Parried)
            {
                Human.SwingWeapon(weaponMask, 10);
                SwingVisual(Parried);
                SlashAction = SwingAction.DownSwing;
            }
        }

        private void SwingVisual(bool parried)
        {
            float swingSize = 0.7f * Weapon.LengthModifier;
            var effect = new SlashEffectRound(Human.World, () => Human.Position, swingSize, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
            if (parried)
                effect.Frame = effect.FrameEnd / 2;
        }
    }
}

using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Effects.Particles;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using static RogueTower.Game;
using static RogueTower.Util;

namespace RogueTower.Actions.Attack
{
    class ActionSlash : ActionAttack
    {
        public Weapon Weapon;
        public SwingAction SlashAction;
        public float SlashStartTime;
        public float SlashUpTime;
        public float SlashDownTime;
        public float SlashFinishTime;

        public bool IsUpSwing => SlashAction == SwingAction.UpSwing || SlashAction == SwingAction.StartSwing;
        public bool IsDownSwing => SlashAction == SwingAction.DownSwing || SlashAction == SwingAction.FinishSwing;
        public override bool CanParry => IsUpSwing;

        public override bool Done => IsDownSwing;

        public enum SwingAction
        {
            StartSwing,
            UpSwing,
            DownSwing,
            FinishSwing,
        }

        public ActionSlash(EnemyHuman player, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime, Weapon weapon) : base(player)
        {
            SlashStartTime = slashStartTime;
            SlashUpTime = slashUpTime;
            SlashDownTime = slashDownTime;
            SlashFinishTime = slashFinishTime;
            Weapon = weapon;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch (SlashAction)
            {
                default:
                case (SwingAction.StartSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-90 - 22));
                    break;
                case (SwingAction.UpSwing):
                    basePose.RightArm = ArmState.Angular(11);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-90 - 45));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(4);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(45 + 22));
                    break;
                case (SwingAction.FinishSwing):
                    basePose.RightArm = ArmState.Angular(4);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(45 + 22));
                    break;
            }

        }

        public override void UpdateDelta(float delta)
        {
            switch (SlashAction)
            {
                case (SwingAction.StartSwing):
                    SlashStartTime -= delta;
                    if (SlashStartTime < 0)
                        SlashAction = SwingAction.UpSwing;
                    break;
                case (SwingAction.UpSwing):
                    SlashUpTime -= delta;
                    if (SlashUpTime < 0)
                        Swing();
                    break;
                case (SwingAction.DownSwing):
                    SlashDownTime -= delta;
                    if (SlashDownTime < 0)
                        SlashAction = SwingAction.FinishSwing;
                    break;
                case (SwingAction.FinishSwing):
                    SlashFinishTime -= delta;
                    if (SlashFinishTime < 0)
                        Human.ResetState();
                    break;
            }
        }

        public override void ParryGive(IParryReceiver receiver)
        {
            SwingVisual(true);
            SlashAction = SwingAction.DownSwing;
        }

        public override void ParryReceive(IParryGiver giver)
        {
            SwingVisual(Parried);
            SlashAction = SwingAction.DownSwing;
        }

        public virtual void Swing()
        {
            Vector2 WeaponSize = new Vector2(16, 24) * Weapon.LengthModifier;
            RectangleF weaponMask = RectangleF.Centered(Human.Position + new Vector2(Human.Facing.GetX() * 0.5f * WeaponSize.X, 0), WeaponSize);
            if (Weapon.CanParry == true)
            {
                Vector2 parrySize = new Vector2(22, 22);
                bool success = Human.Parry(new RectangleF(Human.Position + GetFacingVector(Human.Facing) * 8 - parrySize / 2, parrySize));
                if (success)
                    Parried = true;
            }
            if (!Parried)
                Human.SwingWeapon(weaponMask, 10);
            SwingVisual(Parried);
            SlashAction = SwingAction.DownSwing;
        }

        public virtual void SwingVisual(bool parry)
        {
            float swingSize = 0.7f * Weapon.LengthModifier;
            var effect = new SlashEffectRound(Human.World, () => Human.Position, swingSize, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
            if (parry)
                effect.Frame = effect.FrameEnd / 2;
            PlaySFX(sfx_sword_swing, 1.0f, 0.1f, 0.5f);
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }
    }
}

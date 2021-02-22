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
    class ActionStab : ActionAttack
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

        public Sound SwingSound = sfx_sword_stab;

        public bool IsUpSwing => SlashAction == SwingAction.UpSwing;
        public bool IsDownSwing => SlashAction == SwingAction.DownSwing;
        public override bool CanParry => IsUpSwing;

        public override bool Done => IsDownSwing;

        public ActionStab(EnemyHuman player, float upTime, float downTime, Weapon weapon) : base(player)
        {
            SlashUpTime = upTime;
            SlashDownTime = downTime;
            Weapon = weapon;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);

            switch (SlashAction)
            {
                default:
                case (SwingAction.UpSwing):
                    basePose.RightArm = ArmState.Angular(8);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-90 + 22.5f));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.RightArm = ArmState.Angular(0);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(0));
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
            SwingVisual(Parried);
            SlashAction = SwingAction.DownSwing;
        }

        public override void ParryReceive(IParryGiver giver)
        {
            SwingVisual(Parried);
            SlashAction = SwingAction.DownSwing;
        }

        public virtual void Swing()
        {
            float sizeMult = 1;
            Vector2 weaponSize = new Vector2(16, 8);
            weaponSize.X *= Weapon.LengthModifier;
            weaponSize.Y *= Weapon.WidthModifier;
            RectangleF weaponMask = new RectangleF(Human.Position
                + GetFacingVector(Human.Facing) * 8
                + GetFacingVector(Human.Facing) * (weaponSize.X / 2)
                + new Vector2(0, 1)
                - weaponSize / 2f,
                weaponSize);
            /*
            Vector2 PlayerWeaponOffset = Position + FacingVector * 14;
            Vector2 WeaponSize = new Vector2(14 / 2, 14 * 2);
            RectangleF weaponMask = new RectangleF(PlayerWeaponOffset - WeaponSize / 2, WeaponSize);*/
            if (Weapon.CanParry)
            {
                HorizontalFacing Facing = Human.Facing;
                Vector2 Position = Human.Position;
                Vector2 FacingVector = GetFacingVector(Facing);
                Vector2 parrySize = new Vector2(22, 22);
                bool success = Human.Parry(new RectangleF(Position + FacingVector * 8 - parrySize / 2, parrySize));
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

        public virtual void SwingVisual(bool parry)
        {
            Vector2 FacingVector = GetFacingVector(Human.Facing);
            float swingSize = 0.5f * Weapon.LengthModifier;
            var effect = new SlashEffectStraight(Human.World, () => Human.Position + FacingVector * 6, swingSize, 0, Human.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 4);
            if (parry)
                effect.Frame = effect.FrameEnd / 2;
            PlaySFX(SwingSound, 1.0f, 0.1f, 0.5f);
        }
    }
}

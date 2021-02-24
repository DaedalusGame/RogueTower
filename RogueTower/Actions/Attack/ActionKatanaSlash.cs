using Microsoft.Xna.Framework;
using RogueTower.Enemies;
using RogueTower.Items.Weapons;
using static RogueTower.Game;

namespace RogueTower.Actions.Attack
{
    class ActionKatanaSlash : ActionSlash
    {
        public float Angle = 45;
        public float ArmAngle = 0;
        public float InitialTime;
        public WeaponKatana Katana;

        public ActionKatanaSlash(EnemyHuman human, float slashStartTime, float slashUpTime, float slashDownTime, float slashFinishTime, WeaponKatana weapon) : base(human, slashStartTime, slashUpTime, slashDownTime, slashFinishTime, weapon)
        {
            InitialTime = slashFinishTime;
            Katana = weapon;
        }

        public override void GetPose(PlayerState basePose)
        {
            basePose.Body = !Human.InAir ? BodyState.Stand : BodyState.Walk(1);
            Katana.Sheathed = false;
            basePose.Shield = ShieldState.KatanaSheathEmpty(MathHelper.ToRadians(-20));
            basePose.WeaponHold = WeaponHold.Left;
            switch (SlashAction)
            {
                default:
                case (SwingAction.StartSwing):
                    basePose.LeftArm = ArmState.Angular(10);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-135.5f));
                    break;
                case (SwingAction.UpSwing):
                    basePose.LeftArm = ArmState.Angular(13);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(-45));
                    break;
                case (SwingAction.DownSwing):
                    basePose.Body = BodyState.Crouch(1);
                    basePose.LeftArm = ArmState.Angular(1);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(22.5f));
                    break;
                case (SwingAction.FinishSwing):
                    basePose.LeftArm = ArmState.Angular(ArmAngle);
                    basePose.Weapon = Weapon.GetWeaponState(Human, MathHelper.ToRadians(Angle));
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
                    if (Angle < 520)
                    {
                        Angle = MathHelper.Clamp(Angle + (45 * delta), 45, 520);
                        //SwingTwirl();
                    }
                    else
                    {
                        if (SlashFinishTime == InitialTime)
                        {
                            PlaySFX(sfx_clack, 1, 0.45f, 0.5f);
                            new ParryEffect(Human.World, Human.Position, 0, InitialTime);
                        }
                        ArmAngle = MathHelper.Clamp(ArmAngle + 1, 0, 4);
                        SlashFinishTime -= delta;
                        if (SlashFinishTime < 0)
                            Human.ResetState();
                    }
                    break;
            }
        }
        /*public virtual void SwingTwirl()
        {
            RectangleF hitmask = new RectangleF(new Vector2(Human.Box.X - (GetFacingVector(Human.Facing).X * Katana.WeaponSizeMult), Human.Box.Y - (Katana.WeaponSizeMult/2)), 
            new RectangleDebug(Human.World, hitmask, Color.Red, 10);
            if (Weapon.CanParry == true)
            {
                Vector2 parrySize = new Vector2(22, 22);
                bool success = Human.Parry(hitmask);
                if (success)
                    Parried = true;
            }
            if (!Parried)
                Human.SwingWeapon(hitmask, 10);
            SwingVisual(Parried);
        }*/
    }
}

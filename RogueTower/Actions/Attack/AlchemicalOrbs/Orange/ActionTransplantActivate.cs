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
using RogueTower.Effects.Particles;

namespace RogueTower.Actions.Attack.AlchemicalOrbs.Orange
{
    class ActionTransplantActivate : ActionBase
    {
        public enum ActivationState
        {
            Pre,
            Activate
        }

        public ActivationState ActivationAction;
        public float PreActivationTime;
        public float ActivationTime;

        public ActionTransplantActivate(EnemyHuman player, float preActivationTime, float activationTime) : base(player)
        {
            PreActivationTime = preActivationTime;
            ActivationTime = activationTime;
        }

        public override void OnInput()
        {
            //NOOP
        }
        public override void GetPose(PlayerState basePose)
        {
            switch (ActivationAction)
            {
                case (ActivationState.Pre):
                    basePose.RightArm = ArmState.Up;
                    basePose.Weapon = Human.Weapon.GetWeaponState(Human, MathHelper.ToRadians(-90));
                    break;
                case (ActivationState.Activate):
                    basePose.RightArm = ArmState.Forward;
                    break;
            }
        }

        public override void UpdateDiscrete()
        {
            //NOOP
        }

        public override void UpdateDelta(float delta)
        {
            switch (ActivationAction)
            {
                case (ActivationState.Pre):
                    PreActivationTime -= delta;
                    if (PreActivationTime <= 0)
                        ActivationAction = ActivationState.Activate;
                    break;
                case (ActivationState.Activate):
                    ActivationTime -= delta;
                    foreach (var box in Human.World.FindBoxes(RectangleF.Centered(Human.Position, new Vector2(400, 240))))
                    {
                        if (box.Data is Enemy enemy)
                        {
                            Bomb bomb = (Bomb)enemy.StatusEffects.Find(status => status is Bomb);
                            if (bomb != null)
                            {
                                new Explosion(enemy.World, enemy.HomingTarget, new Vector2(16 * bomb.Stacks, 16 * bomb.Stacks), 20, 50, 20 * bomb.Stacks)
                                {
                                    Shooter = Human,
                                    FrameEnd = 20 * bomb.Stacks
                                };
                                new WeaponFlash(Human, 10);
                                PlaySFX(sfx_fingersnap_heavy, 1f);
                                bomb.Remove();
                            }
                        }
                    }
                    if (ActivationTime <= 0)
                        Human.ResetState();
                    break;
            }
        }
    }
}

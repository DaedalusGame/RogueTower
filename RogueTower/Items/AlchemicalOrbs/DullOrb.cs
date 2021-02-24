using Microsoft.Xna.Framework;
using RogueTower.Actions.Attack;
using RogueTower.Actions.Movement;

namespace RogueTower.Items.AlchemicalOrbs
{
    class DullOrb : AlchemicalOrb
    {
        protected DullOrb() : base()
        {

        }

        public DullOrb(Weapons.Weapon weapon) : base(weapon)
        {
            OrbSprite = "orb_sphere";
            OrbColor = Color.SlateGray;
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack)
            {
                if (player.CurrentAction is ActionPunch)
                    player.CurrentAction = new ActionLeftPunch(player, 2, 4, Weapon);
                else
                    player.CurrentAction = new ActionPunch(player, 2, 4, Weapon);
            }
            else if (player.Controls.AltAttack && !(player.CurrentAction is ActionDash))
            {
                player.CurrentAction = new ActionDash(player, 2, 4, 8, 4, false, true);
            }
        }

        protected override Item MakeCopy()
        {
            return new DullOrb();
        }
    }
}

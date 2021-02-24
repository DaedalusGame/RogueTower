using Microsoft.Xna.Framework;
using RogueTower.Actions.Attack.AlchemicalOrbs.Orange;
using RogueTower.Items.Weapons;

namespace RogueTower.Items.AlchemicalOrbs
{
    class OrangeOrb : AlchemicalOrb
    {
        protected OrangeOrb() : base()
        {

        }

        public OrangeOrb(Weapon weapon) : base(weapon)
        {
            OrbSprite = "orb_ring";
            OrbColor = new Color(255, 128, 0, 192);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack && !(player.CurrentAction is ActionTransplantPunch))
            {
                player.CurrentAction = new ActionTransplantPunch(player, 4, 6, Weapon);
            }
            else if (player.Controls.AltAttack && !(player.CurrentAction is ActionTransplantActivate))
            {
                player.CurrentAction = new ActionTransplantActivate(player, 4, 6);
            }
        }

        protected override Item MakeCopy()
        {
            return new OrangeOrb();
        }
    }
}

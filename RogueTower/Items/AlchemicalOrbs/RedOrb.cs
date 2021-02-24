using Microsoft.Xna.Framework;
using RogueTower.Actions.Attack.AlchemicalOrbs.Red;
using RogueTower.Items.Weapons;

namespace RogueTower.Items.AlchemicalOrbs
{
    class RedOrb : AlchemicalOrb
    {
        protected RedOrb() : base()
        {

        }
        public RedOrb(Weapon weapon) : base(weapon)
        {
            OrbSprite = "orb_blade";
            OrbColor = new Color(225, 0, 0, 192);
        }

        public override void HandleAttack(Player player)
        {
            if (player.Controls.Attack && !(player.CurrentAction is ActionCrimsonSaw))
            {
                player.CurrentAction = new ActionCrimsonSaw(player, 5, 10);
            }
        }

        protected override Item MakeCopy()
        {
            return new RedOrb();
        }
    }
}

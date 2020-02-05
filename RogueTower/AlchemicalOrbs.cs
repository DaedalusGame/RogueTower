using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueTower
{
    abstract class AlchemicalOrbs : Item
    {
        public Weapon Weapon;
        public string OrbSprite;
        public Color OrbColor;

        protected AlchemicalOrbs() : base()
        {
        }

        public AlchemicalOrbs(Weapon weapon)
        {
            Weapon = weapon;
        }

        public abstract void HandleAttack(Player player);

        public override void DrawIcon(SceneGame scene, Vector2 position)
        {
            var orbsprite = SpriteLoader.Instance.AddSprite($"content/{OrbSprite}");
            scene.DrawSprite(orbsprite, 0, position - orbsprite.Middle, SpriteEffects.None, 1.0f);
        }
    }

    class DullOrb : AlchemicalOrbs
    {
        protected DullOrb() : base()
        {

        }

        public DullOrb(Weapon weapon) : base(weapon)
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

    class RedOrb : AlchemicalOrbs
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

    class OrangeOrb : AlchemicalOrbs
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

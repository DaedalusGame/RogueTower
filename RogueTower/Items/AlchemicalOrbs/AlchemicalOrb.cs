using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Items.Weapons;

namespace RogueTower.Items.AlchemicalOrbs
{
    abstract class AlchemicalOrb : Item
    {
        public Weapons.Weapon Weapon;
        public string OrbSprite;
        public Color OrbColor;

        protected AlchemicalOrb() : base()
        {
        }

        public AlchemicalOrb(Weapons.Weapon weapon)
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
}

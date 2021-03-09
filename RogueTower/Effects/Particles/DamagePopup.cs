using Microsoft.Xna.Framework;

namespace RogueTower.Effects.Particles
{
    class DamagePopup : Particle
    {
        public float FrameEnd;
        public string Text;
        public Color FontColor;
        public Color BorderColor;
        public Vector2 Offset => new Vector2(0, -16) * (float)LerpHelper.QuadraticOut(0, 1, Frame / FrameEnd);

        public DamagePopup(GameWorld world, Vector2 position, string text, float time, Color? fontColor = null, Color? borderColor = null) : base(world, position)
        {
            Text = text;
            FrameEnd = time;
            FontColor = fontColor ?? Color.White;
            BorderColor = borderColor ?? Color.Black;
        }

        protected override void UpdateDiscrete()
        {
            if (Frame >= FrameEnd)
            {
                Destroy();
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var calcParams = new TextParameters().SetColor(FontColor, BorderColor).SetConstraints(128, 64);
            string fit = FontUtil.FitString(Game.ConvertToSmallPixelText(Text), calcParams);
            var width = FontUtil.GetStringWidth(fit, calcParams);
            var height = FontUtil.GetStringHeight(fit);
            scene.DrawText(fit, Position + Offset - new Vector2(128, height) / 2, Alignment.Center, new TextParameters().SetColor(FontColor, BorderColor).SetConstraints(128, height + 64));
        }
    }
}

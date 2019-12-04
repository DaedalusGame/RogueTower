﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Text;
using System.Collections.Generic;
using ChaiFoxes.FMODAudio;

namespace RogueTower
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager Graphics;
        public SpriteBatch SpriteBatch;

        public Texture2D Pixel;
        public Effect Shader;

        public static Sound bgm_title_theme;
        public static Sound sfx_knife_throw;
        public static Sound sfx_impact_blunt;
        public static Sound sfx_player_hurt;
        public static Sound sfx_player_jump;
        public static Sound sfx_player_land;
        public static Sound sfx_sword_swing;
        public static Sound sfx_sword_bink;
        public static Sound sfx_tile_break;
        public static Sound sfx_tile_icebreak;

        public Scene Scene;

        public int Frame;

        byte[] NoiseAlpha = new byte[256];
        byte[] NoiseBeta = new byte[256];

        public MouseState LastMouseState;
        public MouseState MouseState;

        public KeyboardState LastKeyState;
        public KeyboardState KeyState;

        const int FontSpritesAmount = 64;
        SpriteReference[] FontSprites = new SpriteReference[FontSpritesAmount];

        FrameCounter FPS = new FrameCounter();
        FrameCounter GFPS = new FrameCounter();
        public int HeightTraversed;

        public Game()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public int GetNoiseValue(int x, int y)
        {
            int xa = Util.PositiveMod(x, 16);
            int ya = Util.PositiveMod(y, 16);
            int xb = Util.PositiveMod(Util.FloorDiv(x, 16), 16);
            int yb = Util.PositiveMod(Util.FloorDiv(y, 16), 16);

            return NoiseAlpha[xa + ya * 16] + NoiseBeta[xb + yb * 16];
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Random noiseRandom = new Random(0);
            noiseRandom.NextBytes(NoiseAlpha);
            noiseRandom.NextBytes(NoiseBeta);

            SpriteLoader.Init(GraphicsDevice);

            RenderTarget2D pixel = new RenderTarget2D(GraphicsDevice, 1, 1);
            GraphicsDevice.SetRenderTarget(pixel);
            GraphicsDevice.Clear(Color.White);
            GraphicsDevice.SetRenderTarget(null);
            Pixel = pixel;

            LoadFont();

            SceneGame sceneGame = new SceneGame(this);

            Scene = sceneGame;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Shader = Content.Load<Effect>("effects");

            AudioMgr.Init("content");
            bgm_title_theme =  AudioMgr.LoadStreamedSound("sounds/bgm/generic_theme.ogg");

            sfx_knife_throw = AudioMgr.LoadSound("sounds/sfx/knife_throw.wav");
            sfx_impact_blunt = AudioMgr.LoadSound("sounds/sfx/impact_blunt.wav");
            sfx_player_hurt = AudioMgr.LoadSound("sounds/sfx/player_hurt.wav");
            sfx_player_jump = AudioMgr.LoadSound("sounds/sfx/jump_sfx.wav");
            sfx_player_land = AudioMgr.LoadSound("sounds/sfx/player_land.wav");
            sfx_sword_bink = AudioMgr.LoadSound("sounds/sfx/sword_bink.wav");
            sfx_sword_swing = AudioMgr.LoadSound("sounds/sfx/sword_swing.wav");
            sfx_tile_break = AudioMgr.LoadSound("sounds/sfx/tilebreak_default.wav");
            sfx_tile_icebreak = AudioMgr.LoadSound("sounds/sfx/icetile_swordbreak.wav");

            var musicChannel = bgm_title_theme.Play();
            musicChannel.Looping = true;
            //MediaPlayer.Play(bgm_title_theme);
            //MediaPlayer.IsRepeating = true;
            // TODO: use this.Content to load your game content here

            // Readies the sounds used in our project.


        }
        
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            AudioMgr.Unload();
            // TODO: Unload any non ContentManager content here
        }

        private void LoadFont()
        {
            for (int i = 0; i < FontSpritesAmount; i++)
            {
                FontSprites[i] = SpriteLoader.Instance.AddSprite("content/font/font_" + i + "_0");
                FontSprites[i].ShouldLoad = true;
                int fontIndex = i;
                FontSprites[i].SetLoadFunction(() => LoadFontPart(FontSprites[fontIndex], fontIndex));
            }

            FontUtil.CharInfo[' '] = new CharInfo(0, 4, true);
        }

        private void LoadFontPart(SpriteReference sprite, int index)
        {
            Texture2D tex = sprite.Texture;
            Color[] blah = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(0, new Rectangle(0, 0, tex.Width, tex.Height), blah, 0, blah.Length);

            for (int i = 0; i < FontUtil.CharsPerPage; i++)
            {
                FontUtil.RegisterChar(blah, tex.Width, tex.Height, (char)(index * FontUtil.CharsPerPage + i), i);
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            SpriteLoader.Instance.Update(gameTime);
            AudioMgr.Update();
            MouseState = Mouse.GetState();
            KeyState = Keyboard.GetState();

            Scene.Update(gameTime);

            IsMouseVisible = Scene.ShowCursor;

            LastMouseState = MouseState;
            LastKeyState = KeyState;

            GFPS.Update(gameTime);

            base.Update(gameTime);
        }
        public static Sound PlaySFX(Sound sfx, float volume, float min_pitchmod_val = 0, float max_pitchmod_val = 0)
        {
            Random random = new Random();
            float pitchmodcalc = (float)(random.NextDouble() * (max_pitchmod_val - min_pitchmod_val) + min_pitchmod_val);
            sfx.Volume = volume;
            sfx.Pitch = (float)Math.Pow(2, pitchmodcalc);
            sfx.Play();
            return sfx;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Frame++;

            Scene.Draw(gameTime);

            FPS.Update(gameTime);


            SpriteBatch.Begin(blendState:BlendState.NonPremultiplied);
            DrawText($"FPS: {FPS.AverageFramesPerSecond.ToString("f1")}\nGFPS: {GFPS.AverageFramesPerSecond.ToString("f1")}", new Vector2(0, 0), Alignment.Left, new TextParameters().SetColor(Color.White, Color.Black));
            SpriteBatch.End();

            base.Draw(gameTime);
        }

        public const char PRIVATE_ZONE_BEGIN = (char)0xE000;
        public const char PIXEL_TEXT_ZONE_BEGIN = (char)(PRIVATE_ZONE_BEGIN + 0);
        public const char PIXEL_TEXT_SMALL_ZONE_BEGIN = (char)(PRIVATE_ZONE_BEGIN + 128);
        public const char FORMAT_CODES_BEGIN = (char)(PRIVATE_ZONE_BEGIN + 256);
        public const char FORMAT_BOLD = (char)(FORMAT_CODES_BEGIN + 0);
        public const char FORMAT_ITALIC = (char)(FORMAT_CODES_BEGIN + 1);
        public const char FORMAT_UNDERLINE = (char)(FORMAT_CODES_BEGIN + 2);
        public const char FORMAT_SUBSCRIPT = (char)(FORMAT_CODES_BEGIN + 3);
        public const char FORMAT_SUPERSCRIPT = (char)(FORMAT_CODES_BEGIN + 4);

        public string ConvertToPixelText(string text)
        {
            StringBuilder convertedText = new StringBuilder();

            foreach (char c in text)
            {
                if (c == ' ' || c == '\n')
                    convertedText.Append(c);
                else if (c > 32 && c < 127)
                    convertedText.Append((char)(PIXEL_TEXT_ZONE_BEGIN + c));
                else
                    convertedText.Append((char)(PIXEL_TEXT_ZONE_BEGIN + '?'));
            }

            return convertedText.ToString();
        }

        public string ConvertToSmallPixelText(string text)
        {
            StringBuilder convertedText = new StringBuilder();

            foreach (char c in text)
            {
                if (c == ' ' || c == '\n')
                    convertedText.Append(c);
                else if (c > 32 && c < 127)
                    convertedText.Append((char)(PIXEL_TEXT_SMALL_ZONE_BEGIN + c));
                else
                    convertedText.Append((char)(PIXEL_TEXT_SMALL_ZONE_BEGIN + '?'));
            }

            return convertedText.ToString();
        }

        public void DrawText(string str, Vector2 drawpos, Alignment alignment, TextParameters parameters)
        {
            int lineoffset = 0;
            int totalindex = 0;
            str = FontUtil.FitString(str, parameters);

            foreach (string line in str.Split('\n'))
            {
                if (lineoffset + parameters.LineSeperator > parameters.MaxHeight)
                    break;
                int textwidth = FontUtil.GetStringWidth(line, parameters);
                int offset = (parameters.MaxWidth ?? 0) - textwidth;
                switch (alignment)
                {
                    case (Alignment.Left):
                        offset = 0;
                        break;
                    case (Alignment.Center):
                        offset /= 2;
                        break;
                }
                DrawTextLine(line, drawpos + new Vector2(offset, lineoffset), totalindex, parameters);
                totalindex += line.Length;
                lineoffset += parameters.LineSeperator;
            }
        }

        private void DrawTextLine(string str, Vector2 drawpos, int totalindex, TextParameters parameters)
        {
            int pos = 0;

            foreach (char chr in str)
            {
                if (totalindex > parameters.DialogIndex)
                    break;
                switch (chr)
                {
                    case (FORMAT_BOLD):
                        parameters.Bold = !parameters.Bold;
                        break;
                    case (FORMAT_UNDERLINE):
                        parameters.Underline = !parameters.Underline;
                        break;
                    case (FORMAT_SUBSCRIPT):
                        parameters.ScriptOffset += 8;
                        break;
                    case (FORMAT_SUPERSCRIPT):
                        parameters.ScriptOffset -= 8;
                        break;
                }
                Texture2D tex = FontSprites[chr / FontUtil.CharsPerPage].Texture;
                int index = chr % FontUtil.CharsPerPage;
                int offset = FontUtil.GetCharOffset(chr);
                int width = FontUtil.GetCharWidth(chr);

                var color = parameters.Color(totalindex);
                var border = parameters.Border(totalindex);
                var charOffset = parameters.Offset(totalindex);

                if (border.A > 0)
                { //Only draw outline if it's actually non-transparent
                    SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(pos - offset - 1, parameters.ScriptOffset + 0), FontUtil.GetCharRect(index), border);
                    SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(pos - offset, parameters.ScriptOffset + 1), FontUtil.GetCharRect(index), border);
                    SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(pos - offset, parameters.ScriptOffset - 1), FontUtil.GetCharRect(index), border);
                    if (parameters.Bold)
                    {
                        SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(pos - offset + 2, parameters.ScriptOffset + 0), FontUtil.GetCharRect(index), border);
                        SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(pos - offset + 1, parameters.ScriptOffset + 1), FontUtil.GetCharRect(index), border);
                        SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(pos - offset + 1, parameters.ScriptOffset - 1), FontUtil.GetCharRect(index), border);
                    }
                    else
                    {
                        SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(pos - offset + 1, parameters.ScriptOffset), FontUtil.GetCharRect(index), border);
                    }
                }

                SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(pos - offset, parameters.ScriptOffset), FontUtil.GetCharRect(index), color);
                if (parameters.Bold)
                    SpriteBatch.Draw(tex, drawpos + charOffset + new Vector2(pos - offset + 1, parameters.ScriptOffset), FontUtil.GetCharRect(index), color);
                pos += width + parameters.CharSeperator + (parameters.Bold ? 1 : 0);
                totalindex++;
            }
        }
    }
}

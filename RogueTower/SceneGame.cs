﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Humper.Base;
using static RogueTower.Util;

namespace RogueTower
{
    class WeaponState
    {
        public string Sprite;
        public int Frame;
        public Vector2 Origin;
        public float Angle;

        public WeaponState(string sprite, int frame, Vector2 origin, float angle)
        {
            Sprite = sprite;
            Frame = frame;
            Origin = origin;
            Angle = angle;
        }

        public virtual void Draw(SceneGame game, Vector2 position, SpriteEffects mirror, float depth)
        {
            SpriteReference sprite = SpriteLoader.Instance.AddSprite($"content/{Sprite}", false);

            float angle = Angle;
            if (mirror == SpriteEffects.FlipHorizontally)
            {
                angle = Util.MirrorAngle(angle);
                mirror = SpriteEffects.FlipVertically;
            }

            game.DrawSpriteExt(sprite, Frame, position - Origin, Origin, angle, mirror, depth);
        }

        public class NoneState : WeaponState
        {
            public NoneState() : base("", 0, Vector2.Zero, 0)
            {
            }

            public override void Draw(SceneGame game, Vector2 position, SpriteEffects mirror, float depth)
            {
                //NOOP
            }
        }

        public static WeaponState None => new NoneState();
        public static WeaponState Sword(float angle) => new WeaponState("sword", 0, new Vector2(4, 4), angle);
        public static WeaponState Katana(float angle) => new WeaponState("katana", 0, new Vector2(18, 1), angle);
        public static WeaponState Knife(float angle) => new WeaponState("knife", 0, new Vector2(4, 4), angle);
        public static WeaponState Lance(float angle) => new WeaponState("lance", 0, new Vector2(4, 4), angle);
        public static WeaponState Rapier(float angle) => new WeaponState("rapier", 0, new Vector2(5, 3), angle);
        public static WeaponState WandOrange(float angle) => new WeaponState("wand_orange", 0, new Vector2(2, 4), angle);
        public static WeaponState Warhammer(float angle) => new WeaponState("warhammer", 0, new Vector2(14, 6), angle);
    }

    class ArmState
    {
        public enum Type
        {
            Left,
            Right,
        }

        public string Pose;
        public string PhenoType = "char";
        public int Frame;
        public Vector2[] HoldOffsetLeft;
        public Vector2[] HoldOffsetRight;

        public ArmState(string pose, int frame, Vector2[] holdOffsetLeft, Vector2[] holdOffsetRight)
        {
            Pose = pose;
            Frame = frame;
            HoldOffsetLeft = holdOffsetLeft;
            HoldOffsetRight = holdOffsetRight;
        }

        public ArmState(string pose, int frame, Vector2 holdOffsetLeft, Vector2 holdOffsetRight) : this(pose, frame, new[] { holdOffsetLeft }, new[] { holdOffsetRight })
        {
        }

        public ArmState SetPhenoType(string phenoType)
        {
            PhenoType = phenoType;
            return this;
        }

        public Vector2 GetHoldOffset(Type type)
        {
            switch (type)
            {
                default:
                case Type.Left:
                    return HoldOffsetLeft[Frame % HoldOffsetLeft.Length];
                case Type.Right:
                    return HoldOffsetRight[Frame % HoldOffsetRight.Length];
            }
        }

        private string GetTypeString(Type type)
        {
            switch (type)
            {
                default:
                case Type.Left:
                    return "l";
                case Type.Right:
                    return "r";
            }
        }

        public virtual void Draw(SceneGame game, Type type, Vector2 position, SpriteEffects mirror, float depth)
        {
            SpriteReference sprite = SpriteLoader.Instance.AddSprite($"content/{PhenoType}_{GetTypeString(type)}arm_{Pose}", true);

            game.DrawSprite(sprite, Frame, position, mirror, depth);
        }

        public static ArmState Neutral => new ArmState("neutral", 0, new Vector2(13, 10), new Vector2(3, 10));
        public static ArmState Forward => new ArmState("forward", 0, new Vector2(15, 9), new Vector2(8, 9));
        public static ArmState Pray => new ArmState("pray", 0, new Vector2(10, 10), new Vector2(8, 10));
        public static ArmState Up => new ArmState("up", 0, new Vector2(13, 2), new Vector2(4, 2));
        public static ArmState Low => new ArmState("low", 0, new Vector2(), new Vector2(7, 10));
        public static ArmState Shield => new ShieldState();
        public static ArmState Angular(int frame) => new ArmState("angular", frame, new[] {
            new Vector2(15, 7),
            new Vector2(15, 9),
            new Vector2(15, 10),
            new Vector2(14, 11),
            new Vector2(10, 11),
            new Vector2(9, 10),
            new Vector2(8, 9),
            new Vector2(8, 7),
            new Vector2(8, 5),
            new Vector2(9, 4),
            new Vector2(10, 3),
            new Vector2(14, 3),
            new Vector2(15, 4),
            new Vector2(15, 5),
        }, new[] {
            new Vector2(9,7),
            new Vector2(9,9),
            new Vector2(8,10),
            new Vector2(7,11),
            new Vector2(3,11),
            new Vector2(2,10),
            new Vector2(1,9),
            new Vector2(1,7),
            new Vector2(1,5),
            new Vector2(2,4),
            new Vector2(3,3),
            new Vector2(7,3),
            new Vector2(8,4),
            new Vector2(9,5),
        });

        public class ShieldState : ArmState
        {
            public ShieldState() : base("shield", 0, new Vector2(13, 8), new Vector2())
            {

            }

            public override void Draw(SceneGame game, Type type, Vector2 position, SpriteEffects mirror, float depth)
            {
                SpriteReference shield = SpriteLoader.Instance.AddSprite($"content/char_shield", true);

                base.Draw(game, type, position, mirror, depth);
                game.DrawSprite(shield, 0, position, mirror, 0.9f);
            }
        }
    }

    class BodyState
    {
        public string Pose;
        public int Frame;
        public Vector2 Offset;
        public string PhenoType = "char";

        public BodyState(string pose, int frame, Vector2 offset)
        {
            Pose = pose;
            Frame = frame;
            Offset = offset;
        }

        public BodyState SetPhenoType(string phenoType)
        {
            PhenoType = phenoType;
            return this;
        }

        public virtual void Draw(SceneGame game, Vector2 position, SpriteEffects mirror, float depth)
        {
            SpriteReference sprite = SpriteLoader.Instance.AddSprite($"content/{PhenoType}_body_{Pose}", true);

            game.DrawSprite(sprite, Frame, position, mirror, depth);
        }

        public static BodyState Stand => new BodyState("walk", 0, Vector2.Zero);
        public static BodyState Walk(int frame) => new BodyState("walk", frame, Vector2.Zero);
        public static BodyState Kneel => new BodyState("kneel", 0, new Vector2(0, 1));
        public static BodyState Hit => new BodyState("hit", 0, Vector2.Zero);
        public static BodyState Crouch(int frame) => new BodyState("crouch", frame, new Vector2(1, 2));
        public static BodyState Climb => new BodyState("climb", 0, new Vector2(4, 0));
    }

    class HeadState
    {
        public string Pose;
        public int Frame;
        public string PhenoType = "char";

        public HeadState(string pose, int frame)
        {
            Pose = pose;
            Frame = frame;
        }

        public HeadState SetPhenoType(string phenoType)
        {
            PhenoType = phenoType;
            return this;
        }

        public virtual void Draw(SceneGame game, Vector2 position, SpriteEffects mirror, float depth)
        {
            SpriteReference sprite = SpriteLoader.Instance.AddSprite($"content/{PhenoType}_head_{Pose}", true);

            game.DrawSprite(sprite, Frame, position + new Vector2(0,16 - sprite.Height), mirror, depth);
        }

        public static HeadState Forward => new HeadState("front", 0);
        public static HeadState Backward => new HeadState("back", 0);
        public static HeadState Down => new HeadState("down", 0);
    }

    class PlayerState
    {
        public HeadState Head;
        public BodyState Body;
        public ArmState LeftArm;
        public ArmState RightArm;
        public WeaponState Weapon;

        public PlayerState(HeadState head, BodyState body, ArmState leftArm, ArmState rightArm, WeaponState weapon)
        {
            Head = head;
            Body = body;
            LeftArm = leftArm;
            RightArm = rightArm;
            Weapon = weapon;
        }
    }

    struct Shear
    {
        public double Lower, Upper;

        public static Shear All => new Shear(double.NegativeInfinity, double.PositiveInfinity);

        public Shear(double lower, double upper)
        {
            Lower = Math.Min(lower, upper);
            Upper = Math.Max(lower, upper);
        }

        public bool Contains(double value)
        {
            return value >= Lower && value <= Upper;
        }

        public override string ToString()
        {
            return $"{Lower}-{Upper}";
        }
    }

    class SceneGame : Scene
    {
        const float ViewScale = 2f;

        public GameWorld World;
        public Map Map => World.Map;

        public Vector2 Camera;
        public Vector2 CameraSize => new Vector2(Viewport.Width / 2, Viewport.Height / 2);
        public Vector2 CameraPosition => FitCamera(Camera - CameraSize / 2);
        Matrix WorldTransform;
        Matrix Projection;

        Shear DepthShear = Shear.All;
        
        bool GameSpeedToggle = false;

        public int HeightTraversed;

        public GameState gameState = GameState.Game;

        Healthbar Health;
        Healthbar HealthShadow;
        public int CurrentWeaponIndex = 0;

        List<Background> Backgrounds;

        private Matrix CreateViewMatrix()
        {
            return Matrix.Identity
                * Matrix.CreateTranslation(new Vector3(-CameraPosition, 0))
                * Matrix.CreateTranslation(new Vector3(-CameraSize / 2, 0)) //These two lines center the character on (0,0)
                * Matrix.CreateScale(ViewScale, ViewScale, 1) //Scale it up by 2
                * Matrix.CreateTranslation(Viewport.Width / 2, Viewport.Height / 2, 0); //Translate the character to the middle of the viewport
        }

        public enum GameState
        {
            Game,
            Paused
        }

        public SceneGame(Game game) : base(game)
        {
            Template.LoadAll();

            World = new GameWorld(100, 800);

            World.Player = new Player(World, new Vector2(50, World.Height - 50));
            World.Player.SetControl(this);
            World.Player.Weapon = Weapon.PresetWeaponList[CurrentWeaponIndex];

            Health = new Healthbar(() => World.Player.Health, LerpHelper.Linear, 10.0);
            HealthShadow = new Healthbar(() => World.Player.Health, LerpHelper.Linear, 1.0);

            Backgrounds = new List<Background>();
            Backgrounds.Add(new Background(this, SpriteLoader.Instance.AddSprite("content/bg_parallax_layer2"), () => new Vector2(10, 10), new Vector2(0.10f, 0.02f)) {XLooping = true, YLooping = true});
            //Backgrounds.Add(new Background(this, SpriteLoader.Instance.AddSprite("content/bg_parallax_layer4"), () => new Vector2(0, World.Height - CameraSize.Y), new Vector2(0.05f, -1f)) { XLooping = true, YLooping = false });
            //Backgrounds.Add(new Background(this, SpriteLoader.Instance.AddSprite("content/bg_parallax_layer1"), () => new Vector2(0, World.Height - CameraSize.Y), new Vector2(0.05f, -1f)) { XLooping = true, YLooping = false });
            //Backgrounds.Add(new Background(this, SpriteLoader.Instance.AddSprite("content/bg_parallax_layer3"), () => new Vector2(0, World.Height - CameraSize.Y), new Vector2(0.20f, -1f)) { XLooping = true, YLooping = false });
            AddGroundBackground(SpriteLoader.Instance.AddSprite("content/bg_parallax_layer4"), new Vector2(0, 192 - 114), new Vector2(-0.2f, 0.4f));
            AddGroundBackground(SpriteLoader.Instance.AddSprite("content/bg_parallax_layer1"), new Vector2(0, 192 - 0), new Vector2(-0.2f, 0.4f));
            AddGroundBackground(SpriteLoader.Instance.AddSprite("content/bg_parallax_layer3"), new Vector2(0, 192 - 33), new Vector2(-0.4f, 0.2f));


            //Backgrounds.Add(new Background(this, SpriteLoader.Instance.AddSprite("content/bg_parallax_layer4"), () => new Vector2(0, 0), new Vector2(-1.0f, -1.0f)) { XLooping = true, YLooping = false });

        }

        public override bool ShowCursor => true;

        private void AddGroundBackground(SpriteReference sprite, Vector2 offset, Vector2 speed)
        {
            Backgrounds.Add(new Background(this, sprite, () => new Vector2(offset.X, World.Height - offset.Y - (World.Height - CameraSize.Y) * speed.Y), speed) { XLooping = true, YLooping = false });
        }

        private Vector2 FitCamera(Vector2 camera)
        {
            //camera = Vector2.Zero;
            if (camera.X < 0)
                camera.X = 0;
            if (camera.Y < 0)
                camera.Y = 0;
            if (camera.X > World.Width - CameraSize.X)
                camera.X = World.Width - CameraSize.X;
            if (camera.Y > World.Height - CameraSize.Y)
                camera.Y = World.Height - CameraSize.Y;
            return camera;
        }

        public override void Update(GameTime gameTime)
        {

            //Pause Menu Updates
            if (gameState == GameState.Game)
            {
                if (KeyState.IsKeyDown(Keys.Tab) && LastKeyState.IsKeyUp(Keys.Tab) || (PadState.IsButtonDown(Buttons.RightStick) && LastPadState.IsButtonUp(Buttons.RightStick)))
                    GameSpeedToggle = !GameSpeedToggle;
                World.Update(GameSpeedToggle ? 0.1f : 1.0f);
                if (PadState.IsButtonDown(Buttons.RightTrigger) && LastPadState.IsButtonUp(Buttons.RightTrigger))
                    {
                    CurrentWeaponIndex = PositiveMod(CurrentWeaponIndex + 1, Weapon.PresetWeaponList.Length);
                    World.Player.Weapon = Weapon.PresetWeaponList[CurrentWeaponIndex];
                }
                else if (PadState.IsButtonDown(Buttons.RightShoulder) && LastPadState.IsButtonUp(Buttons.RightShoulder))
                {
                    CurrentWeaponIndex = PositiveMod(CurrentWeaponIndex - 1, Weapon.PresetWeaponList.Length);
                    World.Player.Weapon = Weapon.PresetWeaponList[CurrentWeaponIndex];
                }
                if ((KeyState.IsKeyDown(Keys.Enter) && LastKeyState.IsKeyUp(Keys.Enter)) || (PadState.IsButtonDown(Buttons.Start) && LastPadState.IsButtonUp(Buttons.Start)))
                {
                    gameState = GameState.Paused;
                }
            }
            else if (gameState == GameState.Paused)
            {
                if ((KeyState.IsKeyDown(Keys.Enter) && LastKeyState.IsKeyUp(Keys.Enter)) || (PadState.IsButtonDown(Buttons.Start) && LastPadState.IsButtonUp(Buttons.Start)))
                {
                    gameState = GameState.Game;
                }
            }
            Health.Update(1.0f);
            HealthShadow.Update(1.0f);
            HeightTraversed = (int)(World.Height - World.Player.Position.Y) / 16;

            UpdateCamera();
        }

        public void UpdateCamera()
        {
            if (!World.Player.Dead)
            {
                Camera.X = World.Player.Position.X;
                float cameraDistance = World.Player.Position.Y - Camera.Y;
                if (cameraDistance > 30)
                    Camera.Y = World.Player.Position.Y - 30;
                if (cameraDistance < -30)
                    Camera.Y = World.Player.Position.Y + 30;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Projection = Matrix.CreateOrthographicOffCenter(0, Viewport.Width, Viewport.Height, 0, 0, -1);
            WorldTransform = CreateViewMatrix();

            IEnumerable<ScreenShake> screenShakes = World.Effects.OfType<ScreenShake>();
            if (screenShakes.Any())
            {
                ScreenShake screenShake = screenShakes.WithMax(effect => effect.Offset.LengthSquared());
                if (screenShake != null)
                    WorldTransform *= Matrix.CreateTranslation(screenShake.Offset.X, screenShake.Offset.Y, 0);
            }

            //Background Gradient
            SpriteBatch.Begin(blendState: BlendState.NonPremultiplied, effect: Shader);
            Color bg1 = new Color(32, 19, 48);
            Color bg2 = new Color(126, 158, 153);
            Shader.CurrentTechnique = Shader.Techniques["Gradient"];
            Shader.Parameters["gradient_topleft"].SetValue(bg1.ToVector4());
            Shader.Parameters["gradient_topright"].SetValue(bg1.ToVector4());
            Shader.Parameters["gradient_bottomleft"].SetValue(bg2.ToVector4());
            Shader.Parameters["gradient_bottomright"].SetValue(bg2.ToVector4());
            Shader.Parameters["WorldViewProjection"].SetValue(Projection);
            SpriteBatch.Draw(Pixel, new Rectangle(0, 0, (int)Viewport.Width, (int)Viewport.Height), Color.White);
            SpriteBatch.End();

            SpriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointWrap, transformMatrix: WorldTransform);
            foreach(Background bg in Backgrounds)
            {
                bg.Draw(SpriteBatch);
            }
            SpriteBatch.End();

            StartNormalBatch();

            Rectangle drawZone = GetDrawZone();

            DrawMapBackground(World.Map);
            DepthShear = new Shear(double.NegativeInfinity, 0.75);
            DrawHuman(World.Player);
            DepthShear = Shear.All;
            DrawMap(World.Map);
            DepthShear = new Shear(0.75, double.PositiveInfinity);
            DrawHuman(World.Player);
            DepthShear = Shear.All;

            var spikeball = SpriteLoader.Instance.AddSprite("content/spikeball");
            var chain = SpriteLoader.Instance.AddSprite("content/chain");
            var snakeHeadOpen = SpriteLoader.Instance.AddSprite("content/snake_open");
            var snakeHeadClosed = SpriteLoader.Instance.AddSprite("content/snake_closed");
            var snakeBody = SpriteLoader.Instance.AddSprite("content/snake_tail");
            var hydraBody = SpriteLoader.Instance.AddSprite("content/hydra_body");
            var wallGun = SpriteLoader.Instance.AddSprite("content/wall_gun");
            var wallGunBase = SpriteLoader.Instance.AddSprite("content/wall_gun_base");

            foreach (GameObject obj in World.Objects.OrderBy(x => x.DrawOrder))
            {
                if(obj is BallAndChain ballAndChain)
                {
                    Vector2 truePosA = Vector2.Transform(ballAndChain.Position, WorldTransform);
                    Vector2 truePosB = Vector2.Transform(ballAndChain.Position + ballAndChain.Offset, WorldTransform);
                    if (!drawZone.Contains(truePosA) && !drawZone.Contains(truePosB))
                        continue;
                    for (float i = 0; i < ballAndChain.Distance; i += 6f)
                    {
                        DrawSprite(chain, 0, ballAndChain.Position + ballAndChain.OffsetUnit * i - chain.Middle, SpriteEffects.None, 0);
                    }
                    DrawSprite(spikeball, 0, ballAndChain.Position + ballAndChain.Offset - spikeball.Middle, SpriteEffects.None, 0);
                }
                if(obj is MoaiMan moaiMan)
                {
                    Vector2 truePos = Vector2.Transform(moaiMan.Position, WorldTransform);
                    if (!drawZone.Contains(truePos))
                        continue;
                    DrawHuman(moaiMan);
                }
                if(obj is Snake snake)
                {
                    Vector2 truePosA = Vector2.Transform(snake.Position, WorldTransform);
                    Vector2 truePosB = Vector2.Transform(snake.Position + snake.Head.Offset, WorldTransform);
                    if (!drawZone.Contains(truePosA) && !drawZone.Contains(truePosB))
                        continue;
                    foreach (var segment in snake.Segments)
                    {
                        if (!snake.CurrentAction.ShouldRenderSegment(segment))
                            continue;
                        if (segment == snake.Head)
                        {
                            SpriteReference sprite;
                            if (snake.CurrentAction.MouthOpen)
                                sprite = snakeHeadOpen;
                            else
                                sprite = snakeHeadClosed;
                            DrawSprite(sprite, 0, snake.Position + segment.Offset - sprite.Middle, snake.Facing == HorizontalFacing.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
                        }
                        else
                        {
                            DrawSprite(snakeBody, 0, snake.Position + segment.Offset - snakeBody.Middle, SpriteEffects.None, 0);
                        }
                    } 
                }
                if(obj is Hydra hydra)
                {
                    DrawSprite(hydraBody, Frame / 20, hydra.Position - hydraBody.Middle + new Vector2(0,-4), hydra.Facing == HorizontalFacing.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
                }
                if (obj is Cannon cannon)
                {
                    DrawSpriteExt(wallGun, 0, cannon.Position - wallGun.Middle, wallGun.Middle, cannon.Angle, SpriteEffects.None, 0);
                }
            }

            var knife = SpriteLoader.Instance.AddSprite("content/knife");
            var crit = SpriteLoader.Instance.AddSprite("content/crit");
            var slash = SpriteLoader.Instance.AddSprite("content/slash_round");
            var stab = SpriteLoader.Instance.AddSprite("content/slash_straight");
            var punchStraight = SpriteLoader.Instance.AddSprite("content/punch");
            var magicOrange = SpriteLoader.Instance.AddSprite("content/magic_orange");
            var spriteExplosion = SpriteLoader.Instance.AddSprite("content/explosion");
            var fireball = SpriteLoader.Instance.AddSprite("content/fireball");
            var fire = SpriteLoader.Instance.AddSprite("content/fire_small");
            var fireBig = SpriteLoader.Instance.AddSprite("content/fire_big");
            var charge = SpriteLoader.Instance.AddSprite("content/charge");
            var spriteShockwave = SpriteLoader.Instance.AddSprite("content/shockwave");
            foreach (Bullet bullet in World.Bullets)
            {
                if (bullet is Knife)
                {
                    DrawSpriteExt(knife, 0, bullet.Position - knife.Middle, knife.Middle, (float)Math.Atan2(bullet.Velocity.Y, bullet.Velocity.X), SpriteEffects.None, 0);
                }
                if (bullet is Fireball)
                {
                    DrawSpriteExt(fireball, 0, bullet.Position - fireball.Middle, fireball.Middle, -MathHelper.PiOver2 * (int)(bullet.Frame * 0.5) , SpriteEffects.None, 0);
                }
                if (bullet is SpellOrange spellOrange)
                {
                    DrawSprite(magicOrange, Frame, bullet.Position - magicOrange.Middle, SpriteEffects.None, 0);
                }
                if (bullet is Explosion explosion)
                {
                    DrawSprite(spriteExplosion, AnimationFrame(spriteExplosion,explosion.Frame,explosion.FrameEnd), explosion.Position - spriteExplosion.Middle, SpriteEffects.None, 0);
                }
                if(bullet is Shockwave shockwave)
                {
                    DrawSpriteExt(spriteShockwave, (int)shockwave.Frame, bullet.Position - new Vector2(spriteShockwave.Middle.X, spriteShockwave.Height) + new Vector2(0,bullet.Box.Height / 2), new Vector2(spriteShockwave.Middle.X, spriteShockwave.Height), 0, new Vector2(1, (shockwave.ScalingFactor > 1) ? 1 + shockwave.ScalingFactor / 5 : 1), SpriteEffects.None, Color.White, 0);
                }
            }

            foreach (VisualEffect effect in World.Effects)
            {
                if (effect is SlashEffectStraight slashEffectStraight)
                {
                    var slashAngle = slashEffectStraight.Angle;
                    if (slashEffectStraight.Mirror.HasFlag(SpriteEffects.FlipHorizontally))
                        slashAngle = -slashAngle;
                    SpriteBatch.Draw(stab.Texture, slashEffectStraight.Position + new Vector2(8, 8) - new Vector2(8, 8), stab.GetFrameRect(Math.Min(stab.SubImageCount - 1, (int)(stab.SubImageCount * slashEffectStraight.Frame / slashEffectStraight.FrameEnd))), Color.White, slashAngle, stab.Middle, slashEffectStraight.Size, slashEffectStraight.Mirror, 0);
                }
                else if(effect is PunchEffectStraight punchEffectStraight)
                {
                    var punchAngle = punchEffectStraight.Angle;
                    if (punchEffectStraight.Mirror.HasFlag(SpriteEffects.FlipHorizontally))
                        punchAngle = -punchAngle;
                    DrawSpriteExt(punchStraight, AnimationFrame(punchStraight, punchEffectStraight.Frame, punchEffectStraight.FrameEnd), punchEffectStraight.Position - punchStraight.Middle, punchStraight.Middle, punchAngle, punchEffectStraight.Mirror, 0);
                }
                else if (effect is SlashEffectRound slashEffectRound)
                {
                    var slashAngle = slashEffectRound.Angle;
                    if (slashEffectRound.Mirror.HasFlag(SpriteEffects.FlipHorizontally))
                        slashAngle = -slashAngle;
                    if (slashEffectRound.Size > 0.2)
                        SpriteBatch.Draw(slash.Texture, slashEffectRound.Position + new Vector2(8, 8) - new Vector2(8, 8), slash.GetFrameRect(Math.Min(slash.SubImageCount - 1, (int)(slash.SubImageCount * slashEffectRound.Frame / slashEffectRound.FrameEnd) - 1)), Color.LightGray, slashAngle, slash.Middle, slashEffectRound.Size - 0.2f, slashEffectRound.Mirror, 0);
                    SpriteBatch.Draw(slash.Texture, slashEffectRound.Position + new Vector2(8, 8) - new Vector2(8, 8), slash.GetFrameRect(Math.Min(slash.SubImageCount - 1, (int)(slash.SubImageCount * slashEffectRound.Frame / slashEffectRound.FrameEnd))), Color.White, slashAngle, slash.Middle, slashEffectRound.Size, slashEffectRound.Mirror, 0);
                }
                if (effect is KnifeBounced knifeBounced)
                {
                    DrawSpriteExt(knife, 0, knifeBounced.Position - knife.Middle, knife.Middle, knifeBounced.Rotation * knifeBounced.Frame, SpriteEffects.None, 0);
                }
                if (effect is SnakeHead snakeHead)
                {
                    DrawSpriteExt(snakeHeadOpen, 0, snakeHead.Position - snakeHeadOpen.Middle, snakeHeadOpen.Middle, snakeHead.Rotation * snakeHead.Frame, snakeHead.Mirror, 0);
                }
                if (effect is ParryEffect parryEffect)
                {
                    DrawSpriteExt(crit, AnimationFrame(crit,parryEffect.Frame,parryEffect.FrameEnd), parryEffect.Position - crit.Middle, crit.Middle, parryEffect.Angle, SpriteEffects.None, 0);
                }
                if (effect is FireEffect fireEffect)
                {
                    var middle = new Vector2(8, 12);
                    DrawSpriteExt(fire, AnimationFrame(fire, fireEffect.Frame, fireEffect.FrameEnd), fireEffect.Position - middle, middle, fireEffect.Angle, SpriteEffects.None, 0);
                }
                if (effect is BigFireEffect bigFireEffect)
                {
                    var middle = new Vector2(8, 12);
                    DrawSpriteExt(fireBig, AnimationFrame(fireBig, bigFireEffect.Frame, bigFireEffect.FrameEnd), bigFireEffect.Position - middle, middle, bigFireEffect.Angle, SpriteEffects.None, 0);
                }
                if (effect is DamagePopup damagePopup)
                {
                    var calcParams = new TextParameters().SetColor(Color.White, Color.Black).SetConstraints(128, 64);
                    string fit = FontUtil.FitString(Game.ConvertToSmallPixelText(damagePopup.Text), calcParams);
                    var width = FontUtil.GetStringWidth(fit, calcParams);
                    var height = FontUtil.GetStringHeight(fit);
                    DrawText(fit, damagePopup.Position + damagePopup.Offset - new Vector2(128,height) / 2, Alignment.Center, new TextParameters().SetColor(Color.White,Color.Black).SetConstraints(128, height + 64));
                }
                if(effect is RectangleDebug rectDebug)
                {
                    SpriteBatch.Draw(Pixel, rectDebug.Rectangle.ToRectangle(), rectDebug.Color);
                }
                if(effect is ChargeEffect chargeEffect)
                {
                    DrawSpriteExt(charge, (int)-chargeEffect.Frame, chargeEffect.Position - charge.Middle, charge.Middle, chargeEffect.Angle, SpriteEffects.None, 0);
                }
            }

            if(Keyboard.GetState().IsKeyDown(Keys.RightControl) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftTrigger)) 
            { 
                foreach(var box in World.Find(World.Bounds))
                {
                    Color debugColor = Color.Red;
                    if (box.Data is Enemy)
                        debugColor = Color.Lime;
                    SpriteBatch.Draw(Pixel, box.Bounds.ToRectangle(), new Color(debugColor, 0.2f));
                }
            }
            SpriteBatch.End();

            //Pause Screen
            if (gameState != GameState.Paused)
            {
                SpriteBatch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointWrap);

                //This twisted game needs to be reset, and with this health bar we're one step closer to a world without undying borders.
                //Also, please clean this up if you can, if not that's okay too lmao. - Church

                //Healthbar Setup
                var hpBarFill = SpriteLoader.Instance.AddSprite("content/hpbar");
                var hpBarEmpty = SpriteLoader.Instance.AddSprite("content/hpbar_empty");
                var hpBarShadow = SpriteLoader.Instance.AddSprite("content/hpbar_shadow");
                var hpBarBlipFill = SpriteLoader.Instance.AddSprite("content/hpbar_blip");
                var hpBarBlipEmpty = SpriteLoader.Instance.AddSprite("content/hpbar_blip_empty");
                var hpBarBlipShadow = SpriteLoader.Instance.AddSprite("content/hpbar_blip_shadow");
                var HUDBar = SpriteLoader.Instance.AddSprite("content/hud_bar_frame");
                Vector2 HealthBarPos = new Vector2(GraphicsDevice.Viewport.X + 1, GraphicsDevice.Viewport.Height - (HUDBar.Height + 1));
                int HealthWidth = (int)LerpHelper.Linear(0, HUDBar.Width - 4, MathHelper.Clamp((float)World.Player.Health, 0, 100))/100;

                //HealthBar Drawing
                DrawText(Game.ConvertToPixelText("HP"), new Vector2(HealthBarPos.X, HealthBarPos.Y - 16), Alignment.Left, new TextParameters().SetColor(HSVA2RGBA(MathHelper.Clamp((float)World.Player.Health, 0, 330), 1, 1, 192), Color.Black));
                //DrawSpriteExt(HUDBar, 0, HealthBarPos, HUDBar.Middle, 0, new Vector2(1, 1), SpriteEffects.None, Color.White, 0);
                //SpriteBatch.Draw(Pixel, new Rectangle((int)HealthBarPos.X + 2, (int)HealthBarPos.Y+2, HealthWidth, HUDBar.Height - 4), HSVA2RGBA(MathHelper.Clamp((float)World.Player.Health, 0, 330), 1, 1, 192));

                //TODO move blip code into Healthbar?
                /*int blipsMax = 12;
                double hpMax = World.Player.HealthMax;
                double hp = Math.Min(Math.Max(World.Player.Health, 0), hpMax);
                double hpBasePerBlip = 1000;
                int blipsHpMax = Math.Min(blipsMax, (int)(hpMax / hpBasePerBlip));
                double hpPerBlip = hpMax / (blipsHpMax+1);
                int blipsHp = Math.Max(0,Math.Min(blipsMax, (int)Math.Ceiling(hp / hpPerBlip)-1));
                double hpBar = hp - blipsHp * hpPerBlip;

                for (int i = 0; i < blipsHpMax; i++) {
                    DrawSprite(hpBarBlipEmpty, 0, new Vector2(HealthBarPos.X + 8*i, HealthBarPos.Y + 8), SpriteEffects.None, 0);
                    if(i < blipsHp)
                        DrawSprite(hpBarBlipFill, 0, new Vector2(HealthBarPos.X + 8 * i, HealthBarPos.Y + 8), SpriteEffects.None, 0);
                }

                int width = 100;
                int widthFill = (int)Math.Round(width * (hpBar / hpPerBlip));
                if (hpBar > 0 && widthFill <= 0)
                    widthFill = 1;
                if (hpBar < hpPerBlip && widthFill >= width)
                    widthFill = width - 1;
                SpriteBatch.Draw(hpBarEmpty.Texture, new Rectangle((int)HealthBarPos.X, (int)HealthBarPos.Y, width, hpBarEmpty.Height), new Rectangle(0, 0, width, hpBarEmpty.Height), Color.White);
                SpriteBatch.Draw(hpBarFill.Texture, new Rectangle((int)HealthBarPos.X, (int)HealthBarPos.Y, widthFill, hpBarEmpty.Height), new Rectangle(0, 0, widthFill, hpBarEmpty.Height), Color.White);
                //DrawText(Game.ConvertToSmallPixelText(Math.Floor(hp).ToString()), new Vector2(HealthBarPos.X, HealthBarPos.Y), Alignment.Left, new TextParameters().SetColor(Color.White, Color.Black));
                */
                double hpMax = World.Player.HealthMax;
                //double hp = Math.Min(Math.Max(Health.CurrentValue, 0), hpMax);
                double hp = Math.Min(Math.Max(World.Player.Health, 0), hpMax);
                double hpShadow = Math.Min(Math.Max(HealthShadow.CurrentValue, 0), hpMax);
                HealthBarParams parameters = CalculateHealthBar(12, 50, hpMax);

                DrawHealthbar(hpBarBlipEmpty, hpBarEmpty, HealthBarPos, parameters.BlipsHPMax, 1.0, 100);
                DrawHealthbar(hpBarBlipShadow, hpBarShadow, HealthBarPos, parameters.GetBlips(hpShadow), parameters.GetExtra(hpShadow) / parameters.HPPerBlip, 100);
                DrawHealthbar(hpBarBlipFill, hpBarFill, HealthBarPos, parameters.GetBlips(hp), parameters.GetExtra(hp) / parameters.HPPerBlip, 100);
                //DrawHealthbar(hpBarBlipFill, hpBarFill, HealthBarPos, parameters.GetBlips(hp), parameters.GetExtra(hp) / parameters.HPPerBlip, 100);

                DrawText($"Tiles Ascended: {HeightTraversed}\nVelocityX: {World.Player.Velocity.X}\nVelocityY: {World.Player.Velocity.Y}\nOnGround: {World.Player.OnGround}\nOnWall: {World.Player.OnWall}", new Vector2(0, 48), Alignment.Left, new TextParameters().SetColor(Color.White, Color.Black));
                SpriteBatch.End();
            }
            else
            {
                SpriteBatch.Begin(blendState: BlendState.NonPremultiplied);
                SpriteBatch.Draw(Pixel, new Rectangle(GraphicsDevice.Viewport.X, GraphicsDevice.Viewport.Y, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), new Color(0, 0, 0, 128));
                DrawText(Game.ConvertToPixelText("PAUSED"), new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), Alignment.Center, new TextParameters().SetColor(Color.White, Color.Black));
                SpriteBatch.End();
            }
        }

        struct HealthBarParams
        {
            public HealthBarParams(int blipsMax, int blipsHPMax, double hpPerBlip)
            {
                BlipsMax = blipsMax;
                BlipsHPMax = blipsHPMax;
                HPPerBlip = hpPerBlip;
            }

            private int BlipsMax;
            public int BlipsHPMax;
            public double HPPerBlip;

            public int GetBlips(double hp)
            {
                return Math.Max(0, Math.Min(BlipsMax, (int)Math.Ceiling(hp / HPPerBlip) - 1));
            }

            public double GetExtra(double hp)
            {
                return hp - GetBlips(hp) * HPPerBlip;
            }
        }

        private HealthBarParams CalculateHealthBar(int blipsMax, double hpBasePerBlip, double hpMax)
        {
            int blipsHpMax = Math.Min(blipsMax, (int)(hpMax / hpBasePerBlip));
            double hpPerBlip = hpMax / (blipsHpMax + 1);

            return new HealthBarParams(blipsMax, blipsHpMax, hpPerBlip);
        }

        private void DrawHealthbar(SpriteReference spriteBlip, SpriteReference spriteBar, Vector2 position, int blips, double bar, int width)
        {
            for (int i = 0; i < blips; i++)
            {
                DrawSprite(spriteBlip, 0, position + new Vector2(spriteBlip.Width * i, spriteBar.Height), SpriteEffects.None, 0);
            }

            int widthFill = (int)Math.Round(width * bar);
            if (bar > 0 && widthFill <= 0)
                widthFill = 1;
            if (bar < 1 && widthFill >= width)
                widthFill = width - 1;

            SpriteBatch.Draw(spriteBar.Texture, new Rectangle((int)position.X, (int)position.Y, widthFill, spriteBar.Height), new Rectangle(0, 0, widthFill, spriteBar.Height), Color.White);
        }

        private void DrawMapBackground(Map map)
        {
            Random random = new Random();

            var brick = SpriteLoader.Instance.AddSprite("content/bg_brick");
            var brick_miss1 = SpriteLoader.Instance.AddSprite("content/bg_brick_miss1");
            var brick_miss2 = SpriteLoader.Instance.AddSprite("content/bg_brick_miss2");
            var brick_opening = SpriteLoader.Instance.AddSprite("content/bg_brick_opening");
            var brick_platform = SpriteLoader.Instance.AddSprite("content/bg_brick_platform");
            var statue = SpriteLoader.Instance.AddSprite("content/bg_statue");
            var pillar = SpriteLoader.Instance.AddSprite("content/bg_pillar");
            var pillar_detail = SpriteLoader.Instance.AddSprite("content/bg_pillar_detail");
            var pillar_top = SpriteLoader.Instance.AddSprite("content/bg_pillar_top");
            var rail_left = SpriteLoader.Instance.AddSprite("content/bg_rail_left");
            var rail_middle = SpriteLoader.Instance.AddSprite("content/bg_rail_middle");
            var rail_right = SpriteLoader.Instance.AddSprite("content/bg_rail_right");
            var tile4 = SpriteLoader.Instance.AddSprite("content/bg_tile4");
            var tile_detail = SpriteLoader.Instance.AddSprite("content/bg_tile_detail");
            var window = SpriteLoader.Instance.AddSprite("content/bg_window");
            var window_big_left = SpriteLoader.Instance.AddSprite("content/bg_window_big_left");
            var window_big_right = SpriteLoader.Instance.AddSprite("content/bg_window_big_right");

            int ChosenBG;
            WeightedList<SpriteReference> TextureList = new WeightedList<SpriteReference>();
            TextureList.Add(SpriteLoader.Instance.AddSprite("content/bg-defaultwall"), 40);
            TextureList.Add(SpriteLoader.Instance.AddSprite("content/bg-defaultwall2"), 5);
            TextureList.Add(SpriteLoader.Instance.AddSprite("content/bg-defaultwall3"), 25);
            TextureList.Add(SpriteLoader.Instance.AddSprite("content/bg-defaultwall4"), 1);
            TextureList.Add(SpriteLoader.Instance.AddSprite("content/bg-defaultwall6"), 1);
            TextureList.Add(SpriteLoader.Instance.AddSprite("content/bg-defaultwall7"), 1);
            TextureList.Add(SpriteLoader.Instance.AddSprite("content/bg-defaultwall8"), 1);

            Rectangle drawZone = GetDrawZone();
            int drawX = (int)(Camera.X / 16);
            int drawY = (int)(Camera.Y / 16);
            int drawRadius = 30;

            for (int x = MathHelper.Clamp(drawX - drawRadius, 0, map.Width - 1); x <= MathHelper.Clamp(drawX + drawRadius, 0, map.Width - 1); x++)
            {
                for (int y = MathHelper.Clamp(drawY - drawRadius, 0, map.Height - 1); y <= MathHelper.Clamp(drawY + drawRadius, 0, map.Height - 1); y++)
                {
                    Vector2 truePos = Vector2.Transform(new Vector2(x * 16, y * 16), WorldTransform);

                    if (!drawZone.Contains(truePos))
                        continue;

                    TileBG tile = map.Background[x, y];

                    switch (tile)
                    {
                        case (TileBG.Brick):
                            SpriteBatch.Draw(brick.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.BrickMiss1):
                            SpriteBatch.Draw(brick_miss1.Texture, new Vector2(x * 16, y * 16), brick_miss1.GetFrameRect(GetNoiseValue(x, y)), Color.White);
                            break;
                        case (TileBG.BrickMiss2):
                            SpriteBatch.Draw(brick_miss2.Texture, new Vector2(x * 16, y * 16), brick_miss2.GetFrameRect(GetNoiseValue(x, y)), Color.White);
                            break;
                        case (TileBG.BrickPlatform):
                            SpriteBatch.Draw(brick_platform.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.BrickOpening):
                            SpriteBatch.Draw(brick_opening.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.RailLeft):
                            SpriteBatch.Draw(rail_left.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.RailMiddle):
                            SpriteBatch.Draw(rail_middle.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.RailRight):
                            SpriteBatch.Draw(rail_right.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.Tile4):
                            SpriteBatch.Draw(tile4.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.TileDetail):
                            SpriteBatch.Draw(tile_detail.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.Pillar):
                            SpriteBatch.Draw(pillar.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.PillarDetail):
                            SpriteBatch.Draw(pillar_detail.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.PillarTop):
                            SpriteBatch.Draw(pillar_top.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.Window):
                            SpriteBatch.Draw(window.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.WindowBigLeft):
                            SpriteBatch.Draw(window_big_left.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.WindowBigRight):
                            SpriteBatch.Draw(window_big_right.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.Statue):
                            SpriteBatch.Draw(statue.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                    }
                    /*if(tile == TileBG.Brick)
                    {
                        ChosenBG = GetNoiseValue(x, y);
                        SpriteBatch.Draw(TextureList.GetWeighted(ChosenBG).Texture, new Vector2(x * 16, y * 16), Color.White);
                    }*/
                }
            }
        }

        private void DrawMap(Map map)
        {
            var wall = SpriteLoader.Instance.AddSprite("content/wall");
            var wallTop = SpriteLoader.Instance.AddSprite("content/wall_top");
            var wallBottom = SpriteLoader.Instance.AddSprite("content/wall_bottom");
            var wallBottomTop = SpriteLoader.Instance.AddSprite("content/wall_bottom_top");
            var wallBlock = SpriteLoader.Instance.AddSprite("content/wall_block");
            var wallIce = SpriteLoader.Instance.AddSprite("content/wall_ice");
            var wallPressure = SpriteLoader.Instance.AddSprite("content/wall_pressure");
            var wallPressureBottom = SpriteLoader.Instance.AddSprite("content/wall_pressure_bottom");
            var wallTrap = SpriteLoader.Instance.AddSprite("content/wall_trap");
            var wallTrapBottom = SpriteLoader.Instance.AddSprite("content/wall_trap_bottom");
            var grass = SpriteLoader.Instance.AddSprite("content/grass-top");
            var ladder = SpriteLoader.Instance.AddSprite("content/ladder");
            var spike = SpriteLoader.Instance.AddSprite("content/wall_spike");
            var spikeDeath = SpriteLoader.Instance.AddSprite("content/wall_spike_death");
            var breaks = SpriteLoader.Instance.AddSprite("content/breaks");

            Rectangle drawZone = GetDrawZone();
            int drawX = (int)(Camera.X / 16);
            int drawY = (int)(Camera.Y / 16);
            int drawRadius = 30;

            for (int x = MathHelper.Clamp(drawX - drawRadius, 0, map.Width - 1); x <= MathHelper.Clamp(drawX + drawRadius, 0, map.Width - 1); x++)
            {
                for (int y = MathHelper.Clamp(drawY - drawRadius, 0,map.Height-1); y <= MathHelper.Clamp(drawY + drawRadius, 0, map.Height - 1); y++)
                {
                    Vector2 truePos = Vector2.Transform(new Vector2(x * 16, y * 16), WorldTransform);

                    if (!drawZone.Contains(truePos))
                        continue;

                    Tile tile = map.Tiles[x, y];
                    Color color = tile.Color;

                    //TODO: move tile draw code into a method on Tile
                    if (tile is WallBlock) //subtypes before parent type otherwise it draws only the parent
                    {
                        SpriteBatch.Draw(wallBlock.Texture, new Vector2(x * 16, y * 16), color);
                    }
                    else if (tile is WallIce)
                    {
                        SpriteBatch.Draw(wallIce.Texture, new Vector2(x * 16, y * 16), Color.White);
                    }
                    else if (tile is LadderExtend ladderExtendTile)
                    {
                        SpriteBatch.Draw(ladder.Texture, new Vector2(x * 16, y * 16), ladder.GetFrameRect(0), color, 0, Vector2.Zero, 1, ladderExtendTile.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically, 0);
                        SpriteBatch.Draw(ladder.Texture, new Vector2(x * 16, y * 16) + Util.GetFacingVector(ladderExtendTile.Facing) * -2, ladder.GetFrameRect(0), color, 0, Vector2.Zero, 1, ladderExtendTile.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically, 0);
                    }
                    else if (tile is Ladder ladderTile)
                    {
                        SpriteBatch.Draw(ladder.Texture, new Vector2(x * 16, y * 16), ladder.GetFrameRect(0), color, 0, Vector2.Zero, 1, ladderTile.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically, 0);
                    }
                    else if (tile is SpikeDeath)
                    {
                        SpriteBatch.Draw(spikeDeath.Texture, new Vector2(x * 16, y * 16), color);
                    }
                    else if (tile is Spike)
                    {
                        SpriteBatch.Draw(spike.Texture, new Vector2(x * 16, y * 16), color);
                    }
                    else if (tile is Trap trap)
                    {
                        switch (trap.Facing)
                        {
                            case (Wall.WallFacing.Top):
                            case (Wall.WallFacing.Normal):
                                SpriteBatch.Draw(wallTrap.Texture, new Vector2(x * 16, y * 16 - 16), wallTrap.GetFrameRect(trap.Triggered ? 1 : 0), color);
                                break;
                            case (Wall.WallFacing.Bottom):
                            case (Wall.WallFacing.BottomTop):
                                SpriteBatch.Draw(wallTrapBottom.Texture, new Vector2(x * 16, y * 16 - 16), wallTrapBottom.GetFrameRect(trap.Triggered ? 1 : 0), color);
                                break;
                        }
                       
                    }
                    else if (tile is Wall wallTile)
                    {
                        switch(wallTile.Facing)
                        {
                            case (Wall.WallFacing.Normal):
                                SpriteBatch.Draw(wall.Texture, new Vector2(x * 16, y * 16), color);
                                break;
                            case (Wall.WallFacing.Bottom):
                                SpriteBatch.Draw(wallBottom.Texture, new Vector2(x * 16, y * 16), color);
                                break;
                            case (Wall.WallFacing.Top):
                                SpriteBatch.Draw(wallTop.Texture, new Vector2(x * 16, y * 16), color);
                                break;
                            case (Wall.WallFacing.BottomTop):
                                SpriteBatch.Draw(wallBottomTop.Texture, new Vector2(x * 16, y * 16), color);
                                break;
                        }
                        
                    }
                    else if (tile is Grass)
                        SpriteBatch.Draw(grass.Texture, new Vector2(x * 16, y * 16), Color.White);

                    if (tile.Health < tile.MaxHealth)
                        SpriteBatch.Draw(breaks.Texture, new Vector2(x * 16, y * 16), Color.White * (float)(1 - tile.Health / tile.MaxHealth));
                }
            }
        }

        private Rectangle GetDrawZone()
        {
            var drawZone = Viewport.Bounds;
            drawZone.Inflate(32, 32);
            return drawZone;
        }

        private void DrawHuman(EnemyHuman human)
        {
            SpriteEffects mirror = SpriteEffects.None;

            if (human.Facing == HorizontalFacing.Left)
                mirror = SpriteEffects.FlipHorizontally;

            Vector2 position = human.Position;

            PlayerState state = human.GetBasePose();
            human.CurrentAction.GetPose(state);
            human.SetPhenoType(state);

            if (state.Body == BodyState.Kneel)
            {
                position += new Vector2(0, 1);
            }

            if(human.Invincibility > 0 && (int)human.Lifetime % 2 == 0)
            {
                return;
            }
            DrawPlayerState(state, position - new Vector2(8, 8), mirror);
        }

        private int AnimationFrame(SpriteReference sprite, float frame, float frameEnd)
        {
            return (int)MathHelper.Clamp(sprite.SubImageCount * frame / frameEnd, 0, sprite.SubImageCount - 1);
        }

        public void DrawPlayerState(PlayerState state, Vector2 position, SpriteEffects mirror)
        {
            var origin = Vector2.Transform(position, WorldTransform);
            var transform = WorldTransform;
            //FORBIDDEN KNOWLEDGE
            //transform = transform * Matrix.CreateTranslation(-origin.X-16,-origin.Y-16, 0)  * Matrix.CreateRotationZ(MathHelper.Pi * Frame * 0.00f) * Matrix.CreateScale(3.0f, 1.0f, 1.0f) * Matrix.CreateTranslation(origin.X+16, origin.Y+16, 0);

            SpriteBatch.End();

            Matrix testMatrix = new Matrix(
              -1, 0, 0, 0,
              0, -1, 0, 0,
              0, 0, -1, 0,
              0, 0, 0, 1);
            Vector4 testAdd = new Vector4(1, 1, 1, 0);

            ColorMatrix identity = new ColorMatrix(Matrix.Identity, Vector4.Zero);
            ColorMatrix test = new ColorMatrix(testMatrix, testAdd);

            Shader.CurrentTechnique = Shader.Techniques["ColorMatrix"];
            Shader.Parameters["color_matrix"].SetValue(test.Matrix);
            Shader.Parameters["color_add"].SetValue(test.Add);
            Shader.Parameters["WorldViewProjection"].SetValue(transform * Projection);

            SpriteBatch.Begin(SpriteSortMode.FrontToBack, samplerState: SamplerState.PointClamp, transformMatrix: transform, effect: Shader);

            Vector2 offset = state.Body.Offset;
            if (mirror.HasFlag(SpriteEffects.FlipHorizontally))
                offset.X *= -1;

            state.Head.Draw(this, position + offset, mirror, 0.7f);
            state.Body.Draw(this, position + offset, mirror, 0.5f);
            state.LeftArm.Draw(this, ArmState.Type.Left, position + offset, mirror, 0.6f);
            state.RightArm.Draw(this, ArmState.Type.Right, position + offset, mirror, 0.8f);

            var weaponHold = state.RightArm.GetHoldOffset(ArmState.Type.Right);

            if (mirror.HasFlag(SpriteEffects.FlipHorizontally))
                weaponHold.X = 16 - weaponHold.X;
            state.Weapon.Draw(this, position + offset + weaponHold, mirror, 0.9f);

            SpriteBatch.End();
            StartNormalBatch();
        }

        public void DrawSprite(SpriteReference sprite, int frame, Vector2 position, SpriteEffects mirror, float depth)
        {
            if (!DepthShear.Contains(depth))
                return;
            SpriteBatch.Draw(sprite.Texture, position, sprite.GetFrameRect(frame), Color.White, 0, Vector2.Zero, 1, mirror, depth);
        }

        public void DrawSpriteExt(SpriteReference sprite, int frame, Vector2 position, Vector2 origin, float angle, SpriteEffects mirror, float depth)
        {
            DrawSpriteExt(sprite, frame, position, origin, angle, Vector2.One, mirror, Color.White, depth);
        }

        public void DrawSpriteExt(SpriteReference sprite, int frame, Vector2 position, Vector2 origin, float angle, Vector2 scale, SpriteEffects mirror, Color color, float depth)
        {
            if (!DepthShear.Contains(depth))
                return;
            SpriteBatch.Draw(sprite.Texture, position + origin, sprite.GetFrameRect(frame), color, angle, origin, scale, mirror, depth);
        }

        public void StartNormalBatch()
        {
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState:BlendState.NonPremultiplied, transformMatrix: WorldTransform);
        }
    }
}

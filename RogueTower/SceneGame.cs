using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ChaiFoxes.FMODAudio;
using Humper.Base;
using static RogueTower.Game;
using static RogueTower.Util;

namespace RogueTower
{
    enum WeaponHold
    {
        Left,
        Right,
        TwoHand,
    }

    class WeaponState
    {
        public string Sprite;
        public int Frame;
        public Vector2 Origin;
        public Color Color = Color.White;
        public float Angle;
        public float Length;

        public WeaponState(string sprite, int frame, Vector2 origin, float angle, float length)
        {
            Sprite = sprite;
            Frame = frame;
            Origin = origin;
            Angle = angle;
            Length = length;
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

            game.DrawSpriteExt(sprite, Frame, position - Origin, Origin, angle, Vector2.One, mirror, Color, depth);
        }

        public class NoneState : WeaponState
        {
            public NoneState() : base("", 0, Vector2.Zero, 0, 0)
            {
            }

            public override void Draw(SceneGame game, Vector2 position, SpriteEffects mirror, float depth)
            {
                //NOOP
            }
        }

        public static WeaponState None => new NoneState();
        public static WeaponState Boomerang(float angle) => new WeaponState("boomerang", 0, new Vector2(1, 3), angle, 6);
        public static WeaponState Katana(float angle) => new WeaponState("katana", 0, new Vector2(3, 1), angle, 16);
        public static WeaponState Knife(float angle) => new WeaponState("knife", 0, new Vector2(4, 4), angle, 6);
        public static WeaponState Lance(float angle) => new WeaponState("lance", 0, new Vector2(8, 3), angle, 32);
        public static WeaponState Rapier(float angle) => new WeaponState("rapier", 0, new Vector2(5, 3), angle, 16);
        public static WeaponState Sword(float angle) => new WeaponState("sword", 0, new Vector2(4, 4), angle, 10);
        public static WeaponState FireSword(float angle, int frame) => new WeaponState("sword_flame", frame, new Vector2(1, 4), angle, 14);
        public static WeaponState WandOrange(float angle) => new WeaponState("wand_orange", 0, new Vector2(2, 4), angle, 14);
        public static WeaponState WandAzure(float angle) => new WeaponState("wand_azure", 0, new Vector2(2, 4), angle, 14);
        public static WeaponState WandJade(float angle, int frame) => new WeaponState("wand_jade", frame, new Vector2(2, 4), angle, 14);
        public static WeaponState Warhammer(float angle) => new WeaponState("warhammer", 0, new Vector2(14, 6), angle, 32);

        public float GetAngle(SpriteEffects mirror)
        {
            var angle = Angle;
            if (mirror.HasFlag(SpriteEffects.FlipHorizontally))
            {
                angle = MirrorAngle(angle);
            }
            return angle + MathHelper.PiOver2;
        }

        public Vector2 GetOffset(SpriteEffects mirror, float slide)
        {
            return AngleToVector(GetAngle(mirror)) * Length * slide;
        }
    }

    class ShieldState
    {
        public string Sprite;
        public int Frame;
        public Vector2 Origin;
        public Vector2 Offset;
        public Color Color = Color.White;
        public float Angle;
        public float Depth;

        public ShieldState(string sprite, int frame, Vector2 origin, float angle, Vector2 offset, float depth)
        {
            Sprite = sprite;
            Frame = frame;
            Origin = origin;
            Offset = offset;
            Angle = angle;
            Depth = depth;
        }

        public virtual void Draw(SceneGame game, Vector2 position, SpriteEffects mirror, float depth)
        {
            SpriteReference sprite = SpriteLoader.Instance.AddSprite($"content/{Sprite}", false);

            Vector2 offset = Offset;
            float angle = Angle;
            if (mirror == SpriteEffects.FlipHorizontally)
            {
                offset.X = 16 - offset.X;
                angle = Util.MirrorAngle(angle);
                mirror = SpriteEffects.FlipVertically;
            }

            game.DrawSpriteExt(sprite, Frame, position + offset - Origin, Origin, angle, Vector2.One, mirror, Color, Depth);
        }

        public class NoneState : ShieldState
        {
            public NoneState() : base("", 0, Vector2.Zero, 0, Vector2.Zero, 0)
            {
            }

            public override void Draw(SceneGame game, Vector2 position, SpriteEffects mirror, float depth)
            {
                //NOOP
            }
        }

        public static ShieldState None => new NoneState();
        public static ShieldState ShieldForward => new ShieldState("shield", 0, new Vector2(8,8), 0, new Vector2(8, 8), 0.65f);
        public static ShieldState ShieldUp => new ShieldState("shield_up", 0, new Vector2(8, 8), 0, new Vector2(8, 8), 0.65f);
        public static ShieldState ShieldBack => new ShieldState("shield_back", 0, new Vector2(8, 8), 0, new Vector2(8, 8), 0.55f);
        public static ShieldState KatanaSheath(float angle) => new ShieldState("katana_sheathed", 0, new Vector2(15, 2), angle, new Vector2(6, 10), 0.9f);
        public static ShieldState KatanaSheathEmpty(float angle) => new ShieldState("katana_sheathed_empty", 0, new Vector2(15, 2), angle, new Vector2(6, 10), 0.9f);
    }

    class ArmState
    {
        public enum Type
        {
            Left,
            Right,
        }

        static Vector2 CenterLeft = new Vector2(12, 7);
        static Vector2 CenterRight = new Vector2(5, 7);
        static Vector2[] HoldOffsetAngularLeft = new[] {
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
        };
        static Vector2[] HoldOffsetAngularRight = new[] {
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
        };

        public string Pose;
        public Color Color = Color.White;
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
                    return HoldOffsetLeft[PositiveMod(Frame, HoldOffsetLeft.Length)];
                case Type.Right:
                    return HoldOffsetRight[PositiveMod(Frame, HoldOffsetRight.Length)];
            }
        }

        public Vector2 GetHoldDirection(Type type)
        {
            return GetHoldOffset(type) - (type == Type.Left ? CenterLeft : CenterRight);
        }

        public float GetHoldAngle(Type type)
        {
            return VectorToAngle(GetHoldDirection(type));
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

        private static int GetFrameFromAngle(float angle)
        {
            return Enumerable.Range(0,HoldOffsetAngularLeft.Length).WithMin(i => Math.Abs(GetAngleDistance(VectorToAngle(HoldOffsetAngularLeft[i] - CenterLeft),angle)));
        }

        public virtual void Draw(SceneGame game, Type type, Vector2 position, SpriteEffects mirror, float depth)
        {
            SpriteReference sprite = SpriteLoader.Instance.AddSprite($"content/{PhenoType}_{GetTypeString(type)}arm_{Pose}", true);

            game.DrawSprite(sprite, Frame, position, mirror, Color, depth);
        }

        public static ArmState Neutral => new ArmState("neutral", 0, new Vector2(13, 10), new Vector2(3, 10));
        public static ArmState Forward => new ArmState("forward", 0, new Vector2(15, 9), new Vector2(8, 9));
        public static ArmState Pray => new ArmState("pray", 0, new Vector2(10, 10), new Vector2(8, 10));
        public static ArmState Up => new ArmState("up", 0, new Vector2(13, 2), new Vector2(4, 2));
        public static ArmState Low => new ArmState("low", 0, new Vector2(), new Vector2(7, 10));
        public static ArmState Shield => new ArmState("shield", 0, new Vector2(13, 8), new Vector2());
        public static ArmState Angular(int frame) => new ArmState("angular", frame, HoldOffsetAngularLeft, HoldOffsetAngularRight);
        public static ArmState Angular(float angle) => Angular(GetFrameFromAngle(angle));
    }

    class BodyState
    {
        public string Pose;
        public int Frame;
        public Vector2 Offset;
        public Color Color = Color.White;
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

            game.DrawSprite(sprite, Frame, position, mirror, Color, depth);
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
        public Color Color = Color.White;
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

            game.DrawSprite(sprite, Frame, position + new Vector2(0,16 - sprite.Height), mirror, Color, depth);
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
        public ShieldState Shield;
        public WeaponHold WeaponHold = WeaponHold.Right;

        public float WeaponDepth
        {
            get
            {
                switch (WeaponHold)
                {
                    default:
                    case (WeaponHold.Left):
                        return 0.55f;
                    case (WeaponHold.Right):
                        return 0.9f;
                    case (WeaponHold.TwoHand):
                        return (0.9f + 0.55f) / 2;
                }
            }
        }

        public PlayerState(HeadState head, BodyState body, ArmState leftArm, ArmState rightArm, WeaponState weapon, ShieldState shield)
        {
            Head = head;
            Body = body;
            LeftArm = leftArm;
            RightArm = rightArm;
            Weapon = weapon;
            Shield = shield;
        }

        public Vector2 GetWeaponOffset(SpriteEffects mirror)
        {
            Vector2 offset;
            
            switch (WeaponHold)
            {
                default:
                case (WeaponHold.Left):
                    offset = LeftArm.GetHoldOffset(ArmState.Type.Left);
                    break;
                case (WeaponHold.Right):
                    offset = RightArm.GetHoldOffset(ArmState.Type.Right);
                    break;
                case (WeaponHold.TwoHand):
                    offset = (LeftArm.GetHoldOffset(ArmState.Type.Left) + RightArm.GetHoldOffset(ArmState.Type.Right)) / 2;
                    break;
            }

            if (mirror.HasFlag(SpriteEffects.FlipHorizontally))
                offset.X = 16 - offset.X;

            return GetBodyOffset(mirror) + offset;
        }

        public Vector2 GetBodyOffset(SpriteEffects mirror)
        {
            Vector2 offset = Body.Offset;
            if (mirror.HasFlag(SpriteEffects.FlipHorizontally))
                offset.X *= -1;
            return offset;
        }

        public Vector2 GetLeftHandOffset(SpriteEffects mirror)
        {
            Vector2 offset = GetBodyOffset(mirror) + LeftArm.GetHoldOffset(ArmState.Type.Left);
            if (mirror.HasFlag(SpriteEffects.FlipHorizontally))
                offset.X = 16 - offset.X;
            return offset;
        }

        public Vector2 GetRightHandOffset(SpriteEffects mirror)
        {
            Vector2 offset = GetBodyOffset(mirror) + RightArm.GetHoldOffset(ArmState.Type.Right);
            if (mirror.HasFlag(SpriteEffects.FlipHorizontally))
                offset.X = 16 - offset.X;
            return offset;
        }
    }

    struct Shear
    {
        public double Lower, Upper;

        public static Shear All => new Shear(double.NegativeInfinity, double.PositiveInfinity);
        public static Shear Below(double upper) => new Shear(double.NegativeInfinity, upper);
        public static Shear Above(double lower) => new Shear(lower, double.PositiveInfinity);

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

    class DrawStackFrame
    {
        public SpriteSortMode SortMode;
        public BlendState BlendState;
        public SamplerState SamplerState;
        public Matrix Transform;
        public Effect Shader;
        public Action<Matrix> ShaderSetup;

        public DrawStackFrame(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, Matrix transform, Effect shader, Action<Matrix> shaderSetup)
        {
            SortMode = sortMode;
            BlendState = blendState;
            SamplerState = samplerState;
            Transform = transform;
            Shader = shader;
            ShaderSetup = shaderSetup;
        }

        public void Apply(SceneGame scene)
        {
            ShaderSetup(Transform);
            scene.SpriteBatch.Begin(SortMode, BlendState, SamplerState, null, RasterizerState.CullNone, Shader, Transform);
        }
    }

    class SceneGame : Scene
    {
        public static bool DebugWeapons = false; //Makes every weapon appear as the sword, for normalization
        public static bool DebugMasks = false; //Show hitmasks

        const float ViewScale = 2f;

        public GameWorld World;
        public Map Map => World.Map;

        public RenderTarget2D CameraTargetA;
        public RenderTarget2D CameraTargetB;
        public RenderTarget2D DistortionMap;
        public Vector2 Camera;
        public Vector2 CameraSize => new Vector2(Viewport.Width / 2, Viewport.Height / 2);
        public Vector2 CameraPosition => FitCamera(Camera - CameraSize / 2);
        public Matrix WorldTransform;
        Matrix Projection;

        Shear DepthShear = Shear.All;
        
        public int HeightTraversed;

        public GameState gameState = GameState.Game;

        Healthbar Health;
        Healthbar HealthShadow;

        public InputAction InputAction;

        List<Background> Backgrounds;
        Stack<DrawStackFrame> SpriteBatchStack = new Stack<DrawStackFrame>();

        BlendState NonPremultiplied = new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
        };

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
            World.Player.Weapon = Weapon.PresetWeaponList.First();

            PlayerInput playerInput = new PlayerInput(World.Player);
            InputAction = playerInput;
            World.Player.SetControl(playerInput);

            Health = new Healthbar(() => World.Player.Health, LerpHelper.Linear, 10.0);
            HealthShadow = new Healthbar(() => World.Player.Health, LerpHelper.Linear, 1.0);

            Backgrounds = new List<Background>();
            Backgrounds.Add(new Background(this, SpriteLoader.Instance.AddSprite("content/bg_parallax_layer2"), () => new Vector2(10, 10), new Vector2(-0.5f, 0.02f)) {XLooping = true, YLooping = true});
            //Backgrounds.Add(new Background(this, SpriteLoader.Instance.AddSprite("content/bg_parallax_layer4"), () => new Vector2(0, World.Height - CameraSize.Y), new Vector2(0.05f, -1f)) { XLooping = true, YLooping = false });
            //Backgrounds.Add(new Background(this, SpriteLoader.Instance.AddSprite("content/bg_parallax_layer1"), () => new Vector2(0, World.Height - CameraSize.Y), new Vector2(0.05f, -1f)) { XLooping = true, YLooping = false });
            //Backgrounds.Add(new Background(this, SpriteLoader.Instance.AddSprite("content/bg_parallax_layer3"), () => new Vector2(0, World.Height - CameraSize.Y), new Vector2(0.20f, -1f)) { XLooping = true, YLooping = false });
            AddGroundBackground(SpriteLoader.Instance.AddSprite("content/bg_parallax_layer1_new"), new Vector2(0, 192 - 24), new Vector2(-0.2f, 0.4f));
            AddGroundBackground(SpriteLoader.Instance.AddSprite("content/bg_parallax_layer4"), new Vector2(0, 192 - 56), new Vector2(-0.2f, 0.4f));
            //AddGroundBackground(SpriteLoader.Instance.AddSprite("content/bg_parallax_layer6"), new Vector2(0, 46), new Vector2(-0.15f, 0.4f)); //trees
            //AddGroundBackground(SpriteLoader.Instance.AddSprite("content/bg_parallax_layer7"), new Vector2(0, 134), new Vector2(0.025f, -0.4f)); //trees close
            //AddGroundBackground(SpriteLoader.Instance.AddSprite("content/bg_parallax_layer3"), new Vector2(0, 192 - 48), new Vector2(-0.2f, 0.4f));
            AddGroundBackground(SpriteLoader.Instance.AddSprite("content/bg_parallax_layer5"), new Vector2(0 + 48, 544 + 200), new Vector2(-0.2f, -0.1f));
            AddGroundBackground(SpriteLoader.Instance.AddSprite("content/bg_parallax_layer5"), new Vector2(0 + 60, 528 + 200), new Vector2(-0.25f, -0.15f));
            AddGroundBackground(SpriteLoader.Instance.AddSprite("content/bg_parallax_layer5"), new Vector2(0 + 72, 512 + 200), new Vector2(-0.3f, -0.2f));


            PotionAppearance.Randomize(World.Random);

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
            InputAction.HandleInput(this);
            World.Update(InputAction.GameSpeed);
            Health.Update(1.0f);
            HealthShadow.Update(1.0f);
            HeightTraversed = (int)(World.Height - World.Player.Position.Y) / 16;

            Accompaniment1.Volume = CalculateHeightSlide(700 * 16, 600 * 16, World.Player, true);
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

        private void SwapBuffers()
        {
            var helper = CameraTargetA;
            CameraTargetA = CameraTargetB;
            CameraTargetB = helper;
        }

        public override void Draw(GameTime gameTime)
        {
            if (CameraTargetA == null || CameraTargetA.IsContentLost)
            {
                CameraTargetA = new RenderTarget2D(GraphicsDevice, Viewport.Width, Viewport.Height);
            }

            if (CameraTargetB == null || CameraTargetB.IsContentLost)
            {
                CameraTargetB = new RenderTarget2D(GraphicsDevice, Viewport.Width, Viewport.Height);
            }

            if (DistortionMap == null || DistortionMap.IsContentLost)
            {
                DistortionMap = new RenderTarget2D(GraphicsDevice, Viewport.Width, Viewport.Height);
            }


            GraphicsDevice.SetRenderTarget(CameraTargetA);

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
            SpriteBatch.Begin(blendState: NonPremultiplied, rasterizerState: RasterizerState.CullNone, effect: Shader);
            Color bg1 = new Color(32, 19, 48);
            Color bg2 = new Color(126, 158, 153);
            SetupGradient(bg1, bg1, bg2, bg2, Matrix.Identity);
            SpriteBatch.Draw(Pixel, new Rectangle(0, 0, (int)Viewport.Width, (int)Viewport.Height), Color.White);
            SpriteBatch.End();

            SpriteBatch.Begin(SpriteSortMode.Deferred, blendState: NonPremultiplied, samplerState: SamplerState.PointWrap, rasterizerState: RasterizerState.CullNone, transformMatrix: WorldTransform);
            foreach (Background bg in Backgrounds)
            {
                bg.Draw(SpriteBatch);
            }
            SpriteBatch.End();

            PushSpriteBatch(samplerState: SamplerState.PointClamp, blendState: NonPremultiplied, transform: WorldTransform);

            Rectangle drawZone = GetDrawZone();

            var passes = World.Objects.SelectMany(obj => obj.GetDrawPasses().Select(pass => Tuple.Create(obj, pass))).ToLookup(obj => obj.Item2, obj => obj.Item1);

            DrawMapBackground(World.Map);
            DepthShear = Shear.Below(0.75);
            //Pass lower enemy
            foreach (GameObject obj in passes[DrawPass.Background])
            {
                DrawObject(obj, drawZone, DrawPass.Background);
            }
            DepthShear = Shear.All;
            DrawMap(World.Map);
            DepthShear = Shear.Above(0.75);
            //Pass upper enemy
            foreach (GameObject obj in passes[DrawPass.Foreground])
            {
                DrawObject(obj, drawZone, DrawPass.Foreground);
            }
            DepthShear = Shear.All;

            foreach (GameObject obj in passes[DrawPass.Bullet])
            {
                DrawObject(obj, drawZone, DrawPass.Bullet);
            }

            foreach (GameObject obj in passes[DrawPass.Effect])
            {
                DrawObject(obj, drawZone, DrawPass.Effect);
            }

            PopSpriteBatch();
            foreach (GameObject obj in passes[DrawPass.EffectDeath])
            {
                DrawObject(obj, drawZone, DrawPass.EffectDeath);
            }

            GraphicsDevice.SetRenderTarget(DistortionMap);
            var testNoise = SpriteLoader.Instance.AddSprite("content/testnoise2");



            PushSpriteBatch(samplerState: SamplerState.LinearWrap, blendState: BlendState.Additive, transform: WorldTransform, shader: Shader, shaderSetup: (matrix) =>
            {
                float blur = 20f / 255;
                SetupColorMatrix(new ColorMatrix(new Matrix(
                    0, 0, 0, 0,
                    0, 0, 0, 0,
                    0, 0, 0, 0,
                    0, 0, 0, 1),
                    new Vector4(blur, blur, blur, 0)), matrix);
            });
            foreach (GameObject obj in passes[DrawPass.Invisible])
            {
                DrawObject(obj, drawZone, DrawPass.Invisible);
            }
            PopSpriteBatch();
            SpriteBatch.Begin(samplerState: SamplerState.LinearWrap, blendState: BlendState.Additive, rasterizerState: RasterizerState.CullNone);
            //var noiseOffset = AngleToVector(Frame * 0.1f) * 30;
            //noiseOffset = new Vector2(-Frame * 0.4f, -Frame*2);
            //SpriteBatch.Draw(testNoise.Texture, DistortionMap.Bounds, new Rectangle((int)noiseOffset.X, (int)noiseOffset.Y, DistortionMap.Width / 2,DistortionMap.Height / 2), Color.Gray);
            //SpriteBatch.Draw(Pixel, new Rectangle(0,0, DistortionMap.Width / 2, DistortionMap.Height), Color.White);
            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(CameraTargetB);
         
            SpriteBatch.Begin(samplerState: SamplerState.LinearClamp, blendState: BlendState.Additive, rasterizerState: RasterizerState.CullNone, effect: Shader);
            SetupDistortion(DistortionMap, new Vector2(30f / DistortionMap.Width, 30f / DistortionMap.Height), Matrix.Identity, Matrix.Identity);
            SpriteBatch.Draw(CameraTargetA, CameraTargetA.Bounds, Color.White);
            SpriteBatch.End();
            SwapBuffers();

            GraphicsDevice.SetRenderTarget(null);

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: NonPremultiplied, rasterizerState: RasterizerState.CullNone, effect: Shader);
            ColorMatrix color = ColorMatrix.Identity;

            IEnumerable<ScreenFlash> screenFlashes = World.Effects.OfType<ScreenFlash>();
            foreach (ScreenFlash screenFlash in screenFlashes)
            {
                color *= screenFlash.Color();
            }

            SetupColorMatrix(color, Matrix.Identity);
            SpriteBatch.Draw(CameraTargetA, CameraTargetA.Bounds, Color.White);
            //SpriteBatch.Draw(DistortionMap, DistortionMap.Bounds, Color.White);
            SpriteBatch.End();

            StartNormalBatch();

            if (Keyboard.GetState().IsKeyDown(Keys.RightControl) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftTrigger))
            {
                foreach (var box in World.Find(World.Bounds))
                {
                    Color debugColor = Color.Red;
                    if (box.Data is Enemy)
                        debugColor = Color.Lime;
                    SpriteBatch.Draw(Pixel, box.Bounds.ToRectangle(), new Color(debugColor, 0.2f));
                }
            }
            SpriteBatch.End();
            DrawHUD();
        }

        private void DrawHUD()
        {
            SpriteBatch.Begin(blendState: NonPremultiplied, rasterizerState: RasterizerState.CullNone, samplerState: SamplerState.PointWrap);

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
            int HealthWidth = (int)LerpHelper.Linear(0, HUDBar.Width - 4, MathHelper.Clamp((float)World.Player.Health, 0, 100)) / 100;

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

            DrawText($"Tiles Ascended: {HeightTraversed}\n" +
                $"VelocityX: {World.Player.Velocity.X}\n" +
                $"VelocityY: {World.Player.Velocity.Y}\n" +
                $"OnGround: {World.Player.OnGround}\n" +
                $"OnWall: {World.Player.OnWall}\n" +
                $"Room: {(int)(World.Player.Position.X / 8 / 16)},{(int)(World.Player.Position.Y / 8 / 16)}", new Vector2(0, 48), Alignment.Left, new TextParameters().SetColor(Color.White, Color.Black));

            foreach (var inputAction in GetOrderedInputActions(InputAction))
                inputAction.Draw(this);

            //Pause Screen
            if (gameState != GameState.Paused)
            {

            }
            else
            {
                SpriteBatch.Draw(Pixel, new Rectangle(GraphicsDevice.Viewport.X, GraphicsDevice.Viewport.Y, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), new Color(0, 0, 0, 128));
                DrawText(Game.ConvertToPixelText("PAUSED"), new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), Alignment.Center, new TextParameters().SetColor(Color.White, Color.Black));
            }
            SpriteBatch.End();
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
            int blipsHpMax = Math.Min(blipsMax,(int)Math.Ceiling(hpMax / hpBasePerBlip)-1);
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

        IEnumerable<InputAction> GetOrderedInputActions(InputAction action)
        {
            if (action != null)
            {
                yield return action;

                foreach (var subaction in action.SubActions.Reverse<InputAction>())
                {
                    foreach (var i in GetOrderedInputActions(subaction))
                    {
                        yield return i;
                    }
                }
            }
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
            var pillar_bottom_broken = SpriteLoader.Instance.AddSprite("content/bg_pillar_bottom_broken");
            var rail_left = SpriteLoader.Instance.AddSprite("content/bg_rail_left");
            var rail_middle = SpriteLoader.Instance.AddSprite("content/bg_rail_middle");
            var rail_right = SpriteLoader.Instance.AddSprite("content/bg_rail_right");
            var tile4 = SpriteLoader.Instance.AddSprite("content/bg_tile4");
            var tile_detail = SpriteLoader.Instance.AddSprite("content/bg_tile_detail");
            var window = SpriteLoader.Instance.AddSprite("content/bg_window");
            var window_big_left = SpriteLoader.Instance.AddSprite("content/bg_window_big_left");
            var window_big_right = SpriteLoader.Instance.AddSprite("content/bg_window_big_right");
            var black = SpriteLoader.Instance.AddSprite("content/bg_black");
            var hole = SpriteLoader.Instance.AddSprite("content/bg_hole");

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
                        case (TileBG.PillarBottomBroken):
                            SpriteBatch.Draw(pillar_bottom_broken.Texture, new Vector2(x * 16, y * 16), Color.White);
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
                        case (TileBG.Black):
                            SpriteBatch.Draw(black.Texture, new Vector2(x * 16, y * 16), Color.White);
                            break;
                        case (TileBG.BrickHole):
                            SpriteBatch.Draw(hole.Texture, new Vector2(x * 16, y * 16), Color.White);
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

        private IEnumerable<Tile> EnumerateCloseTiles(Map map, int drawX, int drawY, int drawRadius)
        {
            Rectangle drawZone = GetDrawZone();

            for (int x = MathHelper.Clamp(drawX - drawRadius, 0, map.Width - 1); x <= MathHelper.Clamp(drawX + drawRadius, 0, map.Width - 1); x++)
            {
                for (int y = MathHelper.Clamp(drawY - drawRadius, 0, map.Height - 1); y <= MathHelper.Clamp(drawY + drawRadius, 0, map.Height - 1); y++)
                {
                    Vector2 truePos = Vector2.Transform(new Vector2(x * 16, y * 16), WorldTransform);

                    if (!drawZone.Contains(truePos))
                        continue;

                    yield return map.Tiles[x, y];
                }
            }
        }

        private IEnumerable<int> GetPasses(Tile tile)
        {
            if (tile is WallIce)
            {
                yield return 1;
                yield return 2;
            }
            else
                yield return 0;
        }

        private void DrawMap(Map map)
        {
            Rectangle drawZone = GetDrawZone();
            int drawX = (int)(Camera.X / 16);
            int drawY = (int)(Camera.Y / 16);
            int drawRadius = 30;

            var passes = EnumerateCloseTiles(map, drawX, drawY, drawRadius).SelectMany(tile => GetPasses(tile).Select(pass => Tuple.Create(tile, pass))).ToLookup(tile => tile.Item2, tile => tile.Item1);
            DrawMapPass(passes[0]);
            SpriteBatch.End();
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: NonPremultiplied, rasterizerState: RasterizerState.CullNone, transformMatrix: WorldTransform, effect: Shader);
            ColorMatrix iceBackground = new ColorMatrix(new Matrix(
              0, 0, 0, 0,
              0, 0, 0, 0,
              0, 0, 0, 0,
              0, 0, 0, 0.3f),
              new Vector4(0, 0, 0, 0));
            SetupColorMatrix(iceBackground);
            DrawMapPass(passes[1]);
            SpriteBatch.End();
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.Additive, rasterizerState: RasterizerState.CullNone, transformMatrix: WorldTransform, effect: Shader);
            ColorMatrix iceForeground = new ColorMatrix(new Matrix(
              1.2f, 0, 0, 0,
              0, 1.2f, 0, 0,
              0, 0, 1.2f, 0,
              0, 0, 0, 1),
              new Vector4(-0.8f * 0.7f, -0.3f * 0.7f, -0.1f * 0.7f, 0));
            SetupColorMatrix(iceForeground);
            DrawMapPass(passes[2]);
            SpriteBatch.End();
            StartNormalBatch();
        }

        private void DrawMapPass(IEnumerable<Tile> tiles)
        {
            var wall = SpriteLoader.Instance.AddSprite("content/wall");
            var wallTop = SpriteLoader.Instance.AddSprite("content/wall_top");
            var wallBottom = SpriteLoader.Instance.AddSprite("content/wall_bottom");
            var wallBottomTop = SpriteLoader.Instance.AddSprite("content/wall_bottom_top");
            var wallBlock = SpriteLoader.Instance.AddSprite("content/wall_block");
            var wallIce = SpriteLoader.Instance.AddSprite("content/wall_ice");
            var wallIceConnected = SpriteLoader.Instance.AddSprite("content/wall_ice_connected");
            var wallPressure = SpriteLoader.Instance.AddSprite("content/wall_pressure");
            var wallPressureBottom = SpriteLoader.Instance.AddSprite("content/wall_pressure_bottom");
            var wallTrap = SpriteLoader.Instance.AddSprite("content/wall_trap");
            var wallTrapBottom = SpriteLoader.Instance.AddSprite("content/wall_trap_bottom");
            var grass = SpriteLoader.Instance.AddSprite("content/grass-top");
            var ladder = SpriteLoader.Instance.AddSprite("content/ladder");
            var spike = SpriteLoader.Instance.AddSprite("content/wall_spike");
            var spikeDeath = SpriteLoader.Instance.AddSprite("content/wall_spike_death");
            var breaks = SpriteLoader.Instance.AddSprite("content/breaks");

            foreach (Tile tile in tiles)
            {
                int x = tile.X;
                int y = tile.Y;
                Color color = tile.Color;

                //TODO: move tile draw code into a method on Tile
                if (tile is WallBlock) //subtypes before parent type otherwise it draws only the parent
                {
                    SpriteBatch.Draw(wallBlock.Texture, new Vector2(x * 16, y * 16), color);
                }
                else if (tile is WallIce)
                {
                    //SpriteBatch.Draw(wallIce.Texture, new Vector2(x * 16, y * 16), Color.White);
                    int ix = tile.BlobIndex % 7;
                    int iy = tile.BlobIndex / 7;
                    SpriteBatch.Draw(wallIceConnected.Texture, new Vector2(x * 16, y * 16), new Rectangle(ix * 16, iy * 16, 16, 16), Color.White);
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
                    switch (wallTile.Facing)
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

        private Rectangle GetDrawZone()
        {
            var drawZone = Viewport.Bounds;
            drawZone.Inflate(32, 32);
            return drawZone;
        }

        public void DrawObject(GameObject obj, Rectangle drawZone, DrawPass pass)
        {
            if (obj is Enemy enemy && !enemy.GetDrawPoints().Any(pos => drawZone.Contains(Vector2.Transform(pos, WorldTransform))))
            {
                return;
            }

            obj.Draw(this, pass);
        }

        public void DrawHuman(EnemyHuman human)
        {
            SpriteEffects mirror = SpriteEffects.None;

            if (human.Facing == HorizontalFacing.Left)
                mirror = SpriteEffects.FlipHorizontally;

            PlayerState state = human.Pose;

            if(human.Invincibility > 0 && (int)human.Lifetime % 2 == 0)
            {
                return;
            }

            ColorMatrix color = human.VisualBaseColor;
            foreach(var statusEffect in human.StatusEffects)
            {
                color *= statusEffect.ColorMatrix;
            }
            color *= human.VisualFlash();

            DrawPlayerState(state, human.Position - new Vector2(8, 8) + human.VisualOffset(), mirror, color);
        }

        public int AnimationFrame(SpriteReference sprite, float frame, float frameEnd)
        {
            return (int)MathHelper.Clamp(sprite.SubImageCount * frame / frameEnd, 0, sprite.SubImageCount - 1);
        }

        public void DrawPlayerState(PlayerState state, Vector2 position, SpriteEffects mirror, ColorMatrix color)
        {
            var origin = Vector2.Transform(position, WorldTransform);
            var transform = WorldTransform;
            //FORBIDDEN KNOWLEDGE
            //transform = transform * Matrix.CreateTranslation(-origin.X-16,-origin.Y-16, 0)  * Matrix.CreateRotationZ(MathHelper.Pi * Frame * 0.00f) * Matrix.CreateScale(3.0f, 1.0f, 1.0f) * Matrix.CreateTranslation(origin.X+16, origin.Y+16, 0);

            PushSpriteBatch(sortMode: SpriteSortMode.FrontToBack, samplerState: SamplerState.PointClamp, shader: Shader, shaderSetup: (matrix) =>
            {
                SetupColorMatrix(color, matrix);
            });

            if (DebugWeapons)
                state.Weapon = WeaponState.Sword(state.Weapon.Angle);

            Vector2 offset = state.GetBodyOffset(mirror);

            state.Head.Draw(this, position + offset, mirror, 0.7f);
            state.Body.Draw(this, position + offset, mirror, 0.5f);
            state.LeftArm.Draw(this, ArmState.Type.Left, position + offset, mirror, 0.6f);
            state.RightArm.Draw(this, ArmState.Type.Right, position + offset, mirror, 0.8f);
            state.Shield.Draw(this, position + offset, mirror, 0f);

            Vector2 weaponHold = state.GetWeaponOffset(mirror);
            float weaponDepth = state.WeaponDepth;

            state.Weapon.Draw(this, position + weaponHold, mirror, weaponDepth);

            PopSpriteBatch();
        }

        public void DrawSprite(SpriteReference sprite, int frame, Vector2 position, SpriteEffects mirror, float depth)
        {
            DrawSprite(sprite, frame, position, mirror, Color.White, depth);
        }

        public void DrawSprite(SpriteReference sprite, int frame, Vector2 position, SpriteEffects mirror, Color color, float depth)
        {
            if (!DepthShear.Contains(depth))
                return;
            SpriteBatch.Draw(sprite.Texture, position, sprite.GetFrameRect(frame), color, 0, Vector2.Zero, Vector2.One, mirror, depth);
        }

        public void DrawSpriteExt(SpriteReference sprite, int frame, Vector2 position, Vector2 origin, float angle, SpriteEffects mirror, float depth)
        {
            DrawSpriteExt(sprite, frame, position, origin, angle, Vector2.One, mirror, Color.White, depth);
        }

        public void DrawSpriteExt(SpriteReference sprite, int frame, Vector2 position, Vector2 origin, float angle, Vector2 scale, SpriteEffects mirror, Color color, float depth)
        {
            if (!DepthShear.Contains(depth))
                return;
            SpriteBatch.Draw(sprite.Texture, position + origin, sprite.GetFrameRect(frame), color, angle, origin, scale.Mirror(mirror), SpriteEffects.None, depth);
        }

        public void DrawCircle(SpriteReference sprite, int frame, Vector2 position, float radius, Color color)
        {
            PushSpriteBatch(samplerState: SamplerState.LinearClamp, shader: Shader, shaderSetup: (transform) =>
            {
                SetupCircle(sprite.GetFrameMatrix(frame), transform);
            });
            int intRadius = (int)radius;
            var area = new Rectangle((int)position.X - intRadius, (int)position.Y - intRadius, intRadius * 2 + 1, intRadius * 2 + 1);
            SpriteBatch.Draw(sprite.Texture, area, sprite.GetFrameRect(0), color);
            PopSpriteBatch();
        }

        public void StartNormalBatch()
        {
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: NonPremultiplied, rasterizerState: RasterizerState.CullNone, transformMatrix: WorldTransform);
        }

        public void SetupNormal(Matrix transform)
        {
            Shader.CurrentTechnique = Shader.Techniques["BasicColorDrawing"];
            Shader.Parameters["WorldViewProjection"].SetValue(transform * Projection);
        }

        public void SetupColorMatrix(ColorMatrix matrix)
        {
            SetupColorMatrix(matrix,WorldTransform);
        }

        public void SetupColorMatrix(ColorMatrix matrix, Matrix transform)
        {
            Shader.CurrentTechnique = Shader.Techniques["ColorMatrix"];
            Shader.Parameters["color_matrix"].SetValue(matrix.Matrix);
            Shader.Parameters["color_add"].SetValue(matrix.Add);
            Shader.Parameters["WorldViewProjection"].SetValue(transform * Projection);
        }

        public void SetupColorMatrixThreshold(ColorMatrix matrixA, ColorMatrix matrixB, Texture2D map, Matrix mapTransform, float threshold)
        {
            SetupColorMatrixThreshold(matrixA, matrixB, map, mapTransform, threshold, WorldTransform);
        }

        public void SetupColorMatrixThreshold(ColorMatrix matrixA, ColorMatrix matrixB, Texture2D map, Matrix mapTransform, float threshold, Matrix transform)
        {
            Shader.CurrentTechnique = Shader.Techniques["ColorMatrixThreshold"];
            Shader.Parameters["color_matrix"].SetValue(matrixA.Matrix);
            Shader.Parameters["color_add"].SetValue(matrixA.Add);
            Shader.Parameters["threshold_color_matrix"].SetValue(matrixB.Matrix);
            Shader.Parameters["threshold_color_add"].SetValue(matrixB.Add);
            Shader.Parameters["threshold"].SetValue(threshold);
            Shader.Parameters["texture_map"].SetValue(map);
            Shader.Parameters["map_transform"].SetValue(mapTransform);
            Shader.Parameters["WorldViewProjection"].SetValue(transform * Projection);
        }

        public void SetupGradient(Color topleft, Color topright, Color bottomleft, Color bottomright)
        {
            SetupGradient(topleft, topright, bottomleft, bottomright, WorldTransform);
        }

        public void SetupGradient(Color topleft, Color topright, Color bottomleft, Color bottomright, Matrix transform)
        {
            Shader.CurrentTechnique = Shader.Techniques["Gradient"];
            Shader.Parameters["gradient_topleft"].SetValue(topleft.ToVector4());
            Shader.Parameters["gradient_topright"].SetValue(topright.ToVector4());
            Shader.Parameters["gradient_bottomleft"].SetValue(bottomleft.ToVector4());
            Shader.Parameters["gradient_bottomright"].SetValue(bottomright.ToVector4());
            Shader.Parameters["WorldViewProjection"].SetValue(transform * Projection);
        }

        public void SetupClockBetween(float lower, float upper)
        {
            SetupClockBetween(lower, upper, WorldTransform);
        }

        public void SetupClockBetween(float lower, float upper, Matrix transform)
        {
            float target = (lower + upper) / 2;
            float spread = (upper - lower) / 2;
            SetupClockRay(target, spread, transform);
        }

        public void SetupClockRay(float target, float spread)
        {
            SetupClockRay(target, spread, WorldTransform);
        }

        public void SetupClockRay(float target, float spread, Matrix transform)
        {
            Shader.CurrentTechnique = Shader.Techniques["Clock"];
            Shader.Parameters["angle_target"].SetValue(target - MathHelper.PiOver2);
            Shader.Parameters["angle_spread"].SetValue(spread);
            Shader.Parameters["WorldViewProjection"].SetValue(transform * Projection);
        }

        public void SetupCircle(Matrix mapTransform)
        {
            SetupCircle(mapTransform, WorldTransform);
        }

        public void SetupCircle(Matrix mapTransform, Matrix transform)
        {
            Shader.CurrentTechnique = Shader.Techniques["Circle"];
            Shader.Parameters["map_transform"].SetValue(mapTransform);
            Shader.Parameters["WorldViewProjection"].SetValue(transform * Projection);
        }

        public void SetupDistortion(Texture2D map, Vector2 offset, Matrix mapTransform)
        {
            SetupDistortion(map, offset, mapTransform, WorldTransform);
        }

        public void SetupDistortion(Texture2D map, Vector2 offset, Matrix mapTransform, Matrix transform)
        {
            Shader.CurrentTechnique = Shader.Techniques["Distort"];
            Shader.Parameters["distort_offset"].SetValue(offset);
            Shader.Parameters["texture_map"].SetValue(map);
            Shader.Parameters["map_transform"].SetValue(mapTransform);
            Shader.Parameters["WorldViewProjection"].SetValue(transform * Projection);
        }

        public float CalculateHeightSlide(float transitionStart, float transitionEnd, Player player, bool clamp = false)
        {
            var transitionSize = transitionStart - transitionEnd;
            var yRelative = player.Position.Y - transitionEnd;
            return clamp? MathHelper.Clamp(1 - (yRelative / transitionSize), 0, 1) : 1 - (yRelative / transitionSize);
        }

        public void PushSpriteBatch(SpriteSortMode? sortMode = null, BlendState blendState = null, SamplerState samplerState = null, Matrix? transform = null, Effect shader = null, Action<Matrix> shaderSetup = null)
        {
            var lastState = SpriteBatchStack.Any() ? SpriteBatchStack.Peek() : null;
            if (sortMode == null)
                sortMode = lastState?.SortMode ?? SpriteSortMode.Deferred;
            if (blendState == null)
                blendState = lastState?.BlendState ?? NonPremultiplied;
            if (samplerState == null)
                samplerState = lastState?.SamplerState ?? SamplerState.PointClamp;
            if (transform == null)
                transform = lastState?.Transform ?? Matrix.Identity;
            if (shaderSetup == null)
                shaderSetup = SetupNormal;
            var newState = new DrawStackFrame(sortMode.Value, blendState, samplerState, transform.Value, shader, shaderSetup);
            if (!SpriteBatchStack.Empty())
                SpriteBatch.End();
            newState.Apply(this);
            SpriteBatchStack.Push(newState);
        }

        public void PopSpriteBatch()
        {
            SpriteBatch.End();
            SpriteBatchStack.Pop();
            if (!SpriteBatchStack.Empty())
            {
                var lastState = SpriteBatchStack.Peek();
                lastState.Apply(this);
            }
        }
    }
}

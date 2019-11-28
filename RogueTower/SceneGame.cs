using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

            game.DrawSpriteExt(sprite, Frame, position + Origin, Origin, angle, mirror, depth);
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
        public static WeaponState Knife(float angle) => new WeaponState("knife", 0, new Vector2(4, 4), angle);
    }

    class ArmState
    {
        public enum Type
        {
            Left,
            Right,
        }

        public string Pose;
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
            SpriteReference sprite = SpriteLoader.Instance.AddSprite($"content/char_{GetTypeString(type)}arm_{Pose}", true);

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

        public BodyState(string pose, int frame, Vector2 offset)
        {
            Pose = pose;
            Frame = frame;
            Offset = offset;
        }

        public virtual void Draw(SceneGame game, Vector2 position, SpriteEffects mirror, float depth)
        {
            SpriteReference sprite = SpriteLoader.Instance.AddSprite($"content/char_body_{Pose}", true);

            game.DrawSprite(sprite, Frame, position, mirror, depth);
        }

        public static BodyState Stand => new BodyState("walk", 0, Vector2.Zero);
        public static BodyState Walk(Player player) => Walk((int)player.WalkFrame);
        public static BodyState Walk(int frame) => new BodyState("walk", frame, Vector2.Zero);
        public static BodyState Kneel => new BodyState("kneel", 0, new Vector2(0, 1));
        public static BodyState Hit => new BodyState("hit", 0, Vector2.Zero);
        public static BodyState Crouch(Player player) => Crouch((int)player.WalkFrame);
        public static BodyState Crouch(int frame) => new BodyState("crouch", frame, new Vector2(1, 2));
        public static BodyState Climb => new BodyState("climb", 0, new Vector2(4, 0));
    }

    class HeadState
    {
        public string Pose;
        public int Frame;

        public HeadState(string pose, int frame)
        {
            Pose = pose;
            Frame = frame;
        }

        public virtual void Draw(SceneGame game, Vector2 position, SpriteEffects mirror, float depth)
        {
            SpriteReference sprite = SpriteLoader.Instance.AddSprite($"content/char_head_{Pose}", true);

            game.DrawSprite(sprite, Frame, position, mirror, depth);
        }

        public static HeadState Forward => new HeadState("front", 0);
        public static HeadState Backward => new HeadState("back", 0);
        public static HeadState Down => new HeadState("down", 0);
    }

    struct PlayerState
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
        const int ViewScale = 2;

        GameWorld World;
        public Map Map => World.Map;

        Vector2 Camera;
        Vector2 CameraSize => new Vector2(320, 240);
        Vector2 CameraPosition => FitCamera(Camera - CameraSize / 2);
        Matrix WorldTransform => CreateViewMatrix();

        Shear DepthShear = Shear.All;

        bool GameSpeedToggle = false;

        private Matrix CreateViewMatrix()
        {
            return Matrix.Identity
                * Matrix.CreateTranslation(new Vector3(-CameraPosition, 0))
                * Matrix.CreateTranslation(new Vector3(-CameraSize / 2, 0)) //These two lines center the character on (0,0)
                * Matrix.CreateScale(ViewScale, ViewScale, 1) //Scale it up by 2
                * Matrix.CreateTranslation(Viewport.Width / 2, Viewport.Height / 2, 0); //Translate the character to the middle of the viewport
        }

        public SceneGame(Game game) : base(game)
        {
            World = new GameWorld(50, 200);

            World.Player = new Player();
            World.Player.Create(World, 50, World.Height - 50);
            World.Player.SetControl(this);
        }

        public override bool ShowCursor => true;

        private Vector2 FitCamera(Vector2 camera)
        {
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
            if (KeyState.IsKeyDown(Keys.Tab) && LastKeyState.IsKeyUp(Keys.Tab))
                GameSpeedToggle = !GameSpeedToggle;
            World.Update(GameSpeedToggle ? 0.1f : 1.0f);
            UpdateCamera();
        }

        public void UpdateCamera()
        {
            Camera.X = World.Player.Position.X;
            float cameraDistance = World.Player.Position.Y - Camera.Y;
            if (cameraDistance > 30)
                Camera.Y = World.Player.Position.Y - 30;
            if (cameraDistance < -30)
                Camera.Y = World.Player.Position.Y + 30;
        }

        public override void Draw(GameTime gameTime)
        {
            StartNormalBatch();

            SpriteBatch.Draw(Pixel, new Rectangle(0, 0, (int)World.Width, (int)World.Height), Color.LightSkyBlue);

            DepthShear = new Shear(double.NegativeInfinity, 0.75);
            DrawPlayer(World.Player);
            DepthShear = Shear.All;
            DrawMap(World.Map);
            DepthShear = new Shear(0.75, double.PositiveInfinity);
            DrawPlayer(World.Player);
            DepthShear = Shear.All;

            var spikeball = SpriteLoader.Instance.AddSprite("content/spikeball");
            var chain = SpriteLoader.Instance.AddSprite("content/chain");

            foreach(GameObject obj in World.Objects)
            {
                if(obj is BallAndChain ballAndChain)
                {
                    for(float i = 0; i < ballAndChain.Distance; i += 6f)
                    {
                        DrawSprite(chain, 0, ballAndChain.Position + ballAndChain.OffsetUnit * i - chain.Middle, SpriteEffects.None, 0);
                    }
                    DrawSprite(spikeball, 0, ballAndChain.Position + ballAndChain.Offset - spikeball.Middle, SpriteEffects.None, 0);
                }
            }

            var knife = SpriteLoader.Instance.AddSprite("content/knife");
            foreach (Bullet bullet in World.Bullets)
            {
                if (bullet is Knife)
                {
                    DrawSpriteExt(knife, 0, bullet.Position + knife.Middle, knife.Middle, (float)Math.Atan2(bullet.Velocity.Y, bullet.Velocity.X), SpriteEffects.None, 0);
                }
            }

            foreach (VisualEffect effect in World.Effects)
            {
                if (effect is KnifeBounced knifeBounced)
                {
                    DrawSpriteExt(knife, 0, knifeBounced.Position + knife.Middle, knife.Middle, knifeBounced.Rotation * knifeBounced.Frame, SpriteEffects.None, 0);
                }
            }


            SpriteBatch.End();
        }

        private void DrawMap(Map map)
        {
            var wall = SpriteLoader.Instance.AddSprite("content/wall");
            var wallBlock = SpriteLoader.Instance.AddSprite("content/wall_block");
            var wallIce = SpriteLoader.Instance.AddSprite("content/wall_ice");
            var ladder = SpriteLoader.Instance.AddSprite("content/ladder");
            var spike = SpriteLoader.Instance.AddSprite("content/wall_spike");
            var breaks = SpriteLoader.Instance.AddSprite("content/break");

            var drawZone = Viewport.Bounds;
            drawZone.Inflate(32, 32);

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    Vector2 truePos = Vector2.Transform(new Vector2(x * 16, y * 16), WorldTransform);

                    if (!drawZone.Contains(truePos))
                        continue;

                    Tile tile = map.Tiles[x, y];

                    //TODO: move tile draw code into a method on Tile
                    if (tile is WallBlock) //subtypes before parent type otherwise it draws only the parent
                    {
                        SpriteBatch.Draw(wallBlock.Texture, new Vector2(x * 16, y * 16), Color.White);
                    }
                    else if (tile is WallIce)
                    {
                        SpriteBatch.Draw(wallIce.Texture, new Vector2(x * 16, y * 16), Color.White);
                    }
                    else if (tile is Ladder ladderTile)
                    {
                        SpriteBatch.Draw(ladder.Texture, new Vector2(x * 16, y * 16), ladder.GetFrameRect(0), Color.White, 0, Vector2.Zero, 1, ladderTile.Facing == HorizontalFacing.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically, 0);
                    }
                    else if (tile is Spike)
                    {
                        SpriteBatch.Draw(spike.Texture, new Vector2(x * 16, y * 16), Color.White);
                    }
                    else if (tile is Wall)
                    {
                        SpriteBatch.Draw(wall.Texture, new Vector2(x * 16, y * 16), Color.White);
                    }
                }
            }
        }

        private void DrawPlayer(Player player)
        {
            var slash = SpriteLoader.Instance.AddSprite("content/slash_round");

            var head = HeadState.Forward;
            var body = BodyState.Stand;
            var leftArm = ArmState.Shield;
            var rightArm = ArmState.Neutral;
            var weapon = WeaponState.Sword(MathHelper.ToRadians(0));

            SpriteEffects mirror = SpriteEffects.None;

            if (player.Facing == HorizontalFacing.Left)
                mirror = SpriteEffects.FlipHorizontally;

            Vector2 position = player.Position;

            switch (player.CurrentAction)
            {
                case (Player.Action.Move):
                    body = BodyState.Walk(player);
                    break;
                case (Player.Action.WallClimb):
                    body = BodyState.Climb;
                    leftArm = ArmState.Angular(11 + Util.PositiveMod(3 + (int)-player.ClimbFrame, 7));
                    rightArm = ArmState.Angular(11 + Util.PositiveMod((int)-player.ClimbFrame, 7));
                    weapon = WeaponState.None;
                    break;
                case (Player.Action.JumpUp):
                    if (player.Velocity.Y < -0.5)
                        body = BodyState.Walk(1);
                    else
                        body = BodyState.Walk(0);
                    //leftArm = rightArm = ArmState.Up;
                    break;
                case (Player.Action.JumpDown):
                    body = BodyState.Walk(2);
                    //leftArm = rightArm = ArmState.Up;
                    break;
                case (Player.Action.Slash):
                    if (player.InAir)
                        body = BodyState.Walk(1);
                    switch (player.SlashAction)
                    {
                        case (Player.SwordAction.StartSwing):
                            weapon = WeaponState.Sword(MathHelper.ToRadians(-90 - 22));
                            rightArm = ArmState.Angular(11);
                            break;
                        case (Player.SwordAction.UpSwing):
                            weapon = WeaponState.Sword(MathHelper.ToRadians(-90 - 45));
                            rightArm = ArmState.Angular(11);
                            break;
                        case (Player.SwordAction.DownSwing):
                            //weapon = WeaponState.Sword(MathHelper.ToRadians(22));
                            //rightArm = ArmState.Angular(2);
                            body = BodyState.Crouch(1);
                            weapon = WeaponState.Sword(MathHelper.ToRadians(45 + 22));
                            rightArm = ArmState.Angular(4);
                            break;
                        case (Player.SwordAction.FinishSwing):
                            //weapon = WeaponState.Sword(MathHelper.ToRadians(22));
                            //rightArm = ArmState.Angular(3);
                            weapon = WeaponState.Sword(MathHelper.ToRadians(45 + 22));
                            rightArm = ArmState.Angular(4);
                            break;
                    }
                    break;
                case (Player.Action.SlashUp):
                    if (player.InAir)
                        body = BodyState.Walk(1);
                    switch (player.SlashAction)
                    {
                        case (Player.SwordAction.StartSwing):
                            weapon = WeaponState.Sword(MathHelper.ToRadians(100));
                            body = BodyState.Crouch(1);
                            rightArm = ArmState.Angular(6);
                            break;
                        case (Player.SwordAction.UpSwing):
                            weapon = WeaponState.Sword(MathHelper.ToRadians(125));
                            body = BodyState.Crouch(1);
                            rightArm = ArmState.Angular(6);
                            break;
                        case (Player.SwordAction.DownSwing):
                            weapon = WeaponState.Sword(MathHelper.ToRadians(-75));
                            rightArm = ArmState.Angular(11);
                            break;
                        case (Player.SwordAction.FinishSwing):
                            weapon = WeaponState.Sword(MathHelper.ToRadians(-75));
                            rightArm = ArmState.Angular(11);
                            break;
                    }
                    break;
                case (Player.Action.SlashKnife):
                    switch (player.SlashAction)
                    {
                        case (Player.SwordAction.StartSwing):
                            rightArm = ArmState.Angular(5);
                            weapon = WeaponState.Knife(MathHelper.ToRadians(90 + 45));
                            break;
                        case (Player.SwordAction.UpSwing):
                            rightArm = ArmState.Angular(6);
                            weapon = WeaponState.Knife(MathHelper.ToRadians(90 + 45 + 22));
                            break;
                        case (Player.SwordAction.DownSwing):
                            body = BodyState.Crouch(1);
                            rightArm = ArmState.Angular(0);
                            //weapon = WeaponState.Sword(MathHelper.ToRadians(0));
                            weapon = WeaponState.None;
                            break;
                        case (Player.SwordAction.FinishSwing):
                            body = BodyState.Crouch(2);
                            rightArm = ArmState.Angular(0);
                            //weapon = WeaponState.Sword(MathHelper.ToRadians(0));
                            weapon = WeaponState.None;
                            break;
                    }
                    break;
                case (Player.Action.SlashDownward):
                    body = BodyState.Crouch(1);
                    leftArm = ArmState.Angular(4);
                    rightArm = ArmState.Angular(2);
                    weapon = WeaponState.Sword(MathHelper.ToRadians(90));
                    break;
                case (Player.Action.Hit):
                    head = HeadState.Down;
                    body = BodyState.Hit;
                    rightArm = ArmState.Angular(3);
                    break;
            }

            if (body == BodyState.Kneel)
            {
                position += new Vector2(0, 1);
            }

            DrawPlayerState(new PlayerState(head, body, leftArm, rightArm, weapon), position - new Vector2(8, 8), mirror);

            if (player.SlashEffect is SlashEffect slashEffect)
            {
                var slashAngle = slashEffect.Angle;
                if (mirror == SpriteEffects.FlipHorizontally)
                    slashAngle = -slashAngle;
                var slashMirror = mirror | (slashEffect.Mirror ? SpriteEffects.FlipVertically : SpriteEffects.None);
                SpriteBatch.Draw(slash.Texture, position + new Vector2(8, 8) - new Vector2(8, 8), slash.GetFrameRect(Math.Min(slash.SubImageCount - 1, (int)(slash.SubImageCount * slashEffect.Frame / slashEffect.FrameEnd) - 1)), Color.LightGray, slashAngle, slash.Middle, 0.5f, slashMirror, 0);
                SpriteBatch.Draw(slash.Texture, position + new Vector2(8, 8) - new Vector2(8, 8), slash.GetFrameRect(Math.Min(slash.SubImageCount - 1, (int)(slash.SubImageCount * slashEffect.Frame / slashEffect.FrameEnd))), Color.White, slashAngle, slash.Middle, 0.7f, slashMirror, 0);
            }
        }

        public void DrawPlayerState(PlayerState state, Vector2 position, SpriteEffects mirror)
        {
            SpriteBatch.End();
            SpriteBatch.Begin(SpriteSortMode.FrontToBack, samplerState: SamplerState.PointClamp, transformMatrix: WorldTransform);

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
            if (!DepthShear.Contains(depth))
                return;
            SpriteBatch.Draw(sprite.Texture, position - origin, sprite.GetFrameRect(frame), Color.White, angle, origin, 1, mirror, depth);
        }

        public void StartNormalBatch()
        {
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: WorldTransform);
        }
    }
}

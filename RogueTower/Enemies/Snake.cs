using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RogueTower.Game;
using static RogueTower.Util;


namespace RogueTower.Enemies
{
    class Snake : Enemy
    {
        public enum SegmentRender
        {
            Invisible,
            Normal,
            Fat,
        }

        public class SnakeSegment
        {
            public Vector2 Offset => new Vector2((float)Math.Sin(Angle), (float)Math.Cos(Angle)) * Distance * ((float)(Index + 1) / Parent.Segments.Count);

            public Snake Parent;
            public int Index;

            public float Angle => MathHelper.Lerp(StartAngle, EndAngle, Parent.MoveDelta);
            public float Distance => MathHelper.Lerp(StartDistance, EndDistance, Parent.MoveDelta);

            public float StartAngle, EndAngle;
            public float StartDistance, EndDistance;

            public SnakeSegment(Snake parent, int index)
            {
                Parent = parent;
                Index = index;
            }

            public void MoveTowards(float angle, float distance, float speed)
            {
                EndAngle = Util.AngleLerp(EndAngle, angle, speed);
                EndDistance = MathHelper.Lerp(EndDistance, distance, speed);
            }

            public void UpdateDiscrete()
            {
                StartAngle = EndAngle;
                StartDistance = EndDistance;
            }
        }

        public abstract class Action
        {
            public Snake Snake;

            public virtual bool MouthOpen => false;
            public virtual float HeadIndex => Snake.Segments.Count - 1;
            public virtual bool Hidden => false;

            public Action(Snake snake)
            {
                Snake = snake;
            }

            public virtual SegmentRender GetRenderSegment(SnakeSegment segment)
            {
                return SegmentRender.Normal;
            }

            public abstract void UpdateDelta(float delta);

            public abstract void UpdateDiscrete();
        }

        public class ActionIdle : Action
        {
            public float Time;
            public float Offset;

            public override bool MouthOpen => true;

            public ActionIdle(Snake snake) : base(snake)
            {
                Offset = Snake.Random.Next(100);
            }

            public override void UpdateDelta(float delta)
            {
                Time += delta;
            }

            public override void UpdateDiscrete()
            {
                var frame = Time + Offset;
                Vector2 idleCircle = Snake.IdleCircle;
                Vector2 wantedPosition = Snake.IdleOffset + new Vector2((float)Math.Sin(frame / 20f) * idleCircle.X, (float)Math.Cos(frame / 20f) * idleCircle.Y);
                Snake.Move(wantedPosition, 0.2f);
            }
        }

        public class ActionHide : ActionIdle
        {
            public float HideTime;

            public override bool Hidden => true;
            public override float HeadIndex => MathHelper.Clamp(Snake.Segments.Count * (1 - Time / HideTime), 0, Snake.Segments.Count - 1);

            public ActionHide(Snake snake, float hideTime) : base(snake)
            {
                HideTime = hideTime;
            }

            public override void UpdateDelta(float delta)
            {
                base.UpdateDelta(delta);

                if (Time > 1)
                {

                }

                if (Time >= HideTime)
                {
                    Snake.CurrentAction = new ActionHidden(Snake);
                }
            }
        }

        public class ActionHidden : Action
        {
            public float Time;
            public override bool Hidden => true;

            public ActionHidden(Snake snake) : base(snake)
            {
            }

            public override SegmentRender GetRenderSegment(SnakeSegment segment)
            {
                return SegmentRender.Invisible;
            }

            public override void UpdateDelta(float delta)
            {
                Time += delta;
            }

            public override void UpdateDiscrete()
            {
                //NOOP
            }
        }

        public class ActionUnhide : ActionIdle
        {
            public float HideTime;

            public override bool Hidden => true;
            public override float HeadIndex => MathHelper.Clamp(Snake.Segments.Count * (Time / HideTime), 0, Snake.Segments.Count - 1);

            public ActionUnhide(Snake snake, float hideTime) : base(snake)
            {
                HideTime = hideTime;
            }

            public override void UpdateDelta(float delta)
            {
                base.UpdateDelta(delta);

                if (Time >= HideTime)
                {
                    Snake.ResetState();
                }
            }
        }

        public class ActionBite : Action
        {
            public enum BiteState
            {
                Start,
                Bite,
                End,
            }

            public override bool MouthOpen => State == BiteState.Bite;

            public BiteState State;
            public float StartTime;
            public float BiteTime;
            public float EndTime;
            public Vector2 Target;

            public ActionBite(Snake snake, float startTime, float biteTime, float endTime) : base(snake)
            {
                StartTime = startTime;
                BiteTime = biteTime;
                EndTime = endTime;
            }

            public override void UpdateDelta(float delta)
            {
                switch (State)
                {
                    case (BiteState.Start):
                        StartTime -= delta;
                        if (StartTime < 0)
                            State = BiteState.Bite;
                        break;
                    case (BiteState.Bite):
                        BiteTime -= delta;
                        if (BiteTime < 0)
                            State = BiteState.End;
                        break;
                    case (BiteState.End):
                        EndTime -= delta;
                        if (EndTime < 0)
                            Snake.ResetState();
                        break;
                }
            }

            public override void UpdateDiscrete()
            {
                switch (State)
                {
                    case (BiteState.Start):
                        if (Snake.Target != null)
                        {
                            float dx = Snake.Target.Position.X - Snake.Position.X;
                            if (Math.Sign(dx) == GetFacingVector(Snake.Facing).X)
                            {
                                Target = Snake.Target.Position - Snake.Position;
                                Target = Math.Min(Target.Length(), 80) * Vector2.Normalize(Target);
                            }
                        }
                        Snake.Move(Target, 0.1f);
                        break;
                    case (BiteState.Bite):
                        Snake.Move(Target, 0.5f);
                        Snake.Move(Target, 0.5f);
                        Snake.Move(Target, 0.5f);
                        var maskSize = new Vector2(16, 16);
                        bool damaged = false;
                        foreach (var box in Snake.World.FindBoxes(new RectangleF(Snake.Position + Snake.Head.Offset - maskSize / 2, maskSize)))
                        {
                            if (box == Snake.Box || Snake.NoFriendlyFire(box.Data))
                                continue;
                            if (box.Data is Enemy enemy)
                            {
                                enemy.Hit(new Vector2(GetFacingVector(Snake.Facing).X, -2), 20, 50, 20);
                                damaged = true;
                            }
                        }
                        if (damaged)
                            State = BiteState.End;
                        break;
                    case (BiteState.End):
                        Snake.Move(Target, 0.2f);
                        break;
                }
            }
        }

        public class ActionSpit : Action
        {
            public enum SpitState
            {
                Start,
                SpitStart,
                Spit,
                End,
            }

            public override bool MouthOpen => State == SpitState.End || State == SpitState.Spit;

            public Enemy Target;
            public SpitState State;
            public float StartTime;
            public float SpitStartTime;
            public float SpitTime;
            public float EndTime;
            public Vector2 Offset;

            public ActionSpit(Snake snake, Enemy target, Vector2 offset, float startTime, float spitStartTime, float spitTime, float endTime) : base(snake)
            {
                Target = target;
                Offset = offset;
                StartTime = startTime;
                SpitStartTime = spitStartTime;
                SpitTime = spitTime;
                EndTime = endTime;
            }

            public override SegmentRender GetRenderSegment(SnakeSegment segment)
            {
                int i = Snake.Segments.Count - (int)Math.Round(StartTime);
                if (segment.Index == i)
                    return SegmentRender.Fat;
                else
                    return SegmentRender.Normal;
            }

            public override void UpdateDelta(float delta)
            {
                switch (State)
                {
                    case (SpitState.Start):
                        StartTime -= delta;
                        if (StartTime < 0)
                            State = SpitState.SpitStart;
                        break;
                    case (SpitState.SpitStart):
                        SpitStartTime -= delta;
                        if (SpitStartTime < 0)
                        {
                            Fire();
                            State = SpitState.Spit;
                        }
                        break;
                    case (SpitState.Spit):
                        SpitTime -= delta;
                        if (SpitTime < 0)
                            State = SpitState.End;
                        break;
                    case (SpitState.End):
                        EndTime -= delta;
                        if (EndTime < 0)
                            Snake.ResetState();
                        break;
                }
            }

            private void Fire()
            {
                Vector2 firePosition = Snake.Position + Snake.Head.Offset + GetFacingVector(Snake.Facing) * 8;
                int spits = 8;
                for (int i = 0; i < spits; i++)
                {
                    var velocity = Target.HomingTarget + new Vector2(0, -32) - firePosition;
                    if (Snake.Facing == HorizontalFacing.Left && velocity.X > -3)
                        velocity.X = -3;
                    if (Snake.Facing == HorizontalFacing.Right && velocity.X < 3)
                        velocity.X = 3;
                    velocity = Vector2.Normalize(velocity) * (2 + i);
                    new SnakeSpit(Snake.World, firePosition)
                    {
                        Velocity = velocity,
                        Shooter = Snake,
                        FrameEnd = 80,
                    };
                }
            }

            public override void UpdateDiscrete()
            {
                switch (State)
                {
                    case (SpitState.Start):
                        Snake.Move(Offset - GetFacingVector(Snake.Facing) * 16, 0.3f);
                        break;
                    case (SpitState.SpitStart):
                        var spitOffset = Offset + new Vector2(0, 12) + GetFacingVector(Snake.Facing) * 20;
                        Snake.Move(spitOffset, 0.5f);
                        Snake.Move(spitOffset, 0.5f);
                        Snake.Move(spitOffset, 0.5f);
                        break;
                    case (SpitState.End):
                        Snake.Move(Offset, 0.1f);
                        break;
                }
            }
        }

        public class ActionBreath : Action
        {
            public enum BreathState
            {
                Start,
                Breath,
                End,
            }

            public override bool MouthOpen => State == BreathState.Breath || State == BreathState.End;

            public BreathState State;
            public float StartTime;
            public float BiteTime;
            public float EndTime;
            public Enemy Target;
            public Vector2 Offset;

            public ActionBreath(Snake snake, Enemy target, float startTime, float biteTime, float endTime) : base(snake)
            {
                StartTime = startTime;
                BiteTime = biteTime;
                EndTime = endTime;
                Target = target;
            }

            public override SegmentRender GetRenderSegment(SnakeSegment segment)
            {
                int bulgeLength = 8;
                float bulgeSpeed = 0.3f;
                switch (State)
                {
                    case (BreathState.Start):
                        int index = Snake.Segments.Count - (int)Math.Round(StartTime * bulgeSpeed);
                        return segment.Index < index && segment.Index > index - bulgeLength ? SegmentRender.Fat : SegmentRender.Normal;
                    case (BreathState.Breath):
                        return segment.Index > Snake.Segments.Count - (int)Math.Round(BiteTime * bulgeSpeed) && segment.Index > Snake.Segments.Count - bulgeLength ? SegmentRender.Fat : SegmentRender.Normal;
                    default:
                        return SegmentRender.Normal;
                }
            }


            public override void UpdateDelta(float delta)
            {
                switch (State)
                {
                    case (BreathState.Start):
                        StartTime -= delta;
                        if (StartTime < 0)
                            State = BreathState.Breath;
                        break;
                    case (BreathState.Breath):
                        BiteTime -= delta;
                        if (BiteTime < 0)
                            State = BreathState.End;
                        break;
                    case (BreathState.End):
                        EndTime -= delta;
                        if (EndTime < 0)
                            Snake.ResetState();
                        break;
                }
            }

            public override void UpdateDiscrete()
            {
                switch (State)
                {
                    case (BreathState.Start):
                        if (Math.Sign(Target.Position.X - Snake.Position.X) == GetFacingVector(Snake.Facing).X)
                        {
                            FindTarget();
                        }
                        else
                        {
                            Snake.ResetState();
                        }
                        Snake.Move(Offset, 0.5f);
                        break;
                    case (BreathState.Breath):
                        Snake.Move(Offset, 0.1f);
                        Snake.Move(Offset, 0.1f);
                        if (Math.Sign(Target.Position.X - Snake.Position.X) == GetFacingVector(Snake.Facing).X)
                        {
                            FindTarget();
                        }
                        var offset = GetFacingVector(Snake.Facing);
                        if ((int)BiteTime % 5 == 0)
                        {
                            float angle = Snake.Random.NextFloat() * MathHelper.TwoPi;
                            float distance = Snake.Random.NextFloat() * 3;
                            new PoisonBreath(Snake.World, Snake.Position + Snake.Head.Offset + offset * 8 + distance * new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle)))
                            {
                                Velocity = offset * 3.0f,
                                FrameEnd = 40,
                                Shooter = Snake,
                            };
                            /*new Fireball(Snake.World, Snake.Position + Snake.Head.Offset + offset * 8 + distance * new Vector2((float)Math.Sin(angle),(float)Math.Cos(angle)))
                            {
                                Velocity = offset * (Snake.Random.NextFloat() * 1.5f + 0.5f) + new Vector2(Snake.Velocity.X,0),
                                FrameEnd = 40,
                                Shooter = Snake,
                            };*/
                        }
                        break;
                    case (BreathState.End):
                        Snake.Move(Offset, 0.1f);
                        break;
                }
            }

            private void FindTarget()
            {
                var facing = GetFacingVector(Snake.Facing);
                Offset = new Vector2(Snake.Position.X + facing.X * 25, Target.Position.Y) - Snake.Position;
                Offset = Math.Min(Offset.Length(), 80) * Vector2.Normalize(Offset);
            }
        }

        public class ActionHit : Action
        {
            public int Time;
            public Vector2 Target;

            public ActionHit(Snake snake, Vector2 offset, int time) : base(snake)
            {
                Target = snake.Head.Offset + offset;
                Target = Math.Min(Target.Length(), 80) * Vector2.Normalize(Target);
                Time = time;
            }

            public override void UpdateDelta(float delta)
            {
                //NOOP
            }

            public override void UpdateDiscrete()
            {
                Snake.Move(Target, 0.3f);
                Snake.Move(Target, 0.3f);
                Snake.Move(Target, 0.3f);
                Time--;
                if (Time < 0)
                {
                    Snake.ResetState();
                }
            }
        }

        public class ActionDeath : Action
        {
            public int Time;
            public int TimeEnd;
            public Vector2 Target;
            public float SegmentCut;

            public ActionDeath(Snake snake, Vector2 offset, int time) : base(snake)
            {
                Target = snake.Head.Offset + offset;
                Time = time;
                TimeEnd = time;
                SegmentCut = 1;
            }

            public override SegmentRender GetRenderSegment(SnakeSegment segment)
            {
                return segment.Index < SegmentCut * Snake.Segments.Count ? SegmentRender.Normal : SegmentRender.Invisible;
            }

            public override void UpdateDelta(float delta)
            {
                SegmentCut -= delta / TimeEnd;
            }

            public override void UpdateDiscrete()
            {
                Snake.Move(Target, 0.3f);
                Snake.Move(Target, 0.3f);
                Snake.Move(Target, 0.3f);
                Time--;
                if (Time < 0)
                {
                    Snake.Destroy();
                }
                int index = MathHelper.Clamp((int)(SegmentCut * Snake.Segments.Count) - 1, 0, Snake.Segments.Count - 1);
                var segment = Snake.Segments[index];
                var size = new Vector2(8, 8);
                new FireEffect(Snake.World, Snake.Position + segment.Offset - size / 2 + new Vector2(Snake.Random.NextFloat() * size.X, Snake.Random.NextFloat() * size.Y), 0, 5);
            }
        }

        public IBox Box;
        public List<SnakeSegment> Segments = new List<SnakeSegment>();
        public SnakeSegment Head => Segments[(int)CurrentAction.HeadIndex];
        public Vector2 HeadOffset => GetHeadOffset();
        public Vector2 HeadPosition => Position + HeadOffset;
        public Vector2 PositionLast;
        public Vector2 Velocity => Position - PositionLast;

        public Action CurrentAction;

        public int Invincibility = 0;

        public override bool CanParry => false;
        public override bool Incorporeal => CurrentAction.Hidden;
        public override Vector2 HomingTarget => Position + Head.Offset;
        public override Vector2 PopupPosition => Position + Head.Offset;
        public override bool Dead => CurrentAction is ActionDeath;
        public override bool CanDamage => !CurrentAction.Hidden;
        public override bool CanHit => !CurrentAction.Hidden;

        public virtual Vector2 IdleOffset => -10 * GetFacingVector(Facing) + new Vector2(0, InCombat ? -30 : -15);
        public virtual Vector2 IdleCircle => InCombat ? new Vector2(20 * GetFacingVector(Facing).X, 10) : new Vector2(10 * GetFacingVector(Facing).X, 5);

        public Vector2 HomePosition;
        public HorizontalFacing HomeFacing;

        public HorizontalFacing Facing;
        public float MoveDelta;

        public bool InCombat => Target != null;
        public Player Target;
        public int TargetTime;

        public Snake(GameWorld world, Vector2 position) : base(world, position)
        {
            CurrentAction = new ActionIdle(this);
            InitHealth(80);
        }

        public override void Create(float x, float y)
        {
            base.Create(x, y);
            Box = World.Create(x - 8, y - 8, 16, 16);
            Box.AddTags(CollisionTag.NoCollision);
            Box.Data = this;

            for (int i = 0; i < 15; i++)
            {
                Segments.Add(new SnakeSegment(this, i));
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            World.Remove(Box);
        }

        public void ResetState()
        {
            CurrentAction = new ActionIdle(this);
        }

        private Vector2 GetHeadOffset()
        {
            int i = (int)Math.Floor(CurrentAction.HeadIndex);
            int e = (int)Math.Ceiling(CurrentAction.HeadIndex);
            float slide = CurrentAction.HeadIndex % 1;

            return Vector2.Lerp(Segments[i].Offset, Segments[e].Offset, slide);
        }

        public virtual void UpdateAI()
        {
            var viewSize = new Vector2(200, 100);
            RectangleF viewArea = new RectangleF(Position - viewSize / 2, viewSize);

            if (viewArea.Contains(World.Player.Position))
            {
                Target = World.Player;
                TargetTime = 200;
            }
            else
            {
                TargetTime--;
                if (TargetTime <= 0)
                    Target = null;
            }

            if (Target != null) //Engaged
            {
                float dx = Target.Position.X - Position.X;

                if (CurrentAction is ActionHidden)
                {
                    CurrentAction = new ActionUnhide(this, 30);
                }
                else if (CurrentAction is ActionHide)
                {

                }
                else if (CurrentAction is ActionUnhide)
                {

                }
                else if (CurrentAction is ActionIdle idle)
                {
                    if (dx < 0)
                    {
                        Facing = HorizontalFacing.Left;
                    }
                    else if (dx > 0)
                    {
                        Facing = HorizontalFacing.Right;
                    }

                    if (idle.Time > 180)
                    {
                        if (Random.NextDouble() < 0.4)
                            CurrentAction = new ActionSpit(this, Target, new Vector2(0, -70), 60, 20, 20, 20);
                        else
                            CurrentAction = new ActionBite(this, 40, 20, 30);
                    }
                }
            }
            else
            {
                if (CurrentAction is ActionHide)
                {

                }
                else if (CurrentAction is ActionUnhide)
                {

                }
                else if (CurrentAction is ActionIdle)
                {
                    CurrentAction = new ActionHide(this, 80);
                }
            }
        }

        protected override void UpdateDelta(float delta)
        {
            MoveDelta = Math.Min(MoveDelta + delta, 1.0f);
            Lifetime += delta;

            /*Vector2 wantedPosition = Position + new Vector2(-10, -30) + new Vector2((float)Math.Sin(Lifetime / 20f) * 40, (float)Math.Cos(Lifetime / 30f) * 20);

            wantedPosition = World.Player.Position;
            var wantedOffset = wantedPosition - Position;
            wantedOffset = Math.Min(wantedOffset.Length(), 80f) * Vector2.Normalize(wantedOffset);
            Move(wantedOffset);*/

            if (!Stunned)
                CurrentAction.UpdateDelta(delta);

            Box.Teleport(Position.X + Head.Offset.X - Box.Width / 2, Position.Y + Head.Offset.Y - Box.Height / 2);
        }

        private void Move(Vector2 offset, float speed)
        {
            float lastAngle = (float)Math.Atan2(offset.X, offset.Y);
            float lastDistance = offset.Length();
            foreach (SnakeSegment segment in Segments)
            {
                float angle = segment.EndAngle;
                float distance = segment.EndDistance;
                segment.MoveTowards(lastAngle, lastDistance, speed);
                lastAngle = angle;
                lastDistance = distance;
            }
        }

        protected override void UpdateDiscrete()
        {
            MoveDelta = 0;

            //if (!(CurrentAction is ActionHit))
            Invincibility--;

            foreach (SnakeSegment segment in Segments)
            {
                segment.UpdateDiscrete();
            }

            if (!Stunned)
                CurrentAction.UpdateDiscrete();

            PositionLast = Position;

            UpdateAI();
        }

        public override bool NoFriendlyFire(object hit)
        {
            return hit is Snake;
        }

        public override void Hit(Vector2 velocity, int hurttime, int invincibility, double damageIn)
        {
            if (Invincibility > 0 || Dead)
                return;
            Invincibility = invincibility / 10 + hurttime;
            CurrentAction = new ActionHit(this, velocity * 4, hurttime);
            PlaySFX(sfx_player_hurt, 1.0f, 0.1f, 0.3f);
            HandleDamage(damageIn);
            World.Hitstop = 6;
            Hitstop = 6;
            VisualOffset = OffsetHitStun(6);
            for (int i = 0; i < 3; i++)
                new BloodSpatterEffect(World, GetRandomPosition(Box.Bounds, Random), Random.NextFloat() * MathHelper.TwoPi, 3 + Random.NextFloat() * 5);
            new ScreenShakeJerk(World, AngleToVector(Random.NextFloat() * MathHelper.TwoPi) * 4, 3);
        }

        public override void Death()
        {
            base.Death();
            if (!(CurrentAction is ActionDeath))
            {
                new SnakeHead(World, Position + Head.Offset, GetFacingVector(Facing) * 2 + new Vector2(0, -4), Facing == HorizontalFacing.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, Facing == HorizontalFacing.Right ? 0.1f : -0.1f, 30);
                CurrentAction = new ActionDeath(this, GetFacingVector(Facing) * -24 + new Vector2(0, 0), 30);
            }
        }

        public override void DropItems(Vector2 position)
        {
            var drop = new DroppedItem(World, position, Meat.Snake);
            drop.Spread();
        }

        public override IEnumerable<Vector2> GetDrawPoints()
        {
            yield return Position;
            yield return Position + Head.Offset;
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var snakeHeadOpen = SpriteLoader.Instance.AddSprite("content/snake_open");
            var snakeHeadClosed = SpriteLoader.Instance.AddSprite("content/snake_closed");
            var snakeBody = SpriteLoader.Instance.AddSprite("content/snake_tail");
            var snakeBodyBig = SpriteLoader.Instance.AddSprite("content/snake_belly");
            foreach (var segment in Segments)
            {
                var render = CurrentAction.GetRenderSegment(segment);
                if (render == SegmentRender.Invisible)
                    continue;
                if (segment == Head)
                {
                    SpriteReference sprite;
                    if (CurrentAction.MouthOpen)
                        sprite = snakeHeadOpen;
                    else
                        sprite = snakeHeadClosed;
                    scene.DrawSprite(sprite, 0, HeadPosition - sprite.Middle + VisualOffset(), Facing == HorizontalFacing.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 1);
                    break;
                }
                else if (render == SegmentRender.Normal)
                {
                    scene.DrawSprite(snakeBody, 0, Position + segment.Offset - snakeBody.Middle, SpriteEffects.None, 1);
                }
                else if (render == SegmentRender.Fat)
                {
                    scene.DrawSprite(snakeBodyBig, 0, Position + segment.Offset - snakeBodyBig.Middle, SpriteEffects.None, 1);
                }
            }
        }
    }

}

using Humper;
using Humper.Base;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTower.Effects.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RogueTower.Game;
using static RogueTower.Util;


namespace RogueTower.Enemies
{
    class Hydra : EnemyGravity
    {
        public abstract class Action
        {
            public Hydra Hydra;

            public virtual bool MouthOpen => false;

            public Action(Hydra hydra)
            {
                Hydra = hydra;
            }

            public abstract void UpdateDelta(float delta);

            public abstract void UpdateDiscrete();
        }

        public class ActionIdle : Action
        {
            public float Time;

            public virtual float MinDistance => 160;
            public virtual float MaxDistance => 240;
            public override bool MouthOpen => true;

            public ActionIdle(Hydra hydra) : base(hydra)
            {

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

        public class ActionAggressive : ActionIdle
        {
            public override float MinDistance => 60;
            public override float MaxDistance => 90;

            public ActionAggressive(Hydra hydra) : base(hydra)
            {
            }
        }

        public class ActionDeath : Action
        {
            public int Time;

            public ActionDeath(Hydra hydra, int time) : base(hydra)
            {
                Time = time;
            }

            public override void UpdateDelta(float delta)
            {
                //NOOP
            }

            public override void UpdateDiscrete()
            {
                Time--;
                if (Time < 0)
                {
                    Hydra.Destroy();
                }
                var size = Hydra.Box.Bounds.Size;
                if (Time % 3 == 0)
                    new BigFireEffect(Hydra.World, Hydra.Position - size / 2 + new Vector2(Hydra.Random.NextFloat() * size.X, Hydra.Random.NextFloat() * size.Y), 0, 10);
            }
        }

        public Vector2 NeckPosition => Position + GetFacingVector(Facing) * 8 + new Vector2(0, -8);

        public List<SnakeHydra> Heads = new List<SnakeHydra>();
        public Action CurrentAction;

        public float WalkFrame;
        public HorizontalFacing Facing;

        public override bool CanParry => false;
        public override bool Incorporeal => false;
        public override Vector2 HomingTarget => Position;
        public override bool Dead => false;

        public override bool Stunned => Heads.All(x => x.Stunned);

        public bool InCombat => Target != null;

        public Player Target;
        public int TargetTime;

        public Hydra(GameWorld world, Vector2 position) : base(world, position)
        {
            CurrentAction = new ActionIdle(this);
        }

        public override void Create(float x, float y)
        {
            Box = World.Create(x - 8, y - 8, 16, 16);
            Box.AddTags(CollisionTag.NoCollision);
            Box.Data = this;

            int heads = 3;
            for (int i = 0; i < heads; i++)
            {
                Heads.Add(new SnakeHydra(this, (i + 1) % heads));
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            foreach (var head in Heads)
            {
                head.Destroy();
            }
            World.Remove(Box);
        }

        private void Walk(float dx, float speedLimit)
        {
            float adjustedSpeedLimit = speedLimit;
            float baseAcceleraton = 0.03f;
            if (OnGround)
                baseAcceleraton *= GroundFriction;
            float acceleration = baseAcceleraton;

            if (dx < 0 && Velocity.X > -adjustedSpeedLimit)
                Velocity.X = Math.Max(Velocity.X - acceleration, -adjustedSpeedLimit);
            if (dx > 0 && Velocity.X < adjustedSpeedLimit)
                Velocity.X = Math.Min(Velocity.X + acceleration, adjustedSpeedLimit);
            if (Math.Sign(dx) == Math.Sign(Velocity.X))
                AppliedFriction = 1;

            WalkFrame += Velocity.X * 0.25f;
        }

        private void WalkConstrained(float dx, float speedLimit) //Same as walk but don't jump off cliffs
        {
            float offset = Math.Sign(dx);
            if (Math.Sign(dx) == Math.Sign(Velocity.X))
                offset *= Math.Max(1, Math.Abs(Velocity.X));
            var floor = World.FindTiles(Box.Bounds.Offset(new Vector2(16 * offset, 1)));
            if (!floor.Any())
                return;
            Walk(dx, speedLimit);
        }

        private void UpdateAI()
        {
            var viewSize = new Vector2(500, 50);
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

            foreach (var head in Heads)
            {
                head.Target = Target;
            }

            if (Target != null) //Engaged
            {
                float dx = Target.Position.X - Position.X;
                float dy = Target.Position.Y - Position.Y;

                if (CurrentAction is ActionDeath)
                {

                }
                else if (CurrentAction is ActionIdle idle)
                {
                    if (idle.Time > 120)
                    {
                        if (Random.NextDouble() > 0.5)
                        {
                            CurrentAction = new ActionAggressive(this);
                        }
                        else
                        {
                            CurrentAction = new ActionIdle(this);
                        }
                    }

                    if (dx < 0)
                        Facing = HorizontalFacing.Left;
                    else if (dx > 0)
                        Facing = HorizontalFacing.Right;

                    foreach (var head in Heads)
                    {
                        if (head.CurrentAction is Snake.ActionIdle headIdle && headIdle.Time > 80)
                        {
                            head.Facing = Facing;
                            int attackingHeads = Heads.Count(x => !(x.CurrentAction is Snake.ActionIdle || x.CurrentAction is Snake.ActionDeath));
                            if (attackingHeads < 2)
                                SelectAttack(head);
                        }
                    }

                    float moveOffset = -Math.Sign(dx) * Target.Velocity.X * 32;
                    float preferredDistanceMin = idle.MinDistance + moveOffset;
                    float preferredDistanceMax = idle.MaxDistance + moveOffset;
                    if (Math.Abs(dx) > preferredDistanceMax * 1.5f)
                    {
                        WalkConstrained(dx, 1.0f);
                    }
                    else if (Math.Abs(dx) > preferredDistanceMax)
                    {
                        WalkConstrained(dx, 0.5f);
                    }
                    else if (Math.Abs(dx) < preferredDistanceMin)
                    {
                        WalkConstrained(-dx, 0.5f);
                    }
                }
            }
            else //Idle
            {

            }
        }

        private void SelectAttack(SnakeHydra head)
        {
            var weightedList = new WeightedList<Snake.Action>();
            weightedList.Add(new Snake.ActionIdle(head), 30);
            Vector2 dist = Target.Position - Position;
            if (Heads.Count(x => x.CurrentAction is Snake.ActionBite) < 2 && dist.LengthSquared() < 90 * 90)
                weightedList.Add(new Snake.ActionBite(head, 60 + Random.Next(60), 20, 60 + Random.Next(60)), 50);
            if (Heads.Count(x => x.CurrentAction is Snake.ActionSpit) < 1)
                weightedList.Add(new Snake.ActionSpit(head, Target, new Vector2(0, -70), 60, 20, 20, 20), 30);
            if (Heads.Count(x => x.CurrentAction is Snake.ActionBreath) < 1 && Math.Abs(dist.X) < 100)
                weightedList.Add(new Snake.ActionBreath(head, Target, 80, 120, 60), 30);
            head.CurrentAction = weightedList.GetWeighted(Random);
        }

        protected override void UpdateDelta(float delta)
        {
            Lifetime += delta;

            if (Active)
            {
                HandleMovement(delta);

                if (!Stunned)
                    CurrentAction.UpdateDelta(delta);
            }
        }

        protected override void UpdateDiscrete()
        {
            if (Active)
            {
                HandlePhysicsEarly();

                if (!Stunned)
                    CurrentAction.UpdateDiscrete();

                UpdateAI();

                HandlePhysicsLate();

                if (Heads.All(x => x.Destroyed))
                {
                    Death();
                }
            }
        }

        public override void Death()
        {
            base.Death();
            if (!(CurrentAction is ActionDeath))
                CurrentAction = new ActionDeath(this, 50);
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var hydraBody = SpriteLoader.Instance.AddSprite("content/hydra_body");
            scene.DrawSprite(hydraBody, (int)WalkFrame, Position - hydraBody.Middle + new Vector2(0, -4) + VisualOffset(), Facing == HorizontalFacing.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 1);
        }
    }

}

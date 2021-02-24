using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace RogueTower.Enemies
{
    /*
FUCK YOU BALTIMORE!
IF YOU'RE DUMB ENOUGH TO BUY A NEW CAR THIS WEEKEND, YOU'RE A BIG ENOUGH SCHMUCK TO COME TO BIG BILL HELL'S CARS!
BAD DEALS, CARS THAT BREAK DOWN, THIEVES!
IF YOU THINK YOUR GOING TO FIND A BARGAIN AT BIG BILL'S, YOU CAN KISS MY ASS!
IT'S OUR BELIEF THAT YOU'RE SUCH A STUPID MOTHERFUCKER THAT YOU'LL FALL FOR THIS BULLSHIT GUARANTEED!
IF YOU FIND A BETTER DEAL: SHOVE IT UP YOUR UGLY ASS! YOU HEARD US RIGHT: SHOVE IT UP YOUR UGLY ASS!
BRING YOUR TRADE, BRING YOUR TITLE, BRING YOUR WIFE, WE'LL FUCK HER! THAT'S RIGHT WE'LL FUCK YOUR WIFE!
BECAUSE AT BIG BILL HELL'S, YOU'RE FUCKED SIX WAYS FROM SUNDAY!
TAKE A HIKE TO BIG BILL HELL'S!
HOME OF CHALLENGE PISSING, THAT'S RIGHT, CHALLENGE PISSING. HOW DOES IT WORK?
IF YOU CAN PISS 6 FEET IN THE AIR STRAIGHT UP AND NOT GET WET, YOU GET NO DOWN PAYMENT.
DON'T WAIT, DON'T DELAY, DON'T FUCK WITH US OR WE'LL RIP YOUR NUTS OFF!
ONLY AT BIG BILL'S HELL, THE ONLY DEALER THAT TELLS YOU TO FUCK OFF.
HURRY UP, ASSHOLE!
THIS EVENT ENDS THE MINUTE YOU WRITE US A CHECK AND IT BETTER NOT BOUNCE OR YOU'RE A DEAD MOTHERFUCKER.
GO TO HELL.
BIG BILL HELL'S CARS
BALTIMORE'S FILTHIEST AND EXCLUSIVE HOME OF THE MEANEST SONS OF BITCHES IN THE STATE OF MARYLAND, GUARANTEED!!
    */
    class Limzard : Enemy
    {
        public class Segment
        {
            public List<Leg> Legs = new List<Leg>();
            public Vector2 Position;
            public float Angle;
            public float Width;
            public float FollowDistance;

            public Segment(Vector2 position, float width, float followDistance)
            {
                Position = position;
                Width = width;
                FollowDistance = followDistance;
            }

            public void MoveTowards(Vector2 nextPosition)
            {
                Vector2 Distance = nextPosition - Position;
                if (Distance.Length() > FollowDistance)
                    Position = nextPosition - Vector2.Normalize(Distance) * FollowDistance * 0.9f;
                Angle = Util.VectorToAngle(nextPosition - Position);
            }
        }

        public class Leg
        {
            public Random Random = new Random();
            public Segment Segment;
            public Vector2 StartPosition;
            public Vector2 EndPosition;
            public Vector2 IdealOffset;
            public float Length;
            public float Joint;
            public float JointHeight;
            public float MaxDistance;
            public Slider Frame = new Slider(0,1);

            public Vector2 Position => Vector2.Lerp(StartPosition, EndPosition, Frame.Slide);

            public Leg(Segment segment, Vector2 idealOffset, float length, float joint, float jointHeight, float maxDistance)
            {
                Segment = segment;
                Segment.Legs.Add(this);
                StartPosition = segment.Position;
                EndPosition = segment.Position;
                IdealOffset = idealOffset;
                Length = length;
                Joint = joint;
                JointHeight = jointHeight;
                MaxDistance = maxDistance;
            }

            public Vector2 GetIKJoint()
            {
                var start = Segment.Position;
                var end = Position;
                var a = Length * Joint;
                var b = Length * (1 - Joint);
                var c = Vector2.Distance(start, end);

                var h = MathHelper.Lerp(JointHeight, 0, MathHelper.Clamp(c / Length, 0, 1));
                var p = c * Joint;
                var angle = Util.VectorToAngle(end - start);
                return start + Util.RotateVector(new Vector2(h, p), angle);
            }

            public Vector2? GetTargetSpot(GameWorld world)
            {
                Vector2 idealTarget = Segment.Position + Util.RotateVector(IdealOffset, Segment.Angle);
                Tile gripTile = world.Map.FindTile(idealTarget);
                
                return gripTile.GetRandomPosition(Random);
            }

            public void MoveTowards(GameWorld world)
            {
                Vector2? idealTarget = GetTargetSpot(world);
                if (idealTarget.HasValue)
                {
                    var delta = idealTarget.Value - EndPosition;
                    var distance = delta.Length();
                    if (Frame.Done && distance > MaxDistance && Segment.Legs.Where(x => x != this).Any(x => x.Frame.Done))
                    {
                        StartPosition = EndPosition;
                        EndPosition = idealTarget.Value;
                        Frame = new Slider(Math.Min(distance, MaxDistance) / 5);
                    }
                }
            }
        }

        public Limzard(GameWorld world, Vector2 position) : base(world, position)
        {
            Segments.Add(new Segment(Vector2.Zero, 10, 12));
            Segments.Add(new Segment(Vector2.Zero, 8, 10));
            Segments.Add(new Segment(Vector2.Zero, 10, 12));
            Segments.Add(new Segment(Vector2.Zero, 8, 10));
            Segments.Add(new Segment(Vector2.Zero, 7, 10));
            Segments.Add(new Segment(Vector2.Zero, 6, 10));
            Segments.Add(new Segment(Vector2.Zero, 5, 10));
            Segments.Add(new Segment(Vector2.Zero, 4, 10));
            Segments.Add(new Segment(Vector2.Zero, 3, 10));
            Segments.Add(new Segment(Vector2.Zero, 2, 10));
            Segments.Add(new Segment(Vector2.Zero, 1, 10));

            new Leg(Segments[2], new Vector2(12, 24), 30, 0.5f, 15, 30);
            new Leg(Segments[2], new Vector2(-12, 24), 30, 0.5f, -15, 30);
            new Leg(Segments[6], new Vector2(12, 24), 30, 0.5f, 15, 30);
            new Leg(Segments[6], new Vector2(-12, 24), 30, 0.5f, -15, 30);
        }

        public List<Segment> Segments = new List<Segment>();
        public override bool Incorporeal => true;
        public override Vector2 HomingTarget => Position;
        public override Vector2 PopupPosition => Position;
        public override bool Dead => false;

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            foreach(var segment in Segments)
            {
                scene.DrawWireCircle(segment.Position, segment.Width, 20, Color.White);
                foreach(var leg in segment.Legs)
                {
                    scene.DrawWireCircle(leg.Position, 2, 20, Color.White);
                    scene.DrawWireLine(new[] { segment.Position, leg.GetIKJoint(), leg.Position }, Color.White);
                }
            }
        }

        protected override void UpdateDelta(float delta)
        {
            foreach(var segment in Segments)
            {
                foreach(var leg in segment.Legs)
                {
                    leg.Frame += delta;
                }
            }
            //NOOP
        }

        protected override void UpdateDiscrete()
        {
            var playerDelta = World.Player.Position - Position;
            if (playerDelta.Length() > 40)
                Position += Vector2.Normalize(playerDelta) * 4;

            Vector2 segmentPos = Position;
            foreach(var segment in Segments)
            {
                segment.MoveTowards(segmentPos);
                foreach(var leg in segment.Legs)
                {
                    leg.MoveTowards(World);
                }
                segmentPos = segment.Position;
            }
        }
    }
}

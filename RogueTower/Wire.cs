using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueTower
{
    interface IWireConnector
    {
        bool Powered
        {
            get;
        }
    }

    interface IWireNode : IWireConnector
    {
        Vector2 Position
        {
            get;
        }

        void ConnectIn(IWireConnector connector);

        void ConnectOut(IWireConnector connector);
    }

    class Wire : GameObject, IWireConnector
    {
        class Pulse
        {
            public float Start;
            public float End;

            public void Move(float speed)
            {
                Start += speed;
                End += speed;
                Start = MathHelper.Clamp(Start, 0, 1);
                End = MathHelper.Clamp(End, 0, 1);
            }
        }

        public override RectangleF ActivityZone => World.Bounds;
        public Vector2 Start => Input.Position;
        public Vector2 End => Output.Position;
        public float Length => (End - Start).Length();
        public IWireNode Input;
        public IWireNode Output;
        float Frame;

        List<Pulse> Pulses = new List<Pulse>();
        Pulse CurrentPulse;

        public override double DrawOrder => -10;
        public bool Powered => Pulses.Any(pulse => pulse.Start >= 1);

        public Wire(GameWorld world, IWireNode input, IWireNode output) : base(world)
        {
            Input = input;
            Output = output;
            Input.ConnectOut(this);
            Output.ConnectIn(this);
        }

        protected override void UpdateDelta(float delta)
        {
            foreach(var pulse in Pulses)
            {
                pulse.Move(2 * delta / Length);
            }
            Pulses.RemoveAll(pulse => pulse.End >= 1);
            if (Input != null && Input.Powered)
            {
                if (CurrentPulse == null)
                {
                    CurrentPulse = new Pulse();
                    Pulses.Add(CurrentPulse);
                }
                CurrentPulse.End = 0;
            }
            else
            {
                CurrentPulse = null;
            }
            Frame += delta;
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }

        public override IEnumerable<Vector2> GetDrawPoints()
        {
            yield return Start;
            yield return End;
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Wire;
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            var wire_on = SpriteLoader.Instance.AddSprite("content/wire_on");
            var wire_off = SpriteLoader.Instance.AddSprite("content/wire_off");
            var angle = Util.VectorToAngle(End - Start);

            scene.SpriteBatch.Draw(wire_off.Texture, Start, new Rectangle(0, 0, wire_off.Width, (int)Length), Color.White, angle, new Vector2(wire_off.Width / 2, Length), 1.0f, SpriteEffects.None, 0);
            foreach(var pulse in Pulses)
            {
                int start_height = (int)((int)Length * pulse.Start);
                int end_height = (int)((int)Length * pulse.End);
                int delta_height = start_height - end_height;
                scene.SpriteBatch.Draw(wire_on.Texture, Start, new Rectangle(0, -start_height + (int)(Frame * 0.5f), wire_on.Width, delta_height), Color.White, angle, new Vector2(wire_on.Width / 2, start_height), 1.0f, SpriteEffects.None, 0);
            }
        }
    }

    abstract class Node : GameObject, IWireNode
    {
        public override RectangleF ActivityZone => World.Bounds;
        public Vector2 Position
        {
            get;
            set;
        }
        public override double DrawOrder => -5;
        public virtual bool Powered => throw new NotImplementedException();

        public Node(GameWorld world, Vector2 position) : base(world)
        {
            Position = position;
        }

        public virtual void ConnectIn(IWireConnector connector)
        {
            //NOOP
        }

        public virtual void ConnectOut(IWireConnector connector)
        {
            //NOOP
        }

        protected override void UpdateDelta(float delta)
        {
            //NOOP
        }

        protected override void UpdateDiscrete()
        {
            //NOOP
        }

        public override IEnumerable<Vector2> GetDrawPoints()
        {
            yield return Position;
        }

        public override IEnumerable<DrawPass> GetDrawPasses()
        {
            yield return DrawPass.Wire;
        }

        public void DrawNode(SceneGame scene, float fill)
        {
            var node_off = SpriteLoader.Instance.AddSprite("content/wire_node_off");
            var node_on = SpriteLoader.Instance.AddSprite("content/wire_node_on");

            int fill_height = (int)Math.Round(node_on.Height * fill);
            int empty_height = node_on.Height - fill_height;

            scene.DrawSprite(node_off, 0, Position - node_off.Middle, SpriteEffects.None, 0);
            Vector2 middle = new Vector2(node_on.Width / 2, fill_height);
            scene.SpriteBatch.Draw(node_on.Texture, Position + new Vector2(0, node_on.Height / 2), new Rectangle(0, empty_height, node_on.Width, fill_height), Color.White, 0, middle, 1.0f, SpriteEffects.None, 0);
        }
    }

    class NodeGenerator : Node
    {
        public override bool Powered => true;

        public NodeGenerator(GameWorld world, Vector2 position) : base(world, position)
        {
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            DrawNode(scene, 1.0f);
        }
    }

    class NodeRandom : Node
    {
        public override bool Powered => RandomPower;
        public bool RandomPower;
        public Slider RandomSwitch = new Slider(50);

        public NodeRandom(GameWorld world, Vector2 position) : base(world, position)
        {
        }

        protected override void UpdateDelta(float delta)
        {
            RandomSwitch += delta;
            if(RandomSwitch.Done)
            {
                RandomSwitch.Time = 0;
                RandomPower = Random.NextDouble() > 0.5;
            }
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            DrawNode(scene, Powered ? 1.0f : 0.0f);
        }
    }

    class NodeCombine : Node
    {
        public List<IWireConnector> Inputs = new List<IWireConnector>();

        float Time;
        int MinInputs;
        public int ActiveInputs => Inputs.Count(x => x.Powered);
        public float Fill => Inputs.Any(x => x.Powered) ? MathHelper.Clamp((float)ActiveInputs / Math.Min(MinInputs, Inputs.Count), 0, 1) : 0;
        public override bool Powered => Inputs.Any(x => x.Powered) && ActiveInputs >= Math.Min(MinInputs, Inputs.Count);

        public NodeCombine(GameWorld world, Vector2 position, int minInputs) : base(world, position)
        {
            MinInputs = minInputs;
        }

        public override void ConnectIn(IWireConnector input)
        {
            Inputs.Add(input);
        }

        protected override void UpdateDelta(float delta)
        {
            Time += delta;
        }

        public override void Draw(SceneGame scene, DrawPass pass)
        {
            DrawNode(scene, Fill);
        }
    }
}

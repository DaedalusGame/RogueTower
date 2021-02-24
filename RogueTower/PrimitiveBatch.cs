using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    class PrimitiveBatch<T> : IDisposable where T : struct, IVertexType
    {
        BlendState BlendState;
        SamplerState SamplerState;
        DepthStencilState DepthStencilState;
        RasterizerState RasterizerState;
        Effect Effect;
        GraphicsDevice GraphicsDevice;

        Texture Texture;
        BasicEffect BasicEffect;
        EffectPass BasicPass;

        T[] Vertices;
        int PositionInBuffer;

        PrimitiveType PrimitiveType;
        int VerticesPerPrimitive;
        int MinVertices;

        bool HasBegun = false;
        bool IsDisposed = false;

        public PrimitiveBatch(GraphicsDevice graphicsDevice, int size = 500)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            GraphicsDevice = graphicsDevice;

            Vertices = new T[size];

            BasicEffect = new BasicEffect(graphicsDevice);
            BasicEffect.VertexColorEnabled = true;

            BasicEffect.Projection = Matrix.CreateOrthographicOffCenter
                (0, graphicsDevice.Viewport.Width,
                graphicsDevice.Viewport.Height, 0,
                0, 1);
            BasicEffect.World = Matrix.Identity;
            BasicEffect.View = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward,
                Vector3.Up);
            BasicPass = BasicEffect.CurrentTechnique.Passes[0];
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                if (BasicEffect != null)
                    BasicEffect.Dispose();

                IsDisposed = true;
            }
        }

        private void Setup()
        {
            var gd = GraphicsDevice;
            gd.BlendState = BlendState;
            gd.DepthStencilState = DepthStencilState;
            gd.RasterizerState = RasterizerState;
            gd.SamplerStates[0] = SamplerState;

            //BasicPass.Apply();
        }

        public void Begin(PrimitiveType primitiveType, Texture texture = null, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transform = null, Matrix? projection = null)
        {
            if (HasBegun)
            {
                throw new InvalidOperationException
                    ("End must be called before Begin can be called again.");
            }

            this.PrimitiveType = primitiveType;

            PositionInBuffer = 0;

            VerticesPerPrimitive = NumVertsPerPrimitive(primitiveType);
            MinVertices = MinVerts(primitiveType);

            BlendState = blendState ?? BlendState.AlphaBlend;
            SamplerState = samplerState ?? SamplerState.LinearClamp;
            DepthStencilState = depthStencilState ?? DepthStencilState.None;
            RasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            Effect = effect;
            Texture = texture;

            BasicEffect.World = transform ?? Matrix.Identity;
            BasicEffect.Projection = transform ?? Matrix.Identity;
            BasicEffect.View = Matrix.Identity;

            HasBegun = true;
        }

        public void End()
        {
            if (!HasBegun)
            {
                throw new InvalidOperationException
                    ("Begin must be called before End can be called.");
            }

            Flush();

            HasBegun = false;
        }

        private void Flush()
        {
            if (!HasBegun)
            {
                throw new InvalidOperationException
                    ("Begin must be called before Flush can be called.");
            }

            if (PositionInBuffer == 0)
            {
                return;
            }

            Setup();

            Draw(Effect, Texture);

            T v1 = Vertices[PositionInBuffer - 2];
            T v2 = Vertices[PositionInBuffer - 1];
            if (Vertices.Length % 2 == 1)
            {
                T h = v1;
                v1 = v2;
                v2 = h;
            }

            // now that we've drawn, it's ok to reset positionInBuffer back to zero,
            // and write over any vertices that may have been set previously.
            PositionInBuffer = 0;

            switch (PrimitiveType)
            {
                case (PrimitiveType.LineStrip):
                    AddVertex(v2);
                    break;
                case (PrimitiveType.TriangleStrip):
                    AddVertex(v1);
                    AddVertex(v2);
                    break;
            }
        }

        public void Draw(Effect effect, Texture texture)
        {
            int primitiveCount = (PositionInBuffer - MinVertices) / VerticesPerPrimitive;
            GraphicsDevice.Textures[0] = texture;

            if (effect != null)
            {
                var passes = effect.CurrentTechnique.Passes;
                foreach (var pass in passes)
                {
                    pass.Apply();

                    GraphicsDevice.Textures[0] = texture;

                    GraphicsDevice.DrawUserPrimitives<T>(PrimitiveType, Vertices, 0, primitiveCount);
                }
            }
            else
            {
                GraphicsDevice.DrawUserPrimitives<T>(PrimitiveType, Vertices, 0, primitiveCount);
            }
        }

        public void AddVertex(T vertex)
        {
            if (!HasBegun)
            {
                throw new InvalidOperationException
                    ("Begin must be called before AddVertex can be called.");
            }

            bool newPrimitive = ((PositionInBuffer % VerticesPerPrimitive) == 0);

            if (newPrimitive &&
                (PositionInBuffer + VerticesPerPrimitive) >= Vertices.Length)
            {
                Flush();
            }

            Vertices[PositionInBuffer] = vertex;
            PositionInBuffer++;
        }

        static private int NumVertsPerPrimitive(PrimitiveType primitive)
        {
            switch (primitive)
            {
                case PrimitiveType.LineList:
                    return 2;
                case PrimitiveType.TriangleList:
                    return 3;
                case PrimitiveType.LineStrip:
                case PrimitiveType.TriangleStrip:
                    return 1;
                default:
                    throw new InvalidOperationException("primitive is not valid");
            }
        }

        static private int MinVerts(PrimitiveType primitive)
        {
            switch (primitive)
            {
                case PrimitiveType.LineList:
                case PrimitiveType.TriangleList:
                    return 0;
                case PrimitiveType.LineStrip:
                    return 1;
                case PrimitiveType.TriangleStrip:
                    return 2;
                default:
                    throw new InvalidOperationException("primitive is not valid");
            }
        }
    }
}

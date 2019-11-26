using Humper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    class Map
    {
        public int Width, Height;
        public Tile[,] Tiles;

        public GameWorld World;
        public List<IBox> CollisionTiles = new List<IBox>();
        public bool CollisionDirty = true;

        public Map(GameWorld world, int width, int height)
        {
            SetWorld(world);
            SetSize(width, height);
        }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[Width, Height];
            Random random = new Random();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if(x > 10 && x <= Width - 10 && random.NextDouble() <= 0.3)
                        Tiles[x, y] = new Wall(this, x, y);
                    else
                        Tiles[x, y] = new EmptySpace(this,x,y);
                }
            }
        }

        public void SetWorld(GameWorld world)
        {
            World = world;
            CollisionTiles.Clear();
            CollisionDirty = true;
        }

        public void Update()
        {
            if (CollisionDirty)
            {
                UpdateCollisions();
                CollisionDirty = false;
            }
        }

        public void UpdateCollisions()
        {
            foreach (IBox box in CollisionTiles)
            {
                World.Remove(box);
            }

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tile tile = Tiles[x, y];
                    if (!tile.Passable)
                    {
                        IBox box = World.Create(x * 16, y * 16, 16, 16);
                        CollisionTiles.Add(box);
                    }
                }
            }

            IBox leftBox = World.Create(-16+1, 0, 16, Height * 16);
            IBox rightBox = World.Create(Width * 16 - 1, 0, 16, Height * 16);
            IBox baseBox = World.Create(0, Height * 16-1, Width * 16, 16);
            CollisionTiles.Add(leftBox);
            CollisionTiles.Add(rightBox);
            CollisionTiles.Add(baseBox);
        }
    }
}

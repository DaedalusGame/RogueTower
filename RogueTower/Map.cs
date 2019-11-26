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
                    var rand = random.NextDouble();
                    if (x > 10 && x <= Width - 10 && rand <= 0.2)
                    {
                        if (random.NextDouble() < 0.3)
                            Tiles[x, y] = new WallBlock(this, x, y);
                        else
                            Tiles[x, y] = new Wall(this, x, y);
                    }
                    else if (x > 8 && x <= Width - 8 && rand <= 0.3)
                        Tiles[x, y] = new WallIce(this, x, y);
                    else
                        Tiles[x, y] = new EmptySpace(this, x, y);
                }
            }

            for (int i = 0; i < 40; i++)
            {
                int spikewidth = random.Next(4) + 1;
                int spikex = 8 + random.Next(Width - spikewidth - 16);
                int spikey = random.Next(Height - 2) + 2;

                for (int x = 0; x < spikewidth; x++)
                {
                    for(int y = 1; y <= 2; y++)
                        Tiles[spikex + x, spikey - y] = new EmptySpace(this, spikex + x, spikey - y);
                    Tiles[spikex + x, spikey] = new Spike(this, spikex + x, spikey);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                int ladderheight = random.Next(15) + 3;
                int ladderx = 8 + random.Next(Width - 16);
                int laddery = random.Next(Height - ladderheight) ;
                HorizontalFacing ladderfacing = HorizontalFacing.Left;
                if(random.NextDouble() < 0.5)
                    ladderfacing = HorizontalFacing.Right;

                for (int y = 0; y < ladderheight; y++)
                {
                    int facingOffset = (ladderfacing == HorizontalFacing.Right ? 1 : -1);
                    Tiles[ladderx, laddery + y] = new Ladder(this, ladderx, laddery + y, ladderfacing);
                    Tiles[ladderx + facingOffset, laddery + y] = new Wall(this, ladderx + facingOffset, laddery + y);
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
                        IBox box = World.Create(tile.GetBoundingBox().Offset(x*16,y*16));
                        box.Data = tile;
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

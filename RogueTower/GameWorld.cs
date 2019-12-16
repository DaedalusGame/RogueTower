using Humper;
using Humper.Base;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    class GameWorld : World
    {
        public Map Map;
        public Player Player;
        public Queue<GameObject> ObjectQueue = new Queue<GameObject>();
        public List<GameObject> Objects = new List<GameObject>();
        public IEnumerable<Bullet> Bullets => Objects.OfType<Bullet>();
        public IEnumerable<VisualEffect> Effects => Objects.OfType<VisualEffect>();

        public Random Random = new Random();

        public float Hitstop;
        public float Frame;

        public int Width => Map.Width * 16;
        public int Height => Map.Height * 16;

        public GameWorld(int width, int height, float cellSize = 32) : base(width * 16, height * 16, cellSize)
        {
            Map = new Map(this, width, height);
        }

        public void Add(GameObject obj)
        {
            ObjectQueue.Enqueue(obj);
        }

        public IEnumerable<IBox> FindBoxes(float x, float y, float w, float h)
        {
            return this.FindBoxes(new RectangleF(x, y, w, h));
        }

        public IEnumerable<IBox> FindBoxes(RectangleF area)
        {
            return Find(area).Where(box => box.Bounds.Intersects(area)).ToList();
        }

        public IEnumerable<Tile> FindTiles(float x, float y, float w, float h)
        {
            return this.FindTiles(new RectangleF(x, y, w, h));
        }

        public IEnumerable<Tile> FindTiles(RectangleF area)
        {
            return FindBoxes(area).Where(box => box.Data is Tile).Select(box => (Tile)box.Data);
        }

        public void Update(float delta)
        {
            Map.Update();

            float globalDelta = delta;

            if (Hitstop > 0)
                globalDelta = 0;

            Frame += globalDelta;

            foreach (GameObject obj in Objects.ToList())
            {
                obj.Update(globalDelta);
            }

            Objects.RemoveAll(x => x.Destroyed);
            while (ObjectQueue.Count > 0)
            {
                var added = ObjectQueue.Dequeue();
                Objects.Add(added);
            }

            Hitstop -= 1;
        }
    }
}

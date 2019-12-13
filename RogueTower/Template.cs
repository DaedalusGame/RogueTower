using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    enum Direction
    {
        Up,
        Left,
        Down,
        Right,
    }

    enum TravelDirection
    {
        Any,
        Up,
        Left,
        Down,
        Right,
    }

    class Template
    {
        public static List<Template> Templates = new List<Template>();

        public string Name;
        public int Weight;
        public TravelDirection TravelDirection;

        public string Up;
        public string Down;
        public string Right;
        public string Left;

        public int[,] Foreground;
        public int[,] Background;

        public int Connections => GetConnections();
        public List<JObject> Entities = new List<JObject>();

        public string GetConnection(Direction dir)
        {
            switch(dir)
            {
                default:
                    return "none";
                case (Direction.Up):
                    return Up;
                case (Direction.Down):
                    return Down;
                case (Direction.Left):
                    return Left;
                case (Direction.Right):
                    return Right;
            }
        } 

        public int GetConnections()
        {
            int i = 4;
            foreach(Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (GetConnection(direction) == "none")
                    i--;
            }
            return i;
        }

        public static void LoadAll()
        {
            Templates.Clear();
            foreach(var jsonFile in Directory.EnumerateFiles("content/templates", "*.json"))
            {
                Template template = new Template();
                template.Load(jsonFile);
                Templates.Add(template);
            }
        }

        public void Load(string filename)
        {
            var stream = File.OpenText(filename);

            var reader = new JsonTextReader(stream);
            JObject root = JObject.Load(reader);

            Name = Path.GetFileNameWithoutExtension(filename);

            var values = root["values"];

            Up = values["ConnectionUp"].ToObject<string>();
            Down = values["ConnectionDown"].ToObject<string>();
            Left = values["ConnectionLeft"].ToObject<string>();
            Right = values["ConnectionRight"].ToObject<string>();
            Weight = int.Parse(values["Weight"].ToObject<string>());
            TravelDirection = (TravelDirection)Enum.Parse(typeof(TravelDirection), values["Direction"].ToObject<string>());

            foreach (var layer in root["layers"])
            {
                var name = layer["name"].ToObject<string>();
                if(name == "Foreground")
                {
                    int width = layer["gridCellsX"].ToObject<int>();
                    int height = layer["gridCellsY"].ToObject<int>();
                    Foreground = new int[width,height];
                    int i = 0;
                    foreach(var tile in layer["data"])
                    {
                        int x = i % width;
                        int y = i / width;
                        Foreground[x, y] = tile.ToObject<int>(); 
                        i++;
                    }
                }
                if (name == "Background")
                {
                    int width = layer["gridCellsX"].ToObject<int>();
                    int height = layer["gridCellsY"].ToObject<int>();
                    Background = new int[width, height];
                    int i = 0;
                    foreach (var tile in layer["data"])
                    {
                        int x = i % width;
                        int y = i / width;
                        Background[x, y] = tile.ToObject<int>();
                        i++;
                    }
                }
                if (name == "Entities")
                {
                    foreach (var entity in layer["entities"])
                    {
                        Entities.Add((JObject)entity);
                    }
                }
            }

            reader.Close();
        }

        public void PrintEntity(JObject entity, GameWorld world, int px, int py)
        {
            string type = entity["name"].ToObject<string>();
            float ox = entity["x"].ToObject<float>();
            float oy = entity["y"].ToObject<float>();
            var values = entity["values"];

            if (type == "ball_and_chain") //TODO: make a dictionary of (name -> generator delegate)
            {
                float rotation = entity["rotation"].ToObject<float>();
                float speed = values["Speed"].ToObject<float>();
                float distance = values["Distance"].ToObject<float>();
                bool swings = values["Swings"].ToObject<bool>();

                var ballandchain = new BallAndChain(world, new Vector2(px * 16 + ox, py * 16 + oy), MathHelper.ToRadians(rotation), speed, distance);
                ballandchain.Swings = swings;
            }
            if(type == "moaiman")
            {
                bool flipped = entity["flippedX"].ToObject<bool>();
                var moaiman = new MoaiMan(world, new Vector2(px * 16 + ox, py * 16 + oy));
                moaiman.Facing = flipped ? HorizontalFacing.Left : HorizontalFacing.Right;
            }
            if (type == "cannon")
            {
                float rotation = entity["rotation"].ToObject<float>();
                float delay = values["Delay"].ToObject<float>();
                var cannon = new CannonFire(world, new Vector2(px * 16 + ox, py * 16 + oy), MathHelper.ToRadians(rotation));
                cannon.DelayTime = delay;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

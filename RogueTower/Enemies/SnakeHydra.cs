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
    class SnakeHydra : Snake
    {
        public Hydra Body;
        public int Index;

        public override Vector2 Position
        {
            get
            {
                return Body.NeckPosition;
            }
            set
            {
                //NOOP
            }
        }

        public override Vector2 IdleOffset => new Vector2(0, -20) + 15 * AngleToVector(-MathHelper.PiOver2 + MathHelper.PiOver2 / Body.Heads.Count + MathHelper.Pi * Index / Body.Heads.Count);
        //public override Vector2 IdleCircle => new Vector2(15,5);

        public SnakeHydra(Hydra body, int index) : base(body.World, body.Position)
        {
            Body = body;
            Index = index;
        }

        public override void UpdateAI()
        {
            //NOOP
        }
    }
}

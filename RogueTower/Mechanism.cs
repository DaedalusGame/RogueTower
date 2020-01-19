using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace RogueTower
{
    class Mechanism
    {
        public static Mechanism None = new Mechanism();
    }

    class ChainDestroy : Mechanism
    {

    }

    class ChainDestroyDirection : ChainDestroy
    {
        public Direction Direction;

        public ChainDestroyDirection(Direction direction)
        {
            Direction = direction;
        }
    }

    class ChainDestroyStart : ChainDestroy
    {
        
    }
}

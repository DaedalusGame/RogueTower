using Humper.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    interface IParryGiver
    {
        void ParryGive(IParryReceiver receiver, RectangleF box);
    }

    interface IParryReceiver
    {
        bool CanParry
        {
            get;
        }

        void ParryReceive(IParryGiver giver, RectangleF box);
    }
}

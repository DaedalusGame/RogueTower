﻿using RogueTower.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower.Items.Interfaces
{
    interface IEdible
    {
        bool CanEat(Enemy enemy);

        void EatEffect(Enemy enemy);
    }
}

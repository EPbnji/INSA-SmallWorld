﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modele
{
    public class UniteElite : Unite
    {
        public UniteElite()
        {
            pointsAttaque = 3;
            pointsDefense = 1;
            pointsDepl = 1;
            pointsVie = 1;
            pointsVieMax = pointsVie;
            strategySuggestion = new SuggAgressif();
        }

        public override int getDeplSuppl()
        {
            return 2;
        }
    }
}

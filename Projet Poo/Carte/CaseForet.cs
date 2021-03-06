﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibCLR;

namespace Modele
{
    [Serializable]
    public class CaseForet : Case
    {
        public override string ToString()
        {
            return "Forêt";
        }
        public override int bonusPoints(Peuple p)
        {
            if (p is PeupleNain)
                return 2;
            else if (p is PeupleElfe)
                return 0;
            else
                return base.bonusPoints(p);
        }
        public override int toInt()
        {
            return (int)CaseInt.Foret;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimZoo
{
   public class Utilities
    {
        public static bool IsZooAnimal(Pawn p)
        {
            var comp = p.GetComp<PawnComp_IsZooPawn>();
            return comp == null || comp.isZooPawn;
        }
    }
}

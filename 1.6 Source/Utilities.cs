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
            if (p == null || p.RaceProps?.Animal != true)
                return false;

            var comp = p.GetComp<PawnComp_IsZooPawn>();
            return comp != null && comp.isZooPawn;
        }

        public static List<CompExhibitMarker> GetAllExhibitMarkers(Map map)
        {
            var result = new List<CompExhibitMarker>();
            if (map == null) return result;

            foreach (Building building in map.listerBuildings.allBuildingsAnimalPenMarkers)
            {
                var comp = building.TryGetComp<CompExhibitMarker>();
                if (comp != null)
                {
                    result.Add(comp);
                }
            }
            return result;
        }
    }

}

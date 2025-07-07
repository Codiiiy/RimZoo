using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimZoo
{
    public static class RimZoo_Logic
    {
        public static float global_Happiness = 0f;
        public static float total_Rarity = 0f;
        public static float Rating = 0f;
        public static float Variety = 0f;
        public static int Price = 0;
        public static float scaled_Rating;
        public static bool zooOpen = true;
        public static bool[] openHours = new bool[24];


        static RimZoo_Logic()
        {
            for (int i = 8; i < 17; i++)
            {
                openHours[i] = true;
            }
        }
        public static List<CompExhibitMarker> FindAllPens()
        {
            List<CompExhibitMarker> pens = new List<CompExhibitMarker>();

            foreach (var map in Find.Maps)
            {
                foreach (var building in map.listerBuildings.allBuildingsColonist)
                {
                    var compExhibitMarker = building.TryGetComp<CompExhibitMarker>();
                    if (compExhibitMarker != null && compExhibitMarker.PenState.Enclosed)
                    {
                        pens.Add(compExhibitMarker);
                    }
                }
            }
            return pens;
        }


        public static void UpdateRates()
        {
            List<CompExhibitMarker> AllPens = FindAllPens();
            var validPens = AllPens.Where(p => p.Happiness > 0).ToList();


            global_Happiness = validPens.Any() ? validPens.Average(p => p.Happiness) : 0f;


            Variety = AllPens.Select(p => p.selectedAnimal?.defName)
                             .Where(defName => defName != null)
                             .Distinct()
                             .Count();

            total_Rarity = AllPens.Sum(p => p.Rarity);


            Rating = Variety * total_Rarity * global_Happiness;


            scaled_Rating = Mathf.Clamp((Rating / 20000f) * 4.9f + 0.1f, 0.1f, 5f);


            Price = (int)((scaled_Rating - 0.1f) / (5 - 0.1f) * (RimZooMain.settings?.priceMultiplier ?? 1));

        }

        public static int GetPrice()
        {
            UpdateRates();
            return Price;
        }
        public static bool GetZooOpen()
        {
            return zooOpen;
        }
        public static bool GetZooHours(int hour)
        {
            return openHours[hour];
        }

        public static class ExhibitAnimalTracker
        {
            public static HashSet<Pawn> ExhibitAnimals = new HashSet<Pawn>();
            [System.ThreadStatic]
            public static Pawn CurrentPawn;
        }

        public static List<IntVec3> AllAutoCutCells
        {
            get
            {
                List<IntVec3> cells = new List<IntVec3>();
                foreach (var pen in FindAllPens())
                {
                    cells.AddRange(pen.GetAutoCutCells());
                }
                return cells;
            }
        }

    }

}

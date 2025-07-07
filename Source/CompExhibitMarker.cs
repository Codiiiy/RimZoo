using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using static Verse.PenFoodCalculator;

namespace RimZoo
{
    public class CompExhibitMarker : CompAnimalPenMarker
    {
        private bool highlightEnabled = false;
        public List<PenAnimalInfo> ExhibitAnimalsInfos => CachedFoodCalculator != null ? CachedFoodCalculator.ActualAnimalInfos : new List<PenAnimalInfo>();
        public ThingDef selectedAnimal;
        private PenFoodCalculator CachedFoodCalculator
        {
            get
            {
                FieldInfo field = typeof(CompAnimalPenMarker).GetField("cachedFoodCalculator", BindingFlags.Instance | BindingFlags.NonPublic);
                return field?.GetValue(this) as PenFoodCalculator;
            }
        }
        public List<Pawn> AssignedPawns
        {
            get
            {
                IEnumerable<IntVec3> penCells = GetAutoCutCells();
                List<Pawn> pawns = new List<Pawn>();

                if (penCells == null || penCells.Count() == 0 || parent.Map == null || selectedAnimal == null)
                    return pawns;

                foreach (var cell in penCells)
                {
                    var cellPawns = cell.GetThingList(parent.Map)
                                         .OfType<Pawn>()
                                         .Where(p => p.def == selectedAnimal);
                    pawns.AddRange(cellPawns);
                }

                return pawns;
            }
        }

        public float Rarity
        {
            get
            {
                if (selectedAnimal == null || parent?.Map == null || AssignedPawnCount == 0)
                    return 0f;
                return selectedAnimal.GetStatValueAbstract(StatDefOf.MarketValue);
            }
        }
        public float Happiness
        {
            get
            {
                if (parent?.Map == null || selectedAnimal == null)
                {
                    //Log.Warning(" CompExhibitMarker: Happiness calculation failed due to null parent or selected animal.");
                    return 0f;
                }
                IEnumerable<IntVec3> penCells = GetAutoCutCells();
                List<Pawn> pawns = new List<Pawn>();
                foreach (var cell in penCells)
                {
                    var cellPawns = cell.GetThingList(parent.Map).OfType<Pawn>().Where(p => p.def == selectedAnimal);
                    pawns.AddRange(cellPawns);
                }
                if (!pawns.Any())
                    return 0f;
                float penRating = GetPenRating();
                float totalHappiness = 0f;
                foreach (var pawn in pawns)
                {
                    //Log.Warning($"CompExhibitMarker: Pawn {pawn.Name} health percentage: {healthPct}");
                    float foodPct = pawn.needs?.food?.CurLevelPercentage ?? 0f;
                    float pawnHappiness = (GetPawnHealth(pawn) * 0.33f) + (foodPct * 0.33f) + (penRating * 0.33f);
                    totalHappiness += pawnHappiness;
                }
                return totalHappiness / pawns.Count;
            }
        }

        private float GetPawnHealth(Pawn pawn)
        {
            if (pawn == null || !pawn.RaceProps.Animal)
                return 0f;

            var capacities = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
            float total = 0f;
            int count = 0;

            foreach (var cap in capacities)
            {
                if (pawn.health.capacities.CapableOf(cap))
                {
                    total += pawn.health.capacities.GetLevel(cap);
                    count++;
                }
            }

            total += 1f - pawn.health.hediffSet.PainTotal;
            count++;

            return count > 0 ? total / count : 0f;
        }

        public int AssignedPawnCount
        {
            get
            {
                if (parent?.Map == null || selectedAnimal == null)
                    return 0;
                IEnumerable<IntVec3> penCells = GetAutoCutCells();
                List<Pawn> pawns = new List<Pawn>();
                foreach (var cell in penCells)
                {
                    var cellPawns = cell.GetThingList(parent.Map).OfType<Pawn>().Where(p => p.def == selectedAnimal);
                    pawns.AddRange(cellPawns);
                }
                return pawns.Count;
            }
        }

        public float GetPenRating()
        {
            if (CachedFoodCalculator == null)
                return 0f;
            if (CachedFoodCalculator.Unenclosed)
                return 0f;
            if (CachedFoodCalculator.numCellsSoil < 50)
                return 0.25f;
            if (CachedFoodCalculator.numCellsSoil < 100)
                return 0.5f;
            if (CachedFoodCalculator.numCellsSoil < 400)
                return 0.75f;
            return 1.0f;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (selectedAnimal == null)
                selectedAnimal = ThingDefOf.Thrumbo;
            ForceDisplayedAnimalDefs.Clear();
            AnimalFilter.SetDisallowAll();
            ForceDisplayedAnimalDefs.Add(selectedAnimal);
            AnimalFilter.SetAllow(selectedAnimal, true);
            base.PostSpawnSetup(respawningAfterLoad);


        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder(base.CompInspectStringExtra());
            sb.AppendLine();
            sb.Append("Selected Animal: ");
            sb.Append(selectedAnimal != null ? selectedAnimal.label : "None");
            sb.AppendLine();
            sb.Append("Rarity: ");
            sb.Append(Rarity.ToString("F2"));
            sb.AppendLine();
            sb.Append("Happiness: ");
            sb.Append(Happiness.ToString("F2"));
            sb.AppendLine();
            sb.Append("Pawn Count: ");
            sb.Append(AssignedPawnCount);
            sb.AppendLine();
            sb.AppendLine("Happiness Breakdown:");
            sb.Append(GetHappinessBreakdown());
            return sb.ToString().Trim();
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref selectedAnimal, "selectedAnimal");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "Select Animal",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/ExhibitToggle", true),
                action = OpenSpeciesSelectionMenu
            };
            ToggleSelectAnimal(selectedAnimal);

            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Highlight Exhibit Area",
                    action = HighlightValidFenceSpots
                };
            }
        }

        public void ToggleSelectAnimal(ThingDef newAnimal)
        {
            selectedAnimal = newAnimal;
            ForceDisplayedAnimalDefs.Clear();
            AnimalFilter.SetDisallowAll();
            ForceDisplayedAnimalDefs.Add(selectedAnimal);
            AnimalFilter.SetAllow(selectedAnimal, true);
            CachedFoodCalculator?.ResetAndProcessPen(this);
        }

        private string GetHappinessBreakdown()
        {
            if (parent?.Map == null || selectedAnimal == null)
                return "No data";
            IEnumerable<IntVec3> penCells = GetAutoCutCells();
            List<Pawn> pawns = new List<Pawn>();
            foreach (var cell in penCells)
            {
                var cellPawns = cell.GetThingList(parent.Map).OfType<Pawn>().Where(p => p.def == selectedAnimal);
                pawns.AddRange(cellPawns);
            }
            if (!pawns.Any())
                return "No animals";
            float avgHealth = pawns.Average(p => GetPawnHealth(p));
            float avgFood = pawns.Average(p => p.needs?.food?.CurLevelPercentage ?? 0f);
            float penRating = GetPenRating();
            float overall = (avgHealth * 0.33f) + (avgFood * 0.33f) + (penRating * 0.33f);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Overall Happiness: {overall:F2}");
            sb.AppendLine($"Health (33%): {avgHealth:F2}");
            sb.AppendLine($"Food (33%): {avgFood:F2}");
            sb.AppendLine($"Pen Size (33%): {penRating:F2}");
            return sb.ToString();
        }

        private void OpenSpeciesSelectionMenu()
        {
            List<FloatMenuOption> options = DefDatabase<ThingDef>.AllDefs.Where(d => d.race?.Animal == true && !d.IsCorpse).OrderBy(d => d.label).Select(def => new FloatMenuOption(def.label, () => ToggleSelectAnimal(def))).ToList();
            Find.WindowStack.Add(new FloatMenu(options));
        }

        public HashSet<IntVec3> CalculateValidFenceSpots()
        {

            if (parent?.Map == null)
            {
                return new HashSet<IntVec3>();
            }

            HashSet<IntVec3> validFenceSpots = new HashSet<IntVec3>();
            Map map = parent.Map;

            var autoCutCells = GetAutoCutCells();

            int minX = autoCutCells.Min(cell => cell.x);
            int maxX = autoCutCells.Max(cell => cell.x);
            int minZ = autoCutCells.Min(cell => cell.z);
            int maxZ = autoCutCells.Max(cell => cell.z);


            minX = Math.Max(minX - 2, 0);
            maxX = maxX + 2;
            minZ = Math.Max(minZ - 2, 0);
            maxZ = maxZ + 2;

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    IntVec3 cell = new IntVec3(x, 0, z);

                    if ((x == minX || x == maxX || z == minZ || z == maxZ) && !RimZoo_Logic.AllAutoCutCells.Contains(cell) && cell.GetEdifice(map) == null)
                    {
                        validFenceSpots.Add(cell);

                    }
                }
            }

            return validFenceSpots;
        }

        public void HighlightValidFenceSpots()
        {
            highlightEnabled = !highlightEnabled;
            if (!highlightEnabled)
            {

                return;
            }

            if (parent?.Map == null)
            {

                return;
            }
            IEnumerable<IntVec3> fenceSpots = CalculateValidFenceSpots();
            if (!fenceSpots.Any())
            {
                return;
            }

            Color highlightColor = new Color(0.2f, 1f, 0.2f, 0.3f);

            foreach (var cell in fenceSpots)
            {
                if (cell.InBounds(parent.Map))
                {
                    Vector3 drawPos = cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                    Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, SolidColorMaterials.SimpleSolidColorMaterial(highlightColor, false), 0);
                }
            }

            foreach (var cell in fenceSpots)
            {
                if (cell.InBounds(parent.Map))
                {
                    Vector3 drawPos = cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                    Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, SolidColorMaterials.SimpleSolidColorMaterial(highlightColor, false), 0);
                }
            }
        }

    }

}
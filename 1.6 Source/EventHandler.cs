using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimZoo
{
    public class EventHandler : MapComponent
    {
        private int tickCounter;
        private int quadrumTickCounter;
        private const int TicksPerDay = GenDate.TicksPerDay;
        private int randomEventsPerDay;
        private List<int> nextArrivalTicks = new List<int>();
        private List<int> nextMaddenedTicks = new List<int>();
        public List<Pawn> SpawnedGuests = new List<Pawn>();

        public EventHandler(Map map) : base(map)
        {
            ScheduleRandomArrivals();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (++tickCounter >= TicksPerDay)
            {
                tickCounter = 0;
                randomEventsPerDay = Rand.RangeInclusive(1, 3);
                ScheduleRandomArrivals();
                CheckMentalChance();
            }

            if (RimZoo_Logic.zooOpen)
            {
                while (nextArrivalTicks.Count > 0 && tickCounter == nextArrivalTicks[0])
                {
                    int hour = GenLocalDate.HourInteger(map);
                    if (RimZoo_Logic.GetZooHours(hour))
                        TriggerZooGuestArrival();
                    nextArrivalTicks.RemoveAt(0);
                }
            }

            while (nextMaddenedTicks.Count > 0 && quadrumTickCounter == nextMaddenedTicks[0])
            {
                TriggerMaddened();
                nextMaddenedTicks.RemoveAt(0);
            }
        }

        public void RegisterSpawnedGuests(IEnumerable<Pawn> guests)
        {
            foreach (var p in guests)
                if (!SpawnedGuests.Contains(p))
                    SpawnedGuests.Add(p);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tickCounter, "tickCounter");
            Scribe_Values.Look(ref quadrumTickCounter, "quadrumTickCounter");
            Scribe_Values.Look(ref randomEventsPerDay, "randomEventsPerDay");
            Scribe_Collections.Look(ref nextArrivalTicks, "nextArrivalTicks", LookMode.Value);
            Scribe_Collections.Look(ref SpawnedGuests, "spawnedGuests", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (nextArrivalTicks == null)
                    nextArrivalTicks = new List<int>();
                if (nextMaddenedTicks == null)
                    nextMaddenedTicks = new List<int>();
                if (SpawnedGuests == null)
                    SpawnedGuests = new List<Pawn>();
            }
        }

        private void ScheduleRandomArrivals()
        {
            nextArrivalTicks.Clear();
            for (int i = 0; i < randomEventsPerDay; i++)
                nextArrivalTicks.Add(Rand.Range(1, TicksPerDay - 1));
            nextArrivalTicks.Sort();
        }

        private static void TriggerZooGuestArrival()
        {
            if (RimZoo_Logic.FindAllPens().Count == 0)
            {
                //Log.Warning("No pens found for Zoo Guests.");
                return;
            }
            var map = Find.CurrentMap;
            if (map == null)
                return;

            var incidentDef = DefDatabase<IncidentDef>.GetNamed("ZooGuestsArrive", false);
            if (incidentDef != null)
            {
                var parms = new IncidentParms { target = map };
                incidentDef.Worker.TryExecute(parms);
            }
            else
            {
                Log.Warning("ZooGuestsArrive incident not found. Please check your mod setup.");
            }
        }

        public void TriggerMaddened()
        {
            var pens = RimZoo_Logic.FindAllPens();
            if (pens.Count == 0)
            {
                return;
            }

            float threshold = RimZooMain.settings?.MentalThreshold ?? 1f;

            List<CompExhibitMarker> lowHappinessPens = new List<CompExhibitMarker>();

            foreach (var pen in pens)
            {
                if (pen.Happiness < threshold)
                {
                    lowHappinessPens.Add(pen);
                }
            }

            if (lowHappinessPens.Count == 0)
                return;

            CompExhibitMarker chosenPen = lowHappinessPens.RandomElement();

            List<Pawn> pawnsToMadden = new List<Pawn>();
            IEnumerable<IntVec3> penCells = chosenPen.GetAutoCutCells();

            foreach (var cell in penCells)
            {
                var cellPawns = cell.GetThingList(chosenPen.parent.Map)
                    .OfType<Pawn>()
                    .Where(p => p.def == chosenPen.selectedAnimal && p.RaceProps.Animal);
                pawnsToMadden.AddRange(cellPawns);
            }

            bool anyMaddened = false;

            foreach (var pawn in pawnsToMadden)
            {
                if (pawn.mindState != null && pawn.Spawned)
                {
                    if (pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter))
                    {
                        anyMaddened = true;
                    }
                }
            }

            if (anyMaddened)
            {
                Messages.Message(
                    "Some animals have gone mad due to poor exhibit conditions!",
                    chosenPen.parent,
                    MessageTypeDefOf.ThreatSmall
                );
            }
        }
        private void CheckMentalChance()
        {
            float threshold = RimZooMain.settings?.MaddenedChance ?? 1f;

            if (Rand.Chance(threshold))
            {
                TriggerMaddened();
            }
        }
    }
}

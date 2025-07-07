using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimZoo
{
    public class IncidentWorker_ZooGuestsArrive : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            int currentHour = GenLocalDate.HourInteger(map);

            return RimZoo_Logic.zooOpen
                && RimZoo_Logic.openHours[currentHour]
                && RimZoo_Logic.FindAllPens().Any(p => p.AssignedPawnCount > 0);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            int groupSize = Rand.RangeInclusive(1, 7);
            List<Pawn> guests = new List<Pawn>();
            IntVec3 spawnCell = CellFinder.RandomEdgeCell(map);
            var exhibits = RimZoo_Logic.FindAllPens();

            List<Faction> validFactions = Find.FactionManager.AllFactions
                .Where(f => !f.IsPlayer && f.RelationWith(Faction.OfPlayer)?.kind != FactionRelationKind.Hostile)
                .ToList();

            Faction guestFaction = validFactions.Any() ? validFactions.RandomElement() : Faction.OfAncients;

            for (int i = 0; i < groupSize; i++)
            {
                Pawn guest = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, guestFaction);
                guests.Add(guest);
                GenSpawn.Spawn(guest, spawnCell, map);
            }

            if (exhibits != null && exhibits.Count > 0)
            {
                foreach (Pawn guest in guests)
                {
                    Job visitJob = JobMaker.MakeJob(RimZoo_JobDefOf.VisitExhibitMarker);

                    List<LocalTargetInfo> validTargets = new List<LocalTargetInfo>();
                    foreach (var exhibit in exhibits)
                    {

                        IntVec3 visitSpot = FindVisitSpot(exhibit);
                        validTargets.Add(new LocalTargetInfo(visitSpot));
                    }

                    visitJob.targetQueueA = validTargets;
                    guest.jobs.StartJob(visitJob, JobCondition.None, null, false, true);
                }
            }

            string letterText = $"A group of {groupSize} guests has arrived at your zoo. They are now exploring the exhibits.";
            Find.LetterStack.ReceiveLetter("Zoo Guests Arrive", letterText, LetterDefOf.PositiveEvent);
            return true;
        }
        private IntVec3 FindVisitSpot(CompExhibitMarker exhibit)
        {
            List<IntVec3> validSpots = exhibit.CalculateValidFenceSpots().ToList();


            if (validSpots.Count > 0)
            {
                return validSpots.RandomElement();
            }
            else
            {
                return exhibit.parent.Position.RandomAdjacentCell8Way();
            }
        }

        private bool AlreadyTracked(Pawn p)
        {
            var handler = (p.Map?.GetComponent<EventHandler>());
            return handler != null && handler.SpawnedGuests.Contains(p);
        }

        [DefOf]
        class RimZoo_JobDefOf
        {
            public static JobDef VisitExhibitMarker;
        }
    }
}

using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimZoo
{
    public class JobDriver_VisitExhibits : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFail) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOn(() => job.targetQueueA == null || job.targetQueueA.Count == 0);

            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);

            Toil gotoExhibit = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            yield return gotoExhibit;

            Toil wait = Toils_General.Wait(Rand.Range(360, 2160));
            wait.WithProgressBarToilDelay(TargetIndex.A);
            yield return wait;

            Toil checkNext = new Toil();
            checkNext.initAction = () =>
            {
                if (job.targetQueueA != null && job.targetQueueA.Count > 0)
                {
                    ReadyForNextToil();
                    JumpToToil(gotoExhibit);
                }
            };
            yield return checkNext;

            Toil dropSilver = Toils_General.Do(() =>
            {
                Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                silver.stackCount = RimZoo_Logic.GetPrice();
                GenPlace.TryPlaceThing(silver, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            });
            yield return dropSilver;

            Toil leaveMap = new Toil();
            leaveMap.initAction = () =>
            {
                if (RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 exit, TraverseMode.PassDoors))
                {
                    pawn.pather.StartPath(exit, PathEndMode.OnCell);
                }
            };
            leaveMap.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            yield return leaveMap;
        }
    }
}

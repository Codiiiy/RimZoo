using Verse;

namespace RimZoo
{
    public class PawnComp_IsZooPawn : ThingComp
    {
        public bool isZooPawn = false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isZooPawn, "isZooPawn", false);
        }
    }
}

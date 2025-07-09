using LudeonTK;
using RimWorld;
using Verse;

namespace RimZoo
{
    public static class DebugActionsRimZoo
    {
        [DebugAction("RimZoo", "Trigger Zoo Guests", actionType = DebugActionType.Action)]
        private static void TriggerZooGuestArrival()
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                Log.Warning("No active map found.");
                return;
            }

            IncidentDef incidentDef = DefDatabase<IncidentDef>.GetNamed("ZooGuestsArrive", false);
            if (incidentDef != null)
            {
                IncidentParms parms = new IncidentParms { target = map };
                incidentDef.Worker.TryExecute(parms);
                Log.Message("Zoo Guests Arrival triggered via Debug Menu.");
                /*for (int i = 0; i < RimZoo_Logic.openHours.Length; i++)
                {
                    Log.Message($"Bool[{i}] = {RimZoo_Logic.openHours[i]}");
                }*/

            }
            else
            {
                Log.Warning("ZooGuestsArrive IncidentDef not found.");
            }
        }
        [DebugAction("RimZoo", "Trigger Maddened", actionType = DebugActionType.Action)]
        private static void TriggerMaddened()
        {
            Map map = Find.CurrentMap;
            if (map == null) return;
            EventHandler handler = map.GetComponent<EventHandler>();
            if (handler != null)
            {
                handler.TriggerMaddened();
            }
        }

    }
}

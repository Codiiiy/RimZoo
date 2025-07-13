using LudeonTK;
using RimWorld;
using Verse;

namespace RimZoo
{
    public static class DebugActionsRimZoo
    {
        [DebugAction(category: "RimZoo", name: "Trigger Zoo Guests", allowedGameStates = AllowedGameStates.Playing)]
        private static void TriggerZooGuestArrival()
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                Log.Warning("[RimZoo] No active map found.");
                return;
            }

            IncidentDef incidentDef = DefDatabase<IncidentDef>.GetNamed("ZooGuestsArrive", false);
            if (incidentDef != null)
            {
                IncidentParms parms = new IncidentParms { target = map };
                Log.Message($"[RimZoo] Triggering ZooGuestsArrive incident on map: {map} with parms target={parms.target}");

                bool success = incidentDef.Worker.TryExecute(parms);
                Log.Message($"[RimZoo] ZooGuestsArrive incident executed: {success}");

                if (success)
                {
                    Log.Message("Zoo Guests Arrival triggered via Debug Menu.");
                }
                else
                {
                    Log.Warning("[RimZoo] ZooGuestsArrive incident failed to execute.");
                }
            }
            else
            {
                Log.Warning("[RimZoo] ZooGuestsArrive IncidentDef not found.");
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

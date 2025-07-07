using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimZoo
{
    [HarmonyPatch(typeof(AnimalPenUtility), "IsRopeManagedAnimalDef")]
    public static class Patch_IsRopeManagedAnimalDef
    {
        static bool Prefix(ThingDef td, ref bool __result)
        {
            if (td.race != null && td.race.Animal && td.thingCategories.Contains(ThingCategoryDefOf.Animals))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AnimalPenUtility), "ShouldBePennedByDefault")]
    public static class Patch_ShouldBePennedByDefault
    {
        static bool Prefix(ThingDef td, ref bool __result)
        {
            if (td.race != null && td.race.Animal && td.thingCategories.Contains(ThingCategoryDefOf.Animals))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
    public static class CustomFilter
    {
        public static ThingFilter GetCustomFilter()
        {
            ThingFilter filter = new ThingFilter();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.race != null && def.race.FenceBlocked && def.race.Animal && def.thingCategories.Contains(ThingCategoryDefOf.Animals))
                {
                    filter.SetAllow(def, true);
                }
            }
            return filter;
        }
    }

    [HarmonyPatch(typeof(ITab_PenAnimals), "FillTab")]
    public static class Patch_ITab_PenAnimals_FillTab
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            MethodInfo targetMethod = AccessTools.Method(typeof(AnimalPenUtility), nameof(AnimalPenUtility.GetFixedAnimalFilter));
            MethodInfo customMethod = AccessTools.Method(typeof(CustomFilter), nameof(CustomFilter.GetCustomFilter));

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Calls(targetMethod))
                {
                    code[i] = new CodeInstruction(OpCodes.Call, customMethod);
                }
            }

            return code;
        }
    }

    [HarmonyPatch(typeof(CompAnimalPenMarker), "PostSpawnSetup")]
    public static class Patch_CompPenAnimalMarker_PostSpawnSetup
    {
        static void Postfix(CompAnimalPenMarker __instance)
        {
            if (__instance.AnimalFilter == null) return;

            var allowedDefs = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(def =>
                    def.race != null &&
                    def.race.FenceBlocked &&
                    def.race.Animal &&
                    def.thingCategories != null &&
                    def.thingCategories.Contains(ThingCategoryDefOf.Animals)
                );

            __instance.AnimalFilter.SetDisallowAll();
            foreach (var def in allowedDefs)
            {
                __instance.AnimalFilter.SetAllow(def, true);
            }
        }
    }
    [HarmonyPatch(typeof(Thing), "BlocksPawn")]
    public static class Patch_BlocksPawn
    {
        static void Postfix(Thing __instance, Pawn p, ref bool __result)
        {

            if (p?.CurJob != null && p.CurJob.def.defName.ToLowerInvariant().Contains("rope"))
            {
                return;
            }

            if (__result) return;
            if (p == null || __instance == null) return;


            if (__instance is Building_Door door)
            {
                if (!door.Open)
                {
                    __result = true;
                    return;
                }
            }

            CompExhibitMarker comp = __instance.TryGetComp<CompExhibitMarker>();
            if (comp != null && comp.selectedAnimal == p.def)
            {
                __result = true;
                return;
            }


            Map map = __instance.Map;
            if (map != null)
            {
                foreach (Building building in map.listerBuildings.allBuildingsColonist)
                {
                    if (!building.Spawned) continue;

                    var compExhibit = building.TryGetComp<CompExhibitMarker>();
                    if (compExhibit != null && compExhibit.selectedAnimal == p.def)
                    {
                        __result = true;
                        return;
                    }
                }
            }

        }
        [HarmonyPatch(typeof(Building_Door), "PawnCanOpen")]
        public static class Patch_PawnCanOpen
        {
            static void Postfix(Building_Door __instance, Pawn p, ref bool __result)
            {
                if (p == null || __instance == null)
                {
                    return;
                }

                if (p?.CurJob != null && p.CurJob.def.defName.ToLowerInvariant().Contains("rope"))
                {
                    return;
                }


                Map map = __instance.Map;
                if (map != null)
                {
                    foreach (Building building in map.listerBuildings.allBuildingsColonist)
                    {
                        CompExhibitMarker markerComp = building.TryGetComp<CompExhibitMarker>();
                        if (markerComp != null && markerComp.selectedAnimal == p.def)
                        {
                            __result = false;
                            return;
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(AnimalPenGUI), nameof(AnimalPenGUI.DoAllowedAreaMessage))]
        public static class Patch_AnimalPenGUI_DoAllowedAreaMessage
        {
            static bool Prefix(Rect rect, Pawn pawn)
            {
                var currentPen = AnimalPenUtility.GetCurrentPenOf(pawn, allowUnenclosedPens: false);

                Log.Warning($"[ZooMod][DoAllowedAreaMessage] Pawn: {pawn.LabelShort}, Pen: {(currentPen?.label ?? "null")}, Type: {(currentPen?.GetType().Name ?? "null")}");

                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Tiny;

                string label = "(Unpenned)";
                string tooltip = "This animal is not inside any pen.";

                if (currentPen != null)
                {
                    if (currentPen is CompExhibitMarker)
                    {
                        Log.Warning($"[ZooMod][DoAllowedAreaMessage] Pawn {pawn.LabelShort} is in an Exhibit.");
                        label = "In Exhibit: " + currentPen.label;
                        tooltip = label;
                    }
                    else
                    {
                        Log.Warning($"[ZooMod][DoAllowedAreaMessage] Pawn {pawn.LabelShort} is in a regular Pen.");
                        label = "In Pen: " + currentPen.label;
                        tooltip = label;
                    }
                }
                else if (AnimalPenUtility.NeedsToBeManagedByRope(pawn))
                {
                    Log.Warning($"[ZooMod][DoAllowedAreaMessage] Pawn {pawn.LabelShort} needs to be roped.");
                    label = "Needs to be roped to a pen";
                    tooltip = label;
                }
                else if (pawn.RaceProps.Dryad)
                {
                    Log.Warning($"[ZooMod][DoAllowedAreaMessage] Pawn {pawn.LabelShort} is a Dryad.");
                    label = "Cannot assign allowed area to dryad";
                    tooltip = label;
                }

                GUI.color = Color.gray;
                Widgets.Label(rect, label);
                TooltipHandler.TipRegion(rect, tooltip);
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;

                return false;
            }
        }

        [HarmonyPatch(typeof(PawnColumnWorker_AllowedArea), "DoCell")]
        public static class Patch_DoCell_ExhibitCheck
        {
            static bool Prefix(Rect rect, Pawn pawn, PawnTable table)
            {
                var currentPen = AnimalPenUtility.GetCurrentPenOf(pawn, allowUnenclosedPens: false);

                Log.Warning($"[ZooMod][DoCell] Pawn: {pawn.LabelShort}, Pen: {(currentPen?.label ?? "null")}, Type: {(currentPen?.GetType().Name ?? "null")}");

                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Tiny;

                string label = "(Unpenned)";
                string tooltip = "This animal is not inside any pen.";

                if (currentPen != null)
                {
                    if (currentPen is CompExhibitMarker)
                    {
                        Log.Warning($"[ZooMod][DoCell] Pawn {pawn.LabelShort} is in an Exhibit.");
                        label = "In Exhibit: " + currentPen.label;
                        tooltip = label;
                    }
                    else
                    {
                        Log.Warning($"[ZooMod][DoCell] Pawn {pawn.LabelShort} is in a regular Pen.");
                        label = "In Pen: " + currentPen.label;
                        tooltip = label;
                    }
                }
                else if (pawn.playerSettings != null && pawn.playerSettings.SupportsAllowedAreas)
                {
                    Log.Warning($"[ZooMod][DoCell] Pawn {pawn.LabelShort} supports allowed areas, showing selectors.");
                    AreaAllowedGUI.DoAllowedAreaSelectors(rect, pawn);
                    return false;
                }
                else if (AnimalPenUtility.NeedsToBeManagedByRope(pawn))
                {
                    Log.Warning($"[ZooMod][DoCell] Pawn {pawn.LabelShort} needs to be roped.");
                    label = "Needs to be roped to a pen";
                    tooltip = label;
                }
                else if (pawn.RaceProps.Dryad)
                {
                    Log.Warning($"[ZooMod][DoCell] Pawn {pawn.LabelShort} is a Dryad.");
                    label = "Cannot assign allowed area to dryad";
                    tooltip = label;
                }

                GUI.color = Color.gray;
                Widgets.Label(rect, label);
                TooltipHandler.TipRegion(rect, tooltip);
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;

                return false;
            }
        }

    }
}
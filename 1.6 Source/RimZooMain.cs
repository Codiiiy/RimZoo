using HarmonyLib;
using UnityEngine;
using Verse;

namespace RimZoo
{

    public class RimZooMain : Mod
    {
        public static RimZooMainSettings settings;

        public RimZooMain(ModContentPack content) : base(content)
        {
            settings = GetSettings<RimZooMainSettings>();
            var harmony = new Harmony("com.rimzoo.exhibitharmonypatch");
            harmony.PatchAll();
        }

        public override string SettingsCategory()
        {
            return "RimZoo";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }
    }
}

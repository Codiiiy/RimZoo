using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimZoo
{
    [HarmonyPatch(typeof(MainTabWindow_Animals), "DoWindowContents")]
    public static class MainTabWindow_Animals_DoWindowContents_Patch
    {
        public static void Postfix(Rect rect)
        {
            float buttonWidth = Mathf.Min(rect.width, 260f);
            float buttonHeight = 32f;
            float x = rect.x + buttonWidth + 200f;
            float y = rect.y;
            Rect rimZooRect = new Rect(x, y, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(rimZooRect, "Manage RimZoo"))
            {
                if (!Find.WindowStack.IsOpen<Dialog_RimZoo>())
                {
                    Find.WindowStack.Add(new Dialog_RimZoo());
                }
            }
        }
    }
}

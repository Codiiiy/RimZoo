using UnityEngine;
using Verse;

namespace RimZoo
{
    [StaticConstructorOnStartup]
    public static class RimZoo_Textures
    {
        public static readonly Texture2D ExhibitToggleIcon;
        public static readonly Texture2D PawnToggleIcon;

        static RimZoo_Textures()
        {
            ExhibitToggleIcon = ContentFinder<Texture2D>.Get("RimZoo/UI/Icons/ExhibitToggle", true);
            PawnToggleIcon = ContentFinder<Texture2D>.Get("RimZoo/UI/Icons/PawnToggle", true);
        }
    }
}

using UnityEngine;
using Verse;

namespace RimZoo
{
    [StaticConstructorOnStartup]
    public static class RimZoo_Textures
    {
        public static readonly Texture2D ExhibitToggleIcon;

        static RimZoo_Textures()
        {
            ExhibitToggleIcon = ContentFinder<Texture2D>.Get("UI/Icons/ExhibitToggle", true);
        }
    }
}

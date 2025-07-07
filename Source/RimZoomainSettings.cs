using UnityEngine;
using Verse;

namespace RimZoo
{
    public class RimZooMainSettings : ModSettings
    {
        public float priceMultiplier = 1.0f;
        public int visitDurationMinutes = 2;
        public float MentalThreshold = 0.5f;
        public float MaddenedChance = 0.01f;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref priceMultiplier, "priceMultiplier", 1.0f);
            Scribe_Values.Look(ref visitDurationMinutes, "visitDurationMinutes", 2);
            Scribe_Values.Look(ref MentalThreshold, "MentalThreshold", 0.5f);
            Scribe_Values.Look(ref MaddenedChance, "MaddenedChance", 0.01f);
        }

        public void DoWindowContents(Rect inRect)
        {
            float curY = inRect.y;
            float lineHeight = 30f;
            float spacing = 10f;
            float sliderHeight = 24f;

            Widgets.Label(new Rect(inRect.x, curY, inRect.width, lineHeight), "RimZoo Settings");
            curY += lineHeight + spacing * 2;

            Widgets.Label(new Rect(inRect.x, curY, inRect.width, lineHeight), "Price Multiplier: " + priceMultiplier.ToString("F2"));
            curY += lineHeight;
            priceMultiplier = Widgets.HorizontalSlider(new Rect(inRect.x, curY, inRect.width, sliderHeight), priceMultiplier, 1f, 100.0f, false);
            curY += sliderHeight + spacing;


            Widgets.Label(new Rect(inRect.x, curY, inRect.width, lineHeight), "Visit Duration: " + visitDurationMinutes + " minutes");
            curY += lineHeight;
            visitDurationMinutes = Mathf.RoundToInt(Widgets.HorizontalSlider(new Rect(inRect.x, curY, inRect.width, sliderHeight), visitDurationMinutes, 30, 360, true));
            curY += sliderHeight + spacing;

            Widgets.Label(new Rect(inRect.x, curY, inRect.width, lineHeight), "Animal Mental Threshold: " + MentalThreshold.ToString("F2"));
            curY += lineHeight;
            MentalThreshold = Widgets.HorizontalSlider(new Rect(inRect.x, curY, inRect.width, sliderHeight), MentalThreshold, 0.1f, 1.0f, false);
            curY += sliderHeight + spacing;

            Widgets.Label(new Rect(inRect.x, curY, inRect.width, lineHeight), "Maddened Event Chance: " + MaddenedChance.ToString("F2"));
            curY += lineHeight;
            MaddenedChance = Widgets.HorizontalSlider(new Rect(inRect.x, curY, inRect.width, sliderHeight), MaddenedChance, 0.01f, 0.3f, false);
            curY += sliderHeight + spacing;
        }


        public int GetVisitDurationTicks()
        {
            return visitDurationMinutes * 60;
        }
    }
}

using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimZoo
{
    public class Dialog_RimZoo : Window
    {
        public override Vector2 InitialSize => new Vector2(700f, 600f);
        private bool dragging = false;
        private bool dragSetTo = false;
        private int? dragStartIndex = null;
        private Vector2 scrollPosition = Vector2.zero;

        public Dialog_RimZoo()
        {
            forcePause = true;
            doCloseButton = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            RimZoo_Logic.UpdateRates();
            float y = 10f;
            Rect globalRect = new Rect(10, y, inRect.width - 20, 40);
            GUI.BeginGroup(globalRect);
            float x = 10f;
            float labelWidth = (globalRect.width - 20) / 4f;
            Widgets.Label(new Rect(x, 5, labelWidth, 30), $"Happiness: {RimZoo_Logic.global_Happiness:F2}");
            x += labelWidth;
            Widgets.Label(new Rect(x, 5, labelWidth, 30), $"Variety: {RimZoo_Logic.Variety}");
            x += labelWidth;
            Widgets.Label(new Rect(x, 5, labelWidth, 30), $"Scaled Rating: {RimZoo_Logic.scaled_Rating:F2}");
            x += labelWidth;
            Widgets.Label(new Rect(x, 5, labelWidth, 30), $"Price: {RimZoo_Logic.Price:F2}");
            GUI.EndGroup();
            y += globalRect.height + 10f;

            Widgets.CheckboxLabeled(new Rect(10, y, 200, 30), "Zoo Open", ref RimZoo_Logic.zooOpen);
            y += 40f;

            Widgets.Label(new Rect(10, y, 200, 30), "Operating Hours:");
            y += 30f;

            float hourWidth = (inRect.width - 20) / 24f;

            for (int i = 0; i < 24; i++)
            {
                Rect numberRect = new Rect(10 + i * hourWidth, y, hourWidth - 2, 20);
                Widgets.Label(numberRect, i.ToString());
            }
            y += 20f;

            for (int i = 0; i < 24; i++)
            {
                Rect hourRect = new Rect(10 + i * hourWidth, y, hourWidth - 2, 30);
                Color hourColor = RimZoo_Logic.openHours[i] ? Color.green : Color.gray;
                Widgets.DrawBoxSolid(hourRect, hourColor);

                if (Widgets.ButtonInvisible(hourRect))
                {
                    RimZoo_Logic.openHours[i] = !RimZoo_Logic.openHours[i];
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }

                if (dragging && Event.current.type == EventType.MouseDrag && Mouse.IsOver(hourRect))
                {
                    if (RimZoo_Logic.openHours[i] != dragSetTo)
                    {
                        RimZoo_Logic.openHours[i] = dragSetTo;
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                }

                if (Mouse.IsOver(hourRect))
                {
                    Widgets.DrawHighlight(hourRect);
                }
            }

            if (Event.current.type == EventType.MouseUp && dragging)
            {
                dragging = false;
                dragStartIndex = null;
            }

            y += 50f;

            float scrollViewHeight = inRect.height - y - 20f;
            Rect scrollViewRect = new Rect(10, y, inRect.width - 20, scrollViewHeight);

            int itemCount = RimZoo_Logic.FindAllPens().Count;
            float contentHeight = itemCount * 35f;

            Rect contentRect = new Rect(0, 0, scrollViewRect.width - 16, contentHeight);

            Widgets.BeginScrollView(scrollViewRect, ref scrollPosition, contentRect);

            float itemY = 0f;
            foreach (var pen in RimZoo_Logic.FindAllPens())
            {
                string exhibitName = pen.selectedAnimal?.defName ?? "No animal assigned";
                string exhibitInfo = $"Species: {exhibitName}, Happiness: {pen.Happiness:F2}, Rarity: {pen.Rarity:F2}";
                Rect exhibitRect = new Rect(0, itemY, contentRect.width, 30);
                Widgets.DrawMenuSection(exhibitRect);
                if (Widgets.ButtonInvisible(exhibitRect))
                {
                    JumpToExhibit(pen);
                }
                Widgets.Label(new Rect(exhibitRect.x + 5, exhibitRect.y + 5, exhibitRect.width - 10, exhibitRect.height - 10), exhibitInfo);
                itemY += 35f;
            }

            Widgets.EndScrollView();
        }

        private void JumpToExhibit(CompExhibitMarker pen)
        {
            if (pen?.parent?.Position != null)
            {
                CameraJumper.TryJumpAndSelect(pen.parent);
            }
        }
    }
}

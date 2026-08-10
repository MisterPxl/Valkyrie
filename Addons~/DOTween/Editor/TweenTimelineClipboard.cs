using System.Collections.Generic;
using UnityEditor;

namespace Valkyrie.DOTween.Editor
{
    public static class TweenTimelineClipboard
    {
        private static TweenTimeline _timeline;

        public static bool HasTimeline
        {
            get { return _timeline != null; }
        }

        public static void Copy(TweenTimeline timeline)
        {
            _timeline = TweenTimelineCloneUtility.Clone(timeline);
        }

        public static void Paste(TweenPlayer player)
        {
            if (player == null || _timeline == null)
            {
                return;
            }

            Undo.RecordObject(player, "Paste Tween Timeline");
            player.SetTimeline(TweenTimelineCloneUtility.Clone(_timeline));
            EditorUtility.SetDirty(player);
        }

        public static void DuplicateLastStep(TweenPlayer player)
        {
            if (player == null || player.Timeline == null || player.Timeline.Steps.Count == 0)
            {
                return;
            }

            TweenTimeline clone = TweenTimelineCloneUtility.Clone(player.Timeline);
            IList<TweenStepDefinition> steps = clone.Steps;
            TweenStepDefinition last = steps[steps.Count - 1];
            if (last == null)
            {
                return;
            }

            TweenTimeline singleStepTimeline = new TweenTimeline();
            singleStepTimeline.Steps.Add(last);
            TweenStepDefinition duplicated = TweenTimelineCloneUtility.Clone(singleStepTimeline).Steps[0];
            duplicated.RegenerateId();
            steps.Add(duplicated);

            Undo.RecordObject(player, "Duplicate Tween Step");
            player.SetTimeline(clone);
            EditorUtility.SetDirty(player);
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace Valkyrie.DOTween.Editor
{
    public static class TweenTimelineCloneUtility
    {
        public static TweenTimeline Clone(TweenTimeline source)
        {
            if (source == null)
            {
                return new TweenTimeline();
            }

            TimelineCloneContainer sourceContainer = ScriptableObject.CreateInstance<TimelineCloneContainer>();
            TimelineCloneContainer targetContainer = ScriptableObject.CreateInstance<TimelineCloneContainer>();
            try
            {
                sourceContainer.Timeline = source;
                string json = EditorJsonUtility.ToJson(sourceContainer);
                EditorJsonUtility.FromJsonOverwrite(json, targetContainer);
                return targetContainer.Timeline ?? new TweenTimeline();
            }
            finally
            {
                Object.DestroyImmediate(sourceContainer);
                Object.DestroyImmediate(targetContainer);
            }
        }

        private sealed class TimelineCloneContainer : ScriptableObject
        {
            [SerializeField] private TweenTimeline _timeline;

            public TweenTimeline Timeline
            {
                get { return _timeline; }
                set { _timeline = value; }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween.Editor
{
    public sealed class TweenStepEditorSummary
    {
        public int Index { get; }
        public TweenStepDefinition Step { get; }
        public string TypeName { get; }
        public string FullTypeName { get; }
        public bool Enabled { get; }
        public bool HasEstimatedDuration { get; }
        public float EstimatedDuration { get; }
        public bool HasEase { get; }
        public Ease Ease { get; }
        public int Loops { get; }
        public TweenPlacementMode PlacementMode { get; }
        public float InsertAt { get; }
        public string TargetBindingKey { get; }

        public TweenStepEditorSummary(
            int index,
            TweenStepDefinition step,
            string typeName,
            string fullTypeName,
            bool enabled,
            bool hasEstimatedDuration,
            float estimatedDuration,
            bool hasEase,
            Ease ease,
            int loops,
            TweenPlacementMode placementMode,
            float insertAt,
            string targetBindingKey)
        {
            Index = index;
            Step = step;
            TypeName = typeName;
            FullTypeName = fullTypeName;
            Enabled = enabled;
            HasEstimatedDuration = hasEstimatedDuration;
            EstimatedDuration = estimatedDuration;
            HasEase = hasEase;
            Ease = ease;
            Loops = loops;
            PlacementMode = placementMode;
            InsertAt = insertAt;
            TargetBindingKey = targetBindingKey;
        }
    }

    public sealed class TweenTimelineEntry
    {
        public TweenStepEditorSummary Summary { get; }
        public float StartTime { get; }
        public float Duration { get; }

        public TweenTimelineEntry(TweenStepEditorSummary summary, float startTime, float duration)
        {
            Summary = summary;
            StartTime = startTime;
            Duration = duration;
        }
    }

    public sealed class TweenTimelineModel
    {
        public IReadOnlyList<TweenTimelineEntry> Entries { get; }
        public float Duration { get; }

        public TweenTimelineModel(IReadOnlyList<TweenTimelineEntry> entries, float duration)
        {
            Entries = entries;
            Duration = duration;
        }
    }

    public static class TweenSequenceEditorAnalysis
    {
        public static IReadOnlyList<TweenStepEditorSummary> AnalyzeSteps(IList<TweenStepDefinition> steps)
        {
            int count = steps != null ? steps.Count : 0;
            List<TweenStepEditorSummary> summaries = new List<TweenStepEditorSummary>(count);

            for (int index = 0; index < count; index++)
            {
                summaries.Add(AnalyzeStep(index, steps[index]));
            }

            return summaries;
        }

        public static TweenTimelineModel BuildTimeline(IList<TweenStepDefinition> steps)
        {
            IReadOnlyList<TweenStepEditorSummary> summaries = AnalyzeSteps(steps);
            List<TweenTimelineEntry> entries = new List<TweenTimelineEntry>(summaries.Count);
            float sequenceEnd = 0f;
            float lastInsertionStart = 0f;
            bool hasInsertion = false;

            for (int index = 0; index < summaries.Count; index++)
            {
                TweenStepEditorSummary summary = summaries[index];
                float startTime = sequenceEnd;
                float duration = summary.HasEstimatedDuration && IsFinite(summary.EstimatedDuration)
                    ? Mathf.Max(0f, summary.EstimatedDuration)
                    : 0f;

                if (summary.Enabled && summary.Step != null)
                {
                    switch (summary.PlacementMode)
                    {
                        case TweenPlacementMode.Join:
                            startTime = hasInsertion ? lastInsertionStart : 0f;
                            break;
                        case TweenPlacementMode.Insert:
                            startTime = Mathf.Max(0f, summary.InsertAt);
                            break;
                        case TweenPlacementMode.Append:
                        default:
                            startTime = sequenceEnd;
                            break;
                    }

                    lastInsertionStart = startTime;
                    hasInsertion = true;
                    sequenceEnd = Mathf.Max(sequenceEnd, startTime + duration);
                }

                entries.Add(new TweenTimelineEntry(summary, startTime, duration));
            }

            return new TweenTimelineModel(entries, sequenceEnd);
        }

        private static TweenStepEditorSummary AnalyzeStep(int index, TweenStepDefinition step)
        {
            if (step == null)
            {
                return new TweenStepEditorSummary(
                    index,
                    null,
                    "Null",
                    string.Empty,
                    false,
                    false,
                    0f,
                    false,
                    Ease.Unset,
                    0,
                    TweenPlacementMode.Append,
                    0f,
                    string.Empty);
            }

            Type stepType = step.GetType();
            string typeName = UnityEditor.ObjectNames.NicifyVariableName(stepType.Name);
            string fullTypeName = stepType.FullName ?? stepType.Name;
            bool hasDuration = false;
            float duration = 0f;
            float delay = 0f;
            bool hasEase = false;
            Ease ease = Ease.Unset;
            int loops = 1;
            TweenPlacement placement = null;

            TimedTweenStepDefinition timedStep = step as TimedTweenStepDefinition;
            if (timedStep != null)
            {
                hasDuration = true;
                duration = timedStep.Duration;
                delay = timedStep.Delay;
                hasEase = true;
                ease = timedStep.Ease;
                loops = timedStep.Loops;
                placement = timedStep.Placement;
            }
            else
            {
                hasDuration = TryGetPropertyValue(step, "Duration", out duration);
                TryGetPropertyValue(step, "Delay", out delay);
                hasEase = TryGetPropertyValue(step, "Ease", out ease);
                TryGetPropertyValue(step, "Loops", out loops);
                TryGetPropertyValue(step, "Placement", out placement);
            }

            float estimatedDuration = hasDuration
                ? delay + (duration * Mathf.Max(1, loops))
                : 0f;
            TweenPlacementMode placementMode = placement != null
                ? placement.Mode
                : TweenPlacementMode.Append;
            float insertAt = placement != null ? placement.InsertAt : 0f;

            string targetKey;
            if (!TryGetPropertyValue(step, "TargetKey", out targetKey))
            {
                targetKey = string.Empty;
            }
            else if (TweenTargetBinding.IsSelfKey(targetKey))
            {
                targetKey = TweenTargetBinding.SelfKey;
            }

            return new TweenStepEditorSummary(
                index,
                step,
                typeName,
                fullTypeName,
                step.Enabled,
                hasDuration,
                estimatedDuration,
                hasEase,
                ease,
                loops,
                placementMode,
                insertAt,
                targetKey ?? string.Empty);
        }

        private static bool TryGetPropertyValue<T>(object source, string propertyName, out T value)
        {
            value = default(T);
            if (source == null)
            {
                return false;
            }

            PropertyInfo property = source.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            if (property == null || !property.CanRead || !typeof(T).IsAssignableFrom(property.PropertyType))
            {
                return false;
            }

            try
            {
                object result = property.GetValue(source, null);
                if (result == null)
                {
                    return !typeof(T).IsValueType;
                }

                value = (T)result;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.DOTween.Editor
{
    [CustomEditor(typeof(TweenSequenceAsset))]
    public sealed class TweenSequenceAssetEditor : Valkyrie.Editor.ValkyrieEditor
    {
        private const float TimelineRowHeight = 20f;
        private const float TimelineLabelWidth = 150f;

        private SerializedProperty _stepsProperty;
        private SerializedProperty _easeProperty;
        private SerializedProperty _loopsProperty;
        private SerializedProperty _loopTypeProperty;
        private SerializedProperty _updateTypeProperty;
        private SerializedProperty _independentUpdateProperty;
        private SerializedProperty _timeScaleProperty;
        private SerializedProperty _autoKillProperty;
        private SerializedProperty _recyclableProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _stepsProperty = serializedObject.FindProperty("_steps");
            _easeProperty = serializedObject.FindProperty("_ease");
            _loopsProperty = serializedObject.FindProperty("_loops");
            _loopTypeProperty = serializedObject.FindProperty("_loopType");
            _updateTypeProperty = serializedObject.FindProperty("_updateType");
            _independentUpdateProperty = serializedObject.FindProperty("_independentUpdate");
            _timeScaleProperty = serializedObject.FindProperty("_timeScale");
            _autoKillProperty = serializedObject.FindProperty("_autoKill");
            _recyclableProperty = serializedObject.FindProperty("_recyclable");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawScriptField();
            DrawSequenceSettings();

            EditorGUILayout.Space();
            Valkyrie.Editor.ManagedReferenceListRenderer.Draw(
                _stepsProperty,
                typeof(TweenStepDefinition));

            serializedObject.ApplyModifiedProperties();

            TweenSequenceAsset sequenceAsset = target as TweenSequenceAsset;
            if (sequenceAsset == null)
            {
                return;
            }

            EditorGUILayout.Space();
            DrawDiagnostics(sequenceAsset);
            EditorGUILayout.Space();
            DrawVisualSummary(sequenceAsset);
        }

        private static void DrawDiagnostics(TweenSequenceAsset sequenceAsset)
        {
            IReadOnlyList<TweenBuildDiagnostic> diagnostics =
                TweenSequenceEditorValidation.ValidateAsset(sequenceAsset);
            if (diagnostics.Count == 0)
            {
                return;
            }

            EditorGUILayout.LabelField("Configuration Diagnostics", EditorStyles.boldLabel);
            for (int index = 0; index < diagnostics.Count; index++)
            {
                TweenBuildDiagnostic diagnostic = diagnostics[index];
                string location = diagnostic.StepIndex >= 0
                    ? "Step " + (diagnostic.StepIndex + 1) + ": "
                    : string.Empty;
                EditorGUILayout.HelpBox(
                    location + "[" + diagnostic.Code + "] " + diagnostic.Message,
                    ToMessageType(diagnostic.Severity));
            }
        }

        private static MessageType ToMessageType(TweenDiagnosticSeverity severity)
        {
            switch (severity)
            {
                case TweenDiagnosticSeverity.Error:
                    return MessageType.Error;
                case TweenDiagnosticSeverity.Warning:
                    return MessageType.Warning;
                default:
                    return MessageType.Info;
            }
        }

        private void DrawScriptField()
        {
            SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(scriptProperty);
            }
        }

        private void DrawSequenceSettings()
        {
            EditorGUILayout.LabelField("Sequence Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_easeProperty);
            EditorGUILayout.PropertyField(_loopsProperty);
            EditorGUILayout.PropertyField(_loopTypeProperty);
            EditorGUILayout.PropertyField(_updateTypeProperty);
            EditorGUILayout.PropertyField(_independentUpdateProperty);
            EditorGUILayout.PropertyField(_timeScaleProperty);
            EditorGUILayout.PropertyField(_autoKillProperty);
            EditorGUILayout.PropertyField(_recyclableProperty);
        }

        private static void DrawVisualSummary(TweenSequenceAsset sequenceAsset)
        {
            IReadOnlyList<TweenStepEditorSummary> summaries =
                TweenSequenceEditorAnalysis.AnalyzeSteps(sequenceAsset.Steps);

            EditorGUILayout.LabelField("Sequence Summary", EditorStyles.boldLabel);
            DrawSummaryHeader();

            if (summaries.Count == 0)
            {
                EditorGUILayout.HelpBox("The sequence has no steps.", MessageType.Info);
            }
            else
            {
                for (int index = 0; index < summaries.Count; index++)
                {
                    DrawSummaryRow(summaries[index]);
                }
            }

            EditorGUILayout.Space(3f);
            DrawTimeline(TweenSequenceEditorAnalysis.BuildTimeline(sequenceAsset.Steps));
        }

        private static void DrawSummaryHeader()
        {
            Rect row = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect orderRect;
            Rect typeRect;
            Rect durationRect;
            Rect easeRect;
            Rect loopsRect;
            Rect bindingRect;
            SplitSummaryRow(
                row,
                out orderRect,
                out typeRect,
                out durationRect,
                out easeRect,
                out loopsRect,
                out bindingRect);

            GUI.Label(orderRect, "#", EditorStyles.miniBoldLabel);
            GUI.Label(typeRect, "Concrete type", EditorStyles.miniBoldLabel);
            GUI.Label(durationRect, "Duration", EditorStyles.miniBoldLabel);
            GUI.Label(easeRect, "Ease", EditorStyles.miniBoldLabel);
            GUI.Label(loopsRect, "Loops", EditorStyles.miniBoldLabel);
            GUI.Label(bindingRect, "Binding", EditorStyles.miniBoldLabel);
        }

        private static void DrawSummaryRow(TweenStepEditorSummary summary)
        {
            Rect row = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect orderRect;
            Rect typeRect;
            Rect durationRect;
            Rect easeRect;
            Rect loopsRect;
            Rect bindingRect;
            SplitSummaryRow(
                row,
                out orderRect,
                out typeRect,
                out durationRect,
                out easeRect,
                out loopsRect,
                out bindingRect);

            string order = (summary.Index + 1).ToString();
            string duration = summary.HasEstimatedDuration
                ? FormatSeconds(summary.EstimatedDuration)
                : "—";
            string ease = summary.HasEase ? summary.Ease.ToString() : "—";
            string loops = summary.Loops > 0 ? summary.Loops.ToString() : "—";
            string binding = string.IsNullOrEmpty(summary.TargetBindingKey)
                ? "—"
                : summary.TargetBindingKey;
            GUIStyle style = summary.Enabled ? EditorStyles.miniLabel : DisabledMiniLabel;

            GUI.Label(orderRect, order, style);
            GUI.Label(
                typeRect,
                new GUIContent(summary.TypeName, summary.FullTypeName),
                style);
            GUI.Label(durationRect, duration, style);
            GUI.Label(easeRect, ease, style);
            GUI.Label(loopsRect, loops, style);
            GUI.Label(bindingRect, binding, style);
        }

        private static void SplitSummaryRow(
            Rect row,
            out Rect orderRect,
            out Rect typeRect,
            out Rect durationRect,
            out Rect easeRect,
            out Rect loopsRect,
            out Rect bindingRect)
        {
            const float gap = 4f;
            float orderWidth = 22f;
            float durationWidth = 62f;
            float easeWidth = 72f;
            float loopsWidth = 38f;
            float bindingWidth = Mathf.Clamp(row.width * 0.18f, 62f, 120f);
            float typeWidth = Mathf.Max(
                80f,
                row.width - orderWidth - durationWidth - easeWidth - loopsWidth - bindingWidth - (gap * 5f));
            float x = row.x;

            orderRect = new Rect(x, row.y, orderWidth, row.height);
            x += orderWidth + gap;
            typeRect = new Rect(x, row.y, typeWidth, row.height);
            x += typeWidth + gap;
            durationRect = new Rect(x, row.y, durationWidth, row.height);
            x += durationWidth + gap;
            easeRect = new Rect(x, row.y, easeWidth, row.height);
            x += easeWidth + gap;
            loopsRect = new Rect(x, row.y, loopsWidth, row.height);
            x += loopsWidth + gap;
            bindingRect = new Rect(x, row.y, bindingWidth, row.height);
        }

        private static void DrawTimeline(TweenTimelineModel timeline)
        {
            EditorGUILayout.LabelField(
                "Timeline — estimated " + FormatSeconds(timeline.Duration),
                EditorStyles.boldLabel);

            if (timeline.Entries.Count == 0)
            {
                return;
            }

            float height = (timeline.Entries.Count * TimelineRowHeight) + 4f;
            Rect area = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.DrawRect(area, EditorGUIUtility.isProSkin
                ? new Color(0.13f, 0.13f, 0.13f)
                : new Color(0.82f, 0.82f, 0.82f));

            float barAreaWidth = Mathf.Max(1f, area.width - TimelineLabelWidth - 8f);
            float timelineDuration = Mathf.Max(0.0001f, timeline.Duration);

            for (int index = 0; index < timeline.Entries.Count; index++)
            {
                TweenTimelineEntry entry = timeline.Entries[index];
                float y = area.y + 2f + (index * TimelineRowHeight);
                Rect labelRect = new Rect(area.x + 4f, y, TimelineLabelWidth - 8f, TimelineRowHeight);
                Rect trackRect = new Rect(
                    area.x + TimelineLabelWidth,
                    y + 3f,
                    barAreaWidth,
                    TimelineRowHeight - 6f);

                GUI.Label(
                    labelRect,
                    (entry.Summary.Index + 1) + ". " + entry.Summary.TypeName,
                    EditorStyles.miniLabel);
                EditorGUI.DrawRect(trackRect, new Color(0f, 0f, 0f, 0.16f));

                if (!entry.Summary.Enabled || entry.Summary.Step == null)
                {
                    continue;
                }

                float normalizedStart = Mathf.Clamp01(entry.StartTime / timelineDuration);
                float normalizedWidth = Mathf.Clamp01(entry.Duration / timelineDuration);
                float width = Mathf.Max(2f, normalizedWidth * trackRect.width);
                Rect barRect = new Rect(
                    trackRect.x + (normalizedStart * trackRect.width),
                    trackRect.y,
                    Mathf.Min(width, trackRect.xMax - (trackRect.x + (normalizedStart * trackRect.width))),
                    trackRect.height);
                EditorGUI.DrawRect(barRect, GetPlacementColor(entry.Summary.PlacementMode));
                GUI.Label(
                    barRect,
                    new GUIContent(
                        entry.Summary.PlacementMode.ToString(),
                        FormatTimelineTooltip(entry)),
                    EditorStyles.centeredGreyMiniLabel);
            }
        }

        private static string FormatTimelineTooltip(TweenTimelineEntry entry)
        {
            string placement = entry.Summary.PlacementMode == TweenPlacementMode.Insert
                ? "Insert at " + FormatSeconds(entry.Summary.InsertAt)
                : entry.Summary.PlacementMode.ToString();
            return placement +
                   "\nStart: " + FormatSeconds(entry.StartTime) +
                   "\nDuration: " + FormatSeconds(entry.Duration);
        }

        private static Color GetPlacementColor(TweenPlacementMode placementMode)
        {
            switch (placementMode)
            {
                case TweenPlacementMode.Join:
                    return new Color(0.35f, 0.65f, 0.95f, 0.85f);
                case TweenPlacementMode.Insert:
                    return new Color(0.80f, 0.48f, 0.92f, 0.85f);
                case TweenPlacementMode.Append:
                default:
                    return new Color(0.35f, 0.78f, 0.49f, 0.85f);
            }
        }

        private static string FormatSeconds(float seconds)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds))
            {
                return "Invalid";
            }

            return seconds.ToString("0.###") + "s";
        }

        private static GUIStyle DisabledMiniLabel
        {
            get
            {
                GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
                style.normal.textColor = EditorGUIUtility.isProSkin
                    ? new Color(0.55f, 0.55f, 0.55f)
                    : new Color(0.45f, 0.45f, 0.45f);
                return style;
            }
        }
    }
}

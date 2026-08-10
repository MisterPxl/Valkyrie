using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.DOTween.Editor
{
    [CustomEditor(typeof(TweenPlayer), true)]
    public sealed class TweenPlayerEditor : Valkyrie.Editor.ValkyrieEditor
    {
        private readonly List<TweenBuildDiagnostic> _diagnostics = new List<TweenBuildDiagnostic>();

        private SerializedProperty _sourceModeProperty;
        private SerializedProperty _timelineProperty;
        private SerializedProperty _stepsProperty;
        private SerializedProperty _assetProperty;
        private SerializedProperty _targetRootProperty;
        private SerializedProperty _bindingsProperty;
        private SerializedProperty _triggersProperty;
        private SerializedProperty _stepEventsProperty;
        private SerializedProperty _eventsProperty;
        private SerializedProperty _playOnEnableProperty;
        private SerializedProperty _captureSpawnPointOnAwakeProperty;
        private SerializedProperty _idOverrideProperty;
        private SerializedProperty _targetOverrideProperty;
        private SerializedProperty _disableCleanupProperty;
        private SerializedProperty _destroyCleanupProperty;

        private bool _showPreview = true;
        private bool _showSequenceViewer = true;
        private bool _showTargeting;
        private bool _showAdditionalTriggers;
        private bool _showEvents;
        private bool _showAdvanced;

        protected override void OnEnable()
        {
            base.OnEnable();
            _sourceModeProperty = serializedObject.FindProperty("_sourceMode");
            _timelineProperty = serializedObject.FindProperty("_timeline");
            _stepsProperty = _timelineProperty.FindPropertyRelative("_steps");
            _assetProperty = serializedObject.FindProperty("_asset");
            _targetRootProperty = serializedObject.FindProperty("_targetRoot");
            _bindingsProperty = serializedObject.FindProperty("_bindings");
            _triggersProperty = serializedObject.FindProperty("_triggers");
            _stepEventsProperty = serializedObject.FindProperty("_stepEvents");
            _eventsProperty = serializedObject.FindProperty("_events");
            _playOnEnableProperty = serializedObject.FindProperty("_playOnEnable");
            _captureSpawnPointOnAwakeProperty = serializedObject.FindProperty("_captureSpawnPointOnAwake");
            _idOverrideProperty = serializedObject.FindProperty("_idOverride");
            _targetOverrideProperty = serializedObject.FindProperty("_targetOverride");
            _disableCleanupProperty = serializedObject.FindProperty("_disableCleanup");
            _destroyCleanupProperty = serializedObject.FindProperty("_destroyCleanup");
            RefreshValidation();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawScriptField();
            DrawPlayerHeader();
            DrawSource();
            DrawAnimation();
            DrawPreviewControls();
            DrawTimeline();
            DrawTargeting();
            DrawTriggersAndEvents();
            DrawAdvanced();
            DrawDiagnostics();

            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed)
            {
                RefreshValidation();
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

        private void DrawPlayerHeader()
        {
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("DOTween", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_sourceModeProperty, new GUIContent("Mode"));
            DrawPresetPopup();
        }

        private void DrawPresetPopup()
        {
            TweenPlayer player = target as TweenPlayer;
            List<TweenPresetOption> presets = TweenPresetEditorUtility.CollectPresets();
            string[] labels = new string[presets.Count + 1];
            labels[0] = "Custom";
            for (int index = 0; index < presets.Count; index++)
            {
                labels[index + 1] = presets[index].DisplayName;
            }

            EditorGUI.BeginChangeCheck();
            int selectedIndex = EditorGUILayout.Popup("Preset", 0, labels);
            if (EditorGUI.EndChangeCheck() && selectedIndex > 0)
            {
                TweenPresetEditorUtility.ApplyPreset(player, presets[selectedIndex - 1]);
                serializedObject.UpdateIfRequiredOrScript();
                _timelineProperty = serializedObject.FindProperty("_timeline");
                _stepsProperty = _timelineProperty.FindPropertyRelative("_steps");
                RefreshValidation();
            }
        }

        private void DrawPreviewControls()
        {
            TweenPlayer player = target as TweenPlayer;
            EditorGUILayout.Space(2f);
            _showPreview = EditorGUILayout.Foldout(
                _showPreview,
                "Preview Controls",
                true,
                EditorStyles.foldoutHeader);
            if (!_showPreview)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play"))
            {
                if (Application.isPlaying) player.Play();
                else TweenEditModePreview.Play(player);
                RefreshValidation();
            }

            if (GUILayout.Button("Pause"))
            {
                if (Application.isPlaying) player.Pause();
                else TweenEditModePreview.Pause();
            }

            if (GUILayout.Button("Stop"))
            {
                if (Application.isPlaying) player.Kill(false);
                else TweenEditModePreview.Stop();
            }
            EditorGUILayout.EndHorizontal();

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                float duration = Mathf.Max(0f, TweenEditModePreview.Duration);
                float time = TweenEditModePreview.Time;
                EditorGUI.BeginChangeCheck();
                float newTime = EditorGUILayout.Slider("Scrub", time, 0f, duration);
                if (EditorGUI.EndChangeCheck())
                {
                    TweenEditModePreview.Scrub(player, newTime);
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Capture Spawn Point"))
            {
                player.CaptureSpawnPoint();
            }

            if (GUILayout.Button("Capture Values"))
            {
                Undo.RecordObject(player, "Capture Tween Values");
                player.CaptureCurrentValues();
                EditorUtility.SetDirty(player);
                serializedObject.UpdateIfRequiredOrScript();
                RefreshValidation();
            }

            if (GUILayout.Button("Restart From Spawn Point"))
            {
                player.RestartFromSpawnPoint();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSource()
        {
            TweenPlayerSourceMode mode = (TweenPlayerSourceMode)_sourceModeProperty.enumValueIndex;
            if (mode == TweenPlayerSourceMode.Asset)
            {
                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField("Animation Asset", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_assetProperty, new GUIContent("Sequence"));
            }
        }

        private void DrawTargeting()
        {
            EditorGUILayout.Space(2f);
            _showTargeting = EditorGUILayout.Foldout(
                _showTargeting,
                "Targeting",
                true,
                EditorStyles.foldoutHeader);
            if (_showTargeting)
            {
                EditorGUILayout.PropertyField(_targetRootProperty);
                if (ShouldDrawBindings())
                {
                    EditorGUILayout.PropertyField(_bindingsProperty, true);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Bindings become available when a step targets a named Key.",
                        MessageType.None);
                }
            }

            DrawMissingComponentWarning();
        }

        private void DrawAnimation()
        {
            TweenPlayerSourceMode mode = (TweenPlayerSourceMode)_sourceModeProperty.enumValueIndex;
            if (mode == TweenPlayerSourceMode.Asset)
            {
                return;
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(mode == TweenPlayerSourceMode.Single ? "Animation" : "Sequence", EditorStyles.boldLabel);
            if (mode == TweenPlayerSourceMode.Single && _stepsProperty.arraySize > 1)
            {
                EditorGUILayout.HelpBox("Single mode uses the first step. Switch to Sequence to edit the whole list.", MessageType.Warning);
            }

            Valkyrie.Editor.ManagedReferenceListRenderer.Draw(_stepsProperty, typeof(TweenStepDefinition));
            DrawTimelineEditButtons(mode);
        }

        private void DrawTimelineEditButtons(TweenPlayerSourceMode mode)
        {
            TweenPlayer player = target as TweenPlayer;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Timeline"))
            {
                TweenTimelineClipboard.Copy(player.Timeline);
            }

            using (new EditorGUI.DisabledScope(!TweenTimelineClipboard.HasTimeline))
            {
                if (GUILayout.Button("Paste Timeline"))
                {
                    TweenTimelineClipboard.Paste(player);
                    serializedObject.UpdateIfRequiredOrScript();
                    _timelineProperty = serializedObject.FindProperty("_timeline");
                    _stepsProperty = _timelineProperty.FindPropertyRelative("_steps");
                    RefreshValidation();
                }
            }

            if (mode == TweenPlayerSourceMode.Sequence && GUILayout.Button("Duplicate Last Step"))
            {
                TweenTimelineClipboard.DuplicateLastStep(player);
                serializedObject.UpdateIfRequiredOrScript();
                _timelineProperty = serializedObject.FindProperty("_timeline");
                _stepsProperty = _timelineProperty.FindPropertyRelative("_steps");
                RefreshValidation();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTriggersAndEvents()
        {
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Playback", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_playOnEnableProperty, new GUIContent("Play On Enable"));

            _showAdditionalTriggers = EditorGUILayout.Foldout(
                _showAdditionalTriggers,
                "Additional Triggers" + FormatCount(_triggersProperty.arraySize),
                true);
            if (_showAdditionalTriggers)
            {
                Valkyrie.Editor.ManagedReferenceListRenderer.Draw(_triggersProperty, typeof(TweenTrigger));
            }

            EditorGUILayout.Space(2f);
            _showEvents = EditorGUILayout.Foldout(
                _showEvents,
                "Events" + FormatCount(_stepEventsProperty.arraySize),
                true,
                EditorStyles.foldoutHeader);
            if (_showEvents)
            {
                EditorGUILayout.PropertyField(_eventsProperty, true);
                EditorGUILayout.PropertyField(_stepEventsProperty, true);
            }
        }

        private void DrawDiagnostics()
        {
            if (_diagnostics.Count == 0)
            {
                return;
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Diagnostics", EditorStyles.boldLabel);
            for (int index = 0; index < _diagnostics.Count; index++)
            {
                TweenBuildDiagnostic diagnostic = _diagnostics[index];
                MessageType messageType = diagnostic.Severity == TweenDiagnosticSeverity.Error
                    ? MessageType.Error
                    : diagnostic.Severity == TweenDiagnosticSeverity.Warning
                        ? MessageType.Warning
                        : MessageType.Info;
                EditorGUILayout.HelpBox("[" + diagnostic.Code + "] " + diagnostic.Message, messageType);
            }
        }

        private void DrawTimeline()
        {
            TweenPlayerSourceMode mode = (TweenPlayerSourceMode)_sourceModeProperty.enumValueIndex;
            if (mode != TweenPlayerSourceMode.Sequence)
            {
                return;
            }

            TweenPlayer player = target as TweenPlayer;
            if (player == null || player.EffectiveTimeline == null)
            {
                return;
            }

            EditorGUILayout.Space(2f);
            _showSequenceViewer = EditorGUILayout.Foldout(
                _showSequenceViewer,
                "Sequence Viewer",
                true,
                EditorStyles.foldoutHeader);
            if (!_showSequenceViewer)
            {
                return;
            }

            TweenSequenceAssetEditor.DrawVisualSummary(player.EffectiveTimeline.Steps);
        }

        private void DrawAdvanced()
        {
            EditorGUILayout.Space(2f);
            _showAdvanced = EditorGUILayout.Foldout(
                _showAdvanced,
                "Advanced",
                true,
                EditorStyles.foldoutHeader);
            if (!_showAdvanced)
            {
                return;
            }

            TweenPlayerSourceMode mode = (TweenPlayerSourceMode)_sourceModeProperty.enumValueIndex;
            if (mode != TweenPlayerSourceMode.Asset)
            {
                DrawTimelineSettings(_timelineProperty);
            }

            EditorGUILayout.PropertyField(_captureSpawnPointOnAwakeProperty);
            EditorGUILayout.PropertyField(_idOverrideProperty);
            EditorGUILayout.PropertyField(_targetOverrideProperty);
            EditorGUILayout.PropertyField(_disableCleanupProperty);
            EditorGUILayout.PropertyField(_destroyCleanupProperty);
        }

        private bool ShouldDrawBindings()
        {
            TweenPlayerSourceMode mode = (TweenPlayerSourceMode)_sourceModeProperty.enumValueIndex;
            if (mode == TweenPlayerSourceMode.Asset || _bindingsProperty.arraySize > 0)
            {
                return true;
            }

            TweenPlayer player = target as TweenPlayer;
            if (player == null || player.EffectiveTimeline == null)
            {
                return false;
            }

            IList<TweenStepDefinition> steps = player.EffectiveTimeline.Steps;
            for (int index = 0; index < steps.Count; index++)
            {
                ITweenTargetStep targetStep = steps[index] as ITweenTargetStep;
                if (targetStep != null &&
                    targetStep.Target != null &&
                    targetStep.Target.Mode == TweenTargetMode.Key)
                {
                    return true;
                }
            }

            return false;
        }

        private static string FormatCount(int count)
        {
            return count > 0 ? " (" + count + ")" : string.Empty;
        }

        private void DrawMissingComponentWarning()
        {
            TweenPlayer player = target as TweenPlayer;
            if (player == null || player.EffectiveTimeline == null)
            {
                return;
            }

            IList<TweenStepDefinition> steps = player.EffectiveTimeline.Steps;
            TweenBuildContext context = new TweenBuildContext(player.TargetRoot, player.Bindings);
            for (int index = 0; index < steps.Count; index++)
            {
                ITweenTargetStep targetStep = steps[index] as ITweenTargetStep;
                if (targetStep == null) continue;

                Component component;
                context.SetCurrentStep(index, steps[index]);
                if (!context.TryResolve(targetStep.Target, out component))
                {
                    EditorGUILayout.HelpBox("No valid Component was found for the selected animation.", MessageType.Error);
                    return;
                }
            }
        }

        private void RefreshValidation()
        {
            _diagnostics.Clear();
            TweenPlayer player = target as TweenPlayer;
            IReadOnlyList<TweenBuildDiagnostic> diagnostics = TweenSequenceEditorValidation.Validate(player);
            for (int index = 0; index < diagnostics.Count; index++)
            {
                _diagnostics.Add(diagnostics[index]);
            }
        }

        private static void DrawTimelineSettings(SerializedProperty timelineProperty)
        {
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Timeline Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(timelineProperty.FindPropertyRelative("_ease"));
            EditorGUILayout.PropertyField(timelineProperty.FindPropertyRelative("_loops"));
            EditorGUILayout.PropertyField(timelineProperty.FindPropertyRelative("_loopType"));
            EditorGUILayout.PropertyField(timelineProperty.FindPropertyRelative("_updateType"));
            EditorGUILayout.PropertyField(timelineProperty.FindPropertyRelative("_independentUpdate"));
            EditorGUILayout.PropertyField(timelineProperty.FindPropertyRelative("_timeScale"));
            EditorGUILayout.PropertyField(timelineProperty.FindPropertyRelative("_autoKill"));
            EditorGUILayout.PropertyField(timelineProperty.FindPropertyRelative("_recyclable"));
        }
    }
}

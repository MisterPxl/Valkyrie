using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.DOTween.Editor
{
    [CustomEditor(typeof(TweenSequencePlayer))]
    public sealed class TweenSequencePlayerEditor : Valkyrie.Editor.ValkyrieEditor
    {
        private readonly List<TweenBuildDiagnostic> _diagnostics =
            new List<TweenBuildDiagnostic>();

        private SerializedProperty _assetProperty;
        private SerializedProperty _targetRootProperty;
        private SerializedProperty _bindingsProperty;
        private SerializedProperty _playOnEnableProperty;
        private SerializedProperty _idOverrideProperty;
        private SerializedProperty _targetOverrideProperty;
        private SerializedProperty _disableCleanupProperty;
        private SerializedProperty _destroyCleanupProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _assetProperty = serializedObject.FindProperty("_asset");
            _targetRootProperty = serializedObject.FindProperty("_targetRoot");
            _bindingsProperty = serializedObject.FindProperty("_bindings");
            _playOnEnableProperty = serializedObject.FindProperty("_playOnEnable");
            _idOverrideProperty = serializedObject.FindProperty("_idOverride");
            _targetOverrideProperty = serializedObject.FindProperty("_targetOverride");
            _disableCleanupProperty = serializedObject.FindProperty("_disableCleanup");
            _destroyCleanupProperty = serializedObject.FindProperty("_destroyCleanup");

            Undo.undoRedoPerformed += HandleSerializedStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            RefreshValidation();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= HandleSerializedStateChanged;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginChangeCheck();
            DrawScriptField();
            DrawConfiguration();
            bool inspectorChanged = EditorGUI.EndChangeCheck();
            bool applied = serializedObject.ApplyModifiedProperties();

            if (inspectorChanged || applied)
            {
                RefreshValidation();
            }

            EditorGUILayout.Space();
            DrawDiagnostics();

            EditorGUILayout.Space();
            DrawPlaybackControls();
        }

        private void DrawScriptField()
        {
            SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(scriptProperty);
            }
        }

        private void DrawConfiguration()
        {
            EditorGUILayout.LabelField("Sequence", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_assetProperty);
            EditorGUILayout.PropertyField(_targetRootProperty);
            EditorGUILayout.PropertyField(_playOnEnableProperty);

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Runtime Identity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_idOverrideProperty);
            EditorGUILayout.PropertyField(_targetOverrideProperty);

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Target Bindings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_bindingsProperty, true);

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Cleanup", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_disableCleanupProperty);
            EditorGUILayout.PropertyField(_destroyCleanupProperty);
        }

        private void DrawDiagnostics()
        {
            int errorCount = 0;
            int warningCount = 0;
            for (int index = 0; index < _diagnostics.Count; index++)
            {
                TweenDiagnosticSeverity severity = _diagnostics[index].Severity;
                if (severity == TweenDiagnosticSeverity.Error)
                {
                    errorCount++;
                }
                else if (severity == TweenDiagnosticSeverity.Warning)
                {
                    warningCount++;
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                "Pre-play Diagnostics",
                errorCount > 0 ? ErrorHeaderStyle : EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh", GUILayout.Width(70f)))
            {
                RefreshValidation();
            }
            EditorGUILayout.EndHorizontal();

            if (_diagnostics.Count == 0)
            {
                EditorGUILayout.HelpBox("Validation passed.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField(
                errorCount + " error(s), " + warningCount + " warning(s)",
                EditorStyles.miniLabel);
            for (int index = 0; index < _diagnostics.Count; index++)
            {
                TweenBuildDiagnostic diagnostic = _diagnostics[index];
                EditorGUILayout.HelpBox(
                    FormatDiagnostic(diagnostic),
                    ToMessageType(diagnostic.Severity));
            }
        }

        private void DrawPlaybackControls()
        {
            TweenSequencePlayer player = target as TweenSequencePlayer;
            if (player == null)
            {
                return;
            }

            EditorGUILayout.LabelField("Play Mode Controls", EditorStyles.boldLabel);
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Edit Mode preview is intentionally not provided. Enter Play Mode to control the sequence.",
                    MessageType.Info);
            }

            Sequence sequence = player.CurrentSequence;
            bool active = sequence != null && sequence.IsActive();
            bool playing = active && sequence.IsPlaying();

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Play"))
                {
                    player.Play();
                    CopyRuntimeDiagnostics(player.Diagnostics);
                }

                using (new EditorGUI.DisabledScope(!playing))
                {
                    if (GUILayout.Button("Pause"))
                    {
                        player.Pause();
                    }
                }

                using (new EditorGUI.DisabledScope(!active || playing))
                {
                    if (GUILayout.Button("Resume"))
                    {
                        player.Resume();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(!active))
                {
                    if (GUILayout.Button("Rewind"))
                    {
                        player.Rewind();
                    }

                    if (GUILayout.Button("Complete"))
                    {
                        player.Complete();
                    }

                    if (GUILayout.Button("Kill"))
                    {
                        player.Kill();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (Application.isPlaying)
            {
                string state = !active ? "No active sequence" : playing ? "Playing" : "Paused";
                EditorGUILayout.LabelField("State", state);
            }
        }

        private void RefreshValidation()
        {
            TweenSequencePlayer player = target as TweenSequencePlayer;
            IReadOnlyList<TweenBuildDiagnostic> validation =
                TweenSequenceEditorValidation.Validate(player);
            _diagnostics.Clear();
            for (int index = 0; index < validation.Count; index++)
            {
                _diagnostics.Add(validation[index]);
            }

            Repaint();
        }

        private void CopyRuntimeDiagnostics(IReadOnlyList<TweenBuildDiagnostic> diagnostics)
        {
            _diagnostics.Clear();
            for (int index = 0; index < diagnostics.Count; index++)
            {
                _diagnostics.Add(diagnostics[index]);
            }

            Repaint();
        }

        private void HandleSerializedStateChanged()
        {
            serializedObject.UpdateIfRequiredOrScript();
            RefreshValidation();
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode ||
                state == PlayModeStateChange.EnteredPlayMode)
            {
                RefreshValidation();
            }
            else
            {
                Repaint();
            }
        }

        private static string FormatDiagnostic(TweenBuildDiagnostic diagnostic)
        {
            string message = "[" + diagnostic.Code + "] " + diagnostic.Message;
            if (diagnostic.StepIndex >= 0)
            {
                message += "\nStep: " + (diagnostic.StepIndex + 1);
                if (!string.IsNullOrEmpty(diagnostic.StepType))
                {
                    message += " (" + GetShortTypeName(diagnostic.StepType) + ")";
                }
            }

            if (!string.IsNullOrEmpty(diagnostic.BindingKey))
            {
                message += "\nBinding: " + diagnostic.BindingKey;
            }

            if (!string.IsNullOrEmpty(diagnostic.ExpectedType))
            {
                message += "\nExpected: " + diagnostic.ExpectedType;
            }

            if (!string.IsNullOrEmpty(diagnostic.ActualType))
            {
                message += "\nActual: " + diagnostic.ActualType;
            }

            return message;
        }

        private static string GetShortTypeName(string fullTypeName)
        {
            int separator = fullTypeName.LastIndexOf('.');
            return separator >= 0 && separator < fullTypeName.Length - 1
                ? fullTypeName.Substring(separator + 1)
                : fullTypeName;
        }

        private static MessageType ToMessageType(TweenDiagnosticSeverity severity)
        {
            switch (severity)
            {
                case TweenDiagnosticSeverity.Error:
                    return MessageType.Error;
                case TweenDiagnosticSeverity.Warning:
                    return MessageType.Warning;
                case TweenDiagnosticSeverity.Info:
                default:
                    return MessageType.Info;
            }
        }

        private static GUIStyle ErrorHeaderStyle
        {
            get
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = new Color(0.9f, 0.3f, 0.3f);
                return style;
            }
        }
    }
}

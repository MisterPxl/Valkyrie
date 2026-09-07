using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.DOTween.Editor
{
    public sealed class TweenStepEventOptions
    {
        public readonly List<string> Ids = new List<string> { string.Empty };
        public readonly List<string> Labels = new List<string> { "None" };
        public int SelectedIndex { get; private set; }
        public string Warning { get; private set; }

        public static TweenStepEventOptions Build(TweenPlayer player, string currentId)
        {
            var options = new TweenStepEventOptions();
            if (player == null) return options;
            IList<TweenStepDefinition> steps = player.SourceMode == TweenPlayerSourceMode.Asset && player.Asset == null
                ? null : player.EffectiveTimeline.Steps;
            if (steps != null)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    TweenStepDefinition step = steps[i];
                    if (step == null) continue;
                    bool active = step.Enabled && (player.SourceMode != TweenPlayerSourceMode.Single || i == 0);
                    string typeName = ObjectNames.NicifyVariableName(step.GetType().Name.Replace("StepDefinition", ""));
                    string name = step.Name == step.GetType().Name ? typeName : step.Name + " (" + typeName + ")";
                    options.Ids.Add(step.Id);
                    options.Labels.Add((i + 1) + ". " + name + (active ? "" : " — inactive"));
                    if (step.Id != currentId) continue;
                    options.SelectedIndex = options.Ids.Count - 1;
                    if (!active) options.Warning = "This step is disabled or is not used in Single mode.";
                }
            }
            if (!string.IsNullOrEmpty(currentId) && options.SelectedIndex == 0)
            {
                options.Ids.Add(currentId);
                options.Labels.Add("Missing step");
                options.SelectedIndex = options.Ids.Count - 1;
                options.Warning = "The linked step is no longer in this animation. Select another step or None.";
            }
            return options;
        }
    }

    [CustomPropertyDrawer(typeof(TweenStepEventBinding))]
    public sealed class TweenStepEventBindingDrawer : PropertyDrawer
    {
        private static float Gap => EditorGUIUtility.standardVerticalSpacing;
        private static float Line => EditorGUIUtility.singleLineHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var options = GetOptions(property);
            float height = Line + Gap + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_events"), true);
            if (options.Warning != null) height += Line * 2f + Gap;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            bool previousMixed = EditorGUI.showMixedValue;
            try
            {
                var id = property.FindPropertyRelative("_stepId");
                var options = GetOptions(property);
                var row = new Rect(position.x, position.y, position.width, Line);
                EditorGUI.showMixedValue = id.hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();
                int selected = EditorGUI.Popup(row, new GUIContent("Step"), options.SelectedIndex,
                    options.Labels.ConvertAll(text => new GUIContent(text)).ToArray());
                if (EditorGUI.EndChangeCheck()) id.stringValue = options.Ids[selected];
                EditorGUI.showMixedValue = previousMixed;
                float y = row.yMax + Gap;
                if (options.Warning != null)
                {
                    EditorGUI.HelpBox(new Rect(position.x, y, position.width, Line * 2f), options.Warning, MessageType.Warning);
                    y += Line * 2f + Gap;
                }
                var events = property.FindPropertyRelative("_events");
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(events, true)),
                    events, new GUIContent("Events"), true);
            }
            finally { EditorGUI.showMixedValue = previousMixed; EditorGUI.EndProperty(); }
        }

        private static TweenStepEventOptions GetOptions(SerializedProperty property)
        {
            return TweenStepEventOptions.Build(property.serializedObject.targetObject as TweenPlayer,
                property.FindPropertyRelative("_stepId").stringValue);
        }
    }

    [InitializeOnLoad]
    internal static class TweenStepClipboardIntegration
    {
        static TweenStepClipboardIntegration()
        {
            Valkyrie.Editor.ManagedReferenceClipboard.CloneCreated += clone =>
            {
                if (clone is TweenStepDefinition step) step.RegenerateId();
            };
        }
    }
}

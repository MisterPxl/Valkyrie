using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    public static class PropertyRenderer
    {
        private static readonly Color SeparatorColor = new(0.35f, 0.35f, 0.35f, 0.8f);
        private static GUIStyle _subtitleStyle;

        public static void DrawField(SerializedProperty property, object target, InspectedField field)
        {
            if (!ConditionResolver.ShouldDraw(target, field, out string conditionWarning))
                return;

            if (conditionWarning != null)
                EditorGUILayout.HelpBox(conditionWarning, MessageType.Warning);

            DrawTitle(field.Title);
            DrawInfoBoxes(field.InfoBoxes);

            using (new EditorGUI.DisabledScope(field.IsReadOnly))
            {
                if (field.IsManagedReferenceCollection)
                    ManagedReferenceListRenderer.Draw(property, field);
                else if (field.IsManagedReference)
                    ManagedReferenceRenderer.Draw(property, field);
                else
                    EditorGUILayout.PropertyField(property, true);
            }

            DrawRequiredValidation(property, field.Required);
        }

        private static void DrawTitle(TitleAttribute title)
        {
            if (title == null)
                return;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(title.Text, EditorStyles.boldLabel);

            if (!string.IsNullOrEmpty(title.Subtitle))
            {
                _subtitleStyle ??= new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
                };
                EditorGUILayout.LabelField(title.Subtitle, _subtitleStyle);
            }

            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, SeparatorColor);
            EditorGUILayout.Space(2);
        }

        private static void DrawInfoBoxes(InfoBoxAttribute[] infoBoxes)
        {
            if (infoBoxes == null)
                return;

            for (int i = 0; i < infoBoxes.Length; i++)
            {
                var box = infoBoxes[i];
                EditorGUILayout.HelpBox(box.Message, ToMessageType(box.Type));
            }
        }

        private static void DrawRequiredValidation(SerializedProperty property, RequiredAttribute required)
        {
            if (required == null)
                return;

            bool isMissing = property.propertyType switch
            {
                SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
                SerializedPropertyType.String => string.IsNullOrEmpty(property.stringValue),
                SerializedPropertyType.ExposedReference => property.exposedReferenceValue == null,
                SerializedPropertyType.ManagedReference => string.IsNullOrEmpty(property.managedReferenceFullTypename),
                _ => false
            };

            if (!isMissing)
                return;

            string message = !string.IsNullOrEmpty(required.Message)
                ? required.Message
                : $"\"{property.displayName}\" is required";

            EditorGUILayout.HelpBox(message, MessageType.Error);
        }

        private static MessageType ToMessageType(InfoBoxType type)
        {
            return type switch
            {
                InfoBoxType.Warning => MessageType.Warning,
                InfoBoxType.Error => MessageType.Error,
                _ => MessageType.Info
            };
        }
    }
}

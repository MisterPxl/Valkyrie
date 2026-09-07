using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    /// <summary>Shared layout and fixed-rect rendering for root and nested fields.</summary>
    public static class PropertyRenderer
    {
        private static float Line => EditorGUIUtility.singleLineHeight;
        private static float Gap => EditorGUIUtility.standardVerticalSpacing;
        private static float BoxHeight(string text)
        {
            // Height queries also run without a GUI context (e.g. batch validation).
            if (Event.current == null) return Line * 2f + Gap;
            return Mathf.Max(Line * 2f, EditorStyles.helpBox.CalcHeight(new GUIContent(text),
                Mathf.Max(100f, EditorGUIUtility.currentViewWidth - 80f - EditorGUI.indentLevel * 15f))) + Gap;
        }

        public static void DrawField(SerializedProperty property, Object[] targets, InspectedField field)
            => DrawGUILayout(property);

        public static void DrawGUILayout(SerializedProperty property)
        {
            float height = GetHeight(property);
            if (height > 0f) Draw(EditorGUILayout.GetControlRect(true, height), property);
        }

        public static bool IsVisible(SerializedProperty property, out string warning)
        {
            warning = null;
            InspectedField field = SerializedPropertyContext.GetField(property);
            return field == null || ConditionResolver.ShouldDraw(SerializedPropertyContext.GetOwners(property), field, out warning);
        }

        public static float GetHeight(SerializedProperty property)
        {
            if (property == null || !IsVisible(property, out string warning)) return 0f;
            InspectedField field = SerializedPropertyContext.GetField(property);
            float height = ManagedReferencePropertyRouter.GetContentHeight(property);
            if (warning != null) height += BoxHeight(warning);
            if (field?.Title != null)
                height += 9f + Line + Gap + (string.IsNullOrEmpty(field.Title.Subtitle) ? 0f : Line);
            if (field?.InfoBoxes != null)
                foreach (var box in field.InfoBoxes) height += BoxHeight(box.Message);
            string required = GetRequiredMessage(property, field?.Required);
            if (required != null) height += BoxHeight(required);
            return height;
        }

        public static void Draw(Rect rect, SerializedProperty property)
        {
            if (!IsVisible(property, out string warning)) return;
            InspectedField field = SerializedPropertyContext.GetField(property);
            EditorGUI.BeginProperty(rect, new GUIContent(property.displayName), property);
            try
            {
                if (warning != null) DrawBox(ref rect, warning, MessageType.Warning);
                if (field?.Title != null)
                {
                    rect.y += 6f;
                    EditorGUI.LabelField(Take(ref rect, Line), field.Title.Text, EditorStyles.boldLabel);
                    if (!string.IsNullOrEmpty(field.Title.Subtitle))
                        EditorGUI.LabelField(Take(ref rect, Line), field.Title.Subtitle, EditorStyles.miniLabel);
                    rect.y += Gap;
                    EditorGUI.DrawRect(Take(ref rect, 1f), new Color(0.35f, 0.35f, 0.35f, 0.8f));
                    rect.y += 2f;
                }
                if (field?.InfoBoxes != null)
                    foreach (InfoBoxAttribute box in field.InfoBoxes)
                        DrawBox(ref rect, box.Message, box.Type == InfoBoxType.Error ? MessageType.Error
                            : box.Type == InfoBoxType.Warning ? MessageType.Warning : MessageType.Info);
                using (new EditorGUI.DisabledScope(field?.IsReadOnly == true))
                {
                    float height = ManagedReferencePropertyRouter.GetContentHeight(property);
                    ManagedReferencePropertyRouter.DrawContent(Take(ref rect, height), property);
                }
                string required = GetRequiredMessage(property, field?.Required);
                if (required != null) DrawBox(ref rect, required, MessageType.Error);
            }
            finally { EditorGUI.EndProperty(); }
        }

        public static string GetRequiredMessage(SerializedProperty property, RequiredAttribute required)
        {
            if (required == null) return null;
            bool missing = IsMissing(property);
            if (!missing && property.hasMultipleDifferentValues)
            {
                foreach (Object target in property.serializedObject.targetObjects)
                {
                    using var individual = new SerializedObject(target);
                    if (IsMissing(individual.FindProperty(property.propertyPath))) { missing = true; break; }
                }
            }
            return !missing ? null : !string.IsNullOrEmpty(required.Message) ? required.Message
                : $"\"{property.displayName}\" is required";
        }

        private static bool IsMissing(SerializedProperty property)
        {
            if (property == null) return false;
            return property.propertyType switch
            {
                SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
                SerializedPropertyType.String => string.IsNullOrEmpty(property.stringValue),
                SerializedPropertyType.ExposedReference => property.exposedReferenceValue == null,
                SerializedPropertyType.ManagedReference => string.IsNullOrEmpty(property.managedReferenceFullTypename),
                _ => false
            };
        }

        private static Rect Take(ref Rect rect, float height)
        {
            var result = new Rect(rect.x, rect.y, rect.width, height);
            rect.y += height;
            return result;
        }

        private static void DrawBox(ref Rect rect, string message, MessageType type)
        {
            EditorGUI.HelpBox(Take(ref rect, BoxHeight(message) - Gap), message, type);
            rect.y += Gap;
        }
    }
}

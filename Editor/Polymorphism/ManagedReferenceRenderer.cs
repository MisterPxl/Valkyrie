using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    /// <summary>
    /// Renderer for a single <c>[SerializeReference]</c> slot.
    /// Layout mirrors Odin's UX:
    /// <list type="bullet">
    ///   <item>Foldout-style header showing the field name and the current concrete type
    ///         (or <c>None (BaseType)</c> when empty).</item>
    ///   <item>Clicking the header dropdown opens a searchable type picker
    ///         (<see cref="ManagedReferenceTypeDropdown"/>).</item>
    ///   <item>When a value is set, child properties render below in a foldable section.</item>
    /// </list>
    /// </summary>
    public static class ManagedReferenceRenderer
    {
        private const float ObjectFieldButtonWidth = 19f;
        private static GUIStyle _objectFieldButtonStyle;

        public static void Draw(SerializedProperty property, InspectedField field)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUILayout.PropertyField(property, true);
                return;
            }

            DrawElement(property, field.ManagedReferenceBaseType, property.displayName);
        }

        /// <summary>
        /// Draws a single managed reference slot with an explicit base type and label.
        /// Used by both <see cref="Draw"/> and <see cref="ManagedReferenceListRenderer"/>.
        /// </summary>
        public static void DrawElement(SerializedProperty property, System.Type baseType, string label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUILayout.PropertyField(property, true);
                return;
            }

            bool hasValue = !string.IsNullOrEmpty(property.managedReferenceFullTypename);

            // Header line: foldout triangle (when value exists) + label + type dropdown.
            Rect line = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            DrawHeader(line, property, baseType, label, hasValue);

            if (!hasValue)
                return;

            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;
            DrawChildProperties(property);
            EditorGUI.indentLevel--;
        }

        public static float GetElementHeight(SerializedProperty property, System.Type baseType)
        {
            if (property == null)
                return EditorGUIUtility.singleLineHeight;

            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return EditorGUI.GetPropertyHeight(property, true);

            float height = EditorGUIUtility.singleLineHeight;
            bool hasValue = !string.IsNullOrEmpty(property.managedReferenceFullTypename);
            if (!hasValue || !property.isExpanded)
                return height;

            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            if (!iterator.NextVisible(true))
                return height;

            height += EditorGUIUtility.standardVerticalSpacing;

            bool first = true;
            while (!SerializedProperty.EqualContents(iterator, endProperty))
            {
                if (!first)
                    height += EditorGUIUtility.standardVerticalSpacing;

                height += ManagedReferencePropertyRouter.GetPropertyHeight(iterator);
                first = false;

                if (!iterator.NextVisible(false))
                    break;
            }

            return height;
        }

        public static void DrawElement(Rect rect, SerializedProperty property, System.Type baseType, string label, float labelWidthAdjustment = 0f)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(rect, property, true);
                return;
            }

            bool hasValue = !string.IsNullOrEmpty(property.managedReferenceFullTypename);
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            DrawHeader(headerRect, property, baseType, label, hasValue, labelWidthAdjustment);

            if (!hasValue || !property.isExpanded)
                return;

            float y = headerRect.yMax + spacing;
            EditorGUI.indentLevel++;

            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            if (iterator.NextVisible(true))
            {
                while (!SerializedProperty.EqualContents(iterator, endProperty))
                {
                    float height = ManagedReferencePropertyRouter.GetPropertyHeight(iterator);
                    Rect childRect = new Rect(rect.x, y, rect.width, height);
                    ManagedReferencePropertyRouter.DrawGUI(childRect, iterator);

                    y += height + spacing;
                    if (!iterator.NextVisible(false))
                        break;
                }
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawHeader(Rect rect, SerializedProperty property, System.Type baseType, string label, bool hasValue, float labelWidthAdjustment = 0f)
        {
            float labelWidth = Mathf.Max(40f, EditorGUIUtility.labelWidth - labelWidthAdjustment);
            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            Rect dropdownRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height);

            // Foldout doubles as the label when there's a value to expand.
            if (hasValue)
            {
                property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
            }
            else
            {
                EditorGUI.LabelField(labelRect, label);
            }

            // Object-field-styled dropdown matching Unity's native look (image 1 in the spec).
            string dropdownLabel = hasValue
                ? FormatValueLabel(property)
                : $"None ({FormatBaseType(baseType)})";

            GUIContent content = new GUIContent(
                dropdownLabel,
                hasValue ? EditorGUIUtility.IconContent("cs Script Icon").image : null);

            bool clicked = EditorGUI.DropdownButton(dropdownRect, content, FocusType.Keyboard, EditorStyles.objectField);
            DrawObjectFieldButton(dropdownRect);

            if (clicked)
            {
                ManagedReferenceTypeDropdown.Show(
                    dropdownRect,
                    baseType,
                    type => ManagedReferenceMutationService.AssignType(
                        property.serializedObject,
                        property.propertyPath,
                        type,
                        preserveExistingValues: true),
                    includeNoneEntry: hasValue,
                    title: "Select " + FormatBaseType(baseType));
            }

            DrawContextMenu(rect, property, hasValue);
        }

        private static void DrawObjectFieldButton(Rect dropdownRect)
        {
            Rect buttonRect = new Rect(
                dropdownRect.xMax - ObjectFieldButtonWidth,
                dropdownRect.y,
                ObjectFieldButtonWidth,
                dropdownRect.height);

            _objectFieldButtonStyle ??= GUI.skin.FindStyle("ObjectFieldButton") ?? EditorStyles.objectField;
            GUI.Label(buttonRect, GUIContent.none, _objectFieldButtonStyle);
        }

        private static void DrawChildProperties(SerializedProperty property)
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();

            if (!iterator.NextVisible(true))
                return;

            while (!SerializedProperty.EqualContents(iterator, endProperty))
            {
                ManagedReferencePropertyRouter.DrawGUILayout(iterator);
                if (!iterator.NextVisible(false))
                    break;
            }
        }

        private static void DrawContextMenu(Rect rect, SerializedProperty property, bool hasValue)
        {
            if (!hasValue)
                return;

            Event current = Event.current;
            if (current == null || current.type != EventType.ContextClick || !rect.Contains(current.mousePosition))
                return;

            var menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Reset/New Instance"),
                false,
                () => ManagedReferenceMutationService.ResetCurrentType(property.serializedObject, property.propertyPath));
            menu.AddItem(
                new GUIContent("Clear"),
                false,
                () => ManagedReferenceMutationService.AssignType(
                    property.serializedObject,
                    property.propertyPath,
                    null,
                    preserveExistingValues: false));

            menu.ShowAsContext();
            current.Use();
        }

        internal static string FormatValueLabel(SerializedProperty property)
        {
            string fullTypename = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(fullTypename)) return "None";

            int spaceIdx = fullTypename.IndexOf(' ');
            string typeFullName = spaceIdx >= 0 ? fullTypename.Substring(spaceIdx + 1) : fullTypename;
            int lastDot = typeFullName.LastIndexOf('.');
            string shortName = lastDot >= 0 ? typeFullName.Substring(lastDot + 1) : typeFullName;
            return ObjectNames.NicifyVariableName(shortName);
        }

        internal static string FormatBaseType(System.Type baseType)
        {
            if (baseType == null) return "Object";
            return ObjectNames.NicifyVariableName(baseType.Name);
        }
    }
}

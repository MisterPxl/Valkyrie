using System;
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
        public static void DrawElement(SerializedProperty property, Type baseType, string label)
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

        private static void DrawHeader(Rect rect, SerializedProperty property, Type baseType, string label, bool hasValue)
        {
            float labelWidth = EditorGUIUtility.labelWidth;
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

            if (EditorGUI.DropdownButton(dropdownRect, content, FocusType.Keyboard, EditorStyles.objectField))
            {
                ManagedReferenceTypeDropdown.Show(
                    dropdownRect,
                    baseType,
                    type => AssignType(property.serializedObject, property.propertyPath, type),
                    includeNoneEntry: hasValue,
                    title: "Select " + FormatBaseType(baseType));
            }
        }

        private static void DrawChildProperties(SerializedProperty property)
        {
            var iterator = property.Copy();
            var endProperty = iterator.GetEndProperty();

            if (!iterator.NextVisible(true))
                return;

            while (!SerializedProperty.EqualContents(iterator, endProperty))
            {
                EditorGUILayout.PropertyField(iterator, true);
                if (!iterator.NextVisible(false))
                    break;
            }
        }

        private static void AssignType(SerializedObject serializedObject, string propertyPath, Type type)
        {
            var prop = serializedObject.FindProperty(propertyPath);
            if (prop == null) return;

            if (type == null)
            {
                prop.managedReferenceValue = null;
            }
            else
            {
                try
                {
                    prop.managedReferenceValue = Activator.CreateInstance(type);
                    prop.isExpanded = true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Valkyrie: failed to create instance of {type.Name} — {e.Message}");
                    return;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static string FormatValueLabel(SerializedProperty property)
        {
            string fullTypename = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(fullTypename)) return "None";

            int spaceIdx = fullTypename.IndexOf(' ');
            string typeFullName = spaceIdx >= 0 ? fullTypename.Substring(spaceIdx + 1) : fullTypename;
            int lastDot = typeFullName.LastIndexOf('.');
            string shortName = lastDot >= 0 ? typeFullName.Substring(lastDot + 1) : typeFullName;
            return ObjectNames.NicifyVariableName(shortName);
        }

        private static string FormatBaseType(Type baseType)
        {
            if (baseType == null) return "Object";
            return ObjectNames.NicifyVariableName(baseType.Name);
        }
    }
}

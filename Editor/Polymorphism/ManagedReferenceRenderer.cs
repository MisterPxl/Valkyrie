using System;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    public static class ManagedReferenceRenderer
    {
        private static readonly Color HeaderBgColor = new(0.22f, 0.22f, 0.22f, 0.6f);
        private static GUIStyle _headerLabelStyle;

        public static void Draw(SerializedProperty property, InspectedField field)
        {
            bool hasValue = !string.IsNullOrEmpty(property.managedReferenceFullTypename);

            DrawTypeSelector(property, field, hasValue);

            if (!hasValue)
                return;

            EditorGUI.indentLevel++;
            DrawChildProperties(property);
            EditorGUI.indentLevel--;
        }

        private static void DrawTypeSelector(SerializedProperty property, InspectedField field, bool hasValue)
        {
            Rect fullRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

            // Subtle background for the type selector row
            Rect bgRect = new(fullRect.x - 2, fullRect.y, fullRect.width + 4, fullRect.height);
            EditorGUI.DrawRect(bgRect, HeaderBgColor);

            float labelWidth = EditorGUIUtility.labelWidth;
            float clearWidth = 22f;
            float gap = 2f;

            Rect labelRect = new(fullRect.x, fullRect.y, labelWidth, fullRect.height);
            Rect clearRect = new(fullRect.xMax - clearWidth, fullRect.y, clearWidth, fullRect.height);
            Rect dropdownRect = new(
                fullRect.x + labelWidth + gap,
                fullRect.y,
                fullRect.width - labelWidth - clearWidth - gap * 2,
                fullRect.height
            );

            _headerLabelStyle ??= new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
            EditorGUI.LabelField(labelRect, property.displayName, _headerLabelStyle);

            string displayType = hasValue ? ExtractTypeName(property.managedReferenceFullTypename) : "(None)";

            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(displayType), FocusType.Keyboard))
            {
                ShowTypeMenu(property, field.ManagedReferenceBaseType);
            }

            using (new EditorGUI.DisabledScope(!hasValue))
            {
                if (GUI.Button(clearRect, "×", EditorStyles.miniButtonRight))
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static void ShowTypeMenu(SerializedProperty property, Type baseType)
        {
            var types = ManagedReferenceTypeCache.GetCompatibleTypes(baseType);
            var menu = new GenericMenu();

            // Capture stable references for async callback
            string propertyPath = property.propertyPath;
            var serializedObject = property.serializedObject;
            Type currentType = GetCurrentInstanceType(property);

            menu.AddItem(
                new GUIContent("None"),
                currentType == null,
                () => AssignType(serializedObject, propertyPath, null)
            );

            if (types.Length == 0)
            {
                menu.AddDisabledItem(new GUIContent("No compatible types found"));
            }
            else
            {
                menu.AddSeparator("");

                foreach (var type in types)
                {
                    string path = FormatMenuPath(type);
                    bool isSelected = type == currentType;

                    var capturedType = type;
                    menu.AddItem(
                        new GUIContent(path),
                        isSelected,
                        () => AssignType(serializedObject, propertyPath, capturedType)
                    );
                }
            }

            menu.ShowAsContext();
        }

        private static void AssignType(SerializedObject serializedObject, string propertyPath, Type type)
        {
            var prop = serializedObject.FindProperty(propertyPath);
            if (prop == null)
                return;

            if (type == null)
            {
                prop.managedReferenceValue = null;
            }
            else
            {
                try
                {
                    prop.managedReferenceValue = Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Valkyrie: failed to create instance of {type.Name} — {e.Message}");
                    return;
                }
            }

            serializedObject.ApplyModifiedProperties();
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

        private static Type GetCurrentInstanceType(SerializedProperty property)
        {
            try
            {
                return property.managedReferenceValue?.GetType();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// managedReferenceFullTypename format: "assemblyName typeFullName"
        /// </summary>
        private static string ExtractTypeName(string fullTypename)
        {
            if (string.IsNullOrEmpty(fullTypename))
                return "(None)";

            int spaceIdx = fullTypename.IndexOf(' ');
            string typeFullName = spaceIdx >= 0 ? fullTypename.Substring(spaceIdx + 1) : fullTypename;

            int lastDot = typeFullName.LastIndexOf('.');
            string shortName = lastDot >= 0 ? typeFullName.Substring(lastDot + 1) : typeFullName;

            return ObjectNames.NicifyVariableName(shortName);
        }

        private static string FormatMenuPath(Type type)
        {
            string niceName = ObjectNames.NicifyVariableName(type.Name);

            if (string.IsNullOrEmpty(type.Namespace))
                return niceName;

            return type.Namespace.Replace('.', '/') + "/" + niceName;
        }
    }
}

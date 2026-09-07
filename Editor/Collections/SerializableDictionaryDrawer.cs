using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Astra.Valkyrie.Collections;

namespace Astra.Valkyrie.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionaryBase), true)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Editor", "Valkyrie.Editor")]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private static readonly Color DuplicateKeyColor = new(1f, 0.3f, 0.3f, 0.15f);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var entries = property.FindPropertyRelative("_entries");
            if (entries == null)
                return EditorGUIUtility.singleLineHeight;

            float height = EditorGUI.GetPropertyHeight(entries, label, true);

            if (entries.isExpanded && FindDuplicateIndices(entries, out _))
                height += EditorGUIUtility.singleLineHeight * 2 + 4;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var entries = property.FindPropertyRelative("_entries");
            if (entries == null)
            {
                EditorGUI.LabelField(position, label.text, "Error: _entries not found");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var dictLabel = new GUIContent($"{label.text} ({entries.arraySize})", label.tooltip);

            float listHeight = EditorGUI.GetPropertyHeight(entries, dictLabel, true);
            Rect listRect = new(position.x, position.y, position.width, listHeight);

            EditorGUI.PropertyField(listRect, entries, dictLabel, true);

            if (entries.isExpanded && FindDuplicateIndices(entries, out var duplicates))
            {
                HighlightDuplicates(entries, listRect, duplicates);

                float warningHeight = EditorGUIUtility.singleLineHeight * 2;
                Rect warningRect = new(position.x, listRect.yMax + 2, position.width, warningHeight);
                EditorGUI.HelpBox(warningRect,
                    "Duplicate keys detected. Only the first occurrence of each key is used at runtime.",
                    MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }

        private static bool FindDuplicateIndices(SerializedProperty entries, out HashSet<int> duplicates)
        {
            duplicates = null;
            var seen = new Dictionary<string, int>();

            for (int i = 0; i < entries.arraySize; i++)
            {
                var keyProp = entries.GetArrayElementAtIndex(i).FindPropertyRelative("key");
                if (keyProp == null)
                    continue;

                if (!TryGetKeyString(keyProp, out string keyStr))
                    continue;

                if (seen.TryGetValue(keyStr, out _))
                {
                    duplicates ??= new HashSet<int>();
                    duplicates.Add(i);
                }
                else
                {
                    seen[keyStr] = i;
                }
            }

            return duplicates != null;
        }

        // Geometry of Unity's default array drawer (ReorderableListWrapper + ReorderableList
        // internals, UnityCsReference 6000.x): foldout header, header padding, inner list
        // header (minHeaderHeight), top padding, then each element occupies its property
        // height plus a fixed padding — with no extra spacing between elements.
        private const float FoldoutHeaderHeight = 18f;   // ReorderableListWrapper.Constants.kDefaultFoldoutHeaderHeight
        private const float HeaderPadding = 3f;          // ReorderableListWrapper.Constants.kHeaderPadding
        private const float InnerListHeaderHeight = 2f;  // ReorderableList.Defaults.minHeaderHeight (header hidden)
        private const float ListElementTopPadding = 1f;  // ReorderableList.listElementTopPadding when headerHeight <= 5
        private const float ElementPadding = 2f;         // ReorderableList.Defaults.elementPadding

        private static void HighlightDuplicates(SerializedProperty entries, Rect listRect, HashSet<int> duplicates)
        {
            float y = listRect.y + FoldoutHeaderHeight + HeaderPadding + InnerListHeaderHeight + ListElementTopPadding;

            for (int i = 0; i < entries.arraySize; i++)
            {
                var element = entries.GetArrayElementAtIndex(i);
                float elementHeight = EditorGUI.GetPropertyHeight(element, true);

                if (duplicates.Contains(i))
                {
                    // +1/-1 horizontal: the list box insets its content by one pixel.
                    Rect highlight = new(listRect.x + 1, y + ElementPadding / 2f, listRect.width - 2, elementHeight);
                    EditorGUI.DrawRect(highlight, DuplicateKeyColor);
                }

                y += elementHeight + ElementPadding;
            }
        }

        private static bool TryGetKeyString(SerializedProperty prop, out string result)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.String:
                    result = prop.stringValue ?? "";
                    return true;
                case SerializedPropertyType.Integer:
                    result = prop.longValue.ToString();
                    return true;
                case SerializedPropertyType.Float:
                    result = prop.doubleValue.ToString("R");
                    return true;
                case SerializedPropertyType.Boolean:
                    result = prop.boolValue.ToString();
                    return true;
                case SerializedPropertyType.Enum:
                    result = prop.enumValueIndex.ToString();
                    return true;
                case SerializedPropertyType.ObjectReference:
#if UNITY_6000_4_OR_NEWER
                    result = prop.objectReferenceEntityIdValue.ToString();
#else
                    result = prop.objectReferenceInstanceIDValue.ToString();
#endif
                    return true;
                default:
                    result = null;
                    return false;
            }
        }
    }
}

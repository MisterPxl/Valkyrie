using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Valkyrie.Editor
{
    internal static class NestedObjectListRenderer
    {
        private static readonly ConditionalWeakTable<SerializedObject, Dictionary<string, ReorderableList>> Lists = new();

        public static float GetHeight(SerializedProperty property) => GetList(property).GetHeight();
        public static void Draw(Rect rect, SerializedProperty property) => GetList(property).DoList(rect);

        private static ReorderableList GetList(SerializedProperty property)
        {
            var cache = Lists.GetOrCreateValue(property.serializedObject);
            if (cache.TryGetValue(property.propertyPath, out ReorderableList list))
            {
                // Rebind after replacing a containing managed reference or undoing an edit.
                list.serializedProperty = property.Copy();
                return list;
            }
            list = new ReorderableList(property.serializedObject, property.Copy(), true, true, true, true);
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, list.serializedProperty.displayName, EditorStyles.boldLabel);
            list.elementHeightCallback = index => index < list.serializedProperty.arraySize
                ? PropertyRenderer.GetHeight(list.serializedProperty.GetArrayElementAtIndex(index)) + 4f : 0f;
            list.drawElementCallback = (rect, index, active, focused) =>
            {
                if (index >= list.serializedProperty.arraySize) return;
                rect.y += 2f;
                rect.height -= 4f;
                PropertyRenderer.Draw(rect, list.serializedProperty.GetArrayElementAtIndex(index));
            };
            cache[property.propertyPath] = list;
            return list;
        }
    }
}

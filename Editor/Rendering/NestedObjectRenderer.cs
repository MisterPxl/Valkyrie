using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    internal static class NestedObjectRenderer
    {
        private sealed class Slot
        {
            public string Group;
            public readonly List<SerializedProperty> Properties = new();
        }

        private static readonly Dictionary<Type, bool> Needed = new();
        private static readonly List<(Type Type, bool Children)> Drawers = FindDrawers();
        private static float Spacing => EditorGUIUtility.standardVerticalSpacing;

        public static bool Handles(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.Generic) return false;
            Type type = SerializedPropertyContext.GetValueType(property);
            InspectedField field = SerializedPropertyContext.GetField(property);
            if (HasDrawer(type)) return false;
            if (field != null)
                foreach (var attribute in field.FieldInfo.GetCustomAttributes<PropertyAttribute>())
                    if (HasDrawer(attribute.GetType())) return false;
            if (type == null) return false;
            if (!Needed.TryGetValue(type, out bool result))
                Needed[type] = result = NeedsRendering(type, new HashSet<Type>());
            return result;
        }

        private static bool NeedsRendering(Type type, HashSet<Type> visited)
        {
            if (type == null || !visited.Add(type) || HasDrawer(type)
                || typeof(UnityEngine.Object).IsAssignableFrom(type)
                || type.IsPrimitive || type.IsEnum || type == typeof(string)) return false;
            if (Attribute.IsDefined(type, typeof(DisableValkyrieInspectorAttribute), true)) return false;
            if (type.IsArray) return NeedsRendering(type.GetElementType(), visited);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return NeedsRendering(type.GetGenericArguments()[0], visited);
            foreach (InspectedField field in ReflectionCache.Get(type).Fields)
                if (field.HasValkyrieAttributes || field.IsManagedReference || field.IsManagedReferenceCollection
                    || NeedsRendering(field.FieldInfo.FieldType, visited)) return true;
            return false;
        }

        // Read constructor arguments, without depending on Unity's private drawer-registration fields.
        private static List<(Type, bool)> FindDrawers()
        {
            var result = new List<(Type, bool)>();
            foreach (Type drawer in TypeCache.GetTypesDerivedFrom<PropertyDrawer>())
                foreach (CustomAttributeData attr in drawer.GetCustomAttributesData())
                    if (attr.AttributeType == typeof(CustomPropertyDrawer) && attr.ConstructorArguments.Count > 0)
                        result.Add(((Type)attr.ConstructorArguments[0].Value,
                            attr.ConstructorArguments.Count > 1 && (bool)attr.ConstructorArguments[1].Value));
            return result;
        }

        private static bool HasDrawer(Type type)
        {
            if (type == null) return false;
            foreach (var drawer in Drawers)
                for (Type current = type; current != null; current = current.BaseType)
                    if ((current == type || drawer.Children)
                        && (current == drawer.Type || current.IsGenericType && current.GetGenericTypeDefinition() == drawer.Type))
                        return true;
            return false;
        }

        public static float GetHeight(SerializedProperty property)
        {
            if (property.isArray) return NestedObjectListRenderer.GetHeight(property);
            return EditorGUIUtility.singleLineHeight + (property.isExpanded ? GetChildrenHeight(property) : 0f);
        }

        public static void Draw(Rect rect, SerializedProperty property)
        {
            if (property.isArray) { NestedObjectListRenderer.Draw(rect, property); return; }
            var header = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(header, property.isExpanded, property.displayName, true);
            if (property.isExpanded)
                DrawChildren(new Rect(rect.x, header.yMax, rect.width, rect.height - header.height), property);
        }

        public static float GetChildrenHeight(SerializedProperty parent)
        {
            float height = 0f;
            foreach (Slot slot in GetSlots(parent))
            {
                float fieldsHeight = GetFieldsHeight(slot);
                if (fieldsHeight == 0f) continue;
                if (slot.Group == null) height += fieldsHeight;
                else
                {
                    height += Spacing + EditorGUIUtility.singleLineHeight;
                    if (EditorStateCache.Get(GroupKey(parent, slot.Group), false)) height += fieldsHeight;
                }
            }
            return height;
        }

        public static void DrawChildren(Rect rect, SerializedProperty parent)
        {
            EditorGUI.indentLevel++;
            try
            {
                float y = rect.y;
                foreach (Slot slot in GetSlots(parent))
                {
                    if (GetFieldsHeight(slot) == 0f) continue;
                    bool grouped = slot.Group != null;
                    if (grouped)
                    {
                        string key = GroupKey(parent, slot.Group);
                        y += Spacing;
                        var header = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
                        bool expanded = EditorGUI.Foldout(header, EditorStateCache.Get(key, false), slot.Group, true);
                        EditorStateCache.Set(key, expanded);
                        y = header.yMax;
                        if (!expanded) continue;
                        EditorGUI.indentLevel++;
                    }
                    try
                    {
                        foreach (SerializedProperty child in slot.Properties)
                        {
                            float height = PropertyRenderer.GetHeight(child);
                            if (height == 0f) continue;
                            y += Spacing;
                            PropertyRenderer.Draw(new Rect(rect.x, y, rect.width, height), child);
                            y += height;
                        }
                    }
                    finally { if (grouped) EditorGUI.indentLevel--; }
                }
            }
            finally { EditorGUI.indentLevel--; }
        }

        private static float GetFieldsHeight(Slot slot)
        {
            float height = 0f;
            foreach (SerializedProperty child in slot.Properties)
            {
                float childHeight = PropertyRenderer.GetHeight(child);
                if (childHeight > 0f) height += Spacing + childHeight;
            }
            return height;
        }

        private static List<Slot> GetSlots(SerializedProperty parent)
        {
            var slots = new List<Slot>();
            var groups = new Dictionary<string, Slot>();
            SerializedProperty iterator = parent.Copy();
            SerializedProperty end = iterator.GetEndProperty();
            if (!iterator.NextVisible(true)) return slots;
            do
            {
                if (SerializedProperty.EqualContents(iterator, end)) break;
                string group = SerializedPropertyContext.GetField(iterator)?.FoldoutGroup?.GroupName;
                Slot slot;
                if (group == null || !groups.TryGetValue(group, out slot))
                {
                    slot = new Slot { Group = group };
                    slots.Add(slot);
                    if (group != null) groups[group] = slot;
                }
                slot.Properties.Add(iterator.Copy());
            } while (iterator.NextVisible(false));
            return slots;
        }

        private static string GroupKey(SerializedProperty parent, string group)
        {
#if UNITY_6000_3_OR_NEWER
            string id = parent.serializedObject.targetObject.GetEntityId().ToString();
#else
            string id = parent.serializedObject.targetObject.GetInstanceID().ToString();
#endif
            return id + ":" + parent.propertyPath + ":" + group;
        }
    }
}

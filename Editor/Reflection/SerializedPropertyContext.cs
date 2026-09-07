using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Astra.Valkyrie.Editor
{
    /// <summary>Finds the owning object of a field, including polymorphic list elements.</summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Editor", "Valkyrie.Editor")]
    public static class SerializedPropertyContext
    {
        private static readonly Dictionary<(Type, string), FieldInfo> Fields = new();

        public static object GetValue(object root, string path)
        {
            object value = root;
            string[] parts = path.Replace(".Array.data[", "[").Split('.');
            foreach (string part in parts)
            {
                if (value == null) return null;
                int bracket = part.IndexOf('[');
                string name = bracket < 0 ? part : part.Substring(0, bracket);
                if (name.Length > 0)
                    value = FindField(value.GetType(), name)?.GetValue(value);
                if (bracket >= 0)
                {
                    if (!int.TryParse(part.Substring(bracket + 1).TrimEnd(']'), out int index)
                        || !(value is IList list) || index < 0 || index >= list.Count) return null;
                    value = list[index];
                }
            }
            return value;
        }

        public static object[] GetOwners(SerializedProperty property)
        {
            string path = property.propertyPath;
            int dot = path.LastIndexOf('.');
            string parent = dot < 0 ? null : path.Substring(0, dot);
            var targets = property.serializedObject.targetObjects;
            var owners = new object[targets.Length];
            for (int i = 0; i < targets.Length; i++)
                owners[i] = parent == null ? targets[i] : GetValue(targets[i], parent);
            return owners;
        }

        public static InspectedField GetField(SerializedProperty property)
        {
            if (property.name.StartsWith("data[", StringComparison.Ordinal)) return null;
            object[] owners = GetOwners(property);
            object owner = owners.Length > 0 ? owners[0] : null;
            return owner == null ? null : ReflectionCache.Get(owner.GetType()).GetField(property.name);
        }

        public static Type GetValueType(SerializedProperty property)
        {
            return GetValue(property.serializedObject.targetObject, property.propertyPath)?.GetType()
                ?? GetField(property)?.FieldInfo.FieldType;
        }

        private static FieldInfo FindField(Type type, string name)
        {
            var key = (type, name);
            if (Fields.TryGetValue(key, out FieldInfo result)) return result;
            for (Type current = type; current != null; current = current.BaseType)
            {
                result = current.GetField(name, BindingFlags.Instance | BindingFlags.Public
                    | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (result != null) break;
            }
            Fields[key] = result;
            return result;
        }
    }
}

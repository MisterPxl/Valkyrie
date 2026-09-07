using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    /// <summary>Session-local clipboard for serialized managed-reference graphs.</summary>
    public static class ManagedReferenceClipboard
    {
        private sealed class Snapshot
        {
            public string Json;
            public readonly Dictionary<string, UnityEngine.Object> References = new();
        }
        private static Snapshot _snapshot;
        private static Type _type;
        public static bool HasValue => _snapshot != null && _type != null;

        /// <summary>Allows addons to refresh the identity of a pasted or duplicated root.</summary>
        public static event Action<object> CloneCreated;

        public static bool Copy(SerializedObject owner, string path)
        {
            if (owner == null) return false;
            owner.ApplyModifiedProperties();
            SerializedProperty property = owner.FindProperty(path);
            if (property == null || property.propertyType != SerializedPropertyType.ManagedReference
                || property.hasMultipleDifferentValues || property.managedReferenceValue == null) return false;
            object value = property.managedReferenceValue;
            _snapshot = Serialize(value);
            _type = value.GetType();
            return true;
        }

        public static bool CanPaste(SerializedObject owner, string path)
        {
            if (!HasValue || owner == null) return false;
            foreach (UnityEngine.Object target in owner.targetObjects)
            {
                if (target == null) return false;
                using var individual = new SerializedObject(target);
                SerializedProperty property = individual.FindProperty(path);
                Type declared = ManagedReferenceTypeNameUtility.GetFieldType(property);
                if (declared == null || !declared.IsAssignableFrom(_type)) return false;
            }
            return true;
        }

        public static bool Paste(SerializedObject owner, string path)
        {
            if (owner == null) return false;
            owner.ApplyModifiedProperties();
            if (!CanPaste(owner, path)) return false;
            // Create all graphs before changing any selected object.
            var clones = new List<object>();
            foreach (UnityEngine.Object target in owner.targetObjects) clones.Add(Deserialize(_snapshot));
            Undo.SetCurrentGroupName("Paste Managed Reference");
            var targets = owner.targetObjects;
            for (int i = 0; i < targets.Length; i++)
            {
                using var individual = new SerializedObject(targets[i]);
                var property = individual.FindProperty(path);
                property.managedReferenceValue = clones[i];
                property.isExpanded = true;
                individual.ApplyModifiedProperties();
            }
            owner.Update();
            return true;
        }

        public static bool Duplicate(SerializedObject owner, string listPath, int index)
        {
            if (owner == null || index < 0) return false;
            owner.ApplyModifiedProperties();
            var clones = new List<object>();
            foreach (UnityEngine.Object target in owner.targetObjects)
            {
                if (target == null) return false;
                using var individual = new SerializedObject(target);
                var list = individual.FindProperty(listPath);
                if (list == null || !list.isArray || index >= list.arraySize) return false;
                var element = list.GetArrayElementAtIndex(index);
                if (element.propertyType != SerializedPropertyType.ManagedReference) return false;
                clones.Add(element.managedReferenceValue == null ? null : Deserialize(Serialize(element.managedReferenceValue)));
            }
            Undo.SetCurrentGroupName("Duplicate Managed Reference");
            var targets = owner.targetObjects;
            for (int i = 0; i < targets.Length; i++)
            {
                using var individual = new SerializedObject(targets[i]);
                var list = individual.FindProperty(listPath);
                list.InsertArrayElementAtIndex(index + 1);
                var element = list.GetArrayElementAtIndex(index + 1);
                element.managedReferenceValue = clones[i];
                element.isExpanded = true;
                individual.ApplyModifiedProperties();
            }
            owner.Update();
            return true;
        }

        public static bool TryGetListElement(string path, out string listPath, out int index)
        {
            const string marker = ".Array.data[";
            int start = path.LastIndexOf(marker, StringComparison.Ordinal);
            listPath = null;
            index = -1;
            if (start < 0 || !path.EndsWith("]", StringComparison.Ordinal)
                || !int.TryParse(path.Substring(start + marker.Length).TrimEnd(']'), out index)) return false;
            listPath = path.Substring(0, start);
            return true;
        }

        public static void Clear() { _snapshot = null; _type = null; }

        private static Snapshot Serialize(object value)
        {
            var container = ScriptableObject.CreateInstance<ClipboardContainer>();
            try
            {
                container.value = value;
                var snapshot = new Snapshot { Json = EditorJsonUtility.ToJson(container) };
                using var serialized = new SerializedObject(container);
                var iterator = serialized.GetIterator();
                var visited = new HashSet<long>();
                bool enterChildren = true;
                while (iterator.Next(enterChildren))
                {
                    enterChildren = true;
                    if (iterator.propertyType == SerializedPropertyType.ManagedReference)
                        enterChildren = visited.Add(iterator.managedReferenceId);
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference
                        && iterator.propertyPath.StartsWith("value.", StringComparison.Ordinal))
                        snapshot.References[iterator.propertyPath] = iterator.objectReferenceValue;
                }
                return snapshot;
            }
            finally { UnityEngine.Object.DestroyImmediate(container); }
        }

        private static object Deserialize(Snapshot snapshot)
        {
            var container = ScriptableObject.CreateInstance<ClipboardContainer>();
            try
            {
                EditorJsonUtility.FromJsonOverwrite(snapshot.Json, container);
                using (var serialized = new SerializedObject(container))
                {
                    foreach (var reference in snapshot.References)
                    {
                        var property = serialized.FindProperty(reference.Key);
                        if (property != null && property.propertyType == SerializedPropertyType.ObjectReference)
                            property.objectReferenceValue = reference.Value;
                    }
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                object clone = container.value;
                if (clone == null) throw new InvalidOperationException("The copied reference could not be restored.");
                CloneCreated?.Invoke(clone);
                return clone;
            }
            finally { UnityEngine.Object.DestroyImmediate(container); }
        }

        private sealed class ClipboardContainer : ScriptableObject
        {
            [SerializeReference] public object value;
        }
    }
}

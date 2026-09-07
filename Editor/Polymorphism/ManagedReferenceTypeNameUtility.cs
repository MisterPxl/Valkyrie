using System;
using System.Reflection;
using UnityEditor;

namespace Astra.Valkyrie.Editor
{
    internal static class ManagedReferenceTypeNameUtility
    {
        // Assembly.Load + GetType are called for every managed-reference child on
        // every frame; memoize per typename (negative results included). The static
        // cache is wiped by domain reload, which is exactly when it can go stale.
        private static readonly System.Collections.Generic.Dictionary<string, Type> TypeCache = new();

        public static Type GetType(string unityTypename)
        {
            if (string.IsNullOrEmpty(unityTypename))
                return null;

            if (TypeCache.TryGetValue(unityTypename, out Type cached))
                return cached;

            Type resolved = ResolveType(unityTypename);
            TypeCache[unityTypename] = resolved;
            return resolved;
        }

        private static Type ResolveType(string unityTypename)
        {
            int splitIndex = unityTypename.IndexOf(' ');
            if (splitIndex <= 0 || splitIndex >= unityTypename.Length - 1)
                return null;

            string assemblyName = unityTypename.Substring(0, splitIndex);
            string typeName = unityTypename.Substring(splitIndex + 1);

            try
            {
                Assembly assembly = Assembly.Load(assemblyName);
                return assembly.GetType(typeName);
            }
            catch
            {
                return null;
            }
        }

        public static Type GetFieldType(SerializedProperty property)
        {
            if (property == null || property.propertyType != SerializedPropertyType.ManagedReference)
                return null;

            return GetType(property.managedReferenceFieldTypename);
        }

        public static Type GetValueType(SerializedProperty property)
        {
            if (property == null || property.propertyType != SerializedPropertyType.ManagedReference)
                return null;

            return GetType(property.managedReferenceFullTypename);
        }
    }
}

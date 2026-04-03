using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Valkyrie.Editor
{
    public static class ManagedReferenceTypeCache
    {
        private static readonly Dictionary<Type, Type[]> Cache = new();

        public static Type[] GetCompatibleTypes(Type baseType)
        {
            if (baseType == null)
                return Array.Empty<Type>();

            if (!Cache.TryGetValue(baseType, out var types))
            {
                types = BuildTypeList(baseType);
                Cache[baseType] = types;
            }
            return types;
        }

        public static void Clear() => Cache.Clear();

        private static Type[] BuildTypeList(Type baseType)
        {
            return UnityEditor.TypeCache.GetTypesDerivedFrom(baseType)
                .Where(IsValidForManagedReference)
                .OrderBy(t => t.Name)
                .ToArray();
        }

        private static bool IsValidForManagedReference(Type type)
        {
            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                return false;

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return false;

            if (!Attribute.IsDefined(type, typeof(SerializableAttribute)))
                return false;

            bool hasParameterlessCtor = type.IsValueType
                || type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, Type.EmptyTypes, null) != null;

            return hasParameterlessCtor;
        }
    }
}

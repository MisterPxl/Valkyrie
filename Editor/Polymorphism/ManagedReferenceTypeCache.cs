using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Valkyrie.Editor
{
    public static class ManagedReferenceTypeCache
    {
        private static readonly Dictionary<Type, Type[]> Cache = new();
        private static readonly IManagedReferenceCandidateProvider NonGenericProvider = new TypeCacheCandidateProvider();
        private static readonly IManagedReferenceCandidateProvider GenericProvider = new LoadedAssemblyCandidateProvider();
        private static readonly ManagedReferenceTypeEligibilityPolicy EligibilityPolicy = new();
        private static readonly ManagedReferenceTypeCompatibilityPolicy CompatibilityPolicy = new();

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

        internal static bool IsValidForManagedReference(Type type)
        {
            return EligibilityPolicy.IsAllowed(type);
        }

        internal static bool IsCompatible(Type baseType, Type candidateType)
        {
            return CompatibilityPolicy.IsCompatible(baseType, candidateType);
        }

        private static Type[] BuildTypeList(Type baseType)
        {
            IManagedReferenceCandidateProvider provider = baseType.IsGenericType
                ? GenericProvider
                : NonGenericProvider;

            return provider.GetCandidates(baseType)
                .Where(EligibilityPolicy.IsAllowed)
                .Where(type => CompatibilityPolicy.IsCompatible(baseType, type))
                .Distinct()
                .OrderBy(type => type.Namespace)
                .ThenBy(type => type.Name)
                .ToArray();
        }

        private interface IManagedReferenceCandidateProvider
        {
            IEnumerable<Type> GetCandidates(Type baseType);
        }

        private sealed class TypeCacheCandidateProvider : IManagedReferenceCandidateProvider
        {
            public IEnumerable<Type> GetCandidates(Type baseType)
            {
                foreach (Type type in TypeCache.GetTypesDerivedFrom(baseType))
                    yield return type;

                yield return baseType;
            }
        }

        private sealed class LoadedAssemblyCandidateProvider : IManagedReferenceCandidateProvider
        {
            public IEnumerable<Type> GetCandidates(Type baseType)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types = GetTypesSafely(assembly);
                    for (int i = 0; i < types.Length; i++)
                    {
                        Type type = types[i];
                        if (type != null)
                            yield return type;
                    }
                }

                yield return baseType;
            }

            private static Type[] GetTypesSafely(Assembly assembly)
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    return exception.Types;
                }
            }
        }

        private sealed class ManagedReferenceTypeEligibilityPolicy
        {
            public bool IsAllowed(Type type)
            {
                if (type == null)
                    return false;

                if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition || type.ContainsGenericParameters)
                    return false;

                if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    return false;

                if (!Attribute.IsDefined(type, typeof(SerializableAttribute)))
                    return false;

                return HasParameterlessConstructor(type);
            }

            private static bool HasParameterlessConstructor(Type type)
            {
                if (type.IsValueType)
                    return true;

                ConstructorInfo constructor = type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, Type.EmptyTypes, null);

                return constructor != null;
            }
        }

        private sealed class ManagedReferenceTypeCompatibilityPolicy
        {
            public bool IsCompatible(Type baseType, Type candidateType)
            {
                if (baseType == null || candidateType == null)
                    return false;

                if (baseType.IsGenericTypeDefinition || baseType.ContainsGenericParameters)
                    return false;

                if (baseType.IsAssignableFrom(candidateType))
                    return true;

                if (!baseType.IsGenericType)
                    return false;

                return IsCompatibleGeneric(baseType, candidateType);
            }

            private static bool IsCompatibleGeneric(Type baseType, Type candidateType)
            {
                Type genericDefinition = baseType.GetGenericTypeDefinition();
                Type[] targetArguments = baseType.GetGenericArguments();
                Type[] genericParameters = genericDefinition.GetGenericArguments();

                foreach (Type interfaceType in candidateType.GetInterfaces())
                {
                    if (!interfaceType.IsGenericType)
                        continue;

                    if (interfaceType.GetGenericTypeDefinition() != genericDefinition)
                        continue;

                    if (AreArgumentsCompatible(genericParameters, targetArguments, interfaceType.GetGenericArguments()))
                        return true;
                }

                for (Type current = candidateType; current != null && current != typeof(object); current = current.BaseType)
                {
                    if (!current.IsGenericType)
                        continue;

                    if (current.GetGenericTypeDefinition() != genericDefinition)
                        continue;

                    if (AreArgumentsCompatible(genericParameters, targetArguments, current.GetGenericArguments()))
                        return true;
                }

                return false;
            }

            private static bool AreArgumentsCompatible(Type[] genericParameters, Type[] targetArguments, Type[] sourceArguments)
            {
                if (genericParameters.Length != targetArguments.Length || sourceArguments.Length != targetArguments.Length)
                    return false;

                for (int i = 0; i < genericParameters.Length; i++)
                {
                    GenericParameterAttributes variance = genericParameters[i].GenericParameterAttributes
                        & GenericParameterAttributes.VarianceMask;

                    Type target = targetArguments[i];
                    Type source = sourceArguments[i];

                    if (variance == GenericParameterAttributes.Covariant)
                    {
                        if (!target.IsAssignableFrom(source))
                            return false;
                    }
                    else if (variance == GenericParameterAttributes.Contravariant)
                    {
                        if (!source.IsAssignableFrom(target))
                            return false;
                    }
                    else if (source != target)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}

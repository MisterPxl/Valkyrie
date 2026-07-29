using System;
using System.Reflection;
using UnityEditor;

namespace Valkyrie.Editor
{
    internal static class ManagedReferenceTypeNameUtility
    {
        public static Type GetType(string unityTypename)
        {
            if (string.IsNullOrEmpty(unityTypename))
                return null;

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

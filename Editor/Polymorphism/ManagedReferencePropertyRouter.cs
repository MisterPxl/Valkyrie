using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    public static class ManagedReferencePropertyRouter
    {
        private const BindingFlags FieldLookupFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static void DrawGUILayout(SerializedProperty property)
        {
            if (property == null)
                return;

            if (TryGetManagedReferenceBaseType(property, out Type baseType))
            {
                ManagedReferenceRenderer.DrawElement(property, baseType, property.displayName);
                return;
            }

            if (TryGetManagedReferenceCollectionElementType(property, out Type elementType))
            {
                ManagedReferenceListRenderer.Draw(property, elementType);
                return;
            }

            EditorGUILayout.PropertyField(property, true);
        }

        public static float GetPropertyHeight(SerializedProperty property)
        {
            if (property == null)
                return EditorGUIUtility.singleLineHeight;

            if (TryGetManagedReferenceBaseType(property, out Type baseType))
                return ManagedReferenceRenderer.GetElementHeight(property, baseType);

            if (TryGetManagedReferenceCollectionElementType(property, out Type elementType))
                return ManagedReferenceListRenderer.GetHeight(property, elementType);

            return EditorGUI.GetPropertyHeight(property, true);
        }

        public static void DrawGUI(Rect rect, SerializedProperty property)
        {
            if (property == null)
                return;

            if (TryGetManagedReferenceBaseType(property, out Type baseType))
            {
                ManagedReferenceRenderer.DrawElement(rect, property, baseType, property.displayName);
                return;
            }

            if (TryGetManagedReferenceCollectionElementType(property, out Type elementType))
            {
                ManagedReferenceListRenderer.Draw(rect, property, elementType);
                return;
            }

            EditorGUI.PropertyField(rect, property, true);
        }

        public static bool TryGetManagedReferenceBaseType(SerializedProperty property, out Type baseType)
        {
            baseType = null;
            if (property == null || property.propertyType != SerializedPropertyType.ManagedReference)
                return false;

            baseType = ManagedReferenceTypeNameUtility.GetFieldType(property) ?? ResolveDeclaredType(property);
            return baseType != null;
        }

        public static bool TryGetManagedReferenceCollectionElementType(SerializedProperty property, out Type elementType)
        {
            elementType = null;
            if (property == null || !property.isArray || property.propertyType == SerializedPropertyType.String)
                return false;

            FieldInfo field = ResolveField(property);
            if (field != null && field.IsDefined(typeof(SerializeReference), false))
            {
                elementType = GetCollectionElementType(field.FieldType);
                return elementType != null;
            }

            if (property.arraySize > 0)
            {
                SerializedProperty firstElement = property.GetArrayElementAtIndex(0);
                if (firstElement.propertyType == SerializedPropertyType.ManagedReference)
                {
                    elementType = ManagedReferenceTypeNameUtility.GetFieldType(firstElement);
                    return elementType != null;
                }
            }

            return false;
        }

        internal static Type ResolveDeclaredType(SerializedProperty property)
        {
            FieldInfo field = ResolveField(property);
            return field != null ? field.FieldType : null;
        }

        private static FieldInfo ResolveField(SerializedProperty property)
        {
            if (property == null || property.serializedObject == null || property.serializedObject.targetObject == null)
                return null;

            Type currentType = property.serializedObject.targetObject.GetType();
            FieldInfo currentField = null;
            string currentPath = string.Empty;
            string[] segments = property.propertyPath.Split('.');

            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                currentPath = AppendPath(currentPath, segment);

                if (segment == "Array")
                    continue;

                if (segment.StartsWith("data[", StringComparison.Ordinal))
                {
                    currentType = GetCollectionElementType(currentType);
                    currentType = ResolveRuntimeManagedReferenceType(property, currentPath, currentType);
                    continue;
                }

                currentField = FindField(currentType, segment);
                if (currentField == null)
                    return null;

                currentType = currentField.FieldType;
                currentType = ResolveRuntimeManagedReferenceType(property, currentPath, currentType);
            }

            return currentField;
        }

        private static Type ResolveRuntimeManagedReferenceType(SerializedProperty root, string propertyPath, Type fallbackType)
        {
            SerializedProperty property = root.serializedObject.FindProperty(propertyPath);
            if (property == null || property.propertyType != SerializedPropertyType.ManagedReference)
                return fallbackType;

            object value = property.managedReferenceValue;
            return value != null ? value.GetType() : fallbackType;
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            for (Type current = type; current != null && current != typeof(UnityEngine.Object); current = current.BaseType)
            {
                FieldInfo field = current.GetField(fieldName, FieldLookupFlags | BindingFlags.DeclaredOnly);
                if (field != null)
                    return field;
            }

            return null;
        }

        private static Type GetCollectionElementType(Type type)
        {
            if (type == null)
                return null;

            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                return type.GetGenericArguments()[0];

            return null;
        }

        private static string AppendPath(string currentPath, string segment)
        {
            return string.IsNullOrEmpty(currentPath) ? segment : currentPath + "." + segment;
        }
    }
}

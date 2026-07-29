using System;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    public static class ManagedReferenceMutationService
    {
        public static void AssignType(SerializedObject serializedObject, string propertyPath, Type type, bool preserveExistingValues)
        {
            if (serializedObject == null || string.IsNullOrEmpty(propertyPath))
                return;

            UnityEngine.Object[] targets = serializedObject.targetObjects;
            for (int i = 0; i < targets.Length; i++)
            {
                UnityEngine.Object target = targets[i];
                if (target == null)
                    continue;

                SerializedObject individualObject = new SerializedObject(target);
                individualObject.Update();

                SerializedProperty property = individualObject.FindProperty(propertyPath);
                if (property == null || property.propertyType != SerializedPropertyType.ManagedReference)
                    continue;

                SetManagedReference(property, type, preserveExistingValues);
                individualObject.ApplyModifiedProperties();
                individualObject.Update();
            }
        }

        public static void ResetCurrentType(SerializedObject serializedObject, string propertyPath)
        {
            if (serializedObject == null || string.IsNullOrEmpty(propertyPath))
                return;

            Type currentType = null;
            SerializedProperty sourceProperty = serializedObject.FindProperty(propertyPath);
            if (sourceProperty != null)
                currentType = ManagedReferenceTypeNameUtility.GetValueType(sourceProperty);

            if (currentType != null)
                AssignType(serializedObject, propertyPath, currentType, preserveExistingValues: false);
        }

        public static void AppendInstance(SerializedObject serializedObject, string propertyPath, Type type)
        {
            if (serializedObject == null || string.IsNullOrEmpty(propertyPath) || type == null)
                return;

            UnityEngine.Object[] targets = serializedObject.targetObjects;
            for (int i = 0; i < targets.Length; i++)
            {
                UnityEngine.Object target = targets[i];
                if (target == null)
                    continue;

                SerializedObject individualObject = new SerializedObject(target);
                individualObject.Update();

                SerializedProperty listProperty = individualObject.FindProperty(propertyPath);
                if (listProperty == null || !listProperty.isArray)
                    continue;

                int newIndex = listProperty.arraySize;
                listProperty.arraySize = newIndex + 1;

                SerializedProperty element = listProperty.GetArrayElementAtIndex(newIndex);
                if (!SetManagedReference(element, type, preserveExistingValues: false))
                {
                    listProperty.arraySize = newIndex;
                }

                individualObject.ApplyModifiedProperties();
                individualObject.Update();
            }
        }

        public static void RemoveAt(SerializedObject serializedObject, string propertyPath, int index)
        {
            if (serializedObject == null || string.IsNullOrEmpty(propertyPath))
                return;

            UnityEngine.Object[] targets = serializedObject.targetObjects;
            for (int i = 0; i < targets.Length; i++)
            {
                UnityEngine.Object target = targets[i];
                if (target == null)
                    continue;

                SerializedObject individualObject = new SerializedObject(target);
                individualObject.Update();

                SerializedProperty listProperty = individualObject.FindProperty(propertyPath);
                if (listProperty == null || !listProperty.isArray || listProperty.arraySize == 0)
                    continue;

                int removeIndex = index;
                if (removeIndex < 0 || removeIndex >= listProperty.arraySize)
                    removeIndex = listProperty.arraySize - 1;

                listProperty.DeleteArrayElementAtIndex(removeIndex);
                individualObject.ApplyModifiedProperties();
                individualObject.Update();
            }
        }

        internal static bool SetManagedReference(SerializedProperty property, Type type, bool preserveExistingValues)
        {
            if (property == null || property.propertyType != SerializedPropertyType.ManagedReference)
                return false;

            if (type == null)
            {
                property.managedReferenceValue = null;
                return true;
            }

            try
            {
                object instance = CreateInstance(property, type, preserveExistingValues);
                property.managedReferenceValue = instance;
                property.isExpanded = instance != null;
                return instance != null;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Valkyrie: failed to create instance of {type.Name}: {exception.Message}");
                return false;
            }
        }

        private static object CreateInstance(SerializedProperty property, Type type, bool preserveExistingValues)
        {
            if (preserveExistingValues && property.managedReferenceValue != null)
            {
                try
                {
                    string json = JsonUtility.ToJson(property.managedReferenceValue);
                    object restored = JsonUtility.FromJson(json, type);
                    if (restored != null)
                        return restored;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Valkyrie: could not preserve values when switching to {type.Name}: {exception.Message}");
                }
            }

            return Activator.CreateInstance(type, nonPublic: true);
        }
    }
}

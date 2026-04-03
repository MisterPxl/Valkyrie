using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    public static class InspectorRenderer
    {
        public static void Draw(SerializedObject serializedObject, Object[] targets, TypeData typeData)
        {
            serializedObject.Update();

            DrawScriptField(serializedObject);
            DrawLayout(serializedObject, targets[0], typeData);
            ButtonRenderer.DrawButtons(targets, typeData);

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawScriptField(SerializedObject serializedObject)
        {
            var scriptProp = serializedObject.FindProperty("m_Script");
            if (scriptProp == null)
                return;

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(scriptProp);
        }

        private static void DrawLayout(SerializedObject serializedObject, Object target, TypeData typeData)
        {
            int objectId = target.GetInstanceID();

            foreach (var slot in typeData.Layout)
            {
                if (!slot.IsGroup)
                {
                    var prop = serializedObject.FindProperty(slot.Field.Name);
                    if (prop != null)
                        PropertyRenderer.DrawField(prop, target, slot.Field);
                }
                else
                {
                    FoldoutRenderer.Draw(serializedObject, target, objectId, slot);
                }
            }
        }
    }
}

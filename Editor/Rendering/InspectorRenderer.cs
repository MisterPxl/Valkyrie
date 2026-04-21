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
            int objectId = GetStableObjectKey(target);

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

        /// <summary>
        /// Returns a stable per-target id usable as a key for editor-state caches
        /// (foldouts, expansion, etc.). Uses the modern EntityId on Unity 6+ and
        /// falls back to GetInstanceID on older editors.
        /// </summary>
        private static int GetStableObjectKey(Object target)
        {
#if UNITY_6000_0_OR_NEWER
            return target.GetEntityId().GetHashCode();
#else
            return target.GetInstanceID();
#endif
        }
    }
}

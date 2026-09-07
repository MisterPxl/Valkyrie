using UnityEditor;
using UnityEngine;

namespace Astra.Valkyrie.Editor
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Editor", "Valkyrie.Editor")]
    public static class InspectorRenderer
    {
        public static void Draw(SerializedObject serializedObject, Object[] targets, TypeData typeData)
        {
            serializedObject.Update();

            DrawScriptField(serializedObject);
            DrawLayout(serializedObject, targets, typeData);
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

        private static void DrawLayout(SerializedObject serializedObject, Object[] targets, TypeData typeData)
        {
            string objectId = GetStableObjectKey(targets[0]);

            foreach (var slot in typeData.Layout)
            {
                if (!slot.IsGroup)
                {
                    var prop = serializedObject.FindProperty(slot.Field.Name);
                    if (prop != null)
                        PropertyRenderer.DrawField(prop, targets, slot.Field);
                }
                else
                {
                    FoldoutRenderer.Draw(serializedObject, targets, objectId, slot);
                }
            }
        }

        /// <summary>
        /// Returns a stable per-target id usable as a key for editor-state caches
        /// (foldouts, expansion, etc.). Uses the full modern EntityId when available
        /// (not its 32-bit hash, which can collide) and falls back to GetInstanceID
        /// on older editors.
        /// </summary>
        private static string GetStableObjectKey(Object target)
        {
#if UNITY_6000_3_OR_NEWER
            return target.GetEntityId().ToString();
#else
            return target.GetInstanceID().ToString();
#endif
        }
    }
}

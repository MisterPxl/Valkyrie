using UnityEditor;

namespace Astra.Valkyrie.Editor
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Editor", "Valkyrie.Editor")]
    public static class FoldoutRenderer
    {
        public static void Draw(SerializedObject serializedObject, UnityEngine.Object[] targets, string objectId, LayoutSlot slot)
        {
            string stateKey = EditorStateCache.MakeKey(objectId, slot.GroupName);
            bool isExpanded = EditorStateCache.Get(stateKey, false);

            bool newExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(isExpanded, slot.GroupName);
            if (newExpanded != isExpanded)
                EditorStateCache.Set(stateKey, newExpanded);

            // try/finally: an exception while drawing a grouped field must not skip
            // EndFoldoutHeaderGroup, or the whole inspector cascades layout errors.
            try
            {
                if (newExpanded)
                {
                    EditorGUI.indentLevel++;

                    try
                    {
                        foreach (var field in slot.GroupFields)
                        {
                            var prop = serializedObject.FindProperty(field.Name);
                            if (prop != null)
                                PropertyRenderer.DrawField(prop, targets, field);
                        }
                    }
                    finally
                    {
                        EditorGUI.indentLevel--;
                    }
                }
            }
            finally
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
    }
}

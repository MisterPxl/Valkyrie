using UnityEditor;

namespace Valkyrie.Editor
{
    public static class FoldoutRenderer
    {
        public static void Draw(SerializedObject serializedObject, object target, int objectId, LayoutSlot slot)
        {
            string stateKey = EditorStateCache.MakeKey(objectId, slot.GroupName);
            bool isExpanded = EditorStateCache.Get(stateKey, false);

            bool newExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(isExpanded, slot.GroupName);
            if (newExpanded != isExpanded)
                EditorStateCache.Set(stateKey, newExpanded);

            if (newExpanded)
            {
                EditorGUI.indentLevel++;

                foreach (var field in slot.GroupFields)
                {
                    var prop = serializedObject.FindProperty(field.Name);
                    if (prop != null)
                        PropertyRenderer.DrawField(prop, target, field);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}

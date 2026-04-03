using System;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    public static class ButtonRenderer
    {
        private static readonly Color SeparatorColor = new(0.35f, 0.35f, 0.35f, 0.8f);

        public static void DrawButtons(UnityEngine.Object[] targets, TypeData typeData)
        {
            if (typeData.Methods.Length == 0)
                return;

            EditorGUILayout.Space(6);

            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, SeparatorColor);

            EditorGUILayout.Space(4);

            foreach (var method in typeData.Methods)
            {
                if (!method.IsValid)
                {
                    EditorGUILayout.HelpBox(method.InvalidReason, MessageType.Warning);
                    continue;
                }

                if (!GUILayout.Button(method.DisplayName))
                    continue;

                foreach (var target in targets)
                {
                    try
                    {
                        Undo.RecordObject(target, method.DisplayName);
                        method.Invoke(target);
                        EditorUtility.SetDirty(target);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e, target);
                    }
                }
            }
        }
    }
}

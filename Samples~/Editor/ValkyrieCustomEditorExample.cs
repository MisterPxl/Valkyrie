using UnityEditor;
using UnityEngine;
using Valkyrie.Editor;

/// <summary>
/// Demonstrates how a type-specific custom editor can keep Valkyrie's rendering.
/// </summary>
[CustomEditor(typeof(ValkyrieExampleSO))]
public sealed class ValkyrieCustomEditorExample : ValkyrieEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This panel is drawn by a type-specific custom editor.",
            MessageType.Info);
    }
}

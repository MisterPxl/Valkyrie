using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public sealed class ValkyrieScriptableObjectEditor : ValkyrieEditor { }
}

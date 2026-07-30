// Valkyrie installs itself as the default inspector for every ScriptableObject.
// Define VALKYRIE_DISABLE_GLOBAL_INSPECTOR (Project Settings > Player > Scripting Define Symbols)
// to opt out project-wide and fall back to Unity's default inspectors.
// Per-type opt-out is always available via [DisableValkyrieInspector].
#if !VALKYRIE_DISABLE_GLOBAL_INSPECTOR
using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    [CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
    [CanEditMultipleObjects]
    public sealed class ValkyrieScriptableObjectEditor : ValkyrieEditor { }
}
#endif

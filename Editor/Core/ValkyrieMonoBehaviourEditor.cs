using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public sealed class ValkyrieMonoBehaviourEditor : ValkyrieEditor { }
}

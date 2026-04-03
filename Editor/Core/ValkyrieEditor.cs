using UnityEditor;

namespace Valkyrie.Editor
{
    public abstract class ValkyrieEditor : UnityEditor.Editor
    {
        private TypeData _typeData;

        protected virtual void OnEnable()
        {
            if (target != null)
                _typeData = ReflectionCache.Get(target.GetType());
        }

        public override void OnInspectorGUI()
        {
            if (_typeData == null || target == null)
            {
                base.OnInspectorGUI();
                return;
            }

            InspectorRenderer.Draw(serializedObject, targets, _typeData);
        }
    }
}

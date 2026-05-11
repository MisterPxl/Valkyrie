using UnityEditor;

namespace Valkyrie.Editor
{
    public abstract class ValkyrieEditor : UnityEditor.Editor
    {
        private TypeData _typeData;
        private bool _useDefaultInspector;

        protected virtual void OnEnable()
        {
            if (target == null)
                return;

            var targetType = target.GetType();
            _useDefaultInspector = System.Attribute.IsDefined(
                targetType,
                typeof(DisableValkyrieInspectorAttribute),
                inherit: true);

            if (!_useDefaultInspector)
                _typeData = ReflectionCache.Get(targetType);
        }

        public override void OnInspectorGUI()
        {
            if (_useDefaultInspector || _typeData == null || target == null)
            {
                base.OnInspectorGUI();
                return;
            }

            InspectorRenderer.Draw(serializedObject, targets, _typeData);
        }
    }
}

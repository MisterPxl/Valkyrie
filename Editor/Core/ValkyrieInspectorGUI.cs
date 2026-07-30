using System;

namespace Valkyrie.Editor
{
    /// <summary>
    /// Draws Valkyrie's inspector from a custom Unity editor that cannot inherit
    /// from <see cref="ValkyrieEditor"/>.
    /// </summary>
    public static class ValkyrieInspectorGUI
    {
        /// <summary>
        /// Draws the complete Valkyrie inspector for the supplied editor.
        /// Serialization updates and property application are handled internally.
        /// </summary>
        public static void Draw(UnityEditor.Editor editor)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            UnityEngine.Object target = editor.target;
            if (target == null)
                return;

            Type targetType = target.GetType();
            bool useDefaultInspector = Attribute.IsDefined(
                targetType,
                typeof(DisableValkyrieInspectorAttribute),
                inherit: true);

            if (useDefaultInspector)
            {
                editor.DrawDefaultInspector();
                return;
            }

            TypeData typeData = ReflectionCache.Get(targetType);
            InspectorRenderer.Draw(editor.serializedObject, editor.targets, typeData);
        }
    }
}

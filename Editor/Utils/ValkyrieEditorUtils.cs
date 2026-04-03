using System.Reflection;

namespace Valkyrie.Editor
{
    public static class ValkyrieEditorUtils
    {
        private const BindingFlags LookupFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static object GetMemberValue(object target, string memberName)
        {
            if (target == null || string.IsNullOrEmpty(memberName))
                return null;

            var type = target.GetType();

            var field = type.GetField(memberName, LookupFlags);
            if (field != null)
                return field.GetValue(target);

            var prop = type.GetProperty(memberName, LookupFlags);
            if (prop != null)
                return prop.GetValue(target);

            return null;
        }

        public static bool IsTruthy(object value)
        {
            if (value == null) return false;
            if (value is bool b) return b;
            if (value is int i) return i != 0;
            if (value is float f) return f != 0f;
            if (value is string s) return !string.IsNullOrEmpty(s);
            if (value is UnityEngine.Object obj) return obj != null;
            return true;
        }
    }
}

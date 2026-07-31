using System;

namespace Valkyrie
{
    /// <summary>
    /// Groups a concrete <c>[SerializeReference]</c> type inside Valkyrie's managed-reference picker.
    /// Use slash-separated paths to create submenus, for example <c>"Animation/Punch"</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class ManagedReferenceCategoryAttribute : Attribute
    {
        public string Path { get; }
        public string Label { get; }
        public int Order { get; }

        public ManagedReferenceCategoryAttribute(string path)
            : this(path, null, 0)
        {
        }

        public ManagedReferenceCategoryAttribute(string path, string label)
            : this(path, label, 0)
        {
        }

        public ManagedReferenceCategoryAttribute(string path, string label, int order)
        {
            Path = path ?? string.Empty;
            Label = label ?? string.Empty;
            Order = order;
        }
    }
}

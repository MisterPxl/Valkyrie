using System;

namespace Valkyrie
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class FoldoutGroupAttribute : ValkyrieAttribute
    {
        public string GroupName { get; }

        public FoldoutGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}

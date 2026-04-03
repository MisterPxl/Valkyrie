using System;

namespace Valkyrie
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class ValkyrieAttribute : Attribute
    {
        public int Order { get; set; }
    }
}

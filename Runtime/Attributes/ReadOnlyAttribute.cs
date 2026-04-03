using System;

namespace Valkyrie
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ReadOnlyAttribute : ValkyrieAttribute { }
}

using System;

namespace Astra.Valkyrie
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ReadOnlyAttribute : ValkyrieAttribute { }
}

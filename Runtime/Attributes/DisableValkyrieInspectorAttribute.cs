using System;

namespace Astra.Valkyrie
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DisableValkyrieInspectorAttribute : Attribute
    {
    }
}

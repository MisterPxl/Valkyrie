using System;

namespace Valkyrie
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DisableValkyrieInspectorAttribute : Attribute
    {
    }
}

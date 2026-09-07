using System;

namespace Astra.Valkyrie
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class RequiredAttribute : ValkyrieAttribute
    {
        public string Message { get; }

        public RequiredAttribute() { }
        public RequiredAttribute(string message) { Message = message; }
    }
}

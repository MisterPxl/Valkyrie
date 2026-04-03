using System;

namespace Valkyrie
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ButtonAttribute : ValkyrieAttribute
    {
        public string Label { get; }

        public ButtonAttribute() { }
        public ButtonAttribute(string label) { Label = label; }
    }
}

using System;

namespace Astra.Valkyrie
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ButtonAttribute : ValkyrieAttribute
    {
        public string Label { get; }

        public ButtonAttribute() { }
        public ButtonAttribute(string label) { Label = label; }
    }
}

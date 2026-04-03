using System;

namespace Valkyrie
{
    public enum InfoBoxType
    {
        Info,
        Warning,
        Error
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class InfoBoxAttribute : ValkyrieAttribute
    {
        public string Message { get; }
        public InfoBoxType Type { get; }

        public InfoBoxAttribute(string message, InfoBoxType type = InfoBoxType.Info)
        {
            Message = message;
            Type = type;
        }
    }
}

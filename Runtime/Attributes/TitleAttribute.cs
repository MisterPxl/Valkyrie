using System;

namespace Valkyrie
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class TitleAttribute : ValkyrieAttribute
    {
        public string Text { get; }
        public string Subtitle { get; }

        public TitleAttribute(string text, string subtitle = null)
        {
            Text = text;
            Subtitle = subtitle;
        }
    }
}

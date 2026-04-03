using System.Collections.Generic;

namespace Valkyrie.Editor
{
    public static class EditorStateCache
    {
        private static readonly Dictionary<string, object> States = new();

        public static T Get<T>(string key, T defaultValue = default)
        {
            if (States.TryGetValue(key, out var value) && value is T typed)
                return typed;
            return defaultValue;
        }

        public static void Set<T>(string key, T value)
        {
            States[key] = value;
        }

        public static string MakeKey(int objectId, string identifier)
        {
            return string.Concat(objectId.ToString(), ":", identifier);
        }

        public static void Remove(string key)
        {
            States.Remove(key);
        }

        public static void Clear()
        {
            States.Clear();
        }
    }
}

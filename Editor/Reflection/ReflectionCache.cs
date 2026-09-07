using System;
using System.Collections.Generic;

namespace Astra.Valkyrie.Editor
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Editor", "Valkyrie.Editor")]
    public static class ReflectionCache
    {
        private static readonly Dictionary<Type, TypeData> Cache = new();

        public static TypeData Get(Type type)
        {
            if (!Cache.TryGetValue(type, out var data))
            {
                data = TypeData.Build(type);
                Cache[type] = data;
            }
            return data;
        }

        public static void Clear()
        {
            Cache.Clear();
        }
    }
}

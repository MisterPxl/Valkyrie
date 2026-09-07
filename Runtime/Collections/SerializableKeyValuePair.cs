using System;

namespace Astra.Valkyrie.Collections
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Collections", "Valkyrie.Collections")]
    public struct SerializableKeyValuePair<TKey, TValue>
    {
        public TKey key;
        public TValue value;

        public SerializableKeyValuePair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}

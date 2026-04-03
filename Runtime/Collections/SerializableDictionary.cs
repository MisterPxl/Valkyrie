using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Collections
{
    /// <summary>
    /// Non-generic base class used as a target for PropertyDrawer registration.
    /// </summary>
    [Serializable]
    public abstract class SerializableDictionaryBase { }

    /// <summary>
    /// A generic dictionary that Unity can serialize and display in the inspector.
    /// Backed by a serialized list of key/value pairs, with a runtime dictionary cache.
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : SerializableDictionaryBase,
        ISerializationCallbackReceiver,
        IDictionary<TKey, TValue>
    {
        [SerializeField]
        private List<SerializableKeyValuePair<TKey, TValue>> _entries = new();

        [NonSerialized] private Dictionary<TKey, TValue> _dict;
        [NonSerialized] private bool _dirty;

        private Dictionary<TKey, TValue> Dict => _dict ??= BuildDictionary();

        // ── Constructors ─────────────────────────────────

        public SerializableDictionary() { }

        public SerializableDictionary(IDictionary<TKey, TValue> source)
        {
            foreach (var kvp in source)
                _entries.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
        }

        // ── Serialization ────────────────────────────────

        public void OnBeforeSerialize()
        {
            if (!_dirty || _dict == null)
                return;

            _entries.Clear();
            foreach (var kvp in _dict)
                _entries.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));

            _dirty = false;
        }

        public void OnAfterDeserialize()
        {
            _dict = BuildDictionary();
            _dirty = false;
        }

        private Dictionary<TKey, TValue> BuildDictionary()
        {
            var dict = new Dictionary<TKey, TValue>(_entries.Count);

            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                if (e.key is null)
                    continue;
                dict.TryAdd(e.key, e.value);
            }

            return dict;
        }

        // ── Dictionary API ───────────────────────────────

        public TValue this[TKey key]
        {
            get => Dict[key];
            set { Dict[key] = value; _dirty = true; }
        }

        public int Count => Dict.Count;
        public bool IsReadOnly => false;
        public ICollection<TKey> Keys => Dict.Keys;
        public ICollection<TValue> Values => Dict.Values;

        public void Add(TKey key, TValue value)
        {
            Dict.Add(key, value);
            _dirty = true;
        }

        public bool Remove(TKey key)
        {
            if (!Dict.Remove(key))
                return false;
            _dirty = true;
            return true;
        }

        public bool ContainsKey(TKey key) => Dict.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => Dict.TryGetValue(key, out value);

        public void Clear()
        {
            Dict.Clear();
            _dirty = true;
        }

        // ── ICollection<KeyValuePair> (explicit) ─────────

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
            => Add(item.Key, item.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Dict).Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Dict).CopyTo(array, index);

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!((ICollection<KeyValuePair<TKey, TValue>>)Dict).Remove(item))
                return false;
            _dirty = true;
            return true;
        }

        // ── IEnumerable ──────────────────────────────────

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator() => Dict.GetEnumerator();

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => Dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Dict.GetEnumerator();
    }
}

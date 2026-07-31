using System;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    public sealed class TweenTargetBinding
    {
        public const string SelfKey = "Self";

        [SerializeField] private string _key;
        [SerializeField] private UnityEngine.Object _target;

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public UnityEngine.Object Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public TweenTargetBinding()
        {
            _key = string.Empty;
            _target = null;
        }

        public TweenTargetBinding(string key, UnityEngine.Object target)
        {
            _key = key;
            _target = target;
        }

        public static bool IsSelfKey(string key)
        {
            return string.IsNullOrWhiteSpace(key) ||
                   string.Equals(key.Trim(), SelfKey, StringComparison.OrdinalIgnoreCase);
        }
    }
}

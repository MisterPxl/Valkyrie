using System;
using UnityEngine;

namespace Valkyrie.DOTween
{
    public enum TweenTargetMode
    {
        Self,
        Object,
        Key
    }

    [Serializable]
    public sealed class TweenTargetReference
    {
        [SerializeField] private TweenTargetMode _mode = TweenTargetMode.Self;
        [SerializeField] private UnityEngine.Object _target;
        [SerializeField] private string _key = TweenTargetBinding.SelfKey;

        public TweenTargetMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public UnityEngine.Object Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public string DisplayName
        {
            get
            {
                if (_mode == TweenTargetMode.Object)
                {
                    return _target != null ? _target.name : "Object";
                }

                if (_mode == TweenTargetMode.Key)
                {
                    return string.IsNullOrWhiteSpace(_key) ? TweenTargetBinding.SelfKey : _key.Trim();
                }

                return TweenTargetBinding.SelfKey;
            }
        }

        public static TweenTargetReference Self()
        {
            return new TweenTargetReference { _mode = TweenTargetMode.Self };
        }
    }
}

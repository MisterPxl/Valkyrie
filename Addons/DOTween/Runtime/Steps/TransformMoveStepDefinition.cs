using System;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    public sealed class TransformMoveStepDefinition : TimedTweenStepDefinition, ITweenTargetStepDefinition
    {
        [SerializeField] private string _targetKey = TweenTargetBinding.SelfKey;
        [SerializeField] private Vector3 _endValue;
        [SerializeField] private bool _local;
        [SerializeField] private bool _snapping;

        public string TargetKey
        {
            get { return _targetKey; }
            set { _targetKey = value; }
        }

        public Vector3 EndValue
        {
            get { return _endValue; }
            set { _endValue = value; }
        }

        public bool Local
        {
            get { return _local; }
            set { _local = value; }
        }

        public bool Snapping
        {
            get { return _snapping; }
            set { _snapping = value; }
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            if (!ValidateTiming(context) || !ValidateVector3(_endValue, "Move end value", context))
            {
                return false;
            }

            Transform target;
            if (!context.TryResolve(_targetKey, out target))
            {
                return false;
            }

            Tweener tween = _local
                ? target.DOLocalMove(_endValue, Duration, _snapping)
                : target.DOMove(_endValue, Duration, _snapping);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }
    }
}

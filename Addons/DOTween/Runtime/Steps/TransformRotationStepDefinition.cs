using System;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    public sealed class TransformRotationStepDefinition : TimedTweenStepDefinition, ITweenTargetStepDefinition
    {
        [SerializeField] private string _targetKey = TweenTargetBinding.SelfKey;
        [SerializeField] private Vector3 _endValue;
        [SerializeField] private RotateMode _rotateMode = RotateMode.Fast;
        [SerializeField] private bool _local;

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

        public RotateMode RotateMode
        {
            get { return _rotateMode; }
            set { _rotateMode = value; }
        }

        public bool Local
        {
            get { return _local; }
            set { _local = value; }
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            if (!ValidateTiming(context) || !ValidateVector3(_endValue, "Rotation end value", context))
            {
                return false;
            }

            Transform target;
            if (!context.TryResolve(_targetKey, out target))
            {
                return false;
            }

            Tweener tween = _local
                ? target.DOLocalRotate(_endValue, Duration, _rotateMode)
                : target.DORotate(_endValue, Duration, _rotateMode);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }
    }
}

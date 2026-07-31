using System;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    public sealed class TransformScaleStepDefinition : TimedTweenStepDefinition, ITweenTargetStepDefinition
    {
        [SerializeField] private string _targetKey = TweenTargetBinding.SelfKey;
        [SerializeField] private Vector3 _endValue = Vector3.one;

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

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            if (!ValidateTiming(context) || !ValidateVector3(_endValue, "Scale end value", context))
            {
                return false;
            }

            Transform target;
            if (!context.TryResolve(_targetKey, out target))
            {
                return false;
            }

            Tweener tween = target.DOScale(_endValue, Duration);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }
    }
}

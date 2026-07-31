using System;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    [ManagedReferenceCategory("Transform", "Scale", 20)]
    public sealed class TransformScaleStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenTargetStepDefinition, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Vector3 _endValue = Vector3.one;

        public TweenTargetReference Target
        {
            get
            {
                if (_target == null)
                {
                    _target = TweenTargetReference.Self();
                }

                return _target;
            }
        }

        public Type RequiredTargetType
        {
            get { return typeof(Transform); }
        }

        public string TargetKey
        {
            get { return Target.Key; }
            set
            {
                Target.Mode = TweenTargetMode.Key;
                Target.Key = value;
            }
        }

        public Vector3 EndValue
        {
            get { return _endValue; }
            set { _endValue = value; }
        }

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            bool timingValid = base.ValidateDefinition(context);
            bool valueValid = ValidateVector3(_endValue, "Scale end value", context);
            return timingValid && valueValid;
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            if (!ValidateDefinition(context))
            {
                return false;
            }

            Transform target;
            if (!context.TryResolve(Target, out target))
            {
                return false;
            }

            Vector3 currentValue = target.localScale;
            Vector3 endValue = ResolveVector3EndValue(currentValue, _endValue);
            ApplyVector3StartValue(value => target.localScale = value, _endValue);

            Tweener tween = DG.Tweening.DOTween.To(
                () => target.localScale,
                value => target.localScale = value,
                endValue,
                Duration);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            Transform target;
            if (!context.TryResolve(Target, out target))
            {
                return false;
            }

            _endValue = target.localScale;
            return true;
        }
    }
}

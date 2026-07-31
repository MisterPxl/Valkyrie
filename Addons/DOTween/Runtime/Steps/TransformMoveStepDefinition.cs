using System;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    [ManagedReferenceCategory("Transform", "Move", 0)]
    public sealed class TransformMoveStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenTargetStepDefinition, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Vector3 _endValue;
        [SerializeField] private bool _local;
        [SerializeField] private bool _snapping;

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

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            bool timingValid = base.ValidateDefinition(context);
            bool valueValid = ValidateVector3(_endValue, "Move end value", context);
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

            Vector3 currentValue = _local ? target.localPosition : target.position;
            Vector3 endValue = ResolveVector3EndValue(currentValue, _endValue);
            ApplyVector3StartValue(
                value =>
                {
                    if (_local)
                    {
                        target.localPosition = value;
                    }
                    else
                    {
                        target.position = value;
                    }
                },
                _endValue);

            Tweener tween = DG.Tweening.DOTween.To(
                () => _local ? target.localPosition : target.position,
                value =>
                {
                    if (_local)
                    {
                        target.localPosition = value;
                    }
                    else
                    {
                        target.position = value;
                    }
                },
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

            _endValue = _local ? target.localPosition : target.position;
            return true;
        }
    }
}

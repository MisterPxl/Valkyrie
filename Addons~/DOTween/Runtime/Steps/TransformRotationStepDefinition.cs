using System;
using DG.Tweening;
using UnityEngine;

namespace Astra.Valkyrie.Integrations.DOTween
{
    [Serializable]
    [ManagedReferenceCategory("Transform", "Rotate", 10)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public sealed class TransformRotationStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenTargetStepDefinition, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Vector3 _endValue;
        [SerializeField] private RotateMode _rotateMode = RotateMode.Fast;
        [SerializeField] private bool _local;

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

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            bool timingValid = base.ValidateDefinition(context);
            bool valueValid = ValidateVector3(_endValue, "Rotation end value", context);
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

            Vector3 endValue = _endValue;

            Tweener tween = DG.Tweening.DOTween.To(
                () => _local ? target.localEulerAngles : target.eulerAngles,
                value =>
                {
                    if (_local)
                    {
                        target.localEulerAngles = value;
                    }
                    else
                    {
                        target.eulerAngles = value;
                    }
                },
                endValue,
                Duration);
            ConfigureValueTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            Transform target;
            if (!context.TryResolve(Target, out target))
            {
                return false;
            }

            _endValue = _local ? target.localEulerAngles : target.eulerAngles;
            return true;
        }
    }
}

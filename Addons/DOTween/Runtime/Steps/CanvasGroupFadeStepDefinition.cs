using System;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    [ManagedReferenceCategory("UI", "Canvas Group Fade", 100)]
    public sealed class CanvasGroupFadeStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenTargetStepDefinition, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [Range(0f, 1f)]
        [SerializeField] private float _endAlpha = 1f;

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
            get { return typeof(CanvasGroup); }
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

        public float EndAlpha
        {
            get { return _endAlpha; }
            set { _endAlpha = value; }
        }

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            bool valid = base.ValidateDefinition(context);
            if (float.IsNaN(_endAlpha) || float.IsInfinity(_endAlpha))
            {
                context.ReportError(TweenDiagnosticCode.InvalidValue, "CanvasGroup alpha must be finite.");
                valid = false;
            }

            if (ValueMode != TweenValueMode.By && (_endAlpha < 0f || _endAlpha > 1f))
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    "Absolute CanvasGroup alpha must be between zero and one.");
                valid = false;
            }

            return valid;
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            if (!ValidateDefinition(context))
            {
                return false;
            }

            CanvasGroup target;
            if (!context.TryResolve(Target, out target))
            {
                return false;
            }

            float currentValue = target.alpha;
            float endValue = ResolveFloatEndValue(currentValue, _endAlpha);
            ApplyFloatStartValue(value => target.alpha = Mathf.Clamp01(value), _endAlpha);
            Tweener tween = DG.Tweening.DOTween.To(
                () => target.alpha,
                value => target.alpha = Mathf.Clamp01(value),
                endValue,
                Duration);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            CanvasGroup target;
            if (!context.TryResolve(Target, out target))
            {
                return false;
            }

            _endAlpha = target.alpha;
            return true;
        }
    }
}

using System;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    public sealed class CanvasGroupFadeStepDefinition : TimedTweenStepDefinition, ITweenTargetStepDefinition
    {
        [SerializeField] private string _targetKey = TweenTargetBinding.SelfKey;
        [Range(0f, 1f)]
        [SerializeField] private float _endAlpha = 1f;

        public string TargetKey
        {
            get { return _targetKey; }
            set { _targetKey = value; }
        }

        public float EndAlpha
        {
            get { return _endAlpha; }
            set { _endAlpha = value; }
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            if (!ValidateTiming(context))
            {
                return false;
            }

            CanvasGroup target;
            if (!context.TryResolve(_targetKey, out target))
            {
                return false;
            }

            if (float.IsNaN(_endAlpha) || float.IsInfinity(_endAlpha))
            {
                context.ReportError(TweenDiagnosticCode.InvalidValue, "CanvasGroup alpha must be finite.");
                return false;
            }

            if (!Relative && (_endAlpha < 0f || _endAlpha > 1f))
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    "Absolute CanvasGroup alpha must be between zero and one.");
                return false;
            }

            Tweener tween = DG.Tweening.DOTween.To(
                () => target.alpha,
                value => target.alpha = value,
                _endAlpha,
                Duration);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }
    }
}

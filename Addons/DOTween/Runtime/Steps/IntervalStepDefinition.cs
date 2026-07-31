using System;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    public sealed class IntervalStepDefinition : TweenStepDefinition, ITweenTimelineStepDefinition
    {
        [Min(0f)]
        [SerializeField] private float _duration = 1f;
        [SerializeField] private TweenPlacement _placement = new TweenPlacement();

        public float Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public TweenPlacement Placement
        {
            get { return _placement; }
        }

        public float EstimatedDuration
        {
            get { return Mathf.Max(0f, _duration); }
        }

        public bool RequiresPositiveDuration
        {
            get { return true; }
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            if (float.IsNaN(_duration) || float.IsInfinity(_duration) || _duration <= 0f)
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    "Interval duration must be finite and greater than zero.");
                return false;
            }

            if (_placement == null)
            {
                context.ReportError(TweenDiagnosticCode.InvalidValue, "Timeline placement is missing.");
                return false;
            }

            Sequence interval = DG.Tweening.DOTween.Sequence();
            interval.AppendInterval(_duration);
            return _placement.TryAdd(sequence, interval, context);
        }
    }
}

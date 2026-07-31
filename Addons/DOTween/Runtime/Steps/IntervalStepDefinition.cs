using System;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    [ManagedReferenceCategory("Timeline", "Interval", 900)]
    public sealed class IntervalStepDefinition : TweenStep, ITweenTimelineStepDefinition
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

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            return base.ValidateDefinition(context);
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            if (!ValidateDefinition(context))
            {
                return false;
            }

            Sequence interval = DG.Tweening.DOTween.Sequence();
            interval.AppendInterval(_duration);
            return _placement.TryAdd(sequence, interval, context);
        }
    }
}

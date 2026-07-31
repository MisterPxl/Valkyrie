using System;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    public interface ITweenTargetStepDefinition
    {
        string TargetKey { get; }
    }

    public interface ITweenTimelineStepDefinition
    {
        float EstimatedDuration { get; }
        bool RequiresPositiveDuration { get; }
        TweenPlacement Placement { get; }
    }

    public enum TweenPlacementMode
    {
        Append,
        Join,
        Insert
    }

    [Serializable]
    public sealed class TweenPlacement
    {
        [SerializeField] private TweenPlacementMode _mode = TweenPlacementMode.Append;
        [Min(0f)]
        [SerializeField] private float _insertAt;

        public TweenPlacementMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public float InsertAt
        {
            get { return _insertAt; }
            set { _insertAt = value; }
        }

        public bool TryAdd(Sequence sequence, Tween tween, TweenBuildContext context)
        {
            if (sequence == null || tween == null)
            {
                if (tween != null)
                {
                    tween.Kill();
                }

                context.ReportError(TweenDiagnosticCode.BuildFailure, "The step did not produce a valid tween.");
                return false;
            }

            switch (_mode)
            {
                case TweenPlacementMode.Append:
                    sequence.Append(tween);
                    return true;
                case TweenPlacementMode.Join:
                    sequence.Join(tween);
                    return true;
                case TweenPlacementMode.Insert:
                    if (float.IsNaN(_insertAt) || float.IsInfinity(_insertAt) || _insertAt < 0f)
                    {
                        tween.Kill();
                        context.ReportError(
                            TweenDiagnosticCode.InvalidValue,
                            "Insert time must be greater than or equal to zero.");
                        return false;
                    }

                    sequence.Insert(_insertAt, tween);
                    return true;
                default:
                    tween.Kill();
                    context.ReportError(TweenDiagnosticCode.InvalidValue, "The timeline placement mode is invalid.");
                    return false;
            }
        }
    }

    [Serializable]
    public abstract class TweenStepDefinition
    {
        [SerializeField] private bool _enabled = true;

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public abstract bool TryAddTo(Sequence sequence, TweenBuildContext context);
    }

    [Serializable]
    public abstract class TimedTweenStepDefinition : TweenStepDefinition, ITweenTimelineStepDefinition
    {
        [Min(0f)]
        [SerializeField] private float _duration = 1f;
        [Min(0f)]
        [SerializeField] private float _delay;
        [SerializeField] private Ease _ease = Ease.OutQuad;
        [Min(1)]
        [SerializeField] private int _loops = 1;
        [SerializeField] private LoopType _loopType = LoopType.Restart;
        [SerializeField] private bool _relative;
        [SerializeField] private TweenPlacement _placement = new TweenPlacement();

        public float Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public float Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public Ease Ease
        {
            get { return _ease; }
            set { _ease = value; }
        }

        public int Loops
        {
            get { return _loops; }
            set { _loops = value; }
        }

        public LoopType LoopType
        {
            get { return _loopType; }
            set { _loopType = value; }
        }

        public bool Relative
        {
            get { return _relative; }
            set { _relative = value; }
        }

        public TweenPlacement Placement
        {
            get { return _placement; }
        }

        public float EstimatedDuration
        {
            get
            {
                if (_duration < 0f || _delay < 0f || _loops < 1)
                {
                    return 0f;
                }

                return _delay + (_duration * _loops);
            }
        }

        public bool RequiresPositiveDuration
        {
            get { return false; }
        }

        protected bool ValidateTiming(TweenBuildContext context)
        {
            bool valid = true;

            if (float.IsNaN(_duration) || float.IsInfinity(_duration) || _duration < 0f)
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    "Duration must be finite and greater than or equal to zero.");
                valid = false;
            }

            if (float.IsNaN(_delay) || float.IsInfinity(_delay) || _delay < 0f)
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    "Delay must be finite and greater than or equal to zero.");
                valid = false;
            }

            if (_loops < 1)
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    "Step loops must be at least one because infinite loops are not supported inside a sequence.");
                valid = false;
            }

            if (_placement == null)
            {
                context.ReportError(TweenDiagnosticCode.InvalidValue, "Timeline placement is missing.");
                valid = false;
            }

            return valid;
        }

        protected void ConfigureTween(Tween tween)
        {
            tween.SetDelay(_delay);
            tween.SetEase(_ease);
            tween.SetLoops(_loops, _loopType);
            tween.SetRelative(_relative);
        }

        protected bool ValidateVector3(Vector3 value, string valueName, TweenBuildContext context)
        {
            if (float.IsNaN(value.x) || float.IsInfinity(value.x) ||
                float.IsNaN(value.y) || float.IsInfinity(value.y) ||
                float.IsNaN(value.z) || float.IsInfinity(value.z))
            {
                context.ReportError(TweenDiagnosticCode.InvalidValue, valueName + " must contain only finite values.");
                return false;
            }

            return true;
        }

        protected bool TryPlaceTween(Sequence sequence, Tween tween, TweenBuildContext context)
        {
            return _placement.TryAdd(sequence, tween, context);
        }
    }
}

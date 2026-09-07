using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Astra.Valkyrie.Integrations.DOTween
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public interface ITweenTargetStepDefinition
    {
        string TargetKey { get; }
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public interface ITweenTargetStep
    {
        TweenTargetReference Target { get; }
        Type RequiredTargetType { get; }
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public interface ITweenCapturableStep
    {
        bool CaptureCurrentValue(TweenBuildContext context);
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public interface ITweenTimelineStepDefinition
    {
        float EstimatedDuration { get; }
        bool RequiresPositiveDuration { get; }
        TweenPlacement Placement { get; }
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public enum TweenPlacementMode
    {
        Append,
        Join,
        Insert
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public enum TweenValueMode
    {
        To,
        From,
        By
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
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

        public bool Validate(TweenBuildContext context)
        {
            if (_mode == TweenPlacementMode.Insert &&
                (float.IsNaN(_insertAt) || float.IsInfinity(_insertAt) || _insertAt < 0f))
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    "Insert time must be finite and greater than or equal to zero.");
                return false;
            }

            if (_mode != TweenPlacementMode.Append &&
                _mode != TweenPlacementMode.Join &&
                _mode != TweenPlacementMode.Insert)
            {
                context.ReportError(TweenDiagnosticCode.InvalidValue, "The timeline placement mode is invalid.");
                return false;
            }

            return true;
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

            context.RegisterTween(tween);
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
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public abstract class TweenStepDefinition
    {
        [SerializeField] private bool _enabled = true;
        [SerializeField] private string _id;
        [SerializeField] private string _name;

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public string Id
        {
            get
            {
                EnsureId();
                return _id;
            }
        }

        public string Name
        {
            get { return string.IsNullOrWhiteSpace(_name) ? GetType().Name : _name; }
            set { _name = value; }
        }

        public virtual bool ValidateDefinition(TweenBuildContext context)
        {
            ITweenTimelineStepDefinition timelineStep = this as ITweenTimelineStepDefinition;
            if (timelineStep == null)
            {
                return true;
            }

            bool valid = true;
            float estimatedDuration = timelineStep.EstimatedDuration;
            if (float.IsNaN(estimatedDuration) ||
                float.IsInfinity(estimatedDuration) ||
                estimatedDuration < 0f ||
                (timelineStep.RequiresPositiveDuration && estimatedDuration <= 0f))
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    timelineStep.RequiresPositiveDuration
                        ? "Estimated duration must be finite and greater than zero."
                        : "Estimated duration must be finite and greater than or equal to zero.");
                valid = false;
            }

            TweenPlacement placement = timelineStep.Placement;
            if (placement == null)
            {
                context.ReportError(TweenDiagnosticCode.InvalidValue, "Timeline placement is missing.");
                valid = false;
            }
            else if (!placement.Validate(context))
            {
                valid = false;
            }

            return valid;
        }

        public abstract bool TryAddTo(Sequence sequence, TweenBuildContext context);

        public virtual void CollectSnapshotTargets(TweenBuildContext context, IList<UnityEngine.Object> targets)
        {
            ITweenTargetStep targetStep = this as ITweenTargetStep;
            if (targetStep == null || targetStep.Target == null || targets == null)
            {
                return;
            }

            UnityEngine.Object target;
            if (TryResolveSnapshotTarget(context, targetStep, out target) && target != null && !targets.Contains(target))
            {
                targets.Add(target);
            }
        }

        public void EnsureId()
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                _id = Guid.NewGuid().ToString("N");
            }
        }

        public void RegenerateId()
        {
            _id = Guid.NewGuid().ToString("N");
        }

        public virtual bool ValidateTarget(TweenBuildContext context)
        {
            return !(this is ITweenTargetStep targetStep)
                || context.TryResolve(targetStep.Target, targetStep.RequiredTargetType, out _);
        }

        /// <summary>Override to capture additional properties in optional or custom steps.</summary>
        public virtual void CaptureSnapshot(TweenBuildContext context, TweenStateSnapshot snapshot)
        {
            var targets = new List<UnityEngine.Object>();
            CollectSnapshotTargets(context, targets);
            foreach (UnityEngine.Object target in targets)
                snapshot.CaptureTarget(target);
        }

        private static bool TryResolveSnapshotTarget(
            TweenBuildContext context, ITweenTargetStep targetStep, out UnityEngine.Object target)
        {
            return context.TryResolve(targetStep.Target, targetStep.RequiredTargetType, out target);
        }
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public abstract class TweenStep : TweenStepDefinition
    {
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public abstract class TimedTweenStepDefinition : TweenStep, ITweenTimelineStepDefinition
    {
        [Min(0f)]
        [SerializeField] private float _duration = 1f;
        [Min(0f)]
        [SerializeField] private float _delay;
        [SerializeField] private Ease _ease = Ease.OutQuad;
        [Min(1)]
        [SerializeField] private int _loops = 1;
        [SerializeField] private LoopType _loopType = LoopType.Restart;
        [SerializeField] private TweenValueMode _valueMode = TweenValueMode.To;
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

        public TweenValueMode ValueMode
        {
            get { return _valueMode; }
            set { _valueMode = value; }
        }

        public bool Relative
        {
            get { return _valueMode == TweenValueMode.By; }
            set { _valueMode = value ? TweenValueMode.By : TweenValueMode.To; }
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

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            return ValidateTiming(context);
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

            if (_valueMode != TweenValueMode.To &&
                _valueMode != TweenValueMode.From &&
                _valueMode != TweenValueMode.By)
            {
                context.ReportError(TweenDiagnosticCode.InvalidValue, "The tween value mode is invalid.");
                valid = false;
            }

            if (_placement == null)
            {
                context.ReportError(TweenDiagnosticCode.InvalidValue, "Timeline placement is missing.");
                valid = false;
            }
            else if (!_placement.Validate(context))
            {
                valid = false;
            }

            return valid;
        }

        protected void ConfigureTween(Tween tween)
        {
            tween.SetDelay(_delay);
            tween.SetEase(_ease);
            tween.SetLoops(_loops, _loopType);
        }

        /// <summary>Defers From/By evaluation until this tween starts in its sequence.</summary>
        protected void ConfigureValueTween(Tweener tween)
        {
            if (_valueMode == TweenValueMode.From)
                tween.From(setImmediately: false, isRelative: false);
            else
                tween.SetRelative(_valueMode == TweenValueMode.By);
            ConfigureTween(tween);
        }

        [Obsolete("Pass the configured value to DOTween.To and call ConfigureValueTween to defer From/By evaluation.")]
        protected Vector3 ResolveVector3EndValue(Vector3 currentValue, Vector3 configuredValue)
        {
            if (_valueMode == TweenValueMode.By)
            {
                return currentValue + configuredValue;
            }

            return _valueMode == TweenValueMode.From ? currentValue : configuredValue;
        }

        [Obsolete("Pass the configured value to DOTween.To and call ConfigureValueTween to defer From/By evaluation.")]
        protected float ResolveFloatEndValue(float currentValue, float configuredValue)
        {
            if (_valueMode == TweenValueMode.By)
            {
                return currentValue + configuredValue;
            }

            return _valueMode == TweenValueMode.From ? currentValue : configuredValue;
        }

        [Obsolete("Pass the configured value to DOTween.To and call ConfigureValueTween to defer From/By evaluation.")]
        protected Color ResolveColorEndValue(Color currentValue, Color configuredValue)
        {
            if (_valueMode == TweenValueMode.By)
            {
                return currentValue + configuredValue;
            }

            return _valueMode == TweenValueMode.From ? currentValue : configuredValue;
        }

        [Obsolete("Pass the configured value to DOTween.To and call ConfigureValueTween to defer From/By evaluation.")]
        protected void ApplyVector3StartValue(Action<Vector3> setter, Vector3 configuredValue)
        {
            if (_valueMode == TweenValueMode.From && setter != null)
            {
                setter(configuredValue);
            }
        }

        [Obsolete("Pass the configured value to DOTween.To and call ConfigureValueTween to defer From/By evaluation.")]
        protected void ApplyFloatStartValue(Action<float> setter, float configuredValue)
        {
            if (_valueMode == TweenValueMode.From && setter != null)
            {
                setter(configuredValue);
            }
        }

        [Obsolete("Pass the configured value to DOTween.To and call ConfigureValueTween to defer From/By evaluation.")]
        protected void ApplyColorStartValue(Action<Color> setter, Color configuredValue)
        {
            if (_valueMode == TweenValueMode.From && setter != null)
            {
                setter(configuredValue);
            }
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

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public abstract class TimedTweenStep : TimedTweenStepDefinition
    {
    }
}

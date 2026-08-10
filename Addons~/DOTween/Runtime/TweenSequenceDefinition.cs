using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    public sealed class TweenSequenceDefinition
    {
        [SerializeField] private TweenTimeline _timeline = new TweenTimeline();

        public TweenSequenceDefinition()
        {
        }

        internal TweenSequenceDefinition(TweenTimeline timeline)
        {
            _timeline = timeline ?? new TweenTimeline();
        }

        public TweenTimeline Timeline
        {
            get
            {
                if (_timeline == null)
                {
                    _timeline = new TweenTimeline();
                }

                return _timeline;
            }
        }

        public System.Collections.Generic.IList<TweenStepDefinition> Steps
        {
            get { return Timeline.Steps; }
        }

        public Ease Ease
        {
            get { return Timeline.Ease; }
            set { Timeline.Ease = value; }
        }

        public int Loops
        {
            get { return Timeline.Loops; }
            set { Timeline.Loops = value; }
        }

        public LoopType LoopType
        {
            get { return Timeline.LoopType; }
            set { Timeline.LoopType = value; }
        }

        public UpdateType UpdateType
        {
            get { return Timeline.UpdateType; }
            set { Timeline.UpdateType = value; }
        }

        public bool IndependentUpdate
        {
            get { return Timeline.IndependentUpdate; }
            set { Timeline.IndependentUpdate = value; }
        }

        public float TimeScale
        {
            get { return Timeline.TimeScale; }
            set { Timeline.TimeScale = value; }
        }

        public bool AutoKill
        {
            get { return Timeline.AutoKill; }
            set { Timeline.AutoKill = value; }
        }

        public bool Recyclable
        {
            get { return Timeline.Recyclable; }
            set { Timeline.Recyclable = value; }
        }

        public bool ValidateDefinitions(TweenBuildContext context)
        {
            return Timeline.ValidateDefinitions(context);
        }

        public bool TryBuildSequence(TweenBuildContext context, out Sequence sequence)
        {
            return Timeline.TryBuildSequence(context, out sequence);
        }

        internal TweenSequenceBuildSettings CreateSettings()
        {
            return Timeline.CreateSettings();
        }
    }

    internal readonly struct TweenSequenceBuildSettings
    {
        public Ease Ease { get; }
        public int Loops { get; }
        public LoopType LoopType { get; }
        public UpdateType UpdateType { get; }
        public bool IndependentUpdate { get; }
        public float TimeScale { get; }
        public bool AutoKill { get; }
        public bool Recyclable { get; }

        public TweenSequenceBuildSettings(
            Ease ease,
            int loops,
            LoopType loopType,
            UpdateType updateType,
            bool independentUpdate,
            float timeScale,
            bool autoKill,
            bool recyclable)
        {
            Ease = ease;
            Loops = loops;
            LoopType = loopType;
            UpdateType = updateType;
            IndependentUpdate = independentUpdate;
            TimeScale = timeScale;
            AutoKill = autoKill;
            Recyclable = recyclable;
        }
    }

    internal static class TweenSequenceDefinitionBuilder
    {
        public static bool ValidateDefinitions(
            IList<TweenStepDefinition> steps,
            TweenSequenceBuildSettings settings,
            TweenBuildContext context)
        {
            if (context == null)
            {
                return false;
            }

            bool valid = ValidateSettings(settings, context);
            if (steps == null || steps.Count == 0)
            {
                context.ReportError(TweenDiagnosticCode.EmptySequence, "The sequence has no steps.");
                return false;
            }

            int enabledStepCount = 0;
            for (int index = 0; index < steps.Count; index++)
            {
                TweenStepDefinition step = steps[index];
                context.SetCurrentStep(index, step);
                if (step == null)
                {
                    context.ReportError(TweenDiagnosticCode.NullStep, "The step reference is null.");
                    valid = false;
                    continue;
                }

                if (!step.Enabled)
                {
                    continue;
                }

                enabledStepCount++;
                try
                {
                    int diagnosticCount = context.Diagnostics.Count;
                    bool stepValid = step.ValidateDefinition(context);
                    valid &= stepValid;
                    if (!stepValid && context.Diagnostics.Count == diagnosticCount)
                    {
                        context.ReportError(
                            TweenDiagnosticCode.InvalidValue,
                            "The step definition is invalid but did not provide a diagnostic.");
                    }
                }
                catch (Exception exception)
                {
                    context.ReportError(
                        TweenDiagnosticCode.InvalidValue,
                        "The step definition could not be validated: " + exception.Message);
                    valid = false;
                }
            }

            context.SetCurrentStep(-1, null);
            if (enabledStepCount == 0)
            {
                context.ReportError(TweenDiagnosticCode.EmptySequence, "The sequence has no enabled steps.");
                valid = false;
            }

            return valid && !context.HasErrors;
        }

        public static bool TryBuildSequence(
            IList<TweenStepDefinition> steps,
            TweenSequenceBuildSettings settings,
            TweenBuildContext context,
            out Sequence sequence)
        {
            sequence = null;
            if (context == null)
            {
                return false;
            }

            if (!ValidateDefinitions(steps, settings, context))
            {
                return false;
            }

            try
            {
                sequence = DG.Tweening.DOTween.Sequence();
                sequence.SetEase(settings.Ease);
                sequence.SetLoops(settings.Loops, settings.LoopType);
                sequence.SetUpdate(settings.UpdateType, settings.IndependentUpdate);
                sequence.SetAutoKill(settings.AutoKill);
                sequence.SetRecyclable(settings.Recyclable);
                sequence.timeScale = settings.TimeScale;
                sequence.Pause();
            }
            catch (Exception exception)
            {
                context.ReportError(
                    TweenDiagnosticCode.BuildFailure,
                    "DOTween could not create the sequence: " + exception.Message);
                if (sequence != null)
                {
                    sequence.Kill();
                    sequence = null;
                }

                return false;
            }

            int addedStepCount = 0;
            for (int index = 0; index < steps.Count; index++)
            {
                TweenStepDefinition step = steps[index];
                context.SetCurrentStep(index, step);

                if (step == null)
                {
                    context.ReportError(TweenDiagnosticCode.NullStep, "The step reference is null.");
                    continue;
                }

                if (!step.Enabled)
                {
                    continue;
                }

                try
                {
                    int diagnosticCount = context.Diagnostics.Count;
                    bool added = step.TryAddTo(sequence, context);
                    if (added)
                    {
                        addedStepCount++;
                    }

                    if (!added && context.Diagnostics.Count == diagnosticCount)
                    {
                        context.ReportError(
                            TweenDiagnosticCode.BuildFailure,
                            "The step did not add a tween or provide a diagnostic.");
                    }
                }
                catch (Exception exception)
                {
                    context.ReportError(
                        TweenDiagnosticCode.BuildFailure,
                        "The step could not be built: " + exception.Message);
                }
            }

            context.SetCurrentStep(-1, null);
            if (addedStepCount == 0)
            {
                context.ReportError(
                    TweenDiagnosticCode.EmptySequence,
                    "The sequence has no enabled steps that produce tweens.");
            }

            if (context.HasErrors)
            {
                sequence.Kill();
                sequence = null;
                return false;
            }

            return true;
        }

        private static bool ValidateSettings(TweenSequenceBuildSettings settings, TweenBuildContext context)
        {
            bool valid = true;

            if (settings.Loops == 0 || settings.Loops < -1)
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    "Sequence loops must be -1 for infinite playback or at least one.");
                valid = false;
            }

            if (float.IsNaN(settings.TimeScale) || float.IsInfinity(settings.TimeScale) || settings.TimeScale <= 0f)
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    "Sequence time scale must be finite and greater than zero.");
                valid = false;
            }

            return valid;
        }
    }
}

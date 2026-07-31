using System;
using System.Collections.Generic;
using DG.Tweening;

namespace Valkyrie.DOTween.Editor
{
    public static class TweenSequenceEditorValidation
    {
        public static IReadOnlyList<TweenBuildDiagnostic> Validate(TweenSequencePlayer player)
        {
            List<TweenBuildDiagnostic> diagnostics = new List<TweenBuildDiagnostic>();
            if (player == null)
            {
                diagnostics.Add(new TweenBuildDiagnostic(
                    TweenDiagnosticSeverity.Error,
                    TweenDiagnosticCode.MissingAsset,
                    "The TweenSequencePlayer is missing.",
                    -1,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
                return diagnostics;
            }

            TweenBuildContext context = new TweenBuildContext(player.TargetRoot, player.Bindings);
            TweenSequenceAsset asset = player.Asset;
            if (asset == null)
            {
                diagnostics.Add(new TweenBuildDiagnostic(
                    TweenDiagnosticSeverity.Error,
                    TweenDiagnosticCode.MissingAsset,
                    "No TweenSequenceAsset is assigned.",
                    -1,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
                CopyDiagnostics(context.Diagnostics, diagnostics);
                return diagnostics;
            }

            AddNonFiniteTimingDiagnostics(asset, diagnostics);

            Sequence validationSequence = null;
            try
            {
                asset.TryBuildSequence(context, out validationSequence);
            }
            catch (Exception exception)
            {
                diagnostics.Add(new TweenBuildDiagnostic(
                    TweenDiagnosticSeverity.Error,
                    TweenDiagnosticCode.BuildFailure,
                    "Editor validation could not build the sequence: " + exception.Message,
                    -1,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
            }
            finally
            {
                if (validationSequence != null && validationSequence.IsActive())
                {
                    validationSequence.Kill(false);
                }

                CopyDiagnostics(context.Diagnostics, diagnostics);
            }

            if (player.DestroyCleanup == TweenCleanupMode.None)
            {
                diagnostics.Add(new TweenBuildDiagnostic(
                    TweenDiagnosticSeverity.Warning,
                    TweenDiagnosticCode.InvalidValue,
                    "Destroy cleanup None is treated as Kill to prevent orphaned tweens.",
                    -1,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
            }

            return diagnostics;
        }

        public static IReadOnlyList<TweenBuildDiagnostic> ValidateAsset(TweenSequenceAsset asset)
        {
            List<TweenBuildDiagnostic> diagnostics = new List<TweenBuildDiagnostic>();
            if (asset == null)
            {
                diagnostics.Add(CreateDiagnostic(
                    TweenDiagnosticCode.MissingAsset,
                    "The TweenSequenceAsset is missing.",
                    -1,
                    null));
                return diagnostics;
            }

            if (asset.Loops == 0 || asset.Loops < -1)
            {
                diagnostics.Add(CreateDiagnostic(
                    TweenDiagnosticCode.InvalidValue,
                    "Sequence loops must be -1 or at least one.",
                    -1,
                    null));
            }

            if (!IsFinite(asset.TimeScale) || asset.TimeScale <= 0f)
            {
                diagnostics.Add(CreateDiagnostic(
                    TweenDiagnosticCode.InvalidValue,
                    "Sequence time scale must be finite and greater than zero.",
                    -1,
                    null));
            }

            IList<TweenStepDefinition> steps = asset.Steps;
            int enabledStepCount = 0;
            for (int index = 0; index < steps.Count; index++)
            {
                TweenStepDefinition step = steps[index];
                if (step == null)
                {
                    diagnostics.Add(CreateDiagnostic(
                        TweenDiagnosticCode.NullStep,
                        "The step reference is null.",
                        index,
                        null));
                    continue;
                }

                if (!step.Enabled)
                {
                    continue;
                }

                enabledStepCount++;
                TimedTweenStepDefinition timedStep = step as TimedTweenStepDefinition;
                if (timedStep != null)
                {
                    ValidateTimedStep(timedStep, index, diagnostics);
                    continue;
                }

                ITweenTimelineStepDefinition timelineStep = step as ITweenTimelineStepDefinition;
                if (timelineStep != null &&
                    (!IsFinite(timelineStep.EstimatedDuration) ||
                     timelineStep.EstimatedDuration < 0f ||
                     (timelineStep.RequiresPositiveDuration && timelineStep.EstimatedDuration <= 0f)))
                {
                    diagnostics.Add(CreateDiagnostic(
                        TweenDiagnosticCode.InvalidValue,
                        timelineStep.RequiresPositiveDuration
                            ? "Estimated duration must be finite and greater than zero."
                            : "Estimated duration must be finite and non-negative.",
                        index,
                        step));
                }

                if (timelineStep != null)
                {
                    ValidatePlacement(timelineStep.Placement, index, step, diagnostics);
                }
            }

            if (enabledStepCount == 0)
            {
                diagnostics.Add(new TweenBuildDiagnostic(
                    TweenDiagnosticSeverity.Error,
                    TweenDiagnosticCode.EmptySequence,
                    "The sequence has no enabled steps.",
                    -1,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
            }

            return diagnostics;
        }

        private static void ValidateTimedStep(
            TimedTweenStepDefinition step,
            int stepIndex,
            List<TweenBuildDiagnostic> diagnostics)
        {
            if (!IsFinite(step.Duration) || step.Duration < 0f)
            {
                diagnostics.Add(CreateDiagnostic(
                    TweenDiagnosticCode.InvalidValue,
                    "Duration must be finite and non-negative.",
                    stepIndex,
                    step));
            }

            if (!IsFinite(step.Delay) || step.Delay < 0f)
            {
                diagnostics.Add(CreateDiagnostic(
                    TweenDiagnosticCode.InvalidValue,
                    "Delay must be finite and non-negative.",
                    stepIndex,
                    step));
            }

            if (step.Loops < 1)
            {
                diagnostics.Add(CreateDiagnostic(
                    TweenDiagnosticCode.InvalidValue,
                    "Step loops must be at least one.",
                    stepIndex,
                    step));
            }

            ValidatePlacement(step.Placement, stepIndex, step, diagnostics);
        }

        private static void ValidatePlacement(
            TweenPlacement placement,
            int stepIndex,
            TweenStepDefinition step,
            List<TweenBuildDiagnostic> diagnostics)
        {
            if (placement == null)
            {
                diagnostics.Add(CreateDiagnostic(
                    TweenDiagnosticCode.InvalidValue,
                    "Timeline placement is missing.",
                    stepIndex,
                    step));
                return;
            }

            if (placement.Mode == TweenPlacementMode.Insert &&
                (!IsFinite(placement.InsertAt) || placement.InsertAt < 0f))
            {
                diagnostics.Add(CreateDiagnostic(
                    TweenDiagnosticCode.InvalidValue,
                    "Insert time must be finite and non-negative.",
                    stepIndex,
                    step));
            }
        }

        private static TweenBuildDiagnostic CreateDiagnostic(
            TweenDiagnosticCode code,
            string message,
            int stepIndex,
            TweenStepDefinition step)
        {
            Type stepType = step != null ? step.GetType() : null;
            return new TweenBuildDiagnostic(
                TweenDiagnosticSeverity.Error,
                code,
                message,
                stepIndex,
                stepType != null ? stepType.FullName ?? stepType.Name : string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
        }

        private static void AddNonFiniteTimingDiagnostics(
            TweenSequenceAsset asset,
            List<TweenBuildDiagnostic> diagnostics)
        {
            IList<TweenStepDefinition> steps = asset.Steps;
            for (int index = 0; index < steps.Count; index++)
            {
                TweenStepDefinition step = steps[index];
                if (step == null || !step.Enabled)
                {
                    continue;
                }

                TimedTweenStepDefinition timedStep = step as TimedTweenStepDefinition;
                if (timedStep != null)
                {
                    if (!IsFinite(timedStep.Duration))
                    {
                        AddInvalidTimingDiagnostic(
                            diagnostics,
                            index,
                            step,
                            "Duration must be a finite number.");
                    }

                    if (!IsFinite(timedStep.Delay))
                    {
                        AddInvalidTimingDiagnostic(
                            diagnostics,
                            index,
                            step,
                            "Delay must be a finite number.");
                    }

                    continue;
                }

                IReadOnlyList<TweenStepEditorSummary> summary =
                    TweenSequenceEditorAnalysis.AnalyzeSteps(
                        new List<TweenStepDefinition> { step });
                if (summary[0].HasEstimatedDuration &&
                    !IsFinite(summary[0].EstimatedDuration))
                {
                    AddInvalidTimingDiagnostic(
                        diagnostics,
                        index,
                        step,
                        "Estimated duration must be a finite number.");
                }
            }
        }

        private static void AddInvalidTimingDiagnostic(
            List<TweenBuildDiagnostic> diagnostics,
            int stepIndex,
            TweenStepDefinition step,
            string message)
        {
            Type stepType = step.GetType();
            diagnostics.Add(new TweenBuildDiagnostic(
                TweenDiagnosticSeverity.Error,
                TweenDiagnosticCode.InvalidValue,
                message,
                stepIndex,
                stepType.FullName ?? stepType.Name,
                string.Empty,
                string.Empty,
                string.Empty));
        }

        private static void CopyDiagnostics(
            IReadOnlyList<TweenBuildDiagnostic> source,
            List<TweenBuildDiagnostic> destination)
        {
            for (int index = 0; index < source.Count; index++)
            {
                destination.Add(source[index]);
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}

using System;
using System.Collections.Generic;
using DG.Tweening;

namespace Valkyrie.DOTween.Editor
{
    public static class TweenSequenceEditorValidation
    {
        public static IReadOnlyList<TweenBuildDiagnostic> Validate(TweenPlayer player)
        {
            List<TweenBuildDiagnostic> diagnostics = new List<TweenBuildDiagnostic>();
            if (player == null)
            {
                diagnostics.Add(new TweenBuildDiagnostic(
                    TweenDiagnosticSeverity.Error,
                    TweenDiagnosticCode.MissingAsset,
                    "The TweenPlayer is missing.",
                    -1,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
                return diagnostics;
            }

            TweenBuildContext context = new TweenBuildContext(player.TargetRoot, player.Bindings);
            if (player.SourceMode == TweenPlayerSourceMode.Asset && player.Asset == null)
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

            Sequence validationSequence = null;
            try
            {
                player.TryBuildConfiguredSequence(context, out validationSequence);
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

            TweenBuildContext context = new TweenBuildContext(null, null);
            asset.ValidateDefinitions(context);
            CopyDiagnostics(context.Diagnostics, diagnostics);
            return diagnostics;
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

        private static void CopyDiagnostics(
            IReadOnlyList<TweenBuildDiagnostic> source,
            List<TweenBuildDiagnostic> destination)
        {
            for (int index = 0; index < source.Count; index++)
            {
                destination.Add(source[index]);
            }
        }

    }
}

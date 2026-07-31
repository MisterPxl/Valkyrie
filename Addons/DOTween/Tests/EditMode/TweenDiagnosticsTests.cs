using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using Valkyrie.DOTween.Editor;

namespace Valkyrie.DOTween.Tests.EditMode
{
    public sealed class TweenDiagnosticsTests
    {
        [SetUp]
        public void SetUp()
        {
            DG.Tweening.DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        }

        [TearDown]
        public void TearDown()
        {
            DG.Tweening.DOTween.KillAll();
        }

        [Test]
        public void DuplicateBinding_ReportsErrorAndKeepsFirstTarget()
        {
            GameObject first = new GameObject("First");
            GameObject second = new GameObject("Second");
            try
            {
                List<TweenTargetBinding> bindings = new List<TweenTargetBinding>
                {
                    new TweenTargetBinding("Target", first),
                    new TweenTargetBinding("Target", second)
                };

                TweenBuildContext context = new TweenBuildContext(first.transform, bindings);
                Transform resolved;

                Assert.That(context.TryResolve("Target", out resolved), Is.True);
                Assert.That(resolved, Is.SameAs(first.transform));
                AssertDiagnostic(
                    context.Diagnostics,
                    TweenDiagnosticCode.DuplicateBinding,
                    TweenDiagnosticSeverity.Error,
                    -1);
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void MissingBinding_ReportsRequestedKeyAndExpectedType()
        {
            GameObject owner = new GameObject("Owner");
            try
            {
                TweenBuildContext context = new TweenBuildContext(owner.transform, null);
                CanvasGroup resolved;

                Assert.That(context.TryResolve("Panel", out resolved), Is.False);
                TweenBuildDiagnostic diagnostic = FindDiagnostic(
                    context.Diagnostics,
                    TweenDiagnosticCode.MissingBinding);
                Assert.That(diagnostic.Severity, Is.EqualTo(TweenDiagnosticSeverity.Error));
                Assert.That(diagnostic.BindingKey, Is.EqualTo("Panel"));
                Assert.That(diagnostic.ExpectedType, Is.EqualTo(typeof(CanvasGroup).FullName));
                Assert.That(diagnostic.ActualType, Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void WrongTypeBinding_ReportsExpectedAndActualTypes()
        {
            GameObject owner = new GameObject("Owner");
            Texture2D texture = new Texture2D(1, 1);
            try
            {
                List<TweenTargetBinding> bindings = new List<TweenTargetBinding>
                {
                    new TweenTargetBinding("Mover", texture)
                };
                TweenBuildContext context = new TweenBuildContext(owner.transform, bindings);
                Transform resolved;

                Assert.That(context.TryResolve("Mover", out resolved), Is.False);
                TweenBuildDiagnostic diagnostic = FindDiagnostic(
                    context.Diagnostics,
                    TweenDiagnosticCode.WrongBindingType);
                Assert.That(diagnostic.BindingKey, Is.EqualTo("Mover"));
                Assert.That(diagnostic.ExpectedType, Is.EqualTo(typeof(Transform).FullName));
                Assert.That(diagnostic.ActualType, Is.EqualTo(typeof(Texture2D).FullName));
            }
            finally
            {
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void PlayerWithoutAsset_ReportsMissingAsset()
        {
            GameObject owner = new GameObject("Owner");
            try
            {
                TweenSequencePlayer player = owner.AddComponent<TweenSequencePlayer>();
                Sequence sequence;

                Assert.That(player.TryBuildSequence(out sequence), Is.False);
                Assert.That(sequence, Is.Null);
                AssertDiagnostic(
                    player.Diagnostics,
                    TweenDiagnosticCode.MissingAsset,
                    TweenDiagnosticSeverity.Error,
                    -1);
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void NullStep_ReportsItsSequenceIndex()
        {
            GameObject owner = new GameObject("Owner");
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            try
            {
                asset.Steps.Add(null);
                TweenBuildContext context = new TweenBuildContext(owner.transform, null);
                Sequence sequence;

                Assert.That(asset.TryBuildSequence(context, out sequence), Is.False);
                Assert.That(sequence, Is.Null);
                AssertDiagnostic(
                    context.Diagnostics,
                    TweenDiagnosticCode.NullStep,
                    TweenDiagnosticSeverity.Error,
                    0);
            }
            finally
            {
                Object.DestroyImmediate(asset);
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void NegativeTimedStepDuration_ReportsInvalidValue()
        {
            GameObject owner = new GameObject("Owner");
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            try
            {
                TransformMoveStepDefinition move = new TransformMoveStepDefinition
                {
                    Duration = -0.01f,
                    EndValue = Vector3.one
                };
                asset.Steps.Add(move);
                TweenBuildContext context = new TweenBuildContext(owner.transform, null);
                Sequence sequence;

                Assert.That(asset.TryBuildSequence(context, out sequence), Is.False);
                Assert.That(sequence, Is.Null);
                AssertDiagnostic(
                    context.Diagnostics,
                    TweenDiagnosticCode.InvalidValue,
                    TweenDiagnosticSeverity.Error,
                    0);
            }
            finally
            {
                Object.DestroyImmediate(asset);
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void NonPositiveIntervalDuration_ReportsInvalidValue()
        {
            GameObject owner = new GameObject("Owner");
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            try
            {
                asset.Steps.Add(new IntervalStepDefinition { Duration = 0f });
                TweenBuildContext context = new TweenBuildContext(owner.transform, null);
                Sequence sequence;

                Assert.That(asset.TryBuildSequence(context, out sequence), Is.False);
                Assert.That(sequence, Is.Null);
                AssertDiagnostic(
                    context.Diagnostics,
                    TweenDiagnosticCode.InvalidValue,
                    TweenDiagnosticSeverity.Error,
                    0);
            }
            finally
            {
                Object.DestroyImmediate(asset);
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void AssetInspectorValidation_ReportsInvalidStepBeforePlayback()
        {
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            try
            {
                asset.Steps.Add(new IntervalStepDefinition { Duration = 0f });

                IReadOnlyList<TweenBuildDiagnostic> diagnostics =
                    TweenSequenceEditorValidation.ValidateAsset(asset);

                AssertDiagnostic(
                    diagnostics,
                    TweenDiagnosticCode.InvalidValue,
                    TweenDiagnosticSeverity.Error,
                    0);
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        private static void AssertDiagnostic(
            IReadOnlyList<TweenBuildDiagnostic> diagnostics,
            TweenDiagnosticCode code,
            TweenDiagnosticSeverity severity,
            int stepIndex)
        {
            TweenBuildDiagnostic diagnostic = FindDiagnostic(diagnostics, code);
            Assert.That(diagnostic.Severity, Is.EqualTo(severity));
            Assert.That(diagnostic.StepIndex, Is.EqualTo(stepIndex));
        }

        private static TweenBuildDiagnostic FindDiagnostic(
            IReadOnlyList<TweenBuildDiagnostic> diagnostics,
            TweenDiagnosticCode code)
        {
            for (int index = 0; index < diagnostics.Count; index++)
            {
                if (diagnostics[index].Code == code)
                {
                    return diagnostics[index];
                }
            }

            Assert.Fail("Expected diagnostic " + code + ".");
            return null;
        }
    }
}

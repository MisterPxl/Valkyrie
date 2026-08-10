using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Valkyrie.DOTween.Editor;

namespace Valkyrie.DOTween.Tests.EditMode
{
    public sealed class TweenDiagnosticsTests
    {
        [Test]
        public void PlayerInAssetModeWithoutAsset_ReportsMissingAsset()
        {
            GameObject owner = new GameObject("TweenPlayer");
            try
            {
                TweenPlayer player = owner.AddComponent<TweenPlayer>();
                player.SourceMode = TweenPlayerSourceMode.Asset;

                Assert.That(
                    TweenSequenceEditorValidation.Validate(player).Any(diagnostic => diagnostic.Code == TweenDiagnosticCode.MissingAsset),
                    Is.True);
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void ObjectTargetWithWrongComponent_ReportsWrongBindingType()
        {
            GameObject owner = new GameObject("TweenPlayer");
            DummyTarget target = ScriptableObject.CreateInstance<DummyTarget>();
            try
            {
                TweenPlayer player = owner.AddComponent<TweenPlayer>();
                TransformMoveStepDefinition step = new TransformMoveStepDefinition();
                step.Target.Mode = TweenTargetMode.Object;
                step.Target.Target = target;
                player.Timeline.Steps.Add(step);

                Assert.That(
                    TweenSequenceEditorValidation.Validate(player).Any(diagnostic => diagnostic.Code == TweenDiagnosticCode.WrongBindingType),
                    Is.True);
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void TimelineClone_PreservesManagedReferenceStepTypes()
        {
            TweenTimeline timeline = new TweenTimeline();
            timeline.Steps.Add(new TransformScaleStepDefinition());
            timeline.Steps.Add(new CanvasGroupFadeStepDefinition());

            TweenTimeline clone = TweenTimelineCloneUtility.Clone(timeline);

            Assert.That(clone, Is.Not.SameAs(timeline));
            Assert.That(clone.Steps[0], Is.TypeOf<TransformScaleStepDefinition>());
            Assert.That(clone.Steps[1], Is.TypeOf<CanvasGroupFadeStepDefinition>());
        }

        private sealed class DummyTarget : ScriptableObject
        {
        }
    }
}

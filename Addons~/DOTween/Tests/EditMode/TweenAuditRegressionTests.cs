using NUnit.Framework;
using UnityEngine;
using DG.Tweening;
using Astra.Valkyrie.Integrations.DOTween.Editor;
using Object = UnityEngine.Object;

namespace Astra.Valkyrie.Integrations.DOTween.Tests.EditMode
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween.Tests.EditMode", "Valkyrie.DOTween.Tests.EditMode")]
    public class TweenAuditRegressionTests
    {
        [Test]
        public void FromWaitsForItsStepAndRestartsFromTheSameValue()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.SourceMode = TweenPlayerSourceMode.Sequence;
            player.Timeline.Steps.Add(new IntervalStepDefinition { Duration = 1f });
            player.Timeline.Steps.Add(new TransformMoveStepDefinition
            {
                EndValue = Vector3.right * 10, ValueMode = TweenValueMode.From, Ease = Ease.Linear
            });
            Assert.That(player.TryBuildSequence(out Sequence sequence), Is.True);
            Assert.That(owner.transform.position, Is.EqualTo(Vector3.zero));
            sequence.Goto(0.5f);
            Assert.That(owner.transform.position, Is.EqualTo(Vector3.zero));
            sequence.Goto(1.5f);
            Assert.That(owner.transform.position.x, Is.EqualTo(5f).Within(0.001f));
            sequence.Goto(2f);
            Assert.That(owner.transform.position, Is.EqualTo(Vector3.zero));
            sequence.Restart();
            sequence.Goto(1.5f);
            Assert.That(owner.transform.position.x, Is.EqualTo(5f).Within(0.001f));
        }

        [TestCase(LoopType.Restart, 0.25f)]
        [TestCase(LoopType.Yoyo, 0.15f)]
        public void RelativeScalarStepsRespectSequenceLoops(LoopType loopType, float expected)
        {
            var group = owner.AddComponent<CanvasGroup>();
            group.alpha = 0.1f;
            var player = owner.AddComponent<TweenPlayer>();
            player.SourceMode = TweenPlayerSourceMode.Sequence;
            player.Timeline.Loops = 2;
            player.Timeline.LoopType = loopType;
            for (int index = 0; index < 2; index++)
                player.Timeline.Steps.Add(new CanvasGroupFadeStepDefinition
                {
                    EndAlpha = 0.1f, ValueMode = TweenValueMode.By, Ease = Ease.Linear
                });
            Assert.That(player.TryBuildSequence(out Sequence sequence), Is.True);
            sequence.Goto(3.5f);
            Assert.That(group.alpha, Is.EqualTo(expected).Within(0.001f));
        }

        [Test]
        public void StepEventsFollowTheirOwnTimelineAndIgnoreDisabledSteps()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.SourceMode = TweenPlayerSourceMode.Sequence;
            var first = new TransformMoveStepDefinition { EndValue = Vector3.right };
            var second = new IntervalStepDefinition { Duration = 1f };
            var disabled = new IntervalStepDefinition { Enabled = false };
            player.Timeline.Steps.Add(first);
            player.Timeline.Steps.Add(second);
            player.Timeline.Steps.Add(disabled);
            int firstStarts = 0, firstCompletes = 0, secondStarts = 0, secondCompletes = 0, disabledCreates = 0;
            var firstBinding = new TweenStepEventBinding { StepId = first.Id };
            firstBinding.Events.OnStart.AddListener(() => firstStarts++);
            firstBinding.Events.OnComplete.AddListener(() => firstCompletes++);
            var secondBinding = new TweenStepEventBinding { StepId = second.Id };
            secondBinding.Events.OnStart.AddListener(() => secondStarts++);
            secondBinding.Events.OnComplete.AddListener(() => secondCompletes++);
            var disabledBinding = new TweenStepEventBinding { StepId = disabled.Id };
            disabledBinding.Events.OnCreated.AddListener(() => disabledCreates++);
            player.StepEvents.Add(firstBinding);
            player.StepEvents.Add(secondBinding);
            player.StepEvents.Add(disabledBinding);
            Assert.That(player.TryBuildSequence(out Sequence sequence), Is.True);
            sequence.Play();
            sequence.ManualUpdate(0.5f, 0.5f);
            Assert.That(firstStarts, Is.EqualTo(1));
            Assert.That(firstCompletes, Is.Zero);
            Assert.That(secondStarts, Is.Zero);
            sequence.ManualUpdate(1f, 1f);
            Assert.That(firstCompletes, Is.EqualTo(1));
            Assert.That(secondStarts, Is.EqualTo(1));
            Assert.That(secondCompletes, Is.Zero);
            sequence.ManualUpdate(0.5f, 0.5f);
            Assert.That(secondCompletes, Is.EqualTo(1));
            Assert.That(disabledCreates, Is.Zero);
        }

        [Test]
        public void CallbackStepHasItsOwnBindingAtItsInsertionTime()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.SourceMode = TweenPlayerSourceMode.Sequence;
            player.Timeline.Steps.Add(new IntervalStepDefinition { Duration = 2f });
            var step = new CallbackStepDefinition();
            step.Placement.Mode = TweenPlacementMode.Insert;
            step.Placement.InsertAt = 1f;
            player.Timeline.Steps.Add(step);
            int calls = 0, completes = 0;
            step.Callback.AddListener(() => calls++);
            var binding = new TweenStepEventBinding { StepId = step.Id };
            binding.Events.OnComplete.AddListener(() => completes++);
            player.StepEvents.Add(binding);
            Assert.That(player.TryBuildSequence(out Sequence sequence), Is.True);
            sequence.Play();
            sequence.ManualUpdate(0.5f, 0.5f);
            Assert.That(calls, Is.Zero);
            sequence.ManualUpdate(1f, 1f);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(completes, Is.EqualTo(1));
        }

        [Test]
        public void SingleModeIgnoresInvalidLaterStepsInValidationAndPreview()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { EndValue = Vector3.right });
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { TargetKey = "missing" });
            Assert.That(TweenSequenceEditorValidation.Validate(player), Is.Empty);
            TweenEditModePreview.Scrub(player, 0.5f);
            Assert.That(TweenEditModePreview.Duration, Is.EqualTo(1f));
            Assert.That(owner.transform.position.x, Is.GreaterThan(0f));
            TweenEditModePreview.Stop();
            Assert.That(owner.transform.position, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void RepeatedStopDoesNotReapplyAnOldSnapshot()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { EndValue = Vector3.right });
            TweenEditModePreview.Scrub(player, 0.5f);
            TweenEditModePreview.Stop();
            owner.transform.position = Vector3.one * 7;
            TweenEditModePreview.Stop();
            Assert.That(owner.transform.position, Is.EqualTo(Vector3.one * 7));
        }

        [Test]
        public void PlayerOnStepRunsOncePerSequenceLoop()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.Timeline.Loops = 2;
            player.Timeline.Steps.Add(new IntervalStepDefinition { Duration = 1f });
            int calls = 0;
            player.Events.OnStep.AddListener(() => calls++);
            Assert.That(player.TryBuildSequence(out Sequence sequence), Is.True);
            sequence.Play();
            sequence.ManualUpdate(1.1f, 1.1f);
            Assert.That(calls, Is.EqualTo(1));
            sequence.ManualUpdate(0.9f, 0.9f);
            Assert.That(calls, Is.EqualTo(2));
        }
        private GameObject owner;
        [SetUp] public void Setup()
        {
            DG.Tweening.DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
            owner = new GameObject("Audit");
        }
        [TearDown] public void Cleanup()
        {
            TweenEditModePreview.Stop();
            DG.Tweening.DOTween.KillAll(false);
            Object.DestroyImmediate(owner);
        }
        [Test] public void RewindShouldInvokeEventOnce()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { EndValue = Vector3.one });
            Assert.That(player.TryBuildSequence(out Sequence sequence), Is.True);
            sequence.Goto(0.5f, false);
            int calls = 0;
            player.Events.OnRewind.AddListener(() => calls++);
            player.Rewind();
            Assert.That(calls, Is.EqualTo(1));
        }
        [Test] public void UnknownStepBindingShouldNotFire()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { EndValue = Vector3.one });
            var binding = new TweenStepEventBinding { StepId = "nonexistent" };
            int calls = 0;
            binding.Events.OnComplete.AddListener(() => calls++);
            player.StepEvents.Add(binding);
            Assert.That(player.TryBuildSequence(out Sequence sequence), Is.True);
            sequence.Goto(sequence.Duration(false), true);
            Assert.That(calls, Is.Zero);
        }
        [Test] public void SingleModeShouldOnlyBuildFirstStep()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.SourceMode = TweenPlayerSourceMode.Single;
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { EndValue = Vector3.one, Duration = 1f });
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { EndValue = Vector3.one * 2, Duration = 1f });
            Assert.That(player.TryBuildSequence(out Sequence sequence), Is.True);
            Assert.That(sequence.Duration(false), Is.EqualTo(1f));
        }
        [Test] public void InvalidPreviewScrubShouldNotThrow()
        {
            var player = owner.AddComponent<TweenPlayer>();
            Assert.DoesNotThrow(() => TweenEditModePreview.Scrub(player, 0f));
        }
        [Test] public void SequentialRelativeMovesShouldAccumulate()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.SourceMode = TweenPlayerSourceMode.Sequence;
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { EndValue = Vector3.right, ValueMode = TweenValueMode.By });
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { EndValue = Vector3.right, ValueMode = TweenValueMode.By });
            Assert.That(player.TryBuildSequence(out Sequence sequence), Is.True);
            sequence.Goto(sequence.Duration(false), true);
            Assert.That(owner.transform.position.x, Is.EqualTo(2f).Within(0.001f));
        }
        [Test] public void InspectorValidationShouldNotChangeTarget()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { EndValue = Vector3.one * 10, ValueMode = TweenValueMode.From });
            TweenSequenceEditorValidation.Validate(player);
            Assert.That(owner.transform.position, Is.EqualTo(Vector3.zero));
        }
        [Test] public void FailedBuildShouldNotChangeTarget()
        {
            var player = owner.AddComponent<TweenPlayer>();
            player.SourceMode = TweenPlayerSourceMode.Sequence;
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { EndValue = Vector3.one * 10, ValueMode = TweenValueMode.From });
            player.Timeline.Steps.Add(new TransformMoveStepDefinition { TargetKey = "missing" });
            Assert.That(player.TryBuildSequence(out Sequence sequence), Is.False);
            Assert.That(owner.transform.position, Is.EqualTo(Vector3.zero));
        }
    }
}

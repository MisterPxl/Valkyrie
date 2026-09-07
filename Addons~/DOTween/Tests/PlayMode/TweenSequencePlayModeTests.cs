using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace Astra.Valkyrie.Integrations.DOTween.Tests.PlayMode
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween.Tests.PlayMode", "Valkyrie.DOTween.Tests.PlayMode")]
    public sealed class TweenSequencePlayModeTests
    {
        [UnityTest]
        [Timeout(10000)]
        public IEnumerator PlayOnEnableRunsRelativeStepsAndTheirEvents()
        {
            var owner = new GameObject("Lifecycle");
            owner.SetActive(false);
            try
            {
                var player = owner.AddComponent<TweenPlayer>();
                player.SourceMode = TweenPlayerSourceMode.Sequence;
                player.PlayOnEnable = true;
                int completions = 0;
                for (int index = 0; index < 2; index++)
                {
                    var step = new TransformMoveStepDefinition
                    {
                        EndValue = Vector3.right, ValueMode = TweenValueMode.By, Duration = 0.02f
                    };
                    player.Timeline.Steps.Add(step);
                    var binding = new TweenStepEventBinding { StepId = step.Id };
                    binding.Events.OnComplete.AddListener(() => completions++);
                    player.StepEvents.Add(binding);
                }
                owner.SetActive(true);
                Assert.That(player.CurrentSequence, Is.Not.Null);
                yield return player.CurrentSequence.WaitForCompletion();
                Assert.That(owner.transform.position.x, Is.EqualTo(2f).Within(0.001f));
                Assert.That(completions, Is.EqualTo(2));
                owner.SetActive(false);
                Assert.That(player.CurrentSequence, Is.Null);
            }
            finally { Object.Destroy(owner); }
        }
        [SetUp]
        public void SetUp()
        {
            DG.Tweening.DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
            DG.Tweening.DOTween.KillAll(false);
        }

        [TearDown]
        public void TearDown()
        {
            DG.Tweening.DOTween.KillAll(false);
        }

        [Test]
        public void TransformMoveStep_BuildsAndReachesExpectedValue()
        {
            GameObject owner = new GameObject("TweenPlayer");
            try
            {
                TweenPlayer player = owner.AddComponent<TweenPlayer>();
                player.Timeline.Steps.Add(new TransformMoveStepDefinition
                {
                    EndValue = new Vector3(3f, 4f, 5f)
                });

                Sequence sequence;
                Assert.That(player.TryBuildSequence(out sequence), Is.True);

                sequence.Goto(sequence.Duration(false), true);
                Assert.That(owner.transform.position.x, Is.EqualTo(3f).Within(0.001f));
                Assert.That(owner.transform.position.y, Is.EqualTo(4f).Within(0.001f));
                Assert.That(owner.transform.position.z, Is.EqualTo(5f).Within(0.001f));
            }
            finally
            {
                Object.Destroy(owner);
            }
        }

        [Test]
        public void RestartFromSpawnPoint_RestoresCapturedTransformBeforePlaying()
        {
            GameObject owner = new GameObject("TweenPlayer");
            try
            {
                TweenPlayer player = owner.AddComponent<TweenPlayer>();
                owner.transform.localScale = Vector3.one;
                player.Timeline.Steps.Add(new TransformScaleStepDefinition
                {
                    EndValue = Vector3.one * 2f
                });
                player.CaptureSpawnPoint();

                owner.transform.localScale = Vector3.one * 5f;
                Assert.That(player.RestartFromSpawnPoint(), Is.True);
                Assert.That(owner.transform.localScale, Is.EqualTo(Vector3.one));

                Sequence sequence = player.CurrentSequence;
                sequence.Goto(sequence.Duration(false), true);
                Assert.That(owner.transform.localScale.x, Is.EqualTo(2f).Within(0.001f));
            }
            finally
            {
                Object.Destroy(owner);
            }
        }

    }
}

using DG.Tweening;
using NUnit.Framework;
using UnityEngine;

namespace Valkyrie.DOTween.Tests.PlayMode
{
    public sealed class TweenSequencePlayModeTests
    {
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
                player.CaptureSpawnPoint();
                player.Timeline.Steps.Add(new TransformScaleStepDefinition
                {
                    EndValue = Vector3.one * 2f
                });

                owner.transform.localScale = Vector3.one * 5f;
                Assert.That(player.RestartFromSpawnPoint(), Is.True);

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

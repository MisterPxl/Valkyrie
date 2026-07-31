using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Valkyrie.DOTween.Tests.PlayMode
{
    public sealed class TweenSequencePlayModeTests
    {
        private const float Tolerance = 0.001f;

        [SetUp]
        public void SetUp()
        {
            if (System.Type.GetType("DG.Tweening.DOTweenModuleUtils, DOTween.Modules") == null)
            {
                LogAssert.ignoreFailingMessages = true;
            }

            DG.Tweening.DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        }

        [TearDown]
        public void TearDown()
        {
            DG.Tweening.DOTween.Clear(true);
            LogAssert.ignoreFailingMessages = false;
        }

        [Test]
        public void BuiltInSteps_AllBuildAndReachTheirExpectedValues()
        {
            GameObject owner = new GameObject("Owner");
            GameObject moveObject = new GameObject("Move");
            GameObject rotationObject = new GameObject("Rotation");
            GameObject scaleObject = new GameObject("Scale");
            GameObject fadeObject = new GameObject("Fade");
            CanvasGroup canvasGroup = fadeObject.AddComponent<CanvasGroup>();
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            Sequence sequence = null;

            try
            {
                asset.Steps.Add(new TransformMoveStepDefinition
                {
                    TargetKey = "Move",
                    EndValue = new Vector3(2f, 3f, 4f),
                    Local = true,
                    Duration = 0.1f,
                    Ease = Ease.Linear
                });
                asset.Steps.Add(new TransformRotationStepDefinition
                {
                    TargetKey = "Rotation",
                    EndValue = new Vector3(0f, 90f, 0f),
                    Local = true,
                    Duration = 0.1f,
                    Ease = Ease.Linear
                });
                asset.Steps.Add(new TransformScaleStepDefinition
                {
                    TargetKey = "Scale",
                    EndValue = new Vector3(2f, 3f, 4f),
                    Duration = 0.1f,
                    Ease = Ease.Linear
                });
                asset.Steps.Add(new CanvasGroupFadeStepDefinition
                {
                    TargetKey = "Fade",
                    EndAlpha = 0.25f,
                    Duration = 0.1f,
                    Ease = Ease.Linear
                });
                asset.Steps.Add(new IntervalStepDefinition { Duration = 0.1f });

                List<TweenTargetBinding> bindings = new List<TweenTargetBinding>
                {
                    new TweenTargetBinding("Move", moveObject),
                    new TweenTargetBinding("Rotation", rotationObject.transform),
                    new TweenTargetBinding("Scale", scaleObject),
                    new TweenTargetBinding("Fade", fadeObject)
                };
                TweenBuildContext context = new TweenBuildContext(owner.transform, bindings);

                Assert.That(asset.TryBuildSequence(context, out sequence), Is.True);
                Assert.That(context.Diagnostics, Is.Empty);
                Assert.That(sequence.Duration(), Is.EqualTo(0.5f).Within(Tolerance));

                sequence.Goto(sequence.Duration(), false);

                AssertVector(moveObject.transform.localPosition, new Vector3(2f, 3f, 4f));
                Assert.That(
                    Quaternion.Angle(
                        rotationObject.transform.localRotation,
                        Quaternion.Euler(0f, 90f, 0f)),
                    Is.LessThan(Tolerance));
                AssertVector(scaleObject.transform.localScale, new Vector3(2f, 3f, 4f));
                Assert.That(canvasGroup.alpha, Is.EqualTo(0.25f).Within(Tolerance));
            }
            finally
            {
                Kill(sequence);
                Object.DestroyImmediate(asset);
                Object.DestroyImmediate(fadeObject);
                Object.DestroyImmediate(scaleObject);
                Object.DestroyImmediate(rotationObject);
                Object.DestroyImmediate(moveObject);
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void PlacementModes_ProduceDeterministicTimelineOrdering()
        {
            GameObject owner = new GameObject("Owner");
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            Sequence sequence = null;

            try
            {
                TransformMoveStepDefinition move = new TransformMoveStepDefinition
                {
                    EndValue = new Vector3(10f, 0f, 0f),
                    Local = true,
                    Duration = 1f,
                    Ease = Ease.Linear
                };
                TransformScaleStepDefinition scale = new TransformScaleStepDefinition
                {
                    EndValue = new Vector3(3f, 3f, 3f),
                    Duration = 2f,
                    Ease = Ease.Linear
                };
                scale.Placement.Mode = TweenPlacementMode.Join;
                IntervalStepDefinition interval = new IntervalStepDefinition { Duration = 0.5f };
                TransformRotationStepDefinition rotation = new TransformRotationStepDefinition
                {
                    EndValue = new Vector3(0f, 0f, 45f),
                    Local = true,
                    Duration = 1f,
                    Ease = Ease.Linear
                };
                rotation.Placement.Mode = TweenPlacementMode.Insert;
                rotation.Placement.InsertAt = 0.25f;

                asset.Steps.Add(move);
                asset.Steps.Add(scale);
                asset.Steps.Add(interval);
                asset.Steps.Add(rotation);

                TweenBuildContext context = new TweenBuildContext(owner.transform, null);
                Assert.That(asset.TryBuildSequence(context, out sequence), Is.True);
                Assert.That(sequence.Duration(), Is.EqualTo(2.5f).Within(Tolerance));

                sequence.Goto(0.5f, false);

                Assert.That(owner.transform.localPosition.x, Is.EqualTo(5f).Within(Tolerance));
                Assert.That(owner.transform.localScale.x, Is.EqualTo(1.5f).Within(Tolerance));
                Assert.That(
                    Quaternion.Angle(
                        owner.transform.localRotation,
                        Quaternion.Euler(0f, 0f, 11.25f)),
                    Is.LessThan(Tolerance));

                sequence.Goto(2.5f, false);
                Assert.That(owner.transform.localPosition.x, Is.EqualTo(10f).Within(Tolerance));
                Assert.That(owner.transform.localScale.x, Is.EqualTo(3f).Within(Tolerance));
                Assert.That(
                    Quaternion.Angle(
                        owner.transform.localRotation,
                        Quaternion.Euler(0f, 0f, 45f)),
                    Is.LessThan(Tolerance));
            }
            finally
            {
                Kill(sequence);
                Object.DestroyImmediate(asset);
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void PlayerControls_PauseResumeRewindCompleteAndKillSequence()
        {
            GameObject owner = new GameObject("Owner");
            TweenSequenceAsset asset = CreateMoveAsset(Vector3.right, 1f);
            try
            {
                TweenSequencePlayer player = owner.AddComponent<TweenSequencePlayer>();
                player.Asset = asset;
                Sequence sequence;

                Assert.That(player.TryBuildSequence(out sequence), Is.True);
                Assert.That(sequence.IsPlaying(), Is.False);

                player.Resume();
                Assert.That(player.IsPlaying, Is.True);
                player.Pause();
                Assert.That(player.IsPlaying, Is.False);

                sequence.Goto(0.5f, false);
                Assert.That(owner.transform.localPosition.x, Is.EqualTo(0.5f).Within(Tolerance));
                player.Rewind();
                Assert.That(owner.transform.localPosition.x, Is.EqualTo(0f).Within(Tolerance));

                player.Complete();
                Assert.That(owner.transform.localPosition.x, Is.EqualTo(1f).Within(Tolerance));
                Assert.That(sequence.IsActive(), Is.True);

                player.Kill();
                Assert.That(sequence.IsActive(), Is.False);
                Assert.That(player.CurrentSequence, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(asset);
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void Player_AppliesReadableIdAndDefaultOrOverriddenTarget()
        {
            GameObject owner = new GameObject("Owner");
            GameObject targetOverride = new GameObject("Target Override");
            TweenSequenceAsset asset = CreateMoveAsset(Vector3.right, 1f);
            asset.name = "Shared Sequence";

            try
            {
                TweenSequencePlayer player = owner.AddComponent<TweenSequencePlayer>();
                player.Asset = asset;
                Sequence sequence;

                Assert.That(player.TryBuildSequence(out sequence), Is.True);
                TweenSequenceRuntimeIdentity identity = player.RuntimeIdentity;
                Assert.That(identity, Is.Not.Null);
                Assert.That(sequence.stringId, Is.EqualTo(player.EffectiveTweenId));
                Assert.That(sequence.target, Is.SameAs(identity));
                Assert.That(identity.Player, Is.SameAs(player));
                Assert.That(identity.Asset, Is.SameAs(asset));
                Assert.That(identity.ReadableId, Is.EqualTo(player.EffectiveTweenId));

                player.Kill();
                player.IdOverride = "Menu/Intro";
                player.TargetOverride = targetOverride;

                Assert.That(player.TryBuildSequence(out sequence), Is.True);
                Assert.That(sequence.stringId, Is.EqualTo("Menu/Intro"));
                Assert.That(sequence.target, Is.SameAs(targetOverride));
                Assert.That(player.EffectiveTweenTarget, Is.SameAs(targetOverride));
            }
            finally
            {
                Object.DestroyImmediate(asset);
                Object.DestroyImmediate(targetOverride);
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void DisablingPlayer_KillsCurrentSequence()
        {
            GameObject owner = new GameObject("Owner");
            TweenSequenceAsset asset = CreateMoveAsset(Vector3.right, 1f);
            try
            {
                TweenSequencePlayer player = owner.AddComponent<TweenSequencePlayer>();
                player.Asset = asset;
                player.DisableCleanup = TweenCleanupMode.Kill;
                Sequence sequence;
                Assert.That(player.TryBuildSequence(out sequence), Is.True);

                player.enabled = false;

                Assert.That(sequence.IsActive(), Is.False);
                Assert.That(player.CurrentSequence, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(asset);
                Object.DestroyImmediate(owner);
            }
        }

        [UnityTest]
        public IEnumerator DestroyingPlayer_KillsCurrentSequenceEvenWhenNoneWasRequested()
        {
            LogAssert.ignoreFailingMessages = true;
            GameObject owner = new GameObject("Owner");
            TweenSequenceAsset asset = CreateMoveAsset(Vector3.right, 1f);
            TweenSequencePlayer player = owner.AddComponent<TweenSequencePlayer>();
            player.Asset = asset;
            player.DisableCleanup = TweenCleanupMode.None;
            player.DestroyCleanup = TweenCleanupMode.None;
            Sequence sequence;
            Assert.That(player.TryBuildSequence(out sequence), Is.True);

            Object.Destroy(owner);
            yield return null;

            Assert.That(sequence.IsActive(), Is.False);
            Object.DestroyImmediate(asset);
            LogAssert.ignoreFailingMessages = false;
        }

        private static TweenSequenceAsset CreateMoveAsset(Vector3 endValue, float duration)
        {
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            asset.Steps.Add(new TransformMoveStepDefinition
            {
                EndValue = endValue,
                Local = true,
                Duration = duration,
                Ease = Ease.Linear
            });
            return asset;
        }

        private static void AssertVector(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(Tolerance));
        }

        private static void Kill(Sequence sequence)
        {
            if (sequence != null && sequence.IsActive())
            {
                sequence.Kill();
            }
        }
    }
}

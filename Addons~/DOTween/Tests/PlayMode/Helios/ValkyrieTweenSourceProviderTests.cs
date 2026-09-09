using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Astra.Helios.Integrations.DOTween;

namespace Astra.Valkyrie.Integrations.DOTween.Helios.Tests
{
    public sealed class ValkyrieTweenSourceProviderTests
    {
        private GameObject _owner;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_owner != null) Object.Destroy(_owner);
            yield return null;
            ValkyrieTweenSourceProvider.Register();
        }

        [UnityTest]
        public IEnumerator LiveSequenceAndStepTweensAreDescribedThenReleased()
        {
            ValkyrieTweenSourceProvider.Register();
            Assert.That(ValkyrieTweenSourceProvider.IsRegistered, Is.True);
            _owner = new GameObject("IntroPlayer");
            TweenPlayer player = _owner.AddComponent<TweenPlayer>();
            player.SourceMode = TweenPlayerSourceMode.Sequence;
            var move = new TransformMoveStepDefinition { Duration = 5f, EndValue = Vector3.one, Name = "Slide" };
            player.Timeline.Steps.Add(move);
            player.Timeline.Steps.Add(new TransformScaleStepDefinition { Duration = 5f, EndValue = Vector3.one * 2f, Name = "Grow" });
            int before = TweenSequenceRuntimeRegistry.Count;
            Assert.That(player.Play(), Is.True, string.Join("; ", player.Diagnostics.Select(d => d.Message)));
            yield return null;

            Assert.That(TweenSequenceRuntimeRegistry.Count, Is.EqualTo(before + 3), "sequence plus two step tweens");
            var monitor = new DOTweenMonitor();
            DOTweenMonitorSnapshot snapshot = monitor.Capture();
            var described = snapshot.Tweens.Where(t => t.Source != null).ToList();
            string dump = string.Join(" | ", described.Select(t => t.Id + " / " + t.TweenType + " / " + t.Source.Summary));
            // DOTween lists only top-level tweens: the row is the sequence, described with its step count.
            var sequence = described.SingleOrDefault(t => t.Source.Step == "sequence (2 steps)"); Assert.That(sequence, Is.Not.Null, dump);
            Assert.That(sequence.Source.Owner, Does.Contain("IntroPlayer"));
            Assert.That(sequence.Source.Asset, Is.EqualTo("inline sequence"));
            Assert.That(sequence.Source.OwnerObject, Is.SameAs(player));
            Assert.That(sequence.Id, Is.EqualTo(player.EffectiveTweenId));
            Assert.That(DOTweenTweenFilter.Matches(sequence, "introplayer"), Is.True);
            // Step tweens stay resolvable for any tool holding a tween reference.
            var provider = new ValkyrieTweenSourceProvider();
            var steps = TweenSequenceRuntimeRegistry.Enumerate().Where(e => !e.Value.IsSequence && e.Value.Identity.Player == player).ToList();
            Assert.That(steps, Has.Count.EqualTo(2));
            Assert.That(steps.All(e => provider.TryDescribe(e.Key, out var s) && s.Owner.Contains("IntroPlayer")), Is.True);
            Assert.That(steps.Select(e => { provider.TryDescribe(e.Key, out var s); return s.Step; }), Is.EquivalentTo(new[] { "step 0 'Slide'", "step 1 'Grow'" }));

            player.Kill();
            yield return null;
            Assert.That(TweenSequenceRuntimeRegistry.Count, Is.EqualTo(before), "kill must release the registry entries");
            Assert.That(monitor.Capture().Tweens.Any(t => t.Source != null && t.Source.Owner.Contains("IntroPlayer")), Is.False);
        }

        [UnityTest]
        public IEnumerator UnregisteredProviderLeavesSourcesEmptyAndAssetIdentityIsReported()
        {
            _owner = new GameObject("AssetPlayer");
            TweenPlayer player = _owner.AddComponent<TweenPlayer>();
            var asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            asset.name = "IntroSequence";
            asset.Timeline.Steps.Add(new TransformMoveStepDefinition { Duration = 5f, EndValue = Vector3.one });
            player.SourceMode = TweenPlayerSourceMode.Asset;
            player.Asset = asset;
            try
            {
                Assert.That(player.Play(), Is.True, string.Join("; ", player.Diagnostics.Select(d => d.Message)));
                yield return null;
                ValkyrieTweenSourceProvider.Register();
                var withProvider = new DOTweenMonitor().Capture().Tweens.Where(t => t.Source != null && t.Source.Owner.Contains("AssetPlayer")).ToList();
                Assert.That(withProvider, Is.Not.Empty);
                Assert.That(withProvider.All(t => t.Source.Asset == "asset IntroSequence"), Is.True);

                ValkyrieTweenSourceProvider.Unregister();
                Assert.That(ValkyrieTweenSourceProvider.IsRegistered, Is.False);
                Assert.That(new DOTweenMonitor().Capture().Tweens.Any(t => t.Source != null), Is.False, "no provider: no source, tab behaves as before");
                Assert.That(TweenSequenceRuntimeRegistry.TryGet(null, out _), Is.False);
            }
            finally
            {
                player.Kill();
                Object.DestroyImmediate(asset);
            }
        }
    }
}

using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Astra.Valkyrie.Integrations.DOTween.Editor;
using Astra.Valkyrie.Editor;

namespace Astra.Valkyrie.Integrations.DOTween.Tests.EditMode
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween.Tests.EditMode", "Valkyrie.DOTween.Tests.EditMode")]
    public class TweenStepEventPickerTests
    {
        private GameObject _owner;
        private TweenPlayer _player;
        [SetUp] public void SetUp()
        {
            _owner = new GameObject("Picker");
            _player = _owner.AddComponent<TweenPlayer>();
            _player.SourceMode = TweenPlayerSourceMode.Sequence;
        }
        [TearDown] public void TearDown() => Object.DestroyImmediate(_owner);

        [Test] public void SelectionSurvivesRenamingAndReordering()
        {
            var first = new IntervalStepDefinition { Name = "Wait" };
            var second = new TransformMoveStepDefinition { Name = "Entrance" };
            _player.Timeline.Steps.Add(first);
            _player.Timeline.Steps.Add(second);
            string id = second.Id;
            second.Name = "Slide in";
            _player.Timeline.Steps.RemoveAt(1);
            _player.Timeline.Steps.Insert(0, second);
            var options = TweenStepEventOptions.Build(_player, id);
            Assert.That(options.Ids[options.SelectedIndex], Is.EqualTo(id));
            Assert.That(options.Labels[options.SelectedIndex], Does.Contain("Slide in"));
            Assert.That(options.Warning, Is.Null);
        }

        [Test] public void MissingSelectionIsPreservedAndExplained()
        {
            var options = TweenStepEventOptions.Build(_player, "deleted");
            Assert.That(options.Ids[options.SelectedIndex], Is.EqualTo("deleted"));
            Assert.That(options.Warning, Is.Not.Null);
            Assert.That(options.Ids[0], Is.Empty);
        }

        [Test] public void InactiveStepsRemainSelectableWithAWarning()
        {
            _player.SourceMode = TweenPlayerSourceMode.Single;
            _player.Timeline.Steps.Add(new IntervalStepDefinition());
            var second = new IntervalStepDefinition();
            _player.Timeline.Steps.Add(second);
            Assert.That(TweenStepEventOptions.Build(_player, second.Id).Warning, Is.Not.Null);
            _player.SourceMode = TweenPlayerSourceMode.Sequence;
            second.Enabled = false;
            Assert.That(TweenStepEventOptions.Build(_player, second.Id).Warning, Is.Not.Null);
        }

        [Test] public void AssetModeUsesTheAssetsSteps()
        {
            var asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            try
            {
                var step = new IntervalStepDefinition { Name = "Asset step" };
                asset.Timeline.Steps.Add(step);
                _player.Asset = asset;
                _player.SourceMode = TweenPlayerSourceMode.Asset;
                var options = TweenStepEventOptions.Build(_player, step.Id);
                Assert.That(options.Ids[options.SelectedIndex], Is.EqualTo(step.Id));
                Assert.That(options.Warning, Is.Null);
            }
            finally { Object.DestroyImmediate(asset); }
        }

        [Test] public void GenericDuplicationCreatesANewStepIdentity()
        {
            var step = new IntervalStepDefinition();
            _player.Timeline.Steps.Add(step);
            string original = step.Id;
            using var so = new SerializedObject(_player);
            Assert.That(ManagedReferenceClipboard.Duplicate(so, "_timeline._steps", 0), Is.True);
            Assert.That(_player.Timeline.Steps[0].Id, Is.EqualTo(original));
            Assert.That(_player.Timeline.Steps[1].Id, Is.Not.EqualTo(original));
        }
    }
}

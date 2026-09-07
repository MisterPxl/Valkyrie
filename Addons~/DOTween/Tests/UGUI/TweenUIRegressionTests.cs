using NUnit.Framework;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEditor;
using Astra.Valkyrie.Integrations.DOTween.Editor;
using Object = UnityEngine.Object;

namespace Astra.Valkyrie.Integrations.DOTween.Tests.EditMode
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween.Tests.EditMode", "Valkyrie.DOTween.Tests.UGUI")]
    public class TweenUIRegressionTests
    {
        [Test]
        public void PreviewRestoresGraphicColorAndImageFill()
        {
            var ui = new GameObject("Image", typeof(RectTransform), typeof(Image));
            try
            {
                var image = ui.GetComponent<Image>();
                var player = ui.AddComponent<TweenPlayer>();
                player.SourceMode = TweenPlayerSourceMode.Sequence;
                var color = new GraphicColorStepDefinition();
                var fill = new ImageFillAmountStepDefinition();
                fill.Placement.Mode = TweenPlacementMode.Join;
                var context = new TweenBuildContext(ui.transform, null);
                image.color = Color.white;
                image.fillAmount = 1f;
                color.CaptureCurrentValue(context);
                fill.CaptureCurrentValue(context);
                image.color = Color.red;
                image.fillAmount = 0.2f;
                player.Timeline.Steps.Add(color);
                player.Timeline.Steps.Add(fill);
                TweenEditModePreview.Scrub(player, 0.5f);
                Assert.That(image.color, Is.Not.EqualTo(Color.red));
                Assert.That(image.fillAmount, Is.GreaterThan(0.2f));
                TweenEditModePreview.Stop();
                Assert.That(image.color, Is.EqualTo(Color.red));
                Assert.That(image.fillAmount, Is.EqualTo(0.2f));
            }
            finally { Object.DestroyImmediate(ui); }
        }

        [Test]
        public void TextValidationIsReadOnlyAndPreviewRestoresText()
        {
            var ui = new GameObject("Text", typeof(RectTransform), typeof(Text));
            try
            {
                var text = ui.GetComponent<Text>();
                text.text = "Original";
                var player = ui.AddComponent<TweenPlayer>();
                player.Timeline.Steps.Add(new TextTypewriterStepDefinition());
                Assert.That(TweenSequenceEditorValidation.Validate(player), Is.Empty);
                Assert.That(text.text, Is.EqualTo("Original"));
                Assert.That(player.TryBuildSequence(out Sequence sequence), Is.True);
                Assert.That(text.text, Is.EqualTo("Original"));
                player.Kill();
                TweenEditModePreview.Scrub(player, 0.5f);
                Assert.That(text.text, Is.Not.EqualTo("Original"));
                TweenEditModePreview.Stop();
                Assert.That(text.text, Is.EqualTo("Original"));
            }
            finally { Object.DestroyImmediate(ui); }
        }

        [Test]
        public void RestartFromSpawnPointRestoresGraphicAlphaImmediately()
        {
            var ui = new GameObject("Image", typeof(RectTransform), typeof(Image));
            try
            {
                var image = ui.GetComponent<Image>();
                image.color = new Color(1f, 0f, 0f, 0.2f);
                var player = ui.AddComponent<TweenPlayer>();
                player.Timeline.Steps.Add(new GraphicFadeStepDefinition());
                player.CaptureSpawnPoint();
                Assert.That(player.TryBuildSequence(out Sequence sequence), Is.True);
                sequence.Goto(0.5f);
                Assert.That(image.color.a, Is.GreaterThan(0.2f));
                Assert.That(player.RestartFromSpawnPoint(), Is.True);
                Assert.That(image.color.a, Is.EqualTo(0.2f));
            }
            finally { Object.DestroyImmediate(ui); }
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
        [Test] public void PreviewStopShouldRestoreRectTransformSize()
        {
            var ui = new GameObject("UI Audit", typeof(RectTransform));
            try
            {
                var rect = ui.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(50, 60);
                var player = ui.AddComponent<TweenPlayer>();
                player.Timeline.Steps.Add(new RectTransformSizeStepDefinition());
                TweenEditModePreview.Scrub(player, 0.5f);
                TweenEditModePreview.Stop();
                Assert.That(rect.sizeDelta, Is.EqualTo(new Vector2(50, 60)));
            }
            finally { Object.DestroyImmediate(ui); }
        }
    }
}

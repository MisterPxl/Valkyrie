using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Astra.Valkyrie.Integrations.DOTween
{
    [Serializable]
    [ManagedReferenceCategory("UI", "Graphic Color", 600)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.UGUI.Runtime")]
    public sealed class GraphicColorStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Color _color = Color.white;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Graphic); } }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Graphic target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            Color endValue = _color;
            Tweener tween = DG.Tweening.DOTween.To(() => target.color, value => target.color = value, endValue, Duration);
            ConfigureValueTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public override void CaptureSnapshot(TweenBuildContext context, TweenStateSnapshot snapshot)
        {
            if (!context.TryResolve(Target, out Graphic target)) return;
            Color original = target.color;
            snapshot.AddRestoreAction(() => { if (target != null) target.color = original; });
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            Graphic target;
            if (!context.TryResolve(Target, out target)) return false;
            _color = target.color;
            return true;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("UI", "Graphic Fade", 601)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.UGUI.Runtime")]
    public sealed class GraphicFadeStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [Range(0f, 1f)]
        [SerializeField] private float _alpha = 1f;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Graphic); } }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Graphic target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            float endValue = _alpha;
            Tweener tween = DG.Tweening.DOTween.To(() => target.color.a, value => SetAlpha(target, value), endValue, Duration);
            ConfigureValueTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public override void CaptureSnapshot(TweenBuildContext context, TweenStateSnapshot snapshot)
        {
            if (!context.TryResolve(Target, out Graphic target)) return;
            Color original = target.color;
            snapshot.AddRestoreAction(() => { if (target != null) target.color = original; });
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            Graphic target;
            if (!context.TryResolve(Target, out target)) return false;
            _alpha = target.color.a;
            return true;
        }

        private static void SetAlpha(Graphic target, float alpha)
        {
            Color color = target.color;
            color.a = Mathf.Clamp01(alpha);
            target.color = color;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("UI", "Image Fill Amount", 602)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.UGUI.Runtime")]
    public sealed class ImageFillAmountStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [Range(0f, 1f)]
        [SerializeField] private float _fillAmount = 1f;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Image); } }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Image target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            float endValue = _fillAmount;
            Tweener tween = DG.Tweening.DOTween.To(() => target.fillAmount, value => target.fillAmount = Mathf.Clamp01(value), endValue, Duration);
            ConfigureValueTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public override void CaptureSnapshot(TweenBuildContext context, TweenStateSnapshot snapshot)
        {
            if (!context.TryResolve(Target, out Image target)) return;
            float original = target.fillAmount;
            snapshot.AddRestoreAction(() => { if (target != null) target.fillAmount = original; });
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            Image target;
            if (!context.TryResolve(Target, out target)) return false;
            _fillAmount = target.fillAmount;
            return true;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("UI", "RectTransform Width Height", 603)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.UGUI.Runtime")]
    public sealed class RectTransformSizeStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Vector2 _sizeDelta = new Vector2(100f, 100f);

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(RectTransform); } }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            RectTransform target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            Vector2 endValue = _sizeDelta;
            Tweener tween = DG.Tweening.DOTween.To(() => target.sizeDelta, value => target.sizeDelta = value, endValue, Duration);
            ConfigureValueTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            RectTransform target;
            if (!context.TryResolve(Target, out target)) return false;
            _sizeDelta = target.sizeDelta;
            return true;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("UI", "Text Typewriter", 604)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.UGUI.Runtime")]
    public sealed class TextTypewriterStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [TextArea]
        [SerializeField] private string _text = "New text";

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Text); } }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Text target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            string fullText = _text ?? string.Empty;
            int length = 0;
            Tweener tween = DG.Tweening.DOTween.To(
                () => length,
                value =>
                {
                    length = Mathf.Clamp(value, 0, fullText.Length);
                    target.text = fullText.Substring(0, length);
                },
                fullText.Length,
                Duration);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public override void CaptureSnapshot(TweenBuildContext context, TweenStateSnapshot snapshot)
        {
            if (!context.TryResolve(Target, out Text target)) return;
            string original = target.text;
            snapshot.AddRestoreAction(() => { if (target != null) target.text = original; });
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            Text target;
            if (!context.TryResolve(Target, out target)) return false;
            _text = target.text;
            return true;
        }
    }
}

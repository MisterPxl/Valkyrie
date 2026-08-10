using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Valkyrie.DOTween
{
    public enum TweenVectorAxis
    {
        Position,
        Rotation,
        Scale
    }

    [Serializable]
    [ManagedReferenceCategory("Punch", "Position", 200)]
    public sealed class TransformPunchPositionStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Vector3 _punch = Vector3.up;
        [SerializeField] private int _vibrato = 10;
        [SerializeField] private float _elasticity = 1f;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Transform); } }

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            return base.ValidateDefinition(context) && ValidateVector3(_punch, "Punch value", context);
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Transform target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            Tweener tween = target.DOPunchPosition(_punch, Duration, Mathf.Max(0, _vibrato), Mathf.Max(0f, _elasticity));
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            _punch = Vector3.zero;
            return true;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Punch", "Rotation", 201)]
    public sealed class TransformPunchRotationStepDefinition : TimedTweenStep, ITweenTargetStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Vector3 _punch = new Vector3(0f, 0f, 15f);
        [SerializeField] private int _vibrato = 10;
        [SerializeField] private float _elasticity = 1f;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Transform); } }

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            return base.ValidateDefinition(context) && ValidateVector3(_punch, "Punch value", context);
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Transform target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            Tweener tween = target.DOPunchRotation(_punch, Duration, Mathf.Max(0, _vibrato), Mathf.Max(0f, _elasticity));
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Punch", "Scale", 202)]
    public sealed class TransformPunchScaleStepDefinition : TimedTweenStep, ITweenTargetStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Vector3 _punch = Vector3.one * 0.2f;
        [SerializeField] private int _vibrato = 10;
        [SerializeField] private float _elasticity = 1f;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Transform); } }

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            return base.ValidateDefinition(context) && ValidateVector3(_punch, "Punch value", context);
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Transform target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            Tweener tween = target.DOPunchScale(_punch, Duration, Mathf.Max(0, _vibrato), Mathf.Max(0f, _elasticity));
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Shake", "Position", 300)]
    public sealed class TransformShakePositionStepDefinition : TimedTweenStep, ITweenTargetStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Vector3 _strength = Vector3.one;
        [SerializeField] private int _vibrato = 10;
        [SerializeField] private float _randomness = 90f;
        [SerializeField] private bool _fadeOut = true;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Transform); } }

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            return base.ValidateDefinition(context) && ValidateVector3(_strength, "Shake strength", context);
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Transform target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            Tweener tween = target.DOShakePosition(Duration, _strength, Mathf.Max(0, _vibrato), Mathf.Max(0f, _randomness), false, _fadeOut);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Shake", "Rotation", 301)]
    public sealed class TransformShakeRotationStepDefinition : TimedTweenStep, ITweenTargetStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Vector3 _strength = new Vector3(0f, 0f, 15f);
        [SerializeField] private int _vibrato = 10;
        [SerializeField] private float _randomness = 90f;
        [SerializeField] private bool _fadeOut = true;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Transform); } }

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            return base.ValidateDefinition(context) && ValidateVector3(_strength, "Shake strength", context);
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Transform target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            Tweener tween = target.DOShakeRotation(Duration, _strength, Mathf.Max(0, _vibrato), Mathf.Max(0f, _randomness), _fadeOut);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Shake", "Scale", 302)]
    public sealed class TransformShakeScaleStepDefinition : TimedTweenStep, ITweenTargetStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Vector3 _strength = Vector3.one * 0.2f;
        [SerializeField] private int _vibrato = 10;
        [SerializeField] private float _randomness = 90f;
        [SerializeField] private bool _fadeOut = true;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Transform); } }

        public override bool ValidateDefinition(TweenBuildContext context)
        {
            return base.ValidateDefinition(context) && ValidateVector3(_strength, "Shake strength", context);
        }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Transform target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            Tweener tween = target.DOShakeScale(Duration, _strength, Mathf.Max(0, _vibrato), Mathf.Max(0f, _randomness), _fadeOut);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Camera", "Field Of View", 400)]
    public sealed class CameraFieldOfViewStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private float _fieldOfView = 60f;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Camera); } }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Camera target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            float current = target.fieldOfView;
            float endValue = ResolveFloatEndValue(current, _fieldOfView);
            ApplyFloatStartValue(value => target.fieldOfView = value, _fieldOfView);
            Tweener tween = DG.Tweening.DOTween.To(() => target.fieldOfView, value => target.fieldOfView = value, endValue, Duration);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            Camera target;
            if (!context.TryResolve(Target, out target)) return false;
            _fieldOfView = target.fieldOfView;
            return true;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Camera", "Orthographic Size", 401)]
    public sealed class CameraOrthographicSizeStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private float _orthographicSize = 5f;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Camera); } }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Camera target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            float current = target.orthographicSize;
            float endValue = ResolveFloatEndValue(current, _orthographicSize);
            ApplyFloatStartValue(value => target.orthographicSize = value, _orthographicSize);
            Tweener tween = DG.Tweening.DOTween.To(() => target.orthographicSize, value => target.orthographicSize = value, endValue, Duration);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            Camera target;
            if (!context.TryResolve(Target, out target)) return false;
            _orthographicSize = target.orthographicSize;
            return true;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Camera", "Background Color", 402)]
    public sealed class CameraBackgroundColorStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Color _color = Color.black;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Camera); } }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Camera target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            Color current = target.backgroundColor;
            Color endValue = ResolveColorEndValue(current, _color);
            ApplyColorStartValue(value => target.backgroundColor = value, _color);
            Tweener tween = DG.Tweening.DOTween.To(() => target.backgroundColor, value => target.backgroundColor = value, endValue, Duration);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            Camera target;
            if (!context.TryResolve(Target, out target)) return false;
            _color = target.backgroundColor;
            return true;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Renderer", "Material Color", 500)]
    public sealed class MaterialColorStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private string _colorProperty = "_Color";
        [SerializeField] private Color _color = Color.white;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(Renderer); } }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            Renderer renderer;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out renderer)) return false;
            if (renderer.material == null || !renderer.material.HasProperty(_colorProperty))
            {
                context.ReportError(TweenDiagnosticCode.InvalidTarget, "Renderer material does not expose color property '" + _colorProperty + "'.");
                return false;
            }

            Color current = renderer.material.GetColor(_colorProperty);
            Color endValue = ResolveColorEndValue(current, _color);
            ApplyColorStartValue(value => renderer.material.SetColor(_colorProperty, value), _color);
            Tweener tween = DG.Tweening.DOTween.To(
                () => renderer.material.GetColor(_colorProperty),
                value => renderer.material.SetColor(_colorProperty, value),
                endValue,
                Duration);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            Renderer renderer;
            if (!context.TryResolve(Target, out renderer) || renderer.material == null || !renderer.material.HasProperty(_colorProperty)) return false;
            _color = renderer.material.GetColor(_colorProperty);
            return true;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Renderer", "Sprite Color", 501)]
    public sealed class SpriteRendererColorStepDefinition : TimedTweenStep, ITweenTargetStep, ITweenCapturableStep
    {
        [SerializeField] private TweenTargetReference _target = TweenTargetReference.Self();
        [SerializeField] private Color _color = Color.white;

        public TweenTargetReference Target { get { return _target ?? (_target = TweenTargetReference.Self()); } }
        public Type RequiredTargetType { get { return typeof(SpriteRenderer); } }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            SpriteRenderer target;
            if (!ValidateDefinition(context) || !context.TryResolve(Target, out target)) return false;
            Color current = target.color;
            Color endValue = ResolveColorEndValue(current, _color);
            ApplyColorStartValue(value => target.color = value, _color);
            Tweener tween = DG.Tweening.DOTween.To(() => target.color, value => target.color = value, endValue, Duration);
            ConfigureTween(tween);
            return TryPlaceTween(sequence, tween, context);
        }

        public bool CaptureCurrentValue(TweenBuildContext context)
        {
            SpriteRenderer target;
            if (!context.TryResolve(Target, out target)) return false;
            _color = target.color;
            return true;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Callbacks", "Callback", 950)]
    public sealed class CallbackStepDefinition : TweenStep
    {
        [SerializeField] private TweenPlacement _placement = new TweenPlacement();
        [SerializeField] private UnityEvent _callback = new UnityEvent();

        public UnityEvent Callback { get { return _callback; } }

        public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
        {
            if (_placement == null)
            {
                context.ReportError(TweenDiagnosticCode.InvalidValue, "Timeline placement is missing.");
                return false;
            }

            TweenCallback callback = () => _callback.Invoke();
            switch (_placement.Mode)
            {
                case TweenPlacementMode.Append:
                    sequence.AppendCallback(callback);
                    return true;
                case TweenPlacementMode.Join:
                    sequence.InsertCallback(sequence.Duration(false), callback);
                    return true;
                case TweenPlacementMode.Insert:
                    sequence.InsertCallback(_placement.InsertAt, callback);
                    return true;
                default:
                    context.ReportError(TweenDiagnosticCode.InvalidValue, "The timeline placement mode is invalid.");
                    return false;
            }
        }
    }
}

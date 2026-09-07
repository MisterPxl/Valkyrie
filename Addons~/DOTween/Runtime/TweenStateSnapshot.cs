using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astra.Valkyrie.Integrations.DOTween
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public sealed class TweenStateSnapshot
    {
        private readonly List<Entry> _entries = new List<Entry>();

        public void Capture(IList<UnityEngine.Object> targets)
        {
            _entries.Clear();
            if (targets == null)
            {
                return;
            }

            for (int index = 0; index < targets.Count; index++)
            {
                UnityEngine.Object target = targets[index];
                Entry entry;
                if (TryCreateEntry(target, out entry))
                {
                    _entries.Add(entry);
                }
            }
        }

        public void CaptureSteps(TweenBuildContext context, IList<TweenStepDefinition> steps)
        {
            _entries.Clear();
            if (steps == null) return;
            try
            {
                for (int index = 0; index < steps.Count; index++)
                {
                    TweenStepDefinition step = steps[index];
                    if (step == null || !step.Enabled) continue;
                    context.SetCurrentStep(index, step);
                    step.CaptureSnapshot(context, this);
                }
            }
            finally { context.SetCurrentStep(-1, null); }
        }

        public void CaptureTarget(UnityEngine.Object target)
        {
            if (TryCreateEntry(target, out Entry entry))
                _entries.Add(entry);
        }

        /// <summary>Adds restoration for properties supplied by an optional/custom step.</summary>
        public void AddRestoreAction(Action restore)
        {
            if (restore != null) _entries.Add(new ActionEntry(restore));
        }

        public void Clear() => _entries.Clear();

        public void Restore()
        {
            for (int index = 0; index < _entries.Count; index++)
            {
                _entries[index].Restore();
            }
        }

        private static bool TryCreateEntry(UnityEngine.Object target, out Entry entry)
        {
            entry = null;
            if (target is RectTransform rectTransform)
            {
                entry = new RectTransformEntry(rectTransform);
                return true;
            }

            Transform transform = target as Transform;
            if (transform != null)
            {
                entry = new TransformEntry(transform);
                return true;
            }

            CanvasGroup canvasGroup = target as CanvasGroup;
            if (canvasGroup != null)
            {
                entry = new CanvasGroupEntry(canvasGroup);
                return true;
            }

            Camera camera = target as Camera;
            if (camera != null)
            {
                entry = new CameraEntry(camera);
                return true;
            }

            SpriteRenderer spriteRenderer = target as SpriteRenderer;
            if (spriteRenderer != null)
            {
                entry = new SpriteRendererEntry(spriteRenderer);
                return true;
            }

            Renderer renderer = target as Renderer;
            if (renderer != null && renderer.sharedMaterial != null)
            {
                entry = new RendererEntry(renderer);
                return true;
            }

            return false;
        }

        private abstract class Entry
        {
            public abstract void Restore();
        }

        private sealed class ActionEntry : Entry
        {
            private readonly Action _restore;
            public ActionEntry(Action restore) { _restore = restore; }
            public override void Restore() => _restore();
        }

        private sealed class RectTransformEntry : Entry
        {
            private readonly RectTransform _target;
            private readonly TransformEntry _transform;
            private readonly Vector2 _sizeDelta;
            private readonly Vector2 _anchorMin;
            private readonly Vector2 _anchorMax;
            private readonly Vector2 _pivot;
            private readonly Vector3 _anchoredPosition;

            public RectTransformEntry(RectTransform target)
            {
                _target = target;
                _transform = new TransformEntry(target);
                _sizeDelta = target.sizeDelta;
                _anchorMin = target.anchorMin;
                _anchorMax = target.anchorMax;
                _pivot = target.pivot;
                _anchoredPosition = target.anchoredPosition3D;
            }

            public override void Restore()
            {
                if (_target == null) return;
                _transform.Restore();
                _target.anchorMin = _anchorMin;
                _target.anchorMax = _anchorMax;
                _target.pivot = _pivot;
                _target.sizeDelta = _sizeDelta;
                _target.anchoredPosition3D = _anchoredPosition;
            }
        }

        private sealed class TransformEntry : Entry
        {
            private readonly Transform _target;
            private readonly Vector3 _position;
            private readonly Vector3 _localPosition;
            private readonly Quaternion _rotation;
            private readonly Quaternion _localRotation;
            private readonly Vector3 _localScale;

            public TransformEntry(Transform target)
            {
                _target = target;
                _position = target.position;
                _localPosition = target.localPosition;
                _rotation = target.rotation;
                _localRotation = target.localRotation;
                _localScale = target.localScale;
            }

            public override void Restore()
            {
                if (_target == null) return;
                _target.position = _position;
                _target.localPosition = _localPosition;
                _target.rotation = _rotation;
                _target.localRotation = _localRotation;
                _target.localScale = _localScale;
            }
        }

        private sealed class CanvasGroupEntry : Entry
        {
            private readonly CanvasGroup _target;
            private readonly float _alpha;
            private readonly bool _interactable;
            private readonly bool _blocksRaycasts;

            public CanvasGroupEntry(CanvasGroup target)
            {
                _target = target;
                _alpha = target.alpha;
                _interactable = target.interactable;
                _blocksRaycasts = target.blocksRaycasts;
            }

            public override void Restore()
            {
                if (_target == null) return;
                _target.alpha = _alpha;
                _target.interactable = _interactable;
                _target.blocksRaycasts = _blocksRaycasts;
            }
        }

        private sealed class CameraEntry : Entry
        {
            private readonly Camera _target;
            private readonly float _fieldOfView;
            private readonly float _orthographicSize;
            private readonly Color _backgroundColor;

            public CameraEntry(Camera target)
            {
                _target = target;
                _fieldOfView = target.fieldOfView;
                _orthographicSize = target.orthographicSize;
                _backgroundColor = target.backgroundColor;
            }

            public override void Restore()
            {
                if (_target == null) return;
                _target.fieldOfView = _fieldOfView;
                _target.orthographicSize = _orthographicSize;
                _target.backgroundColor = _backgroundColor;
            }
        }

        private sealed class RendererEntry : Entry
        {
            private readonly Renderer _target;
            private readonly Material _material;
            private readonly Color _color;

            public RendererEntry(Renderer target)
            {
                _target = target;
                _material = target.sharedMaterial;
                _color = _material.HasProperty("_Color") ? _material.color : Color.white;
            }

            public override void Restore()
            {
                if (_target == null || _material == null || !_material.HasProperty("_Color")) return;
                _material.color = _color;
            }
        }

        private sealed class SpriteRendererEntry : Entry
        {
            private readonly SpriteRenderer _target;
            private readonly Color _color;

            public SpriteRendererEntry(SpriteRenderer target)
            {
                _target = target;
                _color = target.color;
            }

            public override void Restore()
            {
                if (_target == null) return;
                _target.color = _color;
            }
        }
    }
}

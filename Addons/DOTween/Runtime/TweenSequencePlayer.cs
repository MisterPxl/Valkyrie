using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    public enum TweenCleanupMode
    {
        None,
        Kill,
        CompleteAndKill
    }

    public sealed class TweenSequenceRuntimeIdentity
    {
        public TweenSequencePlayer Player { get; private set; }
        public TweenSequenceAsset Asset { get; private set; }
        public string ReadableId { get; private set; }

        public TweenSequenceRuntimeIdentity(
            TweenSequencePlayer player,
            TweenSequenceAsset asset,
            string readableId)
        {
            Player = player;
            Asset = asset;
            ReadableId = readableId;
        }

        public override string ToString()
        {
            return ReadableId;
        }
    }

    [AddComponentMenu("Valkyrie/DOTween/Tween Sequence Player")]
    [DisallowMultipleComponent]
    public sealed class TweenSequencePlayer : MonoBehaviour
    {
        [SerializeField] private TweenSequenceAsset _asset;
        [SerializeField] private Transform _targetRoot;
        [SerializeField] private List<TweenTargetBinding> _bindings = new List<TweenTargetBinding>();
        [SerializeField] private bool _playOnEnable;
        [SerializeField] private string _idOverride;
        [SerializeField] private UnityEngine.Object _targetOverride;
        [SerializeField] private TweenCleanupMode _disableCleanup = TweenCleanupMode.Kill;
        [SerializeField] private TweenCleanupMode _destroyCleanup = TweenCleanupMode.Kill;

        private readonly List<TweenBuildDiagnostic> _diagnostics = new List<TweenBuildDiagnostic>();
        private Sequence _currentSequence;
        private TweenSequenceRuntimeIdentity _runtimeIdentity;

        public event Action<IReadOnlyList<TweenBuildDiagnostic>> DiagnosticsChanged;

        public TweenSequenceAsset Asset
        {
            get { return _asset; }
            set { _asset = value; }
        }

        public Transform TargetRoot
        {
            get { return _targetRoot != null ? _targetRoot : transform; }
            set { _targetRoot = value; }
        }

        public IList<TweenTargetBinding> Bindings
        {
            get
            {
                if (_bindings == null)
                {
                    _bindings = new List<TweenTargetBinding>();
                }

                return _bindings;
            }
        }

        public bool PlayOnEnable
        {
            get { return _playOnEnable; }
            set { _playOnEnable = value; }
        }

        public string IdOverride
        {
            get { return _idOverride; }
            set { _idOverride = value; }
        }

        public UnityEngine.Object TargetOverride
        {
            get { return _targetOverride; }
            set { _targetOverride = value; }
        }

        public TweenCleanupMode DisableCleanup
        {
            get { return _disableCleanup; }
            set { _disableCleanup = value; }
        }

        public TweenCleanupMode DestroyCleanup
        {
            get { return _destroyCleanup; }
            set { _destroyCleanup = value; }
        }

        public Sequence CurrentSequence
        {
            get { return _currentSequence; }
        }

        public TweenSequenceRuntimeIdentity RuntimeIdentity
        {
            get { return _runtimeIdentity; }
        }

        public IReadOnlyList<TweenBuildDiagnostic> Diagnostics
        {
            get { return _diagnostics; }
        }

        public bool IsPlaying
        {
            get { return _currentSequence != null && _currentSequence.IsActive() && _currentSequence.IsPlaying(); }
        }

        public string EffectiveTweenId
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_idOverride))
                {
                    return _idOverride.Trim();
                }

                string assetName = _asset != null ? _asset.name : "MissingAsset";
                return "Valkyrie.DOTween/" + gameObject.name + "[" + GetInstanceID() + "]/" + assetName;
            }
        }

        public object EffectiveTweenTarget
        {
            get
            {
                if (_targetOverride != null)
                {
                    return _targetOverride;
                }

                return _runtimeIdentity;
            }
        }

        public bool Play()
        {
            Sequence sequence;
            if (!TryBuildSequence(out sequence))
            {
                return false;
            }

            sequence.Play();
            return true;
        }

        public bool TryBuildSequence(out Sequence sequence)
        {
            Kill();
            _diagnostics.Clear();

            TweenBuildContext context = new TweenBuildContext(TargetRoot, _bindings);
            if (_asset == null)
            {
                context.ReportError(TweenDiagnosticCode.MissingAsset, "No TweenSequenceAsset is assigned.");
                CopyAndPublishDiagnostics(context.Diagnostics);
                sequence = null;
                return false;
            }

            bool built = _asset.TryBuildSequence(context, out sequence);
            if (!built || sequence == null || context.HasErrors)
            {
                if (sequence != null)
                {
                    sequence.Kill();
                    sequence = null;
                }

                CopyAndPublishDiagnostics(context.Diagnostics);
                return false;
            }

            try
            {
                string readableId = EffectiveTweenId;
                _runtimeIdentity = new TweenSequenceRuntimeIdentity(this, _asset, readableId);
                object tweenTarget = _targetOverride != null ? (object)_targetOverride : _runtimeIdentity;
                sequence.SetId(readableId);
                sequence.SetTarget(tweenTarget);
                Sequence ownedSequence = sequence;
                sequence.OnKill(() => ReleaseSequence(ownedSequence));
                _currentSequence = sequence;
            }
            catch (Exception exception)
            {
                context.ReportError(
                    TweenDiagnosticCode.BuildFailure,
                    "DOTween identity could not be configured: " + exception.Message);
                sequence.Kill();
                sequence = null;
                _runtimeIdentity = null;
                CopyAndPublishDiagnostics(context.Diagnostics);
                return false;
            }

            CopyAndPublishDiagnostics(context.Diagnostics);
            return true;
        }

        public void Pause()
        {
            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Pause();
            }
        }

        public void Resume()
        {
            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Play();
            }
        }

        public void Rewind(bool includeDelay = true)
        {
            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Rewind(includeDelay);
            }
        }

        public void Complete(bool withCallbacks = false)
        {
            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Complete(withCallbacks);
            }
        }

        public void Kill(bool complete = false)
        {
            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Kill(complete);
            }

            _currentSequence = null;
            _runtimeIdentity = null;
        }

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Cleanup(_disableCleanup);
        }

        private void OnDestroy()
        {
            TweenCleanupMode cleanupMode = _destroyCleanup == TweenCleanupMode.None
                ? TweenCleanupMode.Kill
                : _destroyCleanup;
            Cleanup(cleanupMode);
        }

        private void Cleanup(TweenCleanupMode cleanupMode)
        {
            if (_currentSequence == null)
            {
                return;
            }

            switch (cleanupMode)
            {
                case TweenCleanupMode.None:
                    return;
                case TweenCleanupMode.Kill:
                    Kill();
                    return;
                case TweenCleanupMode.CompleteAndKill:
                    Complete();
                    Kill();
                    return;
                default:
                    Kill();
                    return;
            }
        }

        private void CopyAndPublishDiagnostics(IReadOnlyList<TweenBuildDiagnostic> diagnostics)
        {
            _diagnostics.Clear();
            for (int index = 0; index < diagnostics.Count; index++)
            {
                _diagnostics.Add(diagnostics[index]);
            }

            Action<IReadOnlyList<TweenBuildDiagnostic>> handler = DiagnosticsChanged;
            if (handler != null)
            {
                handler(_diagnostics);
            }
        }

        private void ReleaseSequence(Sequence sequence)
        {
            if (!ReferenceEquals(_currentSequence, sequence))
            {
                return;
            }

            _currentSequence = null;
            _runtimeIdentity = null;
        }
    }
}

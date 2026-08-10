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

    public enum TweenPlayerSourceMode
    {
        Single,
        Sequence,
        Asset
    }

    public sealed class TweenSequenceRuntimeIdentity
    {
        public TweenPlayer Player { get; private set; }
        public TweenSequenceAsset Asset { get; private set; }
        public string ReadableId { get; private set; }

        public TweenSequenceRuntimeIdentity(TweenPlayer player, TweenSequenceAsset asset, string readableId)
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

    [AddComponentMenu("Valkyrie/DOTween/Tween Player")]
    [DisallowMultipleComponent]
    public class TweenPlayer : MonoBehaviour
    {
        [SerializeField] private TweenPlayerSourceMode _sourceMode = TweenPlayerSourceMode.Single;
        [SerializeField] private TweenTimeline _timeline = new TweenTimeline();
        [SerializeField] private TweenSequenceAsset _asset;
        [SerializeField] private Transform _targetRoot;
        [SerializeField] private List<TweenTargetBinding> _bindings = new List<TweenTargetBinding>();
        [SerializeReference] private List<TweenTrigger> _triggers = new List<TweenTrigger>();
        [SerializeField] private List<TweenStepEventBinding> _stepEvents = new List<TweenStepEventBinding>();
        [SerializeField] private TweenPlayerEvents _events = new TweenPlayerEvents();
        [SerializeField] private bool _playOnEnable;
        [SerializeField] private bool _captureSpawnPointOnAwake = true;
        [SerializeField] private string _idOverride;
        [SerializeField] private UnityEngine.Object _targetOverride;
        [SerializeField] private TweenCleanupMode _disableCleanup = TweenCleanupMode.Kill;
        [SerializeField] private TweenCleanupMode _destroyCleanup = TweenCleanupMode.Kill;

        private readonly List<TweenBuildDiagnostic> _diagnostics = new List<TweenBuildDiagnostic>();
        private readonly TweenStateSnapshot _spawnPoint = new TweenStateSnapshot();
        private Sequence _currentSequence;
        private TweenSequenceRuntimeIdentity _runtimeIdentity;

        public event Action<IReadOnlyList<TweenBuildDiagnostic>> DiagnosticsChanged;

        public TweenPlayerSourceMode SourceMode
        {
            get { return _sourceMode; }
            set { _sourceMode = value; }
        }

        public TweenSequenceAsset Asset
        {
            get { return _asset; }
            set { _asset = value; }
        }

        public TweenTimeline Timeline
        {
            get
            {
                if (_timeline == null)
                {
                    _timeline = new TweenTimeline();
                }

                return _timeline;
            }
        }

        public void SetTimeline(TweenTimeline timeline)
        {
            _timeline = timeline ?? new TweenTimeline();
        }

        public TweenTimeline EffectiveTimeline
        {
            get { return _sourceMode == TweenPlayerSourceMode.Asset && _asset != null ? _asset.Timeline : Timeline; }
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

        public IList<TweenTrigger> Triggers
        {
            get
            {
                if (_triggers == null)
                {
                    _triggers = new List<TweenTrigger>();
                }

                return _triggers;
            }
        }

        public IList<TweenStepEventBinding> StepEvents
        {
            get
            {
                if (_stepEvents == null)
                {
                    _stepEvents = new List<TweenStepEventBinding>();
                }

                return _stepEvents;
            }
        }

        public TweenPlayerEvents Events
        {
            get
            {
                if (_events == null)
                {
                    _events = new TweenPlayerEvents();
                }

                return _events;
            }
        }

        public bool PlayOnEnable
        {
            get { return _playOnEnable; }
            set { _playOnEnable = value; }
        }

        public bool CaptureSpawnPointOnAwake
        {
            get { return _captureSpawnPointOnAwake; }
            set { _captureSpawnPointOnAwake = value; }
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

#if UNITY_6000_2_OR_NEWER
                return "Valkyrie.DOTween/" + gameObject.name + "[" + GetEntityId() + "]";
#else
                return "Valkyrie.DOTween/" + gameObject.name + "[" + GetInstanceID() + "]";
#endif
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

        private void Awake()
        {
            if (_captureSpawnPointOnAwake)
            {
                CaptureSpawnPoint();
            }
        }

        private void Start()
        {
            ExecuteTriggers(TweenPlayerLifecycleEvent.Start);
        }

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                Play();
            }

            ExecuteTriggers(TweenPlayerLifecycleEvent.OnEnable);
        }

        private void OnDisable()
        {
            ExecuteTriggers(TweenPlayerLifecycleEvent.OnDisable);
            ApplyCleanup(_disableCleanup);
        }

        private void OnDestroy()
        {
            ExecuteTriggers(TweenPlayerLifecycleEvent.OnDestroy);
            ApplyCleanup(_destroyCleanup == TweenCleanupMode.None ? TweenCleanupMode.Kill : _destroyCleanup);
        }

        [ContextMenu("Play")]
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

        [ContextMenu("Restart")]
        public bool Restart()
        {
            Rewind(true);
            return Play();
        }

        [ContextMenu("Restart From Spawn Point")]
        public bool RestartFromSpawnPoint()
        {
            Kill(false);
            _spawnPoint.Restore();
            return Play();
        }

        [ContextMenu("Capture Spawn Point")]
        public void CaptureSpawnPoint()
        {
            TweenBuildContext context = CreateBuildContext();
            IList<TweenStepDefinition> steps = EffectiveTimeline != null ? EffectiveTimeline.Steps : null;
            List<UnityEngine.Object> targets = new List<UnityEngine.Object>();

            if (steps != null)
            {
                for (int index = 0; index < steps.Count; index++)
                {
                    TweenStepDefinition step = steps[index];
                    if (step == null) continue;
                    context.SetCurrentStep(index, step);
                    step.CollectSnapshotTargets(context, targets);
                }
            }

            context.SetCurrentStep(-1, null);
            _spawnPoint.Capture(targets);
            CopyAndPublishDiagnostics(context.Diagnostics);
        }

        [ContextMenu("Capture Current Tween Values")]
        public bool CaptureCurrentValues()
        {
            TweenBuildContext context = CreateBuildContext();
            IList<TweenStepDefinition> steps = EffectiveTimeline != null ? EffectiveTimeline.Steps : null;
            bool captured = false;
            if (steps != null)
            {
                for (int index = 0; index < steps.Count; index++)
                {
                    ITweenCapturableStep capturableStep = steps[index] as ITweenCapturableStep;
                    if (capturableStep == null) continue;

                    context.SetCurrentStep(index, steps[index]);
                    captured |= capturableStep.CaptureCurrentValue(context);
                }
            }

            context.SetCurrentStep(-1, null);
            CopyAndPublishDiagnostics(context.Diagnostics);
            return captured && !context.HasErrors;
        }

        public bool TryBuildSequence(out Sequence sequence)
        {
            Kill(false);
            _diagnostics.Clear();

            TweenBuildContext context = CreateBuildContext();
            TweenTimeline timeline = ResolveTimeline(context);
            if (timeline == null)
            {
                sequence = null;
                CopyAndPublishDiagnostics(context.Diagnostics);
                return false;
            }

            bool built = timeline.TryBuildSequence(context, out sequence);
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
                _runtimeIdentity = new TweenSequenceRuntimeIdentity(this, _sourceMode == TweenPlayerSourceMode.Asset ? _asset : null, readableId);
                sequence.SetId(readableId);
                sequence.SetTarget(EffectiveTweenTarget);
                sequence.OnStart(InvokeOnStart);
                sequence.OnPlay(InvokeOnPlay);
                sequence.OnUpdate(InvokeOnUpdate);
                sequence.OnComplete(InvokeOnComplete);
                sequence.OnRewind(InvokeOnRewind);
                Sequence ownedSequence = sequence;
                sequence.OnKill(() => ReleaseSequence(ownedSequence));
                _currentSequence = sequence;
                Events.OnCreated.Invoke();
                InvokeAllStepEvents(events => events.OnCreated.Invoke());
            }
            catch (Exception exception)
            {
                context.ReportError(TweenDiagnosticCode.BuildFailure, "DOTween identity could not be configured: " + exception.Message);
                sequence.Kill();
                sequence = null;
                _runtimeIdentity = null;
                CopyAndPublishDiagnostics(context.Diagnostics);
                return false;
            }

            CopyAndPublishDiagnostics(context.Diagnostics);
            return true;
        }

        public bool TryBuildConfiguredSequence(TweenBuildContext context, out Sequence sequence)
        {
            TweenTimeline timeline = ResolveTimeline(context);
            if (timeline == null)
            {
                sequence = null;
                return false;
            }

            return timeline.TryBuildSequence(context, out sequence);
        }

        public void Pause()
        {
            if (_currentSequence != null && _currentSequence.IsActive()) _currentSequence.Pause();
        }

        public void Resume()
        {
            if (_currentSequence != null && _currentSequence.IsActive()) _currentSequence.Play();
        }

        public void Rewind(bool includeDelay = true)
        {
            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Rewind(includeDelay);
                InvokeOnRewind();
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

        private TweenBuildContext CreateBuildContext()
        {
            return new TweenBuildContext(TargetRoot, _bindings);
        }

        private TweenTimeline ResolveTimeline(TweenBuildContext context)
        {
            if (_sourceMode == TweenPlayerSourceMode.Asset)
            {
                if (_asset == null)
                {
                    context.ReportError(TweenDiagnosticCode.MissingAsset, "No TweenSequenceAsset is assigned.");
                    return null;
                }

                return _asset.Timeline;
            }

            return Timeline;
        }

        private void ExecuteTriggers(TweenPlayerLifecycleEvent lifecycleEvent)
        {
            IList<TweenTrigger> triggers = Triggers;
            for (int index = 0; index < triggers.Count; index++)
            {
                TweenTrigger trigger = triggers[index];
                if (trigger != null && trigger.Matches(lifecycleEvent))
                {
                    trigger.Execute(this);
                }
            }
        }

        private void ApplyCleanup(TweenCleanupMode cleanupMode)
        {
            if (cleanupMode == TweenCleanupMode.Kill)
            {
                Kill(false);
            }
            else if (cleanupMode == TweenCleanupMode.CompleteAndKill)
            {
                Kill(true);
            }
        }

        private void InvokeOnStart()
        {
            Events.OnStart.Invoke();
            InvokeAllStepEvents(events => events.OnStart.Invoke());
        }

        private void InvokeOnPlay()
        {
            Events.OnPlay.Invoke();
            InvokeAllStepEvents(events => events.OnPlay.Invoke());
        }

        private void InvokeOnUpdate()
        {
            Events.OnUpdate.Invoke();
        }

        private void InvokeOnComplete()
        {
            Events.OnComplete.Invoke();
            Events.OnStep.Invoke();
            InvokeAllStepEvents(events =>
            {
                events.OnStep.Invoke();
                events.OnComplete.Invoke();
            });
        }

        private void InvokeOnRewind()
        {
            Events.OnRewind.Invoke();
            InvokeAllStepEvents(events => events.OnRewind.Invoke());
        }

        private void InvokeAllStepEvents(Action<TweenPlayerEvents> invoker)
        {
            if (invoker == null || _stepEvents == null)
            {
                return;
            }

            for (int index = 0; index < _stepEvents.Count; index++)
            {
                TweenStepEventBinding binding = _stepEvents[index];
                if (binding != null)
                {
                    invoker(binding.Events);
                }
            }
        }

        private void ReleaseSequence(Sequence sequence)
        {
            if (_currentSequence == sequence)
            {
                _currentSequence = null;
                _runtimeIdentity = null;
            }
        }

        private void CopyAndPublishDiagnostics(IReadOnlyList<TweenBuildDiagnostic> diagnostics)
        {
            _diagnostics.Clear();
            if (diagnostics != null)
            {
                for (int index = 0; index < diagnostics.Count; index++)
                {
                    _diagnostics.Add(diagnostics[index]);
                }
            }

            DiagnosticsChanged?.Invoke(_diagnostics);
        }
    }

}

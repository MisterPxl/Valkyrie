using System;
using UnityEngine;

namespace Valkyrie.DOTween
{
    public enum TweenPlayerAction
    {
        Play,
        Restart,
        RestartFromSpawnPoint,
        Pause,
        Resume,
        Complete,
        Kill,
        Rewind
    }

    public enum TweenPlayerLifecycleEvent
    {
        OnEnable,
        Start,
        OnDisable,
        OnDestroy
    }

    [Serializable]
    public abstract class TweenTrigger
    {
        [SerializeField] private TweenPlayerAction _action = TweenPlayerAction.Play;

        public TweenPlayerAction Action
        {
            get { return _action; }
            set { _action = value; }
        }

        public abstract bool Matches(TweenPlayerLifecycleEvent lifecycleEvent);

        public void Execute(TweenPlayer player)
        {
            if (player == null)
            {
                return;
            }

            switch (_action)
            {
                case TweenPlayerAction.Play:
                    player.Play();
                    break;
                case TweenPlayerAction.Restart:
                    player.Restart();
                    break;
                case TweenPlayerAction.RestartFromSpawnPoint:
                    player.RestartFromSpawnPoint();
                    break;
                case TweenPlayerAction.Pause:
                    player.Pause();
                    break;
                case TweenPlayerAction.Resume:
                    player.Resume();
                    break;
                case TweenPlayerAction.Complete:
                    player.Complete(false);
                    break;
                case TweenPlayerAction.Kill:
                    player.Kill(false);
                    break;
                case TweenPlayerAction.Rewind:
                    player.Rewind(true);
                    break;
            }
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Lifecycle", "On Enable", 0)]
    public sealed class OnEnableTweenTrigger : TweenTrigger
    {
        public override bool Matches(TweenPlayerLifecycleEvent lifecycleEvent)
        {
            return lifecycleEvent == TweenPlayerLifecycleEvent.OnEnable;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Lifecycle", "Start", 1)]
    public sealed class StartTweenTrigger : TweenTrigger
    {
        public override bool Matches(TweenPlayerLifecycleEvent lifecycleEvent)
        {
            return lifecycleEvent == TweenPlayerLifecycleEvent.Start;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Lifecycle", "On Disable", 2)]
    public sealed class OnDisableTweenTrigger : TweenTrigger
    {
        public override bool Matches(TweenPlayerLifecycleEvent lifecycleEvent)
        {
            return lifecycleEvent == TweenPlayerLifecycleEvent.OnDisable;
        }
    }

    [Serializable]
    [ManagedReferenceCategory("Lifecycle", "On Destroy", 3)]
    public sealed class OnDestroyTweenTrigger : TweenTrigger
    {
        public override bool Matches(TweenPlayerLifecycleEvent lifecycleEvent)
        {
            return lifecycleEvent == TweenPlayerLifecycleEvent.OnDestroy;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Valkyrie.DOTween
{
    [Serializable]
    public sealed class TweenPlayerEvents
    {
        [SerializeField] private UnityEvent _onCreated = new UnityEvent();
        [SerializeField] private UnityEvent _onStart = new UnityEvent();
        [SerializeField] private UnityEvent _onPlay = new UnityEvent();
        [SerializeField] private UnityEvent _onUpdate = new UnityEvent();
        [SerializeField] private UnityEvent _onStep = new UnityEvent();
        [SerializeField] private UnityEvent _onComplete = new UnityEvent();
        [SerializeField] private UnityEvent _onRewind = new UnityEvent();

        public UnityEvent OnCreated { get { return _onCreated; } }
        public UnityEvent OnStart { get { return _onStart; } }
        public UnityEvent OnPlay { get { return _onPlay; } }
        public UnityEvent OnUpdate { get { return _onUpdate; } }
        public UnityEvent OnStep { get { return _onStep; } }
        public UnityEvent OnComplete { get { return _onComplete; } }
        public UnityEvent OnRewind { get { return _onRewind; } }
    }

    [Serializable]
    public sealed class TweenStepEventBinding
    {
        [SerializeField] private string _stepId;
        [SerializeField] private TweenPlayerEvents _events = new TweenPlayerEvents();

        public string StepId
        {
            get { return _stepId; }
            set { _stepId = value; }
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
    }

    [Serializable]
    public sealed class TweenStepEvent : UnityEvent<string> { }
}

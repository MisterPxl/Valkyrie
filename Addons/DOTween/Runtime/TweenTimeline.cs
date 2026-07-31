using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [Serializable]
    public sealed class TweenTimeline
    {
        [SerializeReference] private List<TweenStepDefinition> _steps = new List<TweenStepDefinition>();
        [SerializeField] private Ease _ease = Ease.Linear;
        [SerializeField] private int _loops = 1;
        [SerializeField] private LoopType _loopType = LoopType.Restart;
        [SerializeField] private UpdateType _updateType = UpdateType.Normal;
        [SerializeField] private bool _independentUpdate;
        [Min(0.0001f)]
        [SerializeField] private float _timeScale = 1f;
        [SerializeField] private bool _autoKill;
        [SerializeField] private bool _recyclable;

        public IList<TweenStepDefinition> Steps
        {
            get
            {
                if (_steps == null)
                {
                    _steps = new List<TweenStepDefinition>();
                }

                EnsureStepIds();
                return _steps;
            }
        }

        public Ease Ease
        {
            get { return _ease; }
            set { _ease = value; }
        }

        public int Loops
        {
            get { return _loops; }
            set { _loops = value; }
        }

        public LoopType LoopType
        {
            get { return _loopType; }
            set { _loopType = value; }
        }

        public UpdateType UpdateType
        {
            get { return _updateType; }
            set { _updateType = value; }
        }

        public bool IndependentUpdate
        {
            get { return _independentUpdate; }
            set { _independentUpdate = value; }
        }

        public float TimeScale
        {
            get { return _timeScale; }
            set { _timeScale = value; }
        }

        public bool AutoKill
        {
            get { return _autoKill; }
            set { _autoKill = value; }
        }

        public bool Recyclable
        {
            get { return _recyclable; }
            set { _recyclable = value; }
        }

        public bool ValidateDefinitions(TweenBuildContext context)
        {
            return TweenSequenceDefinitionBuilder.ValidateDefinitions(Steps, CreateSettings(), context);
        }

        public bool TryBuildSequence(TweenBuildContext context, out Sequence sequence)
        {
            return TweenSequenceDefinitionBuilder.TryBuildSequence(Steps, CreateSettings(), context, out sequence);
        }

        public void EnsureStepIds()
        {
            if (_steps == null)
            {
                return;
            }

            for (int index = 0; index < _steps.Count; index++)
            {
                TweenStepDefinition step = _steps[index];
                if (step != null)
                {
                    step.EnsureId();
                }
            }
        }

        internal TweenSequenceBuildSettings CreateSettings()
        {
            return new TweenSequenceBuildSettings(
                _ease,
                _loops,
                _loopType,
                _updateType,
                _independentUpdate,
                _timeScale,
                _autoKill,
                _recyclable);
        }
    }
}

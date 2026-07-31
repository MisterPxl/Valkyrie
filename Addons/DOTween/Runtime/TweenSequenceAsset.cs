using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [CreateAssetMenu(fileName = "TweenSequence", menuName = "Valkyrie/DOTween/Tween Sequence")]
    public sealed class TweenSequenceAsset : ScriptableObject
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

        public bool TryBuildSequence(TweenBuildContext context, out Sequence sequence)
        {
            sequence = null;
            if (context == null)
            {
                return false;
            }

            if (!ValidateSettings(context))
            {
                return false;
            }

            try
            {
                sequence = DG.Tweening.DOTween.Sequence();
                sequence.SetEase(_ease);
                sequence.SetLoops(_loops, _loopType);
                sequence.SetUpdate(_updateType, _independentUpdate);
                sequence.SetAutoKill(_autoKill);
                sequence.SetRecyclable(_recyclable);
                sequence.timeScale = _timeScale;
                sequence.Pause();
            }
            catch (Exception exception)
            {
                context.ReportError(
                    TweenDiagnosticCode.BuildFailure,
                    "DOTween could not create the sequence: " + exception.Message);
                if (sequence != null)
                {
                    sequence.Kill();
                    sequence = null;
                }

                return false;
            }

            if (_steps == null || _steps.Count == 0)
            {
                context.ReportError(TweenDiagnosticCode.EmptySequence, "The sequence has no steps.");
                sequence.Kill();
                sequence = null;
                return false;
            }

            for (int index = 0; index < _steps.Count; index++)
            {
                TweenStepDefinition step = _steps[index];
                context.SetCurrentStep(index, step);

                if (step == null)
                {
                    context.ReportError(TweenDiagnosticCode.NullStep, "The step reference is null.");
                    continue;
                }

                if (!step.Enabled)
                {
                    continue;
                }

                try
                {
                    int diagnosticCount = context.Diagnostics.Count;
                    bool added = step.TryAddTo(sequence, context);
                    if (!added && context.Diagnostics.Count == diagnosticCount)
                    {
                        context.ReportError(
                            TweenDiagnosticCode.BuildFailure,
                            "The step did not add a tween or provide a diagnostic.");
                    }
                }
                catch (Exception exception)
                {
                    context.ReportError(
                        TweenDiagnosticCode.BuildFailure,
                        "The step could not be built: " + exception.Message);
                }
            }

            context.SetCurrentStep(-1, null);
            if (context.HasErrors)
            {
                sequence.Kill();
                sequence = null;
                return false;
            }

            return true;
        }

        private bool ValidateSettings(TweenBuildContext context)
        {
            bool valid = true;

            if (_loops == 0 || _loops < -1)
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    "Sequence loops must be -1 for infinite playback or at least one.");
                valid = false;
            }

            if (float.IsNaN(_timeScale) || float.IsInfinity(_timeScale) || _timeScale <= 0f)
            {
                context.ReportError(
                    TweenDiagnosticCode.InvalidValue,
                    "Sequence time scale must be finite and greater than zero.");
                valid = false;
            }

            return valid;
        }
    }
}

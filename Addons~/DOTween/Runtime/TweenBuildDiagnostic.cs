using System;
using UnityEngine;

namespace Valkyrie.DOTween
{
    public enum TweenDiagnosticSeverity
    {
        Info,
        Warning,
        Error
    }

    public enum TweenDiagnosticCode
    {
        MissingAsset,
        EmptySequence,
        NullStep,
        DuplicateBinding,
        MissingBinding,
        WrongBindingType,
        MissingTarget,
        InvalidTarget,
        InvalidValue,
        BuildFailure,
        UnsupportedInAsset,
        PreviewFailure,
        PresetFailure
    }

    [Serializable]
    public sealed class TweenBuildDiagnostic
    {
        [SerializeField] private TweenDiagnosticSeverity _severity;
        [SerializeField] private TweenDiagnosticCode _code;
        [SerializeField] private string _message;
        [SerializeField] private int _stepIndex;
        [SerializeField] private string _stepType;
        [SerializeField] private string _bindingKey;
        [SerializeField] private string _expectedType;
        [SerializeField] private string _actualType;

        public TweenDiagnosticSeverity Severity
        {
            get { return _severity; }
        }

        public TweenDiagnosticCode Code
        {
            get { return _code; }
        }

        public string Message
        {
            get { return _message; }
        }

        public int StepIndex
        {
            get { return _stepIndex; }
        }

        public string StepType
        {
            get { return _stepType; }
        }

        public string BindingKey
        {
            get { return _bindingKey; }
        }

        public string ExpectedType
        {
            get { return _expectedType; }
        }

        public string ActualType
        {
            get { return _actualType; }
        }

        public TweenBuildDiagnostic(
            TweenDiagnosticSeverity severity,
            TweenDiagnosticCode code,
            string message,
            int stepIndex,
            string stepType,
            string bindingKey,
            string expectedType,
            string actualType)
        {
            _severity = severity;
            _code = code;
            _message = message ?? string.Empty;
            _stepIndex = stepIndex;
            _stepType = stepType ?? string.Empty;
            _bindingKey = bindingKey ?? string.Empty;
            _expectedType = expectedType ?? string.Empty;
            _actualType = actualType ?? string.Empty;
        }

        public override string ToString()
        {
            string location = _stepIndex >= 0
                ? "Step " + _stepIndex + " (" + _stepType + ")"
                : "Sequence";
            return _severity + " " + _code + ": " + location + ": " + _message;
        }
    }
}

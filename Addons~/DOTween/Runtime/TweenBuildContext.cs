using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Astra.Valkyrie.Integrations.DOTween
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public sealed class TweenBuildContext
    {
        private readonly Transform _self;
        private readonly Dictionary<string, UnityEngine.Object> _bindings;
        private readonly List<TweenBuildDiagnostic> _diagnostics;
        private int _currentStepIndex;
        private object _currentStep;
        internal readonly List<(TweenStepDefinition Step, Tween Tween)> BuiltTweens
            = new List<(TweenStepDefinition, Tween)>();

        internal void RegisterTween(Tween tween)
        {
            if (_currentStep is TweenStepDefinition step)
                BuiltTweens.Add((step, tween));
        }

        public Transform Self
        {
            get { return _self; }
        }

        public IReadOnlyList<TweenBuildDiagnostic> Diagnostics
        {
            get { return _diagnostics; }
        }

        public bool HasErrors
        {
            get
            {
                for (int index = 0; index < _diagnostics.Count; index++)
                {
                    if (_diagnostics[index].Severity == TweenDiagnosticSeverity.Error)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public TweenBuildContext(Transform self, IList<TweenTargetBinding> bindings)
        {
            _self = self;
            _bindings = new Dictionary<string, UnityEngine.Object>(StringComparer.Ordinal);
            _diagnostics = new List<TweenBuildDiagnostic>();
            _currentStepIndex = -1;
            _currentStep = null;

            if (bindings == null)
            {
                return;
            }

            for (int index = 0; index < bindings.Count; index++)
            {
                TweenTargetBinding binding = bindings[index];
                if (binding == null)
                {
                    Report(
                        TweenDiagnosticSeverity.Warning,
                        TweenDiagnosticCode.MissingBinding,
                        "A null target binding was ignored.",
                        string.Empty,
                        string.Empty,
                        string.Empty);
                    continue;
                }

                string key = NormalizeKey(binding.Key);
                if (TweenTargetBinding.IsSelfKey(key))
                {
                    Report(
                        TweenDiagnosticSeverity.Warning,
                        TweenDiagnosticCode.DuplicateBinding,
                        "'" + TweenTargetBinding.SelfKey + "' is implicit and cannot be overridden in the binding list.",
                        key,
                        string.Empty,
                        GetTypeName(binding.Target));
                    continue;
                }

                if (_bindings.ContainsKey(key))
                {
                    Report(
                        TweenDiagnosticSeverity.Error,
                        TweenDiagnosticCode.DuplicateBinding,
                        "Binding key '" + key + "' is declared more than once.",
                        key,
                        string.Empty,
                        GetTypeName(binding.Target));
                    continue;
                }

                _bindings.Add(key, binding.Target);
            }
        }

        public bool TryResolve<T>(string key, out T target) where T : UnityEngine.Object
        {
            bool resolved = TryResolve(new TweenTargetReference { Mode = TweenTargetMode.Key, Key = key }, typeof(T), out UnityEngine.Object value);
            target = value as T;
            return resolved;
        }

        public bool TryResolve<T>(TweenTargetReference reference, out T target) where T : UnityEngine.Object
        {
            bool resolved = TryResolve(reference, typeof(T), out UnityEngine.Object value);
            target = value as T;
            return resolved;
        }

        /// <summary>Resolves the exact required component, including types from optional assemblies.</summary>
        public bool TryResolve(TweenTargetReference reference, Type requiredType, out UnityEngine.Object target)
        {
            if (requiredType == null || !typeof(UnityEngine.Object).IsAssignableFrom(requiredType))
                throw new ArgumentException("A UnityEngine.Object type is required.", nameof(requiredType));

            target = null;
            UnityEngine.Object source;
            string key = string.Empty;
            bool objectReference = reference != null && reference.Mode == TweenTargetMode.Object;
            if (objectReference)
            {
                source = reference.Target;
            }
            else
            {
                key = reference == null || reference.Mode == TweenTargetMode.Self
                    ? TweenTargetBinding.SelfKey : NormalizeKey(reference.Key);
                if (TweenTargetBinding.IsSelfKey(key))
                    source = _self;
                else if (!_bindings.TryGetValue(key, out source))
                {
                    Report(TweenDiagnosticSeverity.Error, TweenDiagnosticCode.MissingBinding,
                        "No target is bound to key '" + key + "'.", key, requiredType.FullName, string.Empty);
                    return false;
                }
            }

            if (source == null)
            {
                Report(TweenDiagnosticSeverity.Error,
                    objectReference ? TweenDiagnosticCode.MissingTarget : TweenDiagnosticCode.MissingBinding,
                    objectReference ? "The target reference has no object assigned." : "Binding '" + key + "' has no target.",
                    key, requiredType.FullName, string.Empty);
                return false;
            }

            if (requiredType.IsInstanceOfType(source))
                target = source;
            else if (typeof(Component).IsAssignableFrom(requiredType))
            {
                GameObject gameObject = source as GameObject;
                if (gameObject == null && source is Component component)
                    gameObject = component.gameObject;
                if (gameObject != null)
                    target = gameObject.GetComponent(requiredType);
            }

            if (target != null)
                return true;

            Report(TweenDiagnosticSeverity.Error, TweenDiagnosticCode.WrongBindingType,
                "Target '" + key + "' cannot resolve " + requiredType.FullName + ".",
                key, requiredType.FullName, source.GetType().FullName);
            return false;
        }

        public void ReportError(TweenDiagnosticCode code, string message)
        {
            Report(TweenDiagnosticSeverity.Error, code, message, string.Empty, string.Empty, string.Empty);
        }

        public void ReportWarning(TweenDiagnosticCode code, string message)
        {
            Report(TweenDiagnosticSeverity.Warning, code, message, string.Empty, string.Empty, string.Empty);
        }

        public void ReportError(
            TweenDiagnosticCode code,
            string message,
            string bindingKey,
            Type expectedType,
            Type actualType)
        {
            Report(
                TweenDiagnosticSeverity.Error,
                code,
                message,
                bindingKey,
                expectedType != null ? expectedType.FullName : string.Empty,
                actualType != null ? actualType.FullName : string.Empty);
        }

        public void SetCurrentStep(int stepIndex, object step)
        {
            _currentStepIndex = stepIndex;
            _currentStep = step;
        }

        private void Report(
            TweenDiagnosticSeverity severity,
            TweenDiagnosticCode code,
            string message,
            string bindingKey,
            string expectedType,
            string actualType)
        {
            string stepType = _currentStep != null ? _currentStep.GetType().FullName : string.Empty;
            TweenBuildDiagnostic diagnostic = new TweenBuildDiagnostic(
                severity,
                code,
                message,
                _currentStepIndex,
                stepType,
                bindingKey,
                expectedType,
                actualType);
            _diagnostics.Add(diagnostic);
        }

        private static string NormalizeKey(string key)
        {
            return string.IsNullOrWhiteSpace(key) ? TweenTargetBinding.SelfKey : key.Trim();
        }

        private static string GetTypeName(UnityEngine.Object target)
        {
            return target != null ? target.GetType().FullName : string.Empty;
        }

    }
}

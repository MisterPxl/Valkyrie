using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.DOTween
{
    public sealed class TweenBuildContext
    {
        private readonly Transform _self;
        private readonly Dictionary<string, UnityEngine.Object> _bindings;
        private readonly List<TweenBuildDiagnostic> _diagnostics;
        private int _currentStepIndex;
        private object _currentStep;

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
            string normalizedKey = NormalizeKey(key);
            UnityEngine.Object source;

            if (TweenTargetBinding.IsSelfKey(normalizedKey))
            {
                source = _self;
                normalizedKey = TweenTargetBinding.SelfKey;
            }
            else if (!_bindings.TryGetValue(normalizedKey, out source))
            {
                target = null;
                Report(
                    TweenDiagnosticSeverity.Error,
                    TweenDiagnosticCode.MissingBinding,
                    "No target is bound to key '" + normalizedKey + "'.",
                    normalizedKey,
                    typeof(T).FullName,
                    string.Empty);
                return false;
            }

            if (source == null)
            {
                target = null;
                Report(
                    TweenDiagnosticSeverity.Error,
                    TweenDiagnosticCode.MissingBinding,
                    "Binding '" + normalizedKey + "' has no target.",
                    normalizedKey,
                    typeof(T).FullName,
                    string.Empty);
                return false;
            }

            target = source as T;
            if (target != null)
            {
                return true;
            }

            target = ResolveComponent<T>(source);
            if (target != null)
            {
                return true;
            }

            Report(
                TweenDiagnosticSeverity.Error,
                TweenDiagnosticCode.WrongBindingType,
                "Binding '" + normalizedKey + "' cannot resolve " + typeof(T).FullName + ".",
                normalizedKey,
                typeof(T).FullName,
                source.GetType().FullName);
            return false;
        }

        public bool TryResolve<T>(TweenTargetReference reference, out T target) where T : UnityEngine.Object
        {
            if (reference == null || reference.Mode == TweenTargetMode.Self)
            {
                return TryResolve(TweenTargetBinding.SelfKey, out target);
            }

            if (reference.Mode == TweenTargetMode.Key)
            {
                return TryResolve(reference.Key, out target);
            }

            if (reference.Target == null)
            {
                target = null;
                Report(
                    TweenDiagnosticSeverity.Error,
                    TweenDiagnosticCode.MissingTarget,
                    "The target reference has no object assigned.",
                    string.Empty,
                    typeof(T).FullName,
                    string.Empty);
                return false;
            }

            target = reference.Target as T;
            if (target != null)
            {
                return true;
            }

            target = ResolveComponent<T>(reference.Target);
            if (target != null)
            {
                return true;
            }

            Report(
                TweenDiagnosticSeverity.Error,
                TweenDiagnosticCode.WrongBindingType,
                "The target reference cannot resolve " + typeof(T).FullName + ".",
                reference.DisplayName,
                typeof(T).FullName,
                reference.Target.GetType().FullName);
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

        private static T ResolveComponent<T>(UnityEngine.Object source) where T : UnityEngine.Object
        {
            if (!typeof(Component).IsAssignableFrom(typeof(T)))
            {
                return null;
            }

            GameObject gameObject = source as GameObject;
            if (gameObject != null)
            {
                return gameObject.GetComponent(typeof(T)) as T;
            }

            Component component = source as Component;
            if (component != null)
            {
                return component.GetComponent(typeof(T)) as T;
            }

            return null;
        }
    }
}

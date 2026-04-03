using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    public sealed class InspectedMethod
    {
        public MethodInfo MethodInfo { get; }
        public ButtonAttribute ButtonAttribute { get; }
        public string DisplayName { get; }
        public bool IsValid { get; }
        public string InvalidReason { get; }

        public InspectedMethod(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            ButtonAttribute = methodInfo.GetCustomAttribute<ButtonAttribute>();

            DisplayName = !string.IsNullOrEmpty(ButtonAttribute?.Label)
                ? ButtonAttribute.Label
                : ObjectNames.NicifyVariableName(methodInfo.Name);

            if (methodInfo.ReturnType != typeof(void))
            {
                IsValid = false;
                InvalidReason = $"[Button] \"{methodInfo.Name}\" must return void";
            }
            else if (methodInfo.GetParameters().Length > 0)
            {
                IsValid = false;
                InvalidReason = $"[Button] \"{methodInfo.Name}\" must have no parameters";
            }
            else
            {
                IsValid = true;
            }
        }

        public void Invoke(object target)
        {
            try
            {
                MethodInfo.Invoke(target, null);
            }
            catch (TargetInvocationException e)
            {
                Debug.LogException(e.InnerException ?? e);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}

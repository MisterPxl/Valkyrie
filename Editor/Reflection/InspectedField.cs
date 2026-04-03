using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Valkyrie.Editor
{
    public sealed class InspectedField
    {
        public FieldInfo FieldInfo { get; }
        public string Name => FieldInfo.Name;
        public ValkyrieAttribute[] Attributes { get; }
        public bool HasValkyrieAttributes => Attributes.Length > 0;

        public ConditionalAttribute Conditional { get; }
        public TitleAttribute Title { get; }
        public RequiredAttribute Required { get; }
        public FoldoutGroupAttribute FoldoutGroup { get; }
        public InfoBoxAttribute[] InfoBoxes { get; }
        public bool IsReadOnly { get; }
        public bool IsManagedReference { get; }
        public Type ManagedReferenceBaseType { get; }

        public InspectedField(FieldInfo fieldInfo)
        {
            FieldInfo = fieldInfo;
            Attributes = fieldInfo
                .GetCustomAttributes(typeof(ValkyrieAttribute), true)
                .Cast<ValkyrieAttribute>()
                .OrderBy(a => a.Order)
                .ToArray();

            Conditional = GetAttribute<ConditionalAttribute>();
            Title = GetAttribute<TitleAttribute>();
            Required = GetAttribute<RequiredAttribute>();
            FoldoutGroup = GetAttribute<FoldoutGroupAttribute>();
            IsReadOnly = HasAttribute<ReadOnlyAttribute>();

            var boxes = GetAttributes<InfoBoxAttribute>();
            InfoBoxes = boxes.Length > 0 ? boxes : null;

            IsManagedReference = fieldInfo.IsDefined(typeof(SerializeReference), false);
            ManagedReferenceBaseType = IsManagedReference ? fieldInfo.FieldType : null;
        }

        public T GetAttribute<T>() where T : ValkyrieAttribute
        {
            for (int i = 0; i < Attributes.Length; i++)
            {
                if (Attributes[i] is T attr)
                    return attr;
            }
            return null;
        }

        public bool HasAttribute<T>() where T : ValkyrieAttribute
        {
            for (int i = 0; i < Attributes.Length; i++)
            {
                if (Attributes[i] is T)
                    return true;
            }
            return false;
        }

        public T[] GetAttributes<T>() where T : ValkyrieAttribute
        {
            return Attributes.OfType<T>().ToArray();
        }
    }
}

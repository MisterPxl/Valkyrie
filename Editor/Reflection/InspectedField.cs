using System;
using System.Collections.Generic;
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

        /// <summary>True if this field is a single <see cref="SerializeReference"/> slot.</summary>
        public bool IsManagedReference { get; }

        /// <summary>
        /// True if this field is a <see cref="SerializeReference"/> collection
        /// (<c>List&lt;T&gt;</c> or <c>T[]</c>). Each element is itself a managed reference.
        /// </summary>
        public bool IsManagedReferenceCollection { get; }

        /// <summary>
        /// Polymorphic base type used to populate the type dropdown.
        /// For a single reference: the field type itself.
        /// For a collection: the element type.
        /// </summary>
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

            bool hasSerializeReference = fieldInfo.IsDefined(typeof(SerializeReference), false);
            if (hasSerializeReference)
            {
                Type elementType = TryGetCollectionElementType(fieldInfo.FieldType);
                if (elementType != null)
                {
                    IsManagedReferenceCollection = true;
                    ManagedReferenceBaseType = elementType;
                }
                else
                {
                    IsManagedReference = true;
                    ManagedReferenceBaseType = fieldInfo.FieldType;
                }
            }
        }

        private static Type TryGetCollectionElementType(Type fieldType)
        {
            if (fieldType.IsArray) return fieldType.GetElementType();

            if (fieldType.IsGenericType
                && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return fieldType.GetGenericArguments()[0];
            }

            return null;
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

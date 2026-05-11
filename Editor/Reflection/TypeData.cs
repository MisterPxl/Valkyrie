using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Valkyrie.Editor
{
    public sealed class LayoutSlot
    {
        public readonly bool IsGroup;
        public readonly InspectedField Field;
        public readonly string GroupName;
        public readonly InspectedField[] GroupFields;

        private LayoutSlot(InspectedField field)
        {
            IsGroup = false;
            Field = field;
        }

        private LayoutSlot(string groupName, InspectedField[] fields)
        {
            IsGroup = true;
            GroupName = groupName;
            GroupFields = fields;
        }

        public static LayoutSlot Single(InspectedField field) => new(field);
        public static LayoutSlot Group(string name, InspectedField[] fields) => new(name, fields);
    }

    public sealed class TypeData
    {
        public InspectedField[] Fields { get; }
        public InspectedMethod[] Methods { get; }
        public LayoutSlot[] Layout { get; }

        private readonly Dictionary<string, InspectedField> _fieldsByName;

        private TypeData(InspectedField[] fields, InspectedMethod[] methods, LayoutSlot[] layout)
        {
            Fields = fields;
            Methods = methods;
            Layout = layout;
            _fieldsByName = new Dictionary<string, InspectedField>(fields.Length);
            foreach (var field in fields)
                _fieldsByName[field.Name] = field;
        }

        public InspectedField GetField(string name)
        {
            _fieldsByName.TryGetValue(name, out var field);
            return field;
        }

        public static TypeData Build(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var fields = CollectFields(type, flags)
                .Where(IsSerializableField)
                .Select(f => new InspectedField(f))
                .ToArray();

            var methods = CollectMethods(type, flags)
                .Select(m => new InspectedMethod(m))
                .Where(m => m.ButtonAttribute != null)
                .ToArray();

            var layout = BuildLayout(fields);

            return new TypeData(fields, methods, layout);
        }

        private static LayoutSlot[] BuildLayout(InspectedField[] fields)
        {
            var layout = new List<LayoutSlot>();
            var groupFields = new Dictionary<string, List<InspectedField>>();
            var groupsSeen = new HashSet<string>();

            foreach (var field in fields)
            {
                if (field.FoldoutGroup == null)
                    continue;

                string name = field.FoldoutGroup.GroupName;
                if (!groupFields.TryGetValue(name, out var list))
                {
                    list = new List<InspectedField>();
                    groupFields[name] = list;
                }
                list.Add(field);
            }

            foreach (var field in fields)
            {
                if (field.FoldoutGroup == null)
                {
                    layout.Add(LayoutSlot.Single(field));
                }
                else if (groupsSeen.Add(field.FoldoutGroup.GroupName))
                {
                    var group = groupFields[field.FoldoutGroup.GroupName];
                    layout.Add(LayoutSlot.Group(field.FoldoutGroup.GroupName, group.ToArray()));
                }
            }

            return layout.ToArray();
        }

        private static List<FieldInfo> CollectFields(Type type, BindingFlags flags)
        {
            var result = new List<FieldInfo>();
            var current = type;

            while (current != null && current != typeof(UnityEngine.Object))
            {
                var declared = current.GetFields(flags | BindingFlags.DeclaredOnly);
                result.InsertRange(0, declared);
                current = current.BaseType;
            }

            return result;
        }

        private static List<MethodInfo> CollectMethods(Type type, BindingFlags flags)
        {
            var result = new List<MethodInfo>();
            var current = type;

            while (current != null && current != typeof(UnityEngine.Object))
            {
                var declared = current.GetMethods(flags | BindingFlags.DeclaredOnly);
                result.InsertRange(0, declared);
                current = current.BaseType;
            }

            return result;
        }

        private static bool IsSerializableField(FieldInfo field)
        {
            if (field.GetCustomAttribute<NonSerializedAttribute>() != null)
                return false;
            if (field.GetCustomAttribute<HideInInspector>() != null)
                return false;
            if (field.IsPublic)
                return true;
            if (field.IsDefined(typeof(SerializeField), false))
                return true;
            if (field.IsDefined(typeof(SerializeReference), false))
                return true;
            return false;
        }
    }
}

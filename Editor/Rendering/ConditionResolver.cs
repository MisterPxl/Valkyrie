using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Valkyrie.Editor
{
    public static class ConditionResolver
    {
        private enum MemberKind { NotFound, Field, Property, Method }

        private readonly struct ResolvedMember
        {
            public readonly MemberKind Kind;
            public readonly FieldInfo Field;
            public readonly PropertyInfo Property;
            public readonly MethodInfo Method;

            public static readonly ResolvedMember NotFound = default;

            public ResolvedMember(FieldInfo field) : this() { Kind = MemberKind.Field; Field = field; }
            public ResolvedMember(PropertyInfo prop) : this() { Kind = MemberKind.Property; Property = prop; }
            public ResolvedMember(MethodInfo method) : this() { Kind = MemberKind.Method; Method = method; }
        }

        private static readonly Dictionary<(Type, string), ResolvedMember> MemberCache = new();

        public static bool ShouldDraw(object target, InspectedField field, out string warning)
        {
            warning = null;

            var conditional = field.Conditional;
            if (conditional == null)
                return true;

            if (target == null || string.IsNullOrEmpty(conditional.ConditionMember))
                return true;

            var type = target.GetType();
            var member = ResolveMember(type, conditional.ConditionMember);

            if (member.Kind == MemberKind.NotFound)
            {
                warning = $"Valkyrie: condition member \"{conditional.ConditionMember}\" not found on {type.Name}";
                return true;
            }

            bool result = EvaluateMember(target, member, conditional.CompareValue);
            return conditional.ShouldBeVisible(result);
        }

        public static bool EvaluateCondition(object target, string memberName, object compareValue = null)
        {
            if (target == null || string.IsNullOrEmpty(memberName))
                return true;

            var member = ResolveMember(target.GetType(), memberName);
            return EvaluateMember(target, member, compareValue);
        }

        private static bool EvaluateMember(object target, ResolvedMember member, object compareValue)
        {
            try
            {
                switch (member.Kind)
                {
                    case MemberKind.Field:
                        return CompareOrTruthCheck(member.Field.GetValue(target), compareValue);
                    case MemberKind.Property:
                        return CompareOrTruthCheck(member.Property.GetValue(target), compareValue);
                    case MemberKind.Method:
                        return (bool)member.Method.Invoke(target, null);
                    default:
                        return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return true;
            }
        }

        private static ResolvedMember ResolveMember(Type type, string memberName)
        {
            var key = (type, memberName);
            if (MemberCache.TryGetValue(key, out var cached))
                return cached;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var field = type.GetField(memberName, flags);
            if (field != null)
                return Cache(key, new ResolvedMember(field));

            var prop = type.GetProperty(memberName, flags);
            if (prop != null)
                return Cache(key, new ResolvedMember(prop));

            var method = type.GetMethod(memberName, flags, null, Type.EmptyTypes, null);
            if (method != null && method.ReturnType == typeof(bool))
                return Cache(key, new ResolvedMember(method));

            MemberCache[key] = ResolvedMember.NotFound;
            return ResolvedMember.NotFound;
        }

        private static ResolvedMember Cache((Type, string) key, ResolvedMember member)
        {
            MemberCache[key] = member;
            return member;
        }

        private static bool CompareOrTruthCheck(object value, object compareValue)
        {
            if (compareValue != null)
                return Equals(value, compareValue);
            return ValkyrieEditorUtils.IsTruthy(value);
        }
    }
}

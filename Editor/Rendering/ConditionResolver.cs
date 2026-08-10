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
            _singleTargetBuffer[0] = target;
            return ShouldDraw(_singleTargetBuffer, field, out warning);
        }

        private static readonly object[] _singleTargetBuffer = new object[1];

        /// <summary>
        /// Multi-object-aware visibility: the field is drawn only when the condition
        /// makes it visible on every selected target, so an edit can never silently
        /// reach an object where the field should be hidden.
        /// </summary>
        public static bool ShouldDraw(object[] targets, InspectedField field, out string warning)
        {
            warning = null;

            var conditional = field.Conditional;
            if (conditional == null)
                return true;

            if (targets == null || targets.Length == 0 || string.IsNullOrEmpty(conditional.ConditionMember))
                return true;

            for (int i = 0; i < targets.Length; i++)
            {
                object target = targets[i];
                if (target == null)
                    continue;

                var type = target.GetType();
                var member = ResolveMember(type, conditional.ConditionMember);

                if (member.Kind == MemberKind.NotFound)
                {
                    warning = $"Valkyrie: condition member \"{conditional.ConditionMember}\" not found on {type.Name}";
                    return true;
                }

                bool result = EvaluateMember(target, member, conditional.CompareValue);
                if (!conditional.ShouldBeVisible(result))
                    return false;
            }

            return true;
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

            var prop = FindProperty(type, memberName, flags);
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

        /// <summary>
        /// GetProperty throws AmbiguousMatchException when a derived class shadows
        /// a base property with `new`; fall back to a declared-only walk from the
        /// most derived type so the shadowing member wins.
        /// </summary>
        private static PropertyInfo FindProperty(Type type, string memberName, BindingFlags flags)
        {
            try
            {
                return type.GetProperty(memberName, flags);
            }
            catch (AmbiguousMatchException)
            {
                for (Type current = type; current != null; current = current.BaseType)
                {
                    var prop = current.GetProperty(memberName, flags | BindingFlags.DeclaredOnly);
                    if (prop != null)
                        return prop;
                }
                return null;
            }
        }

        private static bool CompareOrTruthCheck(object value, object compareValue)
        {
            if (compareValue == null)
                return ValkyrieEditorUtils.IsTruthy(value);

            if (Equals(value, compareValue))
                return true;

            return LooselyEquals(value, compareValue);
        }

        /// <summary>
        /// Attribute arguments rarely match the member's exact runtime type:
        /// <c>[ShowIf("mode", 1)]</c> against an enum field, or an int literal
        /// against a float member. Normalize enums (by underlying value or name)
        /// and numeric widening before giving up.
        /// </summary>
        private static bool LooselyEquals(object value, object compareValue)
        {
            if (value == null)
                return false;

            try
            {
                Type valueType = value.GetType();

                if (valueType.IsEnum)
                {
                    if (compareValue is string name)
                        return string.Equals(value.ToString(), name, StringComparison.Ordinal);
                    if (compareValue is IConvertible)
                        return Equals(value, Enum.ToObject(valueType, compareValue));
                    return false;
                }

                if (value is IConvertible && compareValue is IConvertible
                    && !(value is string) && !(compareValue is string))
                {
                    return Convert.ToDouble(value).Equals(Convert.ToDouble(compareValue));
                }
            }
            catch
            {
                // Incompatible shapes (e.g. non-numeric string) simply do not match.
            }

            return false;
        }
    }
}

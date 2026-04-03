using System;

namespace Valkyrie
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public abstract class ConditionalAttribute : ValkyrieAttribute
    {
        public string ConditionMember { get; }
        public object CompareValue { get; }

        protected ConditionalAttribute(string conditionMember)
        {
            ConditionMember = conditionMember;
        }

        protected ConditionalAttribute(string conditionMember, object compareValue)
        {
            ConditionMember = conditionMember;
            CompareValue = compareValue;
        }

        /// <summary>
        /// Given the evaluated condition result, returns whether the field should be drawn.
        /// </summary>
        public abstract bool ShouldBeVisible(bool conditionResult);
    }
}

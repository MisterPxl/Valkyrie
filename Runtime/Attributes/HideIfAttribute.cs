namespace Valkyrie
{
    public sealed class HideIfAttribute : ConditionalAttribute
    {
        public HideIfAttribute(string conditionMember) : base(conditionMember) { }
        public HideIfAttribute(string conditionMember, object compareValue) : base(conditionMember, compareValue) { }

        public override bool ShouldBeVisible(bool conditionResult) => !conditionResult;
    }
}

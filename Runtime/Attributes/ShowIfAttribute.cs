namespace Valkyrie
{
    public sealed class ShowIfAttribute : ConditionalAttribute
    {
        public ShowIfAttribute(string conditionMember) : base(conditionMember) { }
        public ShowIfAttribute(string conditionMember, object compareValue) : base(conditionMember, compareValue) { }

        public override bool ShouldBeVisible(bool conditionResult) => conditionResult;
    }
}

using GameEngine.Generator;

namespace GameEngine.Rules
{
    public record PowerTextBlock(
        string Name, string TypeInfo, 
        string? FlavorText,
        string PowerUsage, EquatableImmutableList<string> Keywords, 
        string? ActionType, string? AttackType, string? AttackTypeDetails, 
        string? Prerequisite, string? Requirement, string? Trigger,
        string? Target,
        string? Attack,
        EquatableImmutableList<RulesText> RulesText
    );

    public record RulesText(
        string Label,
        string Text
    );
}

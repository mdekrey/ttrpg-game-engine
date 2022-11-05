using PrincipleStudios.OpenApiCodegen.Json.Extensions;
namespace GameEngine.Web.Api;

public partial record PowerTextBlock
{
    public PowerTextBlock(
        string Name,
        string TypeInfo,
        string? FlavorText,
        string PowerUsage,
        global::System.Collections.Generic.IEnumerable<string> Keywords,
        string? ActionType,
        string? AttackType,
        string? AttackTypeDetails,
        string? Prerequisite,
        string? Requirement,
        string? Trigger,
        string? Target,
        string? Attack,
        global::System.Collections.Generic.IEnumerable<RulesText> RulesText,
        PowerTextBlock? AssociatedPower
    ) : this(
        Name,
        TypeInfo,
        FlavorText == null ? null : Optional.Create(FlavorText),
        PowerUsage,
        Keywords,
        ActionType == null ? null : Optional.Create(ActionType),
        AttackType == null ? null : Optional.Create(AttackType),
        AttackTypeDetails == null ? null : Optional.Create(AttackTypeDetails),
        Prerequisite == null ? null : Optional.Create(Prerequisite),
        Requirement == null ? null : Optional.Create(Requirement),
        Trigger == null ? null : Optional.Create(Trigger),
        Target == null ? null : Optional.Create(Target),
        Attack == null ? null : Optional.Create(Attack),
        RulesText,
        AssociatedPower == null ? null : Optional.Create(AssociatedPower))
    {

    }
}

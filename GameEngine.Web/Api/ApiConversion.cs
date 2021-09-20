using GameEngine.Generator;
using System.Collections.Immutable;

namespace GameEngine.Web.Api;

public static class ApiConversion
{
    public static GameEngine.Generator.ClassProfile FromApi(this Api.ClassProfile apiModel) =>
        new Generator.ClassProfile(apiModel.Role.FromApi(), apiModel.PowerSource, apiModel.Tools.Select(FromApi).ToImmutableList());

    public static GameEngine.Generator.ToolProfile FromApi(this Api.ToolProfile apiModel) =>
        new GameEngine.Generator.ToolProfile(
            Type: FromApi(apiModel.ToolType),
            Range: FromApi(apiModel.ToolRange),
            Abilities: apiModel.Abilities.Select(a => FromApi(a)).ToImmutableList(),
            PreferredDamageTypes: apiModel.PreferredDamageTypes.Select(a => FromApi(a)).ToImmutableList(),
            PowerProfileConfig: apiModel.PowerProfileConfig.FromApi()
        );

    public static GameEngine.DamageType FromApi(this Api.DamageType apiModel) =>
        apiModel switch
        {
            Api.DamageType.Normal => GameEngine.DamageType.Normal,
            Api.DamageType.Fire => GameEngine.DamageType.Fire,
            Api.DamageType.Cold => GameEngine.DamageType.Cold,
            Api.DamageType.Necrotic => GameEngine.DamageType.Necrotic,
            Api.DamageType.Radiant => GameEngine.DamageType.Radiant,
            Api.DamageType.Lightning => GameEngine.DamageType.Lightning,
            Api.DamageType.Thunder => GameEngine.DamageType.Thunder,
            Api.DamageType.Poison => GameEngine.DamageType.Poison,
            Api.DamageType.Force => GameEngine.DamageType.Force,
            _ => throw new NotSupportedException(),
        };

    public static Generator.PowerProfileConfig FromApi(this Api.PowerProfileConfig apiModel) =>
        new Generator.PowerProfileConfig(
            ModifierChances: apiModel.ModifierChances.Select(a => FromApiToModifier(a)).ToImmutableList(),
            PowerChances: apiModel.PowerChances.Select(a => FromApiToPower(a)).ToImmutableList()
        );

    public static Generator.ModifierChance FromApiToModifier(this Api.ModifierChance apiModel) =>
        new Generator.ModifierChance(apiModel.Selector, apiModel.Weight);

    public static Generator.PowerChance FromApiToPower(this Api.ModifierChance apiModel) =>
        new Generator.PowerChance(apiModel.Selector, apiModel.Weight);

    public static GameEngine.Rules.Ability FromApi(this Api.Ability apiModel) =>
        apiModel switch
        {
            Api.Ability.Strength => Rules.Ability.Strength,
            Api.Ability.Constitution => Rules.Ability.Constitution,
            Api.Ability.Dexterity => Rules.Ability.Dexterity,
            Api.Ability.Intelligence => Rules.Ability.Intelligence,
            Api.Ability.Wisdom => Rules.Ability.Wisdom,
            Api.Ability.Charisma => Rules.Ability.Charisma,
            _ => throw new NotSupportedException(),
        };

    public static Generator.ToolRange FromApi(this Api.ToolRange apiModel) =>
        apiModel switch
        {
            Api.ToolRange.Melee => Generator.ToolRange.Melee,
            Api.ToolRange.Range => Generator.ToolRange.Range,
            _ => throw new NotSupportedException(),
        };

    public static Generator.ToolType FromApi(this Api.ToolType apiModel) =>
        apiModel switch
        {
            Api.ToolType.Weapon => Generator.ToolType.Weapon,
            Api.ToolType.Implement => Generator.ToolType.Implement,
            _ => throw new NotSupportedException(),
        };

    public static GameEngine.Rules.ClassRole FromApi(this Api.CharacterRole apiModel) =>
        apiModel switch
        {
            Api.CharacterRole.Controller => GameEngine.Rules.ClassRole.Controller,
            Api.CharacterRole.Defender => GameEngine.Rules.ClassRole.Defender,
            Api.CharacterRole.Leader => GameEngine.Rules.ClassRole.Leader,
            Api.CharacterRole.Striker => GameEngine.Rules.ClassRole.Striker,
            _ => throw new NotSupportedException(),
        };

    public static Api.PowerTextBlock ToApi(this GameEngine.Rules.PowerTextBlock model) =>
        new Api.PowerTextBlock(
            Name: model.Name,
            TypeInfo: model.TypeInfo,
            FlavorText: model.FlavorText,
            PowerUsage: model.PowerUsage,
            Keywords: model.Keywords,
            ActionType: model.ActionType,
            AttackType: model.AttackType,
            AttackTypeDetails: model.AttackTypeDetails,
            Prerequisite: model.Prerequisite,
            Requirement: model.Requirement,
            Trigger: model.Trigger,
            Target: model.Target,
            Attack: model.Attack,
            RulesText: model.RulesText.Select(ToApi).ToImmutableList()
        );

    public static Api.RulesText ToApi(this GameEngine.Rules.RulesText model) =>
        new Api.RulesText(Label: model.Label, Text: model.Text);

    public static Api.PowerProfile ToApi(this GameEngine.Generator.PowerProfile model) =>
        new Api.PowerProfile(
            Level: model.Level,
            Usage: model.Usage.ToApi(),
            Tool: model.Tool.ToApi(),
            ToolRange: model.ToolRange.ToApi(),
            PowerSource: model.PowerSource
            // Attacks: model.Attacks,
            // Modifiers: model.Modifiers
        );

    public static Api.PowerFrequency ToApi(this Rules.PowerFrequency model) =>
        model switch
        {
            Rules.PowerFrequency.AtWill => Api.PowerFrequency.AtWill,
            Rules.PowerFrequency.Encounter => Api.PowerFrequency.Encounter,
            Rules.PowerFrequency.Daily => Api.PowerFrequency.Daily,
            _ => throw new NotSupportedException(),
        };

    public static Api.ToolRange ToApi(this Generator.ToolRange apiModel) =>
        apiModel switch
        {
            Generator.ToolRange.Melee => Api.ToolRange.Melee,
            Generator.ToolRange.Range => Api.ToolRange.Range,
            _ => throw new NotSupportedException(),
        };

    public static Api.ToolType ToApi(this Generator.ToolType apiModel) =>
        apiModel switch
        {
            Generator.ToolType.Weapon => Api.ToolType.Weapon,
            Generator.ToolType.Implement => Api.ToolType.Implement,
            _ => throw new NotSupportedException(),
        };


    public static Api.ClassDetailsReadOnly ToApi(this Generator.GeneratedClassDetails generatedClassDetails) =>
        new ClassDetailsReadOnly(
            generatedClassDetails.Powers.Select(p => p.ToApiTemporary())
        );

    public static Api.PowerTextProfile ToApiTemporary(this Generator.PowerProfile powerProfile) =>
        new Api.PowerTextProfile(
            Text: powerProfile.ToPowerTextBlock().ToApi(),
            Profile: powerProfile.ToApi()
        );
}
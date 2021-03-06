using GameEngine.Generator;
using GameEngine.Generator.Text;
using GameEngine.Web.AsyncServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Web.Api;

public static class ApiConversion
{
    public static GameEngine.Generator.ClassProfile FromApi(this Api.EditableClassDescriptor apiModel) =>
        new Generator.ClassProfile(apiModel.Role.FromApi(), apiModel.PowerSource, apiModel.Tools.Select(FromApi).ToImmutableList());

    public static Api.EditableClassDescriptor ToApi(this GameEngine.Generator.ClassProfile model, string name, string description) =>
        new Api.EditableClassDescriptor(Name: name, Description: description, Role: model.Role.ToApi(), PowerSource: model.PowerSource, Tools: model.Tools.Select(ToApi).ToImmutableList());

    public static Api.ClassProfile ToApi(this GameEngine.Generator.ClassProfile model) =>
        new Api.ClassProfile(Role: model.Role.ToApi(), PowerSource: model.PowerSource, Tools: model.Tools.Select(ToApi).ToImmutableList());

    public static GameEngine.Generator.ClassProfile FromApi(this Api.ClassProfile model) =>
        new GameEngine.Generator.ClassProfile(Role: model.Role.FromApi(), PowerSource: model.PowerSource, Tools: model.Tools.Select(FromApi).ToImmutableList());

    public static GameEngine.Generator.ToolProfile FromApi(this Api.ToolProfile apiModel) =>
        new GameEngine.Generator.ToolProfile(
            Type: FromApi(apiModel.ToolType),
            Range: FromApi(apiModel.ToolRange),
            Abilities: apiModel.Abilities.Select(a => FromApi(a)).ToImmutableList(),
            PreferredDamageTypes: apiModel.PreferredDamageTypes.Select(a => a.Select(FromApi).ToImmutableList()).ToImmutableList(),
            PowerProfileConfigs: apiModel.PowerProfileConfigs.Select(a => a.FromApi()).ToImmutableList(),
            PossibleRestrictions: apiModel.PossibleRestrictions.ToImmutableList()
        );

    public static Api.ToolProfile ToApi(this Generator.ToolProfile model) =>
        new Api.ToolProfile(
            ToolType: ToApi(model.Type),
            ToolRange: ToApi(model.Range),
            Abilities: model.Abilities.Select(a => ToApi(a)).ToImmutableList(),
            PreferredDamageTypes: model.PreferredDamageTypes.Select(a => a.Select(ToApi).ToImmutableList()).ToImmutableList(),
            PowerProfileConfigs: model.PowerProfileConfigs.Select(a => a.ToApi()).ToImmutableList(),
            PossibleRestrictions: model.PossibleRestrictions.ToImmutableList()
        );

    public static GameEngine.DamageType FromApi(this Api.DamageType apiModel) =>
        apiModel switch
        {
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

    public static Api.DamageType ToApi(this GameEngine.DamageType model) =>
        model switch
        {
            GameEngine.DamageType.Fire => Api.DamageType.Fire,
            GameEngine.DamageType.Cold => Api.DamageType.Cold,
            GameEngine.DamageType.Necrotic => Api.DamageType.Necrotic,
            GameEngine.DamageType.Radiant => Api.DamageType.Radiant,
            GameEngine.DamageType.Lightning => Api.DamageType.Lightning,
            GameEngine.DamageType.Thunder => Api.DamageType.Thunder,
            GameEngine.DamageType.Poison => Api.DamageType.Poison,
            GameEngine.DamageType.Force => Api.DamageType.Force,
            _ => throw new NotSupportedException(),
        };

    public static Api.PowerHighLevelInfo ToApi(this Generator.PowerHighLevelInfo apiModel) =>
        new Api.PowerHighLevelInfo(
            Level: apiModel.Level,
            Usage: apiModel.Usage.ToApi(),
            ClassProfile: apiModel.ClassProfile.ToApi(),
            ToolIndex: apiModel.ToolProfileIndex,
            PowerProfileIndex: apiModel.PowerProfileConfigIndex);

    public static Generator.PowerHighLevelInfo FromApi(this Api.PowerHighLevelInfo apiModel) =>
        new Generator.PowerHighLevelInfo(
            Level: apiModel.Level,
            Usage: apiModel.Usage.FromApi(),
            ClassProfile: apiModel.ClassProfile.FromApi(),
            ToolProfileIndex: apiModel.ToolIndex,
            PowerProfileConfigIndex: apiModel.PowerProfileIndex);

    public static Generator.PowerProfileConfig FromApi(this Api.PowerProfileConfig apiModel) =>
        new Generator.PowerProfileConfig(
            PowerChances: apiModel.PowerChances.Select(FromApiToPower).ToImmutableList(),
            Name: apiModel.Name
        );

    public static Api.PowerProfileConfig ToApi(this Generator.PowerProfileConfig apiModel) =>
        new Api.PowerProfileConfig(
            PowerChances: apiModel.PowerChances.Select(p => ToApi(p)).ToImmutableList(),
            Name: apiModel.Name
        );

    public static Generator.PowerProfileConfig.PowerChance FromApiToPower(this Api.PowerChance apiModel) =>
        new Generator.PowerProfileConfig.PowerChance(apiModel.Selector, apiModel.Weight);

    public static Api.PowerChance ToApi(this Generator.PowerProfileConfig.PowerChance apiModel) =>
        new Api.PowerChance(apiModel.Selector, apiModel.Weight);

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

    public static Api.Ability ToApi(this GameEngine.Rules.Ability model) =>
        model switch
        {
            Rules.Ability.Strength => Api.Ability.Strength,
            Rules.Ability.Constitution => Api.Ability.Constitution,
            Rules.Ability.Dexterity => Api.Ability.Dexterity,
            Rules.Ability.Intelligence => Api.Ability.Intelligence,
            Rules.Ability.Wisdom => Api.Ability.Wisdom,
            Rules.Ability.Charisma => Api.Ability.Charisma,
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

    public static Api.CharacterRole ToApi(this GameEngine.Rules.ClassRole model) =>
        model switch
        {
            GameEngine.Rules.ClassRole.Controller => Api.CharacterRole.Controller,
            GameEngine.Rules.ClassRole.Defender => Api.CharacterRole.Defender,
            GameEngine.Rules.ClassRole.Leader => Api.CharacterRole.Leader,
            GameEngine.Rules.ClassRole.Striker => Api.CharacterRole.Striker,
            _ => throw new NotSupportedException(),
        };

    public static Api.ProgressState ToApi(this AsyncServices.ProgressState model) =>
        model switch
        {
            AsyncServices.ProgressState.InProgress => Api.ProgressState.InProgress,
            AsyncServices.ProgressState.Finished => Api.ProgressState.Finished,
            AsyncServices.ProgressState.Locked => Api.ProgressState.ReadOnly,
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
            RulesText: model.RulesText.Select(ToApi).ToImmutableList(),
            AssociatedPower: model.AssociatedPower?.ToApi()
        );

    public static Api.RulesText ToApi(this GameEngine.Rules.RulesText model) =>
        new Api.RulesText(Label: model.Label, Text: model.Text);
    public static Api.ClassDescriptor ToApi(this GameEngine.Web.AsyncServices.ClassDetails model) =>
        new Api.ClassDescriptor(Name: model.Name, Description: model.Description, State: model.ProgressState.ToApi(), Role: model.ClassProfile.Role.ToApi(), PowerSource: model.ClassProfile.PowerSource, Tools: model.ClassProfile.Tools.Select(ToApi).ToImmutableList());

    //public static Api.PowerProfile ToApi(this GameEngine.Generator.ClassPowerProfile model) =>
    //    new Api.PowerProfile(
    //        Usage: model.PowerInfo.Usage.ToApi(),
    //        Tool: model.PowerInfo.ToolType.ToApi(),
    //        ToolRange: model.PowerInfo.ToolRange.ToApi(),
    //        Level: model.PowerInfo.Level
    //    // Attacks: model.Attacks,
    //    // Modifiers: model.Modifiers
    //    );

    public static Api.PowerFrequency ToApi(this Rules.PowerFrequency model) =>
        model switch
        {
            Rules.PowerFrequency.AtWill => Api.PowerFrequency.AtWill,
            Rules.PowerFrequency.Encounter => Api.PowerFrequency.Encounter,
            Rules.PowerFrequency.Daily => Api.PowerFrequency.Daily,
            _ => throw new NotSupportedException(),
        };

    public static Rules.PowerFrequency FromApi(this Api.PowerFrequency model) =>
        model switch
        {
            Api.PowerFrequency.AtWill => Rules.PowerFrequency.AtWill,
            Api.PowerFrequency.Encounter => Rules.PowerFrequency.Encounter,
            Api.PowerFrequency.Daily => Rules.PowerFrequency.Daily,
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


    public static Api.ClassDetailsReadOnly ToApi(this ClassDetails generatedClassDetails, IEnumerable<PowerDetails> powers) =>
        new ClassDetailsReadOnly(
            Name: generatedClassDetails.Name,
            Description: generatedClassDetails.Description,
            Role: generatedClassDetails.ClassProfile.Role.ToApi(),
            PowerSource: generatedClassDetails.ClassProfile.PowerSource,
            Tools: generatedClassDetails.ClassProfile.Tools.Select(ToApi),
            State: generatedClassDetails.ProgressState.ToApi(),
            Powers: powers.Select(p => p.ToApi())
        );

    public static Api.PowerTextProfile ToApi(this PowerDetails powerProfile)
    {
        var (textBlock, flavor) = powerProfile.Profile.ToPowerContext().ToPowerTextBlock(powerProfile.Flavor);
        return new Api.PowerTextProfile(
            Id: powerProfile.PowerId.ToString(),
            Text: textBlock.ToApi(),
            Level: powerProfile.Profile.PowerInfo.Level,
            Usage: powerProfile.Profile.PowerInfo.Usage.ToApi(),
            Flavor: flavor.ToApi()
        );
    }

    public static Dictionary<string, string> ToApi(this FlavorText flavorText)
    {
        return flavorText.Fields.ToDictionary(f => f.Key, f => f.Value);
    }

    public static FlavorText FromApi(this Dictionary<string, string> flavorText)
    {
        return new FlavorText(flavorText.ToImmutableDictionary());
    }

    public static Generator.PowerProfile FromApi(this Api.PowerProfile profile, JsonSerializer serializer)
    {
        return new Generator.PowerProfile(
            Attacks: profile.Attacks.Select(a => ToObject<AttackProfile>(a)).ToImmutableList(),
            Modifiers: profile.Modifiers.Select(a => ToObject<Generator.Modifiers.IPowerModifier>(a)).ToImmutableList(),
            Effects: profile.Effects.Select(a => ToObject<TargetEffect>(a)).ToImmutableList()
        );
        T ToObject<T>(Newtonsoft.Json.Linq.JToken o) => o.ToObject<T>(serializer)!;
    }

}
using GameEngine.Web.RulesDatabase;
using System.Threading.Tasks;
using GameEngine.Web.Api;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PrincipleStudios.OpenApiCodegen.Json.Extensions;

namespace GameEngine.Web.Legacy;

public class LegacyData
{
    private readonly RulesDbContext context;
    private static readonly IReadOnlyList<string> allowedClassPowerTypes = new[] { "Attack", "Utility" };

    public LegacyData(RulesDatabase.RulesDbContext context)
    {
        this.context = context;
    }

    public async Task<ILegacyRule?> GetLegacyRule(string id)
    {
        var legacyRule = await GetLegacyRules(rule => rule.WizardsId == id).SingleOrDefaultAsync();
        if (legacyRule == null) return null;

        return legacyRule.Type switch
        {
            "Class" => await LoadLegacyClassAsync(legacyRule),
            "Race" => await LoadLegacyRaceAsync(legacyRule),
            "Power" => await LoadLegacyPowerAsync(legacyRule),
            "Feat" => await LoadLegacyFeatAsync(legacyRule),
            "Gear" => await LoadLegacyGearAsync(legacyRule),
            "Armor" => await LoadLegacyArmorAsync(legacyRule),
            "Weapon" => await LoadLegacyWeaponAsync(legacyRule),
            "Magic Item" => await LoadLegacyMagicItemAsync(legacyRule),
            _ => ToDetails(legacyRule),
        };
    }

    public async Task<LegacyClassDetails?> GetLegacyClassAsync(string id)
    {
        var legacyRule = await GetLegacyRule(id, "Class");
        if (legacyRule == null)
            return null;
        return await LoadLegacyClassAsync(legacyRule);
    }

    public async Task<LegacyRaceDetails?> GetLegacyRaceAsync(string id)
    {
        var legacyRule = await GetLegacyRule(id, "Race");
        if (legacyRule == null)
            return null;
        return await LoadLegacyRaceAsync(legacyRule);
    }

    public async Task<LegacyFeatDetails?> GetLegacyFeatAsync(string id)
    {
        var legacyRule = await GetLegacyRule(id, "Feat");
        if (legacyRule == null)
            return null;
        return await LoadLegacyFeatAsync(legacyRule);
    }

    public async Task<LegacyPowerDetails?> GetLegacyPowerAsync(string id)
    {
        var legacyRule = await GetLegacyRule(id, "Power");
        if (legacyRule == null)
            return null;
        return await LoadLegacyPowerAsync(legacyRule);
    }

    public async Task<LegacyGearDetails?> GetLegacyGearAsync(string id)
    {
        var legacyRule = await GetLegacyRule(id, "Gear");
        if (legacyRule == null)
            return null;
        return await LoadLegacyGearAsync(legacyRule);
    }

    public async Task<LegacyArmorDetails?> GetLegacyArmorAsync(string id)
    {
        var legacyRule = await GetLegacyRule(id, "Armor");
        if (legacyRule == null)
            return null;
        return await LoadLegacyArmorAsync(legacyRule);
    }

    public async Task<LegacyWeaponDetails?> GetLegacyWeaponAsync(string id)
    {
        var legacyRule = await GetLegacyRule(id, "Weapon");
        if (legacyRule == null)
            return null;
        return await LoadLegacyWeaponAsync(legacyRule);
    }

    public async Task<LegacyMagicItemDetails?> GetLegacyMagicItemAsync(string id)
    {
        var legacyRule = await GetLegacyRule(id, "Magic Item");
        if (legacyRule == null)
            return null;
        return await LoadLegacyMagicItemAsync(legacyRule);
    }

    internal async Task<IEnumerable<LegacyClassSummary>> GetLegacyClassesAsync()
    {
        var results = await GetLegacyRules(rule => rule.Type == "Class").ToArrayAsync();
        return results.Select(rule => new LegacyClassSummary(
            WizardsId: rule.WizardsId, 
            Name: rule.Name, 
            FlavorText: rule.FlavorText, 
            Type: rule.Type, 
            PowerSource: rule.RulesText.SingleOrDefault(r => r.Label == "Power Source")?.Text.Split('.')[0] ?? "",
            Role: rule.RulesText.SingleOrDefault(r => r.Label == "Role")?.Text.Split('.')[0] ?? ""
        )).ToArray();
    }

    internal async Task<IEnumerable<LegacyArmorSummary>> GetAllLegacyArmorAsync(string? search)
    {
        var ruleQueryable = GetLegacyRules(rule => rule.Type == "Armor");
        if (search is { Length: > 0 })
        {
            search = search.ToLower();
            ruleQueryable = ruleQueryable.Where(rule =>
                rule.Name.ToLower().Contains(search)
                || rule.FlavorText.ToLower().Contains(search)
                || rule.RulesText.Any(rt =>
                    rt.Text.ToLower().Contains(search)
                    || (rt.Text.Length > 0 && rt.Label.ToLower().Contains(search))
                )
            );
        }
        return (await ruleQueryable.ToArrayAsync()).Select(rule => new Api.LegacyArmorSummary(
            WizardsId: rule.WizardsId,
            Name: rule.Name,
            FlavorText: rule.Description,
            Type: rule.Type,
            Gold: rule.RulesText.SingleOrDefault(r => r.Label == "Gold")?.Text is string goldText && int.TryParse(goldText, out var gold) ? gold : throw new InvalidOperationException($"Price not found at {rule.WizardsId}"),
            ArmorCategory: rule.RulesText.SingleOrDefault(r => r.Label == "Armor Category")?.Text ?? "",
            ArmorType: rule.RulesText.SingleOrDefault(r => r.Label == "Armor Type")?.Text ?? throw new InvalidOperationException($"Armor Type not found at {rule.WizardsId}"),
            Weight: rule.RulesText.SingleOrDefault(r => r.Label == "Weight")?.Text is string weightText && int.TryParse(weightText, out var weight) ? weight : throw new InvalidOperationException($"Weight not found at {rule.WizardsId}"),
            Speed: rule.RulesText.SingleOrDefault(r => r.Label == "Speed")?.Text is string speedText && int.TryParse(speedText, out var speed) ? speed : null,
            Check: rule.RulesText.SingleOrDefault(r => r.Label == "Check")?.Text is string checkText && int.TryParse(checkText, out var check) ? check : null,
            MinimumEnhancementBonus: rule.RulesText.SingleOrDefault(r => r.Label == "Minimum Enhancement Bonus")?.Text is string enhText && int.TryParse(enhText, out var enh) ? enh : null,
            ArmorBonus: rule.RulesText.SingleOrDefault(r => r.Label == "Armor Bonus")?.Text is string armorBonusText && int.TryParse(armorBonusText, out var armorBonus) ? armorBonus : 0
        )).ToArray();
    }

    internal async Task<IEnumerable<LegacyWeaponSummary>> GetAllLegacyWeaponsAsync(string? search)
    {
        var ruleQueryable = GetLegacyRules(rule => rule.Type == "Weapon");
        if (search is { Length: > 0 })
        {
            search = search.ToLower();
            ruleQueryable = ruleQueryable.Where(rule =>
                rule.Name.ToLower().Contains(search)
                || rule.FlavorText.ToLower().Contains(search)
                || rule.RulesText.Any(rt =>
                    rt.Text.ToLower().Contains(search)
                    || (rt.Text.Length > 0 && rt.Label.ToLower().Contains(search))
                )
            );
        }
        return (await ruleQueryable.ToArrayAsync())
            .Where(rule => rule.RulesText.All(rt => rt is { Label: not "_Primary End" } or { Text: { Length: 0 } }))
            .Select(rule => new Api.LegacyWeaponSummary(
                WizardsId: rule.WizardsId,
                Name: rule.Name,
                FlavorText: rule.Description,
                Type: rule.Type,
                Gold: rule.RulesText.SingleOrDefault(r => r.Label == "Gold")?.Text is string goldText && int.TryParse(goldText, out var gold) ? gold : null,
                WeaponCategory: rule.RulesText.SingleOrDefault(r => r.Label == "Weapon Category")?.Text ?? throw new InvalidOperationException($"Weapon Category not found at {rule.WizardsId}"),
                HandsRequired: rule.RulesText.SingleOrDefault(r => r.Label == "Hands Required")?.Text ?? throw new InvalidOperationException($"Hands Required not found at {rule.WizardsId}"),
                Weight: rule.RulesText.SingleOrDefault(r => r.Label == "Weight")?.Text is string weightText && int.TryParse(weightText, out var weight) ? weight : null,
                ProficiencyBonus: rule.RulesText.SingleOrDefault(r => r.Label == "Proficiency Bonus")?.Text is string profText && int.TryParse(profText, out var prof) ? prof : null,
                Range: rule.RulesText.SingleOrDefault(r => r.Label == "Range")?.Text,
                Damage: rule.RulesText.SingleOrDefault(r => r.Label == "Damage")?.Text ?? throw new InvalidOperationException($"Damage not found at {rule.WizardsId}"),
                Group: rule.RulesText.SingleOrDefault(r => r.Label == "Group")?.Text ?? throw new InvalidOperationException($"Group not found at {rule.WizardsId}"),
                Properties: rule.RulesText.SingleOrDefault(r => r.Label == "Properties")?.Text ?? "",
                Size: rule.RulesText.SingleOrDefault(r => r.Label == "Size")?.Text ?? "Medium"
            )).ToArray();
    }

    internal async Task<IEnumerable<LegacyGearSummary>> GetAllLegacyGearAsync(string? search)
    {
        var ruleQueryable = GetLegacyRules(rule => rule.Type == "Gear");
        if (search is { Length: > 0 })
        {
            search = search.ToLower();
            ruleQueryable = ruleQueryable.Where(rule =>
                rule.Name.ToLower().Contains(search)
                || rule.FlavorText.ToLower().Contains(search)
                || rule.RulesText.Any(rt =>
                    rt.Text.ToLower().Contains(search)
                    || (rt.Text.Length > 0 && rt.Label.ToLower().Contains(search))
                )
            );
        }
        return (await ruleQueryable.ToArrayAsync()).Select(rule => new Api.LegacyGearSummary(
            WizardsId: rule.WizardsId,
            Name: rule.Name,
            FlavorText: rule.Description,
            Type: rule.Type,
            Gold: rule.RulesText.SingleOrDefault(r => r.Label == "Gold")?.Text is string goldText && int.TryParse(goldText, out var gold) ? gold : null,
            Silver: rule.RulesText.SingleOrDefault(r => r.Label == "Silver")?.Text is string silverText && int.TryParse(silverText, out var silver) ? silver : null,
            Copper: rule.RulesText.SingleOrDefault(r => r.Label == "Copper")?.Text is string copperText && int.TryParse(copperText, out var copper) ? copper : null,
            Category: rule.RulesText.SingleOrDefault(r => r.Label == "Category")?.Text ?? throw new InvalidOperationException($"Category not found at {rule.WizardsId}"),
            Weight: rule.RulesText.SingleOrDefault(r => r.Label == "Weight")?.Text is string weightText && int.TryParse(weightText, out var weight) ? weight : null,
            Count: rule.RulesText.SingleOrDefault(r => r.Label == "count")?.Text is string countText && int.TryParse(countText, out var count) ? count : 1
        )).ToArray();
    }

    internal async Task<IEnumerable<LegacyMagicItemSummary>> GetLegacyMagicItemsAsync(int? minLevel, int? maxLevel, string? search)
    {
        var levelNumbers = Enumerable.Range(1, 30);
        if (minLevel is int minLevelValue) levelNumbers = levelNumbers.Where(level => level >= minLevelValue);
        if (maxLevel is int maxLevelValue) levelNumbers = levelNumbers.Where(level => level <= maxLevelValue);
        var levels = levelNumbers.Select(i => i.ToString()).Concat(new[] { "", "0" }).ToArray();
        var ruleQueryable = GetLegacyRules(rule => rule.Type == "Magic Item");
        if (minLevel != null || maxLevel != null)
            ruleQueryable = ruleQueryable.Where(rule => levels.Contains(rule.Level));
        if (search is { Length: > 0 })
        {
            search = search.ToLower();
            ruleQueryable = ruleQueryable.Where(rule => 
                rule.Name.ToLower().Contains(search) 
                || rule.FlavorText.ToLower().Contains(search)
                || rule.RulesText.Any(rt => 
                    rt.Text.ToLower().Contains(search)
                    || (rt.Text.Length > 0 && rt.Label.ToLower().Contains(search))
                )
            );
        }
        var results = await ruleQueryable.ToArrayAsync();
        return results.Select(rule => new Api.LegacyMagicItemSummary(
            WizardsId: rule.WizardsId,
            Name: rule.Name,
            FlavorText: rule.FlavorText,
            Type: rule.Type,
            Level: int.TryParse(rule.Level, out var level) ? level : null,
            Gold: rule.RulesText.SingleOrDefault(r => r.Label == "Gold")?.Text is string goldText && int.TryParse(goldText, out var gold) ? gold : null,
            MagicItemType: rule.RulesText.SingleOrDefault(r => r.Label == "Magic Item Type")?.Text ?? throw new InvalidOperationException($"Magic Item Type not found at {rule.WizardsId}")
        )).ToArray();
    }

    internal async Task<IEnumerable<LegacyRuleSummary>> GetLegacyRacesAsync()
    {
        var results = await GetLegacyRules(rule => rule.Type == "Race").ToArrayAsync();
        return results.Select(rule => new Api.LegacyRuleSummary(
            WizardsId: rule.WizardsId,
            Name: rule.Name,
            FlavorText: rule.FlavorText,
            Type: rule.Type
        )).ToArray();
    }

    internal async Task<IEnumerable<LegacyFeatSummary>> GetLegacyFeatsAsync(string[] tiers, string? search)
    {
        var ruleQueryable = GetLegacyRules(rule => rule.Type == "Feat");
        if (search is { Length: > 0 })
        {
            search = search.ToLower();
            ruleQueryable = ruleQueryable.Where(rule =>
                rule.Name.ToLower().Contains(search)
                || rule.FlavorText.ToLower().Contains(search)
                || rule.RulesText.Any(rt =>
                    rt.Text.ToLower().Contains(search)
                    || (rt.Text.Length > 0 && rt.Label.ToLower().Contains(search))
                )
            );
        }
        if (tiers.Length > 0)
        {
            ruleQueryable = ruleQueryable.Where(rule => !rule.RulesText.Any(t => t.Label == "Tier" && tiers.Contains(t.Text)));
        }
        return (await ruleQueryable.ToArrayAsync()).Select(rule => new LegacyFeatSummary(
            WizardsId: rule.WizardsId,
            Name: rule.Name,
            FlavorText: rule.RulesText.SingleOrDefault(r => r.Label == "Short Description")?.Text ?? "",
            Type: rule.Type,
            Prerequisites: rule.Prereqs
        )).ToArray();
    }


    private async Task<LegacyClassDetails> LoadLegacyClassAsync(ImportedRule legacyRule)
    {
        var result = ToDetails(legacyRule);

        var featureNames = result.Rules.Single(r => r.Label == "_PARSED_CLASS_FEATURE").Text.Split(',').Select(name => name.Trim()).ToArray();

        // No, this isn't a bug with the source data. The original app searches by name.
        var classFeatures = await GetLegacyRules(rule => rule.Type == "Class Feature" && featureNames.Contains(rule.Name)).ToArrayAsync();
        var allClassFeatures = await LoadOrderedAsync(classFeatures, (rule) => LoadClassFeatureAsync(rule, classId: legacyRule.WizardsId));

        var builds = await GetLegacyRules(rule => rule.Type == "Build" && rule.Class.WizardsId == legacyRule.WizardsId).ToArrayAsync();

        var powerRules = await GetLegacyRules(rule => rule.Type == "Power" && rule.Class.WizardsId == legacyRule.WizardsId && allowedClassPowerTypes.Contains(rule.PowerType)).ToArrayAsync();
        var powers = await LoadOrderedAsync(powerRules, LoadLegacyPowerAsync);

        return new(result, builds.Select(b => ToDetails(b)).ToArray(), allClassFeatures, powers);
    }

    private async Task<LegacyRaceDetails> LoadLegacyRaceAsync(ImportedRule legacyRule)
    {
        var result = ToDetails(legacyRule);

        var traitIds = result.Rules.Single(r => r.Label == "Racial Traits").Text.Split(',').Select(id => id.Trim()).ToArray();
        var racialTraits = await GetLegacyRules(rule => rule.Type == "Racial Trait" && traitIds.Contains(rule.WizardsId)).ToArrayAsync();
        var allRacialTraits = await LoadOrderedAsync(racialTraits, LoadRacialTraitAsync);

        return new(result, allRacialTraits);
    }

    private async Task<LegacyGearDetails?> LoadLegacyGearAsync(ImportedRule legacyRule)
    {
        await Task.Yield();
        var result = ToDetails(legacyRule);
        return new(result);
    }

    private async Task<LegacyWeaponDetails?> LoadLegacyWeaponAsync(ImportedRule legacyRule)
    {
        var secondaryEndId = legacyRule.RulesText.SingleOrDefault(r => r.Label == "_Secondary End")?.Text;
        var secondaryEnd = secondaryEndId is { Length: > 0 } ? await GetLegacyRules(rule => rule.Type == "Weapon" && rule.WizardsId == secondaryEndId).SingleOrDefaultAsync() : null;
        var result = ToDetails(legacyRule);
        var secondary = ToDetails(secondaryEnd);
        return new(result, SecondaryEnd: secondary == null ? null : Optional.Create(secondary));
    }

    private async Task<LegacyArmorDetails?> LoadLegacyArmorAsync(ImportedRule legacyRule)
    {
        await Task.Yield();
        var result = ToDetails(legacyRule);
        return new(result);
    }

    private static readonly Regex displayPowerWithUsage = new Regex(@"(?<id>[^\(]+)(\((?<usage>[^\)]+)\))?");
    private async Task<LegacyMagicItemDetails?> LoadLegacyMagicItemAsync(ImportedRule legacyRule)
    {
        var powerIdAndUsages = legacyRule.RulesText.SingleOrDefault(r => r.Label == "_DisplayPowers")?.Text.Split(',')
            .Select(part =>
            {
                var match = displayPowerWithUsage.Match(part.Trim());
                var id = match.Groups["id"].Value;
                var usage = match.Groups["usage"]?.Value;
                return (id, usage);
            }).ToArray();
        var powerIds = powerIdAndUsages?.Select(tuple => tuple.id).ToArray();
        var result = ToDetails(legacyRule);
        var powerRules = powerIds == null ? Enumerable.Empty<LegacyPowerDetails>()
            : Enumerable.Zip(powerIdAndUsages!.Select(tuple => tuple.usage), await LoadOrderedAsync(await GetLegacyRules(rule => rule.Type == "Power" && powerIds.Contains(rule.WizardsId)).ToArrayAsync(), LoadLegacyPowerAsync))
                .Select(zipped => new LegacyPowerDetails(
                    WizardsId: zipped.Second.WizardsId,
                    Name: zipped.Second.Name,
                    FlavorText: zipped.Second.FlavorText,
                    Type: zipped.Second.Type,
                    Description: zipped.Second.Description,
                    ShortDescription: zipped.Second.ShortDescription,
                    Display: zipped.Second.Display,
                    PowerUsage: zipped.First ?? zipped.Second.PowerUsage,
                    ActionType: zipped.Second.ActionType,
                    PowerType: zipped.Second.PowerType,
                    EncounterUses: zipped.Second.EncounterUses,
                    Level: zipped.Second.Level,
                    Sources: zipped.Second.Sources,
                    Rules: zipped.Second.Rules,
                    Keywords: zipped.Second.Keywords,
                    ChildPower: zipped.Second.ChildPower
                ));

        return new(result, Level: int.TryParse(legacyRule.Level, out var level) ? level : null, Powers: powerRules);
    }

    private async Task<LegacyClassFeatureDetails> LoadClassFeatureAsync(ImportedRule rule, string classId)
    {
        var arg = ToDetails(rule);
        var powerIds = arg.Rules.SingleOrDefault(r => r.Label == "Powers")?.Text.Split(',').Select(id => id.Trim()).ToArray()
            ?? arg.Rules.SingleOrDefault(r => r.Label == "_DisplayPowers")?.Text.Split(',').Select(id => id.Trim()).ToArray();
        var powerRules = powerIds == null ? Enumerable.Empty<ImportedRule>()
            : await GetLegacyRules(rule => rule.Type == "Power" && powerIds.Contains(rule.WizardsId) && (rule.Class.WizardsId == classId || rule.Class.WizardsId == "")).ToArrayAsync();
        var powers = await LoadOrderedAsync(powerRules, LoadLegacyPowerAsync);

        var subfeatureIds = arg.Rules.SingleOrDefault(r => r.Label == "_PARSED_SUB_FEATURES")?.Text.Split(',').Select(id => id.Trim()).ToArray()
            ?? arg.Rules.SingleOrDefault(r => r.Label == "_PARSED_CHILD_FEATURES")?.Text.Split(',').Select(id => id.Trim()).ToArray();
        var subfeatureRules = subfeatureIds == null ? Enumerable.Empty<ImportedRule>()
            : await GetLegacyRules(rule => rule.Type == "Class Feature" && subfeatureIds.Contains(rule.WizardsId)).ToArrayAsync();
        var subfeatures = await LoadOrderedAsync(subfeatureRules, rule => LoadClassFeatureAsync(rule, classId: classId));

        return new(
            Details: arg,
            Powers: powers.ToArray(),
            SubFeatures: subfeatures.ToArray()
        );
    }

    private async Task<LegacyRacialTraitDetails> LoadRacialTraitAsync(ImportedRule rule)
    {
        var arg = ToDetails(rule);
        var powerIds = arg.Rules.SingleOrDefault(r => r.Label == "Powers")?.Text.Split(',').Select(id => id.Trim()).ToArray();
        var powerRules = powerIds == null ? Enumerable.Empty<ImportedRule>()
            : await GetLegacyRules(rule => rule.Type == "Power" && powerIds.Contains(rule.WizardsId)).ToArrayAsync();

        var subfeatureIds = arg.Rules.SingleOrDefault(r => r.Label == "_PARSED_SUB_FEATURES")?.Text.Split(',').Select(id => id.Trim()).ToArray();
        var subfeatureRules = subfeatureIds == null ? Enumerable.Empty<ImportedRule>()
            : await GetLegacyRules(rule => rule.Type == "Racial Trait" && subfeatureIds.Contains(rule.WizardsId)).ToArrayAsync();
        var subfeatures = await LoadOrderedAsync(subfeatureRules, LoadRacialTraitAsync);

        var powers = (await LoadOrderedAsync(powerRules, LoadLegacyPowerAsync)).Concat(subfeatures.SelectMany(f => f.Powers)).ToArray();
        return new(Details: arg, Powers: powers, SubTraits: subfeatures.Select(f => f.Details).ToArray());
    }

    private async Task<LegacyFeatDetails> LoadLegacyFeatAsync(ImportedRule rule)
    {
        var arg = ToDetails(rule);
        var powerIds = arg.Rules.SingleOrDefault(r => r.Label == "_DisplayPowers")?.Text.Split(',').Select(id => id.Trim()).ToArray();
        var powerRules = powerIds == null ? Enumerable.Empty<ImportedRule>()
            : await GetLegacyRules(rule => rule.Type == "Power" && powerIds.Contains(rule.WizardsId)).ToArrayAsync();

        var powers = (await LoadOrderedAsync(powerRules, LoadLegacyPowerAsync)).ToArray();
        return new(Details: arg, Prerequisites: rule.Prereqs, Powers: powers);
    }

    private async Task<LegacyPowerDetails> LoadLegacyPowerAsync(ImportedRule rule)
    {
        var childPowerId = rule.RulesText.SingleOrDefault(r => r.Label == "_ChildPower")?.Text;
        var childPower = childPowerId != null && await GetLegacyRule(childPowerId, "Power") is ImportedRule childPowerRule
            ? Optional.Create(await LoadLegacyPowerAsync(childPowerRule))
            : null;
        return new(
            WizardsId: rule.WizardsId,
            Name: rule.Name,
            FlavorText: rule.FlavorText,
            Type: rule.Type,
            Description: rule.Description,
            ShortDescription: rule.ShortDescription,
            Rules: rule.RulesText.Select(e => new LegacyRuleText(e.Label, e.Text)),
            PowerUsage: rule.PowerUsage,
            EncounterUses: rule.EncounterUses,
            PowerType: rule.PowerType,
            Level: rule.Level,
            ActionType: rule.ActionType,
            Display: rule.Display,
            Keywords: rule.Keywords.Select(k => k.KeywordName),
            Sources: rule.Sources.Select(e => e.SourceName),
            ChildPower: childPower
        );
    }

    private async Task<ImportedRule?> GetLegacyRule(string id, string type)
    {
        return await GetLegacyRules(rule => rule.Type == type && rule.WizardsId == id)
            .SingleOrDefaultAsync();
    }

    private IQueryable<ImportedRule> GetLegacyRules(Expression<Func<ImportedRule, bool>> whereClause)
    {
        return context.ImportedRules
            .Where(whereClause)
            .Where(rule => !rule.Sources.All(s => s.SourceName.StartsWith("Dragon Magazine") || s.SourceName.StartsWith("Dungeon Magazine") || s.SourceName.StartsWith("Vor Rukoth")))
            .Include(rule => rule.RulesText.OrderBy(r => r.Order))
            .Include(rule => rule.Keywords)
            .Include(rule => rule.Sources);
    }

    [return: NotNullIfNotNull("rule")]
    private static Api.LegacyRuleDetails? ToDetails(ImportedRule? rule)
    {
        return rule == null ? null
            : new Api.LegacyRuleDetails(
                WizardsId: rule.WizardsId,
                Name: rule.Name,
                FlavorText: rule.FlavorText,
                Type: rule.Type,
                Description: rule.Description,
                ShortDescription: rule.ShortDescription,
                Rules: rule.RulesText.Select(e => new LegacyRuleText(e.Label, e.Text)),
                Sources: rule.Sources.Select(e => e.SourceName)
            );
    }

    private static Task<IEnumerable<TResult>> LoadOrderedAsync<TInput, TResult>(IEnumerable<TInput> input, Func<TInput, Task<TResult>> mapAsync)
    {
        return input.Aggregate(Task.FromResult(Enumerable.Empty<TResult>()), async (prev, next) =>
        {
            return (await prev).Append(await mapAsync(next));
        });
    }

}

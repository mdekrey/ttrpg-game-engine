using GameEngine.Web.RulesDatabase;
using System.Threading.Tasks;
using GameEngine.Web.Api;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
            "Power" => await LoadPowerAsync(legacyRule),
            "Feat" => await LoadLegacyFeatAsync(legacyRule),
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

    internal async Task<IEnumerable<LegacyFeatSummary>> GetLegacyFeatsAsync()
    {
        var results = await GetLegacyRules(rule => rule.Type == "Feat").ToArrayAsync();
        return results.Select(rule => new LegacyFeatSummary(
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
        var powers = await LoadOrderedAsync(powerRules, LoadPowerAsync);

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

    private async Task<LegacyClassFeatureDetails> LoadClassFeatureAsync(ImportedRule rule, string classId)
    {
        var arg = ToDetails(rule);
        var powerIds = arg.Rules.SingleOrDefault(r => r.Label == "_DisplayPowers")?.Text.Split(',').Select(id => id.Trim()).ToArray();
        var powerRules = powerIds == null ? Enumerable.Empty<ImportedRule>()
            : await GetLegacyRules(rule => rule.Type == "Power" && powerIds.Contains(rule.WizardsId) && (rule.Class.WizardsId == classId || rule.Class.WizardsId == "")).ToArrayAsync();
        var powers = await LoadOrderedAsync(powerRules, LoadPowerAsync);

        var subfeatureIds = arg.Rules.SingleOrDefault(r => r.Label == "_PARSED_SUB_FEATURES")?.Text.Split(',').Select(id => id.Trim()).ToArray();
        var subfeatureRules = subfeatureIds == null ? Enumerable.Empty<ImportedRule>()
            : await GetLegacyRules(rule => rule.Type == "Class Feature" && subfeatureIds.Contains(rule.WizardsId)).ToArrayAsync();
        var subfeatures = await LoadOrderedAsync(subfeatureRules, rule => LoadClassFeatureAsync(rule, classId: classId));

        return new(
            ClassFeatureDetails: arg,
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

        var powers = (await LoadOrderedAsync(powerRules, LoadPowerAsync)).Concat(subfeatures.SelectMany(f => f.Powers)).ToArray();
        return new(RacialTraitDetails: arg, Powers: powers, SubTraits: subfeatures.Select(f => f.RacialTraitDetails).ToArray());
    }

    private async Task<LegacyFeatDetails> LoadLegacyFeatAsync(ImportedRule rule)
    {
        var arg = ToDetails(rule);
        var powerIds = arg.Rules.SingleOrDefault(r => r.Label == "_DisplayPowers")?.Text.Split(',').Select(id => id.Trim()).ToArray();
        var powerRules = powerIds == null ? Enumerable.Empty<ImportedRule>()
            : await GetLegacyRules(rule => rule.Type == "Power" && powerIds.Contains(rule.WizardsId)).ToArrayAsync();

        var powers = (await LoadOrderedAsync(powerRules, LoadPowerAsync)).ToArray();
        return new(FeatDetails: arg, Prerequisites: rule.Prereqs, Powers: powers);
    }

    private async Task<LegacyPowerDetails> LoadPowerAsync(ImportedRule rule)
    {
        var childPowerId = rule.RulesText.SingleOrDefault(r => r.Label == "_ChildPower")?.Text;
        var childPower = childPowerId != null && await GetLegacyRule(childPowerId, "Power") is ImportedRule childPowerRule
            ? await LoadPowerAsync(childPowerRule)
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

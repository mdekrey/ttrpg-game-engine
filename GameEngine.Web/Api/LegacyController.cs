using GameEngine.Web.RulesDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GameEngine.Web.Api;

public class LegacyController : LegacyControllerBase
{
    private readonly RulesDbContext context;

    public LegacyController(RulesDatabase.RulesDbContext context)
    {
        this.context = context;
    }

    protected override async Task<GetLegacyClassActionResult> GetLegacyClass(string id)
    {
        var legacyRule = await GetLegacyRule(id, "Class");
        if (legacyRule == null)
            return GetLegacyClassActionResult.NotFound();

        var result = ToDetails(legacyRule);

        var featureNames = result.Rules.Single(r => r.Label == "_PARSED_CLASS_FEATURE").Text.Split(',').Select(name => name.Trim()).ToArray();

        // No, this isn't a bug with the source data. The original app searches by name.
        var classFeatures = await GetLegacyRules(rule => rule.Type == "Class Feature" && featureNames.Contains(rule.Name)).ToArrayAsync();
        var allClassFeatures = await LoadOrderedAsync(classFeatures, LoadClassFeatureAsync(id));

        var builds = await GetLegacyRules(rule => rule.Type == "Build" && rule.Class.WizardsId == id).ToArrayAsync();

        var powerRules = await GetLegacyRules(rule => rule.Type == "Power" && rule.Class.WizardsId == id).ToArrayAsync();
        var powers = await LoadOrderedAsync(powerRules, LoadPowerAsync);

        return GetLegacyClassActionResult.Ok(new(result, builds.Select(b => ToDetails(b)).ToArray(), allClassFeatures, powers));
    }

    private Func<ImportedRule, Task<LegacyClassFeatureDetails>> LoadClassFeatureAsync(string classId)
    {
        return async (rule) =>
        {
            var arg = ToDetails(rule);
            var powerIds = arg.Rules.SingleOrDefault(r => r.Label == "_DisplayPowers")?.Text.Split(',').Select(id => id.Trim()).ToArray();
            var powerRules = powerIds == null ? Enumerable.Empty<ImportedRule>()
                : await GetLegacyRules(rule => rule.Type == "Power" && powerIds.Contains(rule.WizardsId) && (rule.Class.WizardsId == classId || rule.Class.WizardsId == "")).ToArrayAsync();

            var subfeatureIds = arg.Rules.SingleOrDefault(r => r.Label == "_PARSED_SUB_FEATURES")?.Text.Split(',').Select(id => id.Trim()).ToArray();
            var subfeatureRules = subfeatureIds == null ? Enumerable.Empty<ImportedRule>()
                : await GetLegacyRules(rule => rule.Type == "Class Feature" && subfeatureIds.Contains(rule.WizardsId)).ToArrayAsync();
            var subfeatures = await LoadOrderedAsync(subfeatureRules, LoadClassFeatureAsync(classId));

            return new(ClassFeatureDetails: arg, Powers: powerRules.Select(ToPower).ToArray(), SubFeatures: subfeatures);
        };
    }

    protected override async Task<GetLegacyRaceActionResult> GetLegacyRace(string id)
    {
        var legacyRule = await GetLegacyRule(id, "Race");
        if (legacyRule == null)
            return GetLegacyRaceActionResult.NotFound();

        var result = ToDetails(legacyRule);

        var traitIds = result.Rules.Single(r => r.Label == "Racial Traits").Text.Split(',').Select(id => id.Trim()).ToArray();
        var racialTraits = await GetLegacyRules(rule => rule.Type == "Racial Trait" && traitIds.Contains(rule.WizardsId)).ToArrayAsync();
        var allRacialTraits = await LoadOrderedAsync(racialTraits, LoadRacialTraitAsync);

        return GetLegacyRaceActionResult.Ok(new(result, allRacialTraits));
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

    private Task<LegacyPowerDetails> LoadPowerAsync(ImportedRule rule)
    {
        return Task.FromResult(ToPower(rule));
    }

    protected override async Task<GetLegacyClassesActionResult> GetLegacyClasses()
    {
        var results = await (from rule in context.ImportedRules
                             where rule.Type == "Class"
                             select ToSummary(rule)).ToArrayAsync();
        return GetLegacyClassesActionResult.Ok(results);
    }

    protected override async Task<GetLegacyRacesActionResult> GetLegacyRaces()
    {
        var results = await (from rule in context.ImportedRules
                             where rule.Type == "Race"
                             select ToSummary(rule)).ToArrayAsync();
        return GetLegacyRacesActionResult.Ok(results);
    }

    private static Api.LegacyRuleSummary ToSummary(ImportedRule rule)
    {
        return new Api.LegacyRuleSummary(
            WizardsId: rule.WizardsId,
            Name: rule.Name,
            FlavorText: rule.FlavorText,
            Type: rule.Type
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

    private static Api.LegacyPowerDetails ToPower(ImportedRule rule)
    {
        return new (
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

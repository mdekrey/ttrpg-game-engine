using GameEngine.Web.Legacy;
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
    private readonly LegacyData legacyData;

    public LegacyController(RulesDatabase.RulesDbContext context, Legacy.LegacyData legacyData)
    {
        this.context = context;
        this.legacyData = legacyData;
    }

    protected override async Task<GetLegacyClassActionResult> GetLegacyClass(string id)
    {
        return await legacyData.GetLegacyClassAsync(id) is LegacyClassDetails result
            ? GetLegacyClassActionResult.Ok(result)
            : GetLegacyClassActionResult.NotFound();
    }

    protected override async Task<GetLegacyRaceActionResult> GetLegacyRace(string id)
    {
        return await legacyData.GetLegacyRaceAsync(id) is LegacyRaceDetails result
            ? GetLegacyRaceActionResult.Ok(result)
            : GetLegacyRaceActionResult.NotFound();
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

}

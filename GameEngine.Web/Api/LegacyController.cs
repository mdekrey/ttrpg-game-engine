using GameEngine.Web.RulesDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
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
        var results = await GetLegacyRule(id);
        if (results == null)
            return GetLegacyClassActionResult.NotFound();

        return GetLegacyClassActionResult.Ok(results);
    }

    protected override async Task<GetLegacyRaceActionResult> GetLegacyRace(string id)
    {
        var results = await (from rule in context.ImportedRules
                             where rule.Type == "Race" && rule.WizardsId == id
                             select ToDetails(rule)).SingleOrDefaultAsync();
        if (results == null)
            return GetLegacyRaceActionResult.NotFound();

        return GetLegacyRaceActionResult.Ok(results);
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


    private async Task<LegacyRuleDetails?> GetLegacyRule(string id)
    {
        var result = await (from rule in context.ImportedRules
                            where rule.Type == "Class" && rule.WizardsId == id
                            select rule).Include(rule => rule.RulesText.OrderBy(r => r.Order)).SingleOrDefaultAsync();

        return result == null ? null : ToDetails(result);
    }

    private static Api.LegacyRuleDetails ToDetails(ImportedRule rule)
    {
        return new Api.LegacyRuleDetails(
            WizardsId: rule.WizardsId,
            Name: rule.Name,
            FlavorText: rule.FlavorText,
            Type: rule.Type,
            Description: rule.Description,
            ShortDescription: rule.ShortDescription,
            Rules: rule.RulesText.Select(e => new LegacyRuleText(e.Label, e.Text))
        );
    }

}

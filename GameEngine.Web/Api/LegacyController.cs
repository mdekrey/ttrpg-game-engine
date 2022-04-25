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
        var results = await (from rule in context.ImportedRules
                             where rule.Type == "Class" && rule.WizardsId == id
                             select ToDetails(rule)).SingleOrDefaultAsync();
        if (results == null)
            return GetLegacyClassActionResult.NotFound();

        return GetLegacyClassActionResult.Ok(results);
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

    private static Api.LegacyRuleDetails ToDetails(ImportedRule rule)
    {
        return new Api.LegacyRuleDetails(
            WizardsId: rule.WizardsId,
            Name: rule.Name,
            FlavorText: rule.FlavorText,
            Type: rule.Type,
            Description: rule.Description,
            ShortDescription: rule.ShortDescription);
    }

}

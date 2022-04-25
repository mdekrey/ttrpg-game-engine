using GameEngine.Web.RulesDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Web.Api;

public class LegacyController : LegacyControllerBase
{
    private readonly RulesDbContext context;

    public LegacyController(RulesDatabase.RulesDbContext context)
    {
        this.context = context;
    }

    protected override async Task<GetLegacyClassesActionResult> GetLegacyClasses()
    {
        var results = await (from rule in context.ImportedRules
                             where rule.Type == "Class"
                             select new Api.LegacyRuleSummary(rule.WizardsId, rule.Name, rule.FlavorText, rule.Type)).ToArrayAsync();
        return GetLegacyClassesActionResult.Ok(results);
    }

    protected override async Task<GetLegacyRacesActionResult> GetLegacyRaces()
    {
        var results = await (from rule in context.ImportedRules 
                             where rule.Type == "Race"
                             select new Api.LegacyRuleSummary(rule.WizardsId, rule.Name, rule.FlavorText, rule.Type)).ToArrayAsync();
        return GetLegacyRacesActionResult.Ok(results);
    }
}

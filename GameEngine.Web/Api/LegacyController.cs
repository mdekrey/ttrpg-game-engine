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

    protected override async Task<GetLegacyFeatActionResult> GetLegacyFeat(string id)
    {
        return await legacyData.GetLegacyFeatAsync(id) is LegacyFeatDetails result
            ? GetLegacyFeatActionResult.Ok(result)
            : GetLegacyFeatActionResult.NotFound();
    }

    protected override async Task<GetLegacyPowerActionResult> GetLegacyPower(string id)
    {
        return await legacyData.GetLegacyPowerAsync(id) is LegacyPowerDetails result
            ? GetLegacyPowerActionResult.Ok(result)
            : GetLegacyPowerActionResult.NotFound();
    }

    protected override async Task<GetLegacyClassesActionResult> GetLegacyClasses()
    {
        return GetLegacyClassesActionResult.Ok(await legacyData.GetLegacyClassesAsync());
    }

    protected override async Task<GetLegacyRacesActionResult> GetLegacyRaces()
    {
        return GetLegacyRacesActionResult.Ok(await legacyData.GetLegacyRacesAsync());
    }

    protected override async Task<GetLegacyFeatsActionResult> GetLegacyFeats()
    {
        return GetLegacyFeatsActionResult.Ok(await legacyData.GetLegacyFeatsAsync());
    }
}

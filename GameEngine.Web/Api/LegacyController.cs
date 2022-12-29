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

    protected override async Task<GetLegacyGearActionResult> GetLegacyGear(string id)
    {
        return await legacyData.GetLegacyGearAsync(id) is LegacyGearDetails result
            ? GetLegacyGearActionResult.Ok(result)
            : GetLegacyGearActionResult.NotFound();
    }

    protected override async Task<GetLegacyArmorActionResult> GetLegacyArmor(string id)
    {
        return await legacyData.GetLegacyArmorAsync(id) is LegacyArmorDetails result
            ? GetLegacyArmorActionResult.Ok(result)
            : GetLegacyArmorActionResult.NotFound();
    }

    protected override async Task<GetLegacyWeaponActionResult> GetLegacyWeapon(string id)
    {
        return await legacyData.GetLegacyWeaponAsync(id) is LegacyWeaponDetails result
            ? GetLegacyWeaponActionResult.Ok(result)
            : GetLegacyWeaponActionResult.NotFound();
    }

    protected override async Task<GetLegacyMagicItemActionResult> GetLegacyMagicItem(string id)
    {
        return await legacyData.GetLegacyMagicItemAsync(id) is LegacyMagicItemDetails result
            ? GetLegacyMagicItemActionResult.Ok(result)
            : GetLegacyMagicItemActionResult.NotFound();
    }

    protected override async Task<GetLegacyClassesActionResult> GetLegacyClasses()
    {
        return GetLegacyClassesActionResult.Ok(await legacyData.GetLegacyClassesAsync());
    }

    protected override async Task<GetLegacyRacesActionResult> GetLegacyRaces()
    {
        return GetLegacyRacesActionResult.Ok(await legacyData.GetLegacyRacesAsync());
    }

    protected override async Task<GetLegacyFeatsActionResult> GetLegacyFeats(IEnumerable<Tier>? tiers, string? search)
    {
        return GetLegacyFeatsActionResult.Ok(await legacyData.GetLegacyFeatsAsync(tiers?.Select(tier => tier.ToString("g")).ToArray() ?? Array.Empty<string>(), search));
    }

    protected override async Task<GetLegacyItemsActionResult> GetLegacyItems(string? search)
    {
        return GetLegacyItemsActionResult.Ok(new(
            Gear: await legacyData.GetAllLegacyGearAsync(search),
            Armor: await legacyData.GetAllLegacyArmorAsync(search),
            Weapons: await legacyData.GetAllLegacyWeaponsAsync(search)));
    }

    protected override async Task<GetLegacyMagicItemsActionResult> GetLegacyMagicItems(int? minLevel, int? maxLevel, string? search)
    {
        return GetLegacyMagicItemsActionResult.Ok(await legacyData.GetLegacyMagicItemsAsync(minLevel, maxLevel, search));
    }

    protected override async Task<GetLegacyPowersActionResult> GetLegacyPowers(int? minLevel, int? maxLevel, string? search)
    {
        return GetLegacyPowersActionResult.Ok(
            await legacyData.GetLegacyPowersAsync(minLevel, maxLevel, search)
        );
    }
}

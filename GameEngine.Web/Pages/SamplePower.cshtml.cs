﻿using GameEngine.Generator;
using GameEngine.Rules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace GameEngine.Web.Pages;
public class SamplePowerModel : PageModel
{
    private readonly ILogger<SamplePowerModel> _logger;

    public SamplePowerModel(ILogger<SamplePowerModel> logger)
    {
        _logger = logger;
    }

    public PowerTextBlock? Power { get; private set; }

    public void OnGet()
    {
        var (configName, level, powerFrequency) = ("MeleeWeapon", 19, PowerFrequency.Daily);

        var target = new PowerGenerator((min, max) => max - 1);

        ToolProfile toolProfile = new(ToolType.Weapon, ToolRange.Melee, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Normal }.ToImmutableList(), new(ImmutableList<ModifierChance>.Empty.Add(new("$", 1)), ImmutableList<PowerChance>.Empty.Add(new("$", 1))));
        ClassProfile classProfile = new(ClassRole.Striker, PowerSource.Martial, ImmutableList<ToolProfile>.Empty.Add(toolProfile));

        var powerHighLevelInfo = new PowerHighLevelInfo(level, powerFrequency, toolProfile, classProfile);
        var powerProfile = target.GenerateProfile(powerHighLevelInfo);

        PowerTextBlock power = powerProfile.ToPowerTextBlock();

        Power = power;
    }

}

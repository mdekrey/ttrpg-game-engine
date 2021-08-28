﻿using GameEngine.Generator;
using GameEngine.Rules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        var (configName, level, powerFrequency, powerTemplate) = ("MeleeWeapon", 19, PowerFrequency.Daily, PowerDefinitions.ConditionsPowerTemplateName);

        var target = new PowerGenerator((min, max) => max - 1);

        ToolProfile toolProfile = new(ToolType.Weapon, ToolRange.Melee, PowerSource.Martial, DefenseType.Fortitude, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Normal }.ToImmutableList(), new(ImmutableList<ModifierChance>.Empty, PowerDefinitions.powerTemplates.Keys.ToImmutableList()));

        var powerHighLevelInfo = new PowerHighLevelInfo(level, powerFrequency, toolProfile, ClassRole.Striker);
        var powerProfile = target.GenerateProfile(powerHighLevelInfo, new[] { powerTemplate }.ToImmutableList());

        PowerTextBlock power = powerProfile.ToPowerTextBlock();

        Power = power;
    }

}

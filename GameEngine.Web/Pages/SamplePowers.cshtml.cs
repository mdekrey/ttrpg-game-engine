using GameEngine.Generator;
using GameEngine.Rules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Immutable;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Web.Pages;
public class SamplePowersModel : PageModel
{
    private readonly ILogger<SamplePowerModel> _logger;

    public SamplePowersModel(ILogger<SamplePowerModel> logger)
    {
        _logger = logger;
    }

    public PowerProfiles? PowerProfile { get; private set; }

    public PowerTextBlock[]? AtWill1 { get; private set; }
    public PowerTextBlock[]? Encounter1 { get; private set; }
    public PowerTextBlock[]? Daily1 { get; private set; }
    public PowerTextBlock[]? Encounter3 { get; private set; }
    public PowerTextBlock[]? Daily5 { get; private set; }
    public PowerTextBlock[]? Encounter7 { get; private set; }
    public PowerTextBlock[]? Daily9 { get; private set; }
    public PowerTextBlock[]? Encounter11 { get; private set; }
    public PowerTextBlock[]? Encounter13 { get; private set; }
    public PowerTextBlock[]? Daily15 { get; private set; }
    public PowerTextBlock[]? Encounter17 { get; private set; }
    public PowerTextBlock[]? Daily19 { get; private set; }
    public PowerTextBlock[]? Daily20 { get; private set; }
    public PowerTextBlock[]? Encounter23 { get; private set; }
    public PowerTextBlock[]? Daily25 { get; private set; }
    public PowerTextBlock[]? Encounter27 { get; private set; }
    public PowerTextBlock[]? Daily29 { get; private set; }

    public void OnGet()
    {
        var classProfile = new ClassProfile(
                ClassRole.Striker,
                new ToolProfile[] {
                    new(
                        ToolType.Weapon, ToolRange.Range, PowerSource.Martial,
                        DefenseType.Fortitude,
                        new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                        new[] { DamageType.Normal, DamageType.Fire }.ToImmutableList(),
                        new PowerProfileConfig(
                            ImmutableList<ModifierChance>.Empty.Add(new("$", 1)),
                            Build(new PowerChance("$", 1))
                        )
                    )
                }.ToImmutableList()
            );

        var target = new PowerGenerator(new Random(751).Next);

        var powerProfile = target.GenerateProfiles(classProfile);

        PowerProfile = powerProfile;


        AtWill1 = powerProfile.AtWill1.Select(p => p.ToPowerTextBlock()).ToArray();
        Encounter1 = powerProfile.Encounter1.Select(p => p.ToPowerTextBlock()).ToArray();
        Daily1 = powerProfile.Daily1.Select(p => p.ToPowerTextBlock()).ToArray();
        Encounter3 = powerProfile.Encounter3.Select(p => p.ToPowerTextBlock()).ToArray();
        Daily5 = powerProfile.Daily5.Select(p => p.ToPowerTextBlock()).ToArray();
        Encounter7 = powerProfile.Encounter7.Select(p => p.ToPowerTextBlock()).ToArray();
        Daily9 = powerProfile.Daily9.Select(p => p.ToPowerTextBlock()).ToArray();
        Encounter11 = powerProfile.Encounter11.Select(p => p.ToPowerTextBlock()).ToArray();
        Encounter13 = powerProfile.Encounter13.Select(p => p.ToPowerTextBlock()).ToArray();
        Daily15 = powerProfile.Daily15.Select(p => p.ToPowerTextBlock()).ToArray();
        Encounter17 = powerProfile.Encounter17.Select(p => p.ToPowerTextBlock()).ToArray();
        Daily19 = powerProfile.Daily19.Select(p => p.ToPowerTextBlock()).ToArray();
        Daily20 = powerProfile.Daily20.Select(p => p.ToPowerTextBlock()).ToArray();
        Encounter23 = powerProfile.Encounter23.Select(p => p.ToPowerTextBlock()).ToArray();
        Daily25 = powerProfile.Daily25.Select(p => p.ToPowerTextBlock()).ToArray();
        Encounter27 = powerProfile.Encounter27.Select(p => p.ToPowerTextBlock()).ToArray();
        Daily29 = powerProfile.Daily29.Select(p => p.ToPowerTextBlock()).ToArray();

    }

}

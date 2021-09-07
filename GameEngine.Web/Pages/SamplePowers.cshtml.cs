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
    public PowerTextBlock[]? PowerText { get; private set; }

    public void OnGet()
    {
        var classProfile = new ClassProfile(
                ClassRole.Striker,
                PowerSource.Martial,
                new ToolProfile[] {
                    new(
                        ToolType.Weapon, ToolRange.Range, 
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


        PowerText = powerProfile.Powers.Select(p => p.ToPowerTextBlock()).ToArray();

    }

}

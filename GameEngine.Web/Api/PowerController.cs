using GameEngine.Generator;
using GameEngine.Generator.Text;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Web.Api;

public class PowerController : PowerControllerBase
{
    private readonly PowerGenerator powerGenerator;

    public PowerController(ILogger<PowerGenerator> powerGeneratorLogger)
    {
        this.powerGenerator = new PowerGenerator(new Random().Next, powerGeneratorLogger);
    }

    protected override Task<GenerateSamplePowerActionResult> GenerateSamplePower(Api.PowerHighLevelInfo generateSamplePowerBody)
    {
        if (!ModelState.IsValid) return Task.FromResult(GenerateSamplePowerActionResult.BadRequest(ModelState.ToApiModelErrors()));
        var powerInfo = generateSamplePowerBody.FromApi();
        var powerProfile = powerGenerator.GenerateProfile(powerInfo);

        if (powerProfile == null)
            return Task.FromResult(GenerateSamplePowerActionResult.BadRequest(new() { { "NoPowers", new[] { "Could not generate any powers" } } }));

        var classPowerProfile = new ClassPowerProfile(powerInfo.ToPowerInfo(), powerProfile);
        var powerToken = powerProfile.GetProfileToken();

        var (textBlock, flavor) = classPowerProfile.ToPowerContext().ToPowerTextBlock(FlavorText.Empty);

        return Task.FromResult(GenerateSamplePowerActionResult.Ok(new GenerateSamplePowerResponse(
            Power: textBlock.ToApi(),
            PowerJson: powerToken.ToString()
        )));
    }
}

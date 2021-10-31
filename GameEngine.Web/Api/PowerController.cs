﻿using GameEngine.Generator;
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

    protected override Task<TypeSafeGenerateSamplePowerResult> GenerateSamplePowerTypeSafe(GenerateSamplePowerRequest generateSamplePowerBody)
    {
        if (!ModelState.IsValid) return Task.FromResult(TypeSafeGenerateSamplePowerResult.BadRequest(ModelState.ToApiModelErrors()));
        var classProfile = generateSamplePowerBody.ClassProfile.FromApi();
        var toolProfile = classProfile.Tools[generateSamplePowerBody.ToolIndex];
        var powerProfileConfig = toolProfile.PowerProfileConfigs[generateSamplePowerBody.PowerProfileIndex];
        var powerProfile = powerGenerator.GenerateProfile(new PowerHighLevelInfo(
            Level: generateSamplePowerBody.Level, 
            Usage: generateSamplePowerBody.Usage.FromApi(),
            ClassProfile: classProfile,
            ToolProfile: toolProfile,
            PowerProfileConfig: powerProfileConfig));

        var classPowerProfile = new ClassPowerProfile(generateSamplePowerBody.Level, powerProfile);
        var (powerToken, modTokens) = powerProfile.GetProfileTokens();

        return Task.FromResult(TypeSafeGenerateSamplePowerResult.Ok(new GenerateSamplePowerResponse(
            Power: classPowerProfile.ToPowerTextBlock().ToApi(),
            PowerJson: powerToken.ToString(),
            ModifierJson: modTokens.Select(token => token.ToString())
        )));
    }
}
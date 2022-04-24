using GameEngine.Generator;
using GameEngine.Generator.Context;
using GameEngine.Generator.Modifiers;
using GameEngine.Generator.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Web.Api;

public class PowerGenerationController : PowerGenerationControllerBase
{
    private readonly PowerGenerator powerGenerator;
    private readonly JsonSerializer serializer;

    public PowerGenerationController(ILogger<PowerGenerator> powerGeneratorLogger, IOptions<GameEngine.Web.Storage.GameStorageOptions> options)
    {
        this.powerGenerator = new PowerGenerator(new Random().Next, powerGeneratorLogger);
        this.serializer = options.Value.CreateJsonSerializer();

    }

    protected override async Task<BeginPowerGenerationActionResult> BeginPowerGeneration(PowerHighLevelInfo beginPowerGenerationBody)
    {
        await Task.Yield();
        if (!ModelState.IsValid) return BeginPowerGenerationActionResult.BadRequest(ModelState.ToApiModelErrors());

        var state = powerGenerator.GetInitialBuildState(beginPowerGenerationBody.FromApi());

        var initialOptions = new[] { state.PowerProfile }.PreApply(state.BuildContext).ToArray();
        var finalized = Finalize(initialOptions.First(), state.BuildContext);

        var options = initialOptions.Concat(PowerGenerator.GetPossibleUpgrades(state));

        return BeginPowerGenerationActionResult.Ok(ToApi(state, options, finalized));
    }

    protected override async Task<ContinuePowerGenerationActionResult> ContinuePowerGeneration(ContinuePowerGenerationRequest continuePowerGenerationBody)
    {
        await Task.Yield();
        if (!ModelState.IsValid) return ContinuePowerGenerationActionResult.BadRequest(ModelState.ToApiModelErrors());

        Generator.PowerGeneratorState state;
        Generator.PowerProfile profile;

        try
        {
            state = FromApi(continuePowerGenerationBody.State);
            profile = continuePowerGenerationBody.Profile.FromApi(serializer);
        }
        catch (JsonSerializationException)
        {
            return ContinuePowerGenerationActionResult.BadRequest(new());
        }
        var finalized = Finalize(profile, state.BuildContext);
        var result = continuePowerGenerationBody.Advance
            ? PowerGenerator.ResolveState(state, profile)
            : state with { PowerProfile = profile };

        var options = PowerGenerator.GetPossibleUpgrades(result);

        return ContinuePowerGenerationActionResult.Ok(ToApi(result, options, finalized));
    }

    private Api.PowerGeneratorChoices ToApi(Generator.PowerGeneratorState state, IEnumerable<Generator.PowerProfile> options, Generator.PowerProfile finalizedProfile)
    {
        return new PowerGeneratorChoices(ToApi(state), FinalizeToApi(finalizedProfile, originalFlavor: state.FlavorText, buildContext: state.BuildContext), options.Select(profile => FinalizeToApi(profile, originalFlavor: state.FlavorText, buildContext: state.BuildContext)));
    }

    private Api.PowerGeneratorState ToApi(Generator.PowerGeneratorState state)
    {
        return new Api.PowerGeneratorState(state.Iteration, BuildContextToApi(state.BuildContext), ToApi(state.Stage), ToApi(state.PowerProfile), state.FlavorText.ToApi());
    }

    private Api.LimitBuildContext BuildContextToApi(Generator.IBuildContext buildContext)
    {
        return buildContext is Generator.LimitBuildContext lb ? ToApi(lb) : throw new InvalidOperationException();
    }

    private Api.LimitBuildContext ToApi(Generator.LimitBuildContext lb)
    {
        return new Api.LimitBuildContext(lb.PowerInfo.ToApi(), lb.Limits.Initial, lb.Limits.Minimum, lb.Limits.MaxComplexity);
    }

    private Api.UpgradeStage ToApi(Generator.UpgradeStage stage)
    {
        return stage switch
        {
            Generator.UpgradeStage.InitializeAttacks => Api.UpgradeStage.InitializeAttacks,
            Generator.UpgradeStage.Standard => Api.UpgradeStage.Standard,
            Generator.UpgradeStage.Finalize => Api.UpgradeStage.Finalize,
            Generator.UpgradeStage.Finished => Api.UpgradeStage.Finished,
            _ => throw new NotSupportedException(),
        };
    }

    private Api.PowerProfileChoice FinalizeToApi(Generator.PowerProfile profile, FlavorText originalFlavor, IBuildContext buildContext)
    {
        var finished = Finalize(profile, buildContext);
        var (text, flavor) = new PowerContext(finished, buildContext.PowerInfo).ToPowerTextBlock(originalFlavor);

        return ToApi(profile, flavor, text, buildContext.PowerInfo.Level, buildContext.PowerInfo.Usage);
    }

    private static Generator.PowerProfile Finalize(Generator.PowerProfile profile, IBuildContext buildContext)
    {
        var finished = PowerGenerator.AddFinishingTouches(profile, buildContext.PowerInfo);
        return buildContext.Build(finished);
    }

    private Api.PowerProfileChoice ToApi(Generator.PowerProfile profile, FlavorText flavor, Rules.PowerTextBlock text, int level, Rules.PowerFrequency usage)
    {
        return new Api.PowerProfileChoice(Profile: ToApi(profile), Level: level, Usage: usage.ToApi(), FlavorText: flavor.ToApi(), Text: text.ToApi());
    }

    private Api.PowerProfile ToApi(Generator.PowerProfile profile)
    {
        return new PowerProfile(
            Attacks: profile.Attacks.Select(GetObject),
            Modifiers: profile.Modifiers.Select(GetObject),
            Effects: profile.Effects.Select(GetObject)
        );

        Newtonsoft.Json.Linq.JObject GetObject(object o) => Newtonsoft.Json.Linq.JObject.FromObject(o, serializer);
    }

    private Generator.PowerGeneratorState FromApi(Api.PowerGeneratorState state)
    {
        return new Generator.PowerGeneratorState(state.Iteration, state.PowerProfile.FromApi(serializer), FromApi(state.BuildContext), FromApi(state.Stage), state.FlavorText.FromApi());
    }

    private Generator.UpgradeStage FromApi(UpgradeStage stage)
    {
        return stage switch
        {
            Api.UpgradeStage.InitializeAttacks => Generator.UpgradeStage.InitializeAttacks,
            Api.UpgradeStage.Standard => Generator.UpgradeStage.Standard,
            Api.UpgradeStage.Finalize => Generator.UpgradeStage.Finalize,
            Api.UpgradeStage.Finished => Generator.UpgradeStage.Finished,
            _ => throw new NotSupportedException(),
        };
    }

    private Generator.LimitBuildContext FromApi(Api.LimitBuildContext apiModel)
    {
        return new Generator.LimitBuildContext(apiModel.PowerInfo.FromApi(), new PowerLimits(apiModel.Initial, apiModel.Minimum, apiModel.MaxComplexity));
    }
}

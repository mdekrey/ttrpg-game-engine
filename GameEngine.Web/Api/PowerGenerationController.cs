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

    protected override async Task<TypeSafeBeginPowerGenerationResult> BeginPowerGenerationTypeSafe(PowerHighLevelInfo beginPowerGenerationBody)
    {
        await Task.Yield();
        if (!ModelState.IsValid) return TypeSafeBeginPowerGenerationResult.BadRequest(ModelState.ToApiModelErrors());

        var state = powerGenerator.GetInitialBuildState(beginPowerGenerationBody.FromApi());

        var options = PowerGenerator.GetPossibleUpgrades(state);

        return TypeSafeBeginPowerGenerationResult.Ok(ToApi(state, options));
    }

    protected override async Task<TypeSafeContinuePowerGenerationResult> ContinuePowerGenerationTypeSafe(ContinuePowerGenerationRequest continuePowerGenerationBody)
    {
        await Task.Yield();
        if (!ModelState.IsValid) return TypeSafeContinuePowerGenerationResult.BadRequest(ModelState.ToApiModelErrors());

        Generator.PowerGeneratorState state;
        Generator.PowerProfile profile;

        try
        {
            state = FromApi(continuePowerGenerationBody.State);
            profile = FromApi(continuePowerGenerationBody.Profile);
        }
        catch (JsonSerializationException)
        {
            return TypeSafeContinuePowerGenerationResult.BadRequest(new());
        }
        var result = PowerGenerator.ResolveState(state, profile);

        var options = PowerGenerator.GetPossibleUpgrades(result);

        return TypeSafeContinuePowerGenerationResult.Ok(ToApi(result, options));
    }

    private Api.PowerGeneratorChoices ToApi(Generator.PowerGeneratorState state, IEnumerable<Generator.PowerProfile> options)
    {
        return new PowerGeneratorChoices(ToApi(state), options.Select(profile => ToApi(profile, state.BuildContext)));
    }

    private Api.PowerGeneratorState ToApi(Generator.PowerGeneratorState state)
    {
        return new Api.PowerGeneratorState(state.Iteration, BuildContextToApi(state.BuildContext), ToApi(state.Stage), ToApi(state.PowerProfile));
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

    private Api.PowerProfileChoice ToApi(Generator.PowerProfile profile, IBuildContext buildContext)
    {
        var finished = PowerGenerator.AddFinishingTouches(profile, buildContext.PowerInfo);
        finished = buildContext.Build(finished);
        var (text, _) = new PowerContext(finished, buildContext.PowerInfo).ToPowerTextBlock(FlavorText.Empty);

        return new Api.PowerProfileChoice(Profile: ToApi(profile), Level: buildContext.PowerInfo.Level, Usage: buildContext.PowerInfo.Usage.ToApi(), Text: text.ToApi());
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

    private Generator.PowerProfile FromApi(Api.PowerProfile profile)
    {
        return new Generator.PowerProfile(
            Attacks: profile.Attacks.Select(a => ToObject<AttackProfile>(a)).ToImmutableList(),
            Modifiers: profile.Modifiers.Select(a => ToObject<IPowerModifier>(a)).ToImmutableList(),
            Effects: profile.Effects.Select(a => ToObject<TargetEffect>(a)).ToImmutableList()
        );
        T ToObject<T>(Newtonsoft.Json.Linq.JObject o) => o.ToObject<T>(serializer)!;
    }

    private Generator.PowerGeneratorState FromApi(Api.PowerGeneratorState state)
    {
        return new Generator.PowerGeneratorState(state.Iteration, FromApi(state.PowerProfile), FromApi(state.BuildContext), FromApi(state.Stage));
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

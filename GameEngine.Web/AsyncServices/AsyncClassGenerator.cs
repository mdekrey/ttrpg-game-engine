﻿
using GameEngine.Generator;
using GameEngine.Web.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Web.AsyncServices;
public class AsyncClassGenerator
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly GameStorage gameStorage;
    private readonly ILogger<PowerGenerator> powerGeneratorLogger;

    public AsyncClassGenerator(IServiceScopeFactory serviceScopeFactory, Storage.GameStorage gameStorage, ILogger<PowerGenerator> powerGeneratorLogger)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.gameStorage = gameStorage;
        this.powerGeneratorLogger = powerGeneratorLogger;
    }

    internal async Task<Guid> BeginGeneratingNewClass(ClassProfile classProfile, string name)
    {
        var classId = Guid.NewGuid();
        var classDetails = new GeneratedClassDetails(name, classProfile, ImmutableList<NamedPowerProfile>.Empty);

        await gameStorage.SaveAsync<AsyncProcessed<GeneratedClassDetails>>(classId, new(classDetails, InProgress: true, CorrelationToken: Guid.NewGuid())).ConfigureAwait(false);

        AsyncClassGenerationProcess.Initiate(serviceScopeFactory, classId, powerGeneratorLogger);

        return classId;
    }

    internal async Task ResumeGeneratingNewClass(Guid classId)
    {
        if (await gameStorage.UpdateAsync<GeneratedClassDetails>(classId, t => t with { InProgress = true }) is GameStorage.Status<AsyncProcessed<GeneratedClassDetails>>.Success)
            AsyncClassGenerationProcess.Initiate(serviceScopeFactory, classId, powerGeneratorLogger);
    }
}

class AsyncClassGenerationProcess
{
    private readonly Guid classId;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly PowerGenerator powerGenerator;

    public AsyncClassGenerationProcess(Guid classId, IServiceScopeFactory serviceScopeFactory, ILogger<PowerGenerator> powerGeneratorLogger)
    {
        this.classId = classId;
        this.serviceScopeFactory = serviceScopeFactory;
        this.powerGenerator = new PowerGenerator(new Random().Next, powerGeneratorLogger);
    }

    public async Task Run()
    {
        var (_, classProfile, powers) = await GetClassDetails().ConfigureAwait(false);

        var addingTask = Task.CompletedTask;
        var shouldContinue = true;
        while (shouldContinue)
        {
            var result = powers.Items.Select(p => p.Profile).ToImmutableList();
            shouldContinue = false;
            powerGenerator.GeneratePowerProfiles(classProfile, () => result, newPower =>
            {
                shouldContinue = true;
                result = result.Add(newPower);
                addingTask = addingTask.ContinueWith(async t =>
                {
                    await AddAsync(newPower).ConfigureAwait(false);
                }).Unwrap();
            });
            if (shouldContinue)
            {
                await addingTask.ConfigureAwait(false);
                (_, _, powers) = await GetClassDetails().ConfigureAwait(false);
            }
        }
        await addingTask.ConfigureAwait(false);
        await FinishAsync().ConfigureAwait(false);
    }

    private async Task<GeneratedClassDetails> GetClassDetails()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<GameStorage>();

        // TODO - retry
        var classDetails = await storage.LoadAsync<AsyncProcessed<GeneratedClassDetails>>(classId).ConfigureAwait(false);

        return classDetails is GameStorage.Status<AsyncProcessed<GeneratedClassDetails>>.Success { Value: var next }
            ? next.Original
            : throw new InvalidOperationException();
    }

    private async Task<ImmutableList<Generator.NamedPowerProfile>> AddAsync(Generator.ClassPowerProfile powerProfile)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<GameStorage>();
        
        // TODO - retry
        var status = await storage.UpdateAsync<GeneratedClassDetails>(classId, current => current with
        {
            Original = current.Original with
            {
                Powers = current.Original.Powers.Items.Add(new NamedPowerProfile(Guid.NewGuid(), "Unknown", "", powerProfile)),
            }
        }).ConfigureAwait(false);

        return status is GameStorage.Status<AsyncProcessed<GeneratedClassDetails>>.Success { Value: var next }
            ? next.Original.Powers
            : (await storage.LoadAsync<AsyncProcessed<GeneratedClassDetails>>(classId).ConfigureAwait(false)) is GameStorage.Status<AsyncProcessed<GeneratedClassDetails>>.Success { Value: var orig }
                ? orig.Original.Powers
                : throw new InvalidOperationException();
    }

    private async Task FinishAsync()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<GameStorage>();

        // TODO - retry
        await storage.UpdateAsync<GeneratedClassDetails>(classId, current => current with
        {
            InProgress = false,
        }).ConfigureAwait(false);
    }

    internal static async void Initiate(IServiceScopeFactory serviceScopeFactory, Guid classId, ILogger<PowerGenerator> powerGeneratorLogger)
    {
        await Task.Run(async () =>
        {
            var process = new AsyncClassGenerationProcess(classId, serviceScopeFactory, powerGeneratorLogger);
            await process.Run().ConfigureAwait(false);

        }).ConfigureAwait(false);
    }
}
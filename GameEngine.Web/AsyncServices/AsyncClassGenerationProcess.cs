
using GameEngine.Generator;
using GameEngine.Web.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Web.AsyncServices;

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
        var storage = scope.ServiceProvider.GetRequiredService<IGameStorage>();

        // TODO - retry
        var classDetails = await storage.LoadAsync<AsyncProcessed<GeneratedClassDetails>>(classId).ConfigureAwait(false);

        return classDetails is StorageStatus<AsyncProcessed<GeneratedClassDetails>>.Success { Value: var next }
            ? next.Original
            : throw new InvalidOperationException();
    }

    private async Task<ImmutableList<NamedPowerProfile>> AddAsync(Generator.ClassPowerProfile powerProfile)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IGameStorage>();
        
        // TODO - retry
        var status = await storage.UpdateAsync<GeneratedClassDetails>(classId, current => current with
        {
            Original = current.Original with
            {
                Powers = current.Original.Powers.Items.Add(new NamedPowerProfile(Guid.NewGuid(), Generator.Text.FlavorText.Empty, powerProfile)),
            }
        }).ConfigureAwait(false);

        return status is StorageStatus<AsyncProcessed<GeneratedClassDetails>>.Success { Value: var next }
            ? next.Original.Powers
            : (await storage.LoadAsync<AsyncProcessed<GeneratedClassDetails>>(classId).ConfigureAwait(false)) is StorageStatus<AsyncProcessed<GeneratedClassDetails>>.Success { Value: var orig }
                ? orig.Original.Powers
                : throw new InvalidOperationException();
    }

    private async Task FinishAsync()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IGameStorage>();

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
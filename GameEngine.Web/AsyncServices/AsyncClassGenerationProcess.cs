
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
    private static ObjectFactory asyncClassGenerationProcessFactory = ActivatorUtilities.CreateFactory(typeof(AsyncClassGenerationProcess), new System.Type[] { typeof(Guid), });

    private readonly Guid classId;
    private readonly TableKey classTableKey;
    private readonly ITableStorage<ClassDetails> classStorage;
    private readonly ITableStorage<PowerDetails> powerStorage;
    private readonly PowerGenerator powerGenerator;

    public AsyncClassGenerationProcess(Guid classId, ITableStorage<ClassDetails> classStorage, ITableStorage<PowerDetails> powerStorage, ILogger<PowerGenerator> powerGeneratorLogger)
    {
        this.classId = classId;
        this.classTableKey = ClassDetails.ToTableKey(classId);
        this.classStorage = classStorage;
        this.powerStorage = powerStorage;
        this.powerGenerator = new PowerGenerator(new Random().Next, powerGeneratorLogger);

    }

    public async Task Run()
    {
        var details = await GetClassDetails().ConfigureAwait(false);
        if (details.ProgressState != ProgressState.InProgress)
            return;
        var classProfile = details.ClassProfile;

        var addingTask = Task.CompletedTask;
        var shouldContinue = true;
        while (shouldContinue)
        {
            var powers = await LoadPowers();
            var result = powers.Select(p => p.Profile).ToImmutableList();
            shouldContinue = false;
            powerGenerator.GeneratePowerProfiles(classProfile, () => result, newPower =>
            {
                shouldContinue = true;
                result = result.Add(newPower);
                addingTask = addingTask.ContinueWith(async t =>
                {
                    var details = await GetClassDetails().ConfigureAwait(false);
                    if (details.ProgressState != ProgressState.InProgress)
                        return;
                    await AddAsync(newPower).ConfigureAwait(false);
                }).Unwrap();
            });
            await addingTask.ConfigureAwait(false);
        }
        await FinishAsync().ConfigureAwait(false);
    }

    private async Task<ClassDetails> GetClassDetails()
    {
        var classDetails = await classStorage.LoadAsync(classTableKey).ConfigureAwait(false);

        return classDetails is StorageStatus<ClassDetails>.Success { Value: var next }
            ? next
            : throw new InvalidOperationException();
    }

    private async Task<ImmutableList<PowerDetails>> AddAsync(Generator.ClassPowerProfile powerProfile)
    {
        var powerDetails = new PowerDetails(classId, Guid.NewGuid(), Generator.Text.FlavorText.Empty, powerProfile);
        var key = powerDetails.ToTableKey();
        await powerStorage.SaveAsync(key, powerDetails);
        return await LoadPowers();
    }

    private async Task<ImmutableList<PowerDetails>> LoadPowers()
    {
        var partitionKey = classId.ToString();
        return (await powerStorage.Query((key, power) => key.PartitionKey == partitionKey).Select(kvp => kvp.Value).ToArrayAsync()).ToImmutableList();
    }

    private async Task<bool> FinishAsync()
    {
        var loadedClass = await classStorage.LoadAsync(classTableKey);
        if (loadedClass is not StorageStatus<ClassDetails>.Success { Value: var value } || value.ProgressState != ProgressState.InProgress)
            return false;

        await classStorage.SaveAsync(classTableKey, value with { ProgressState = ProgressState.Finished });
        return true;
    }

    internal static async void Initiate(Guid classId, IServiceScopeFactory serviceScopeFactory)
    {
        await Task.Run(async () =>
        {
            using var serviceScope = serviceScopeFactory.CreateScope();
            var process = (AsyncClassGenerationProcess)asyncClassGenerationProcessFactory(serviceScope.ServiceProvider, new object[] { classId });
            await process.Run().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
}
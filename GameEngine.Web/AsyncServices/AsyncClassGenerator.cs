
using GameEngine.Generator;
using GameEngine.Web.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Web.AsyncServices;
public class AsyncClassGenerator
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly GameStorage gameStorage;

    public AsyncClassGenerator(IServiceScopeFactory serviceScopeFactory, Storage.GameStorage gameStorage)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.gameStorage = gameStorage;
    }

    internal async Task<Guid> BeginGeneratingNewClass(ClassProfile classProfile)
    {
        var classId = Guid.NewGuid();
        var classDetails = new GeneratedClassDetails(classProfile, ImmutableList<PowerProfile>.Empty);

        // TODO - save class first
        await gameStorage.SaveAsync<AsyncProcessed<GeneratedClassDetails>>(classId, new(new GeneratedClassDetails(classProfile, ImmutableList<PowerProfile>.Empty), InProgress: true) ).ConfigureAwait(false);

        AsyncClassGenerationProcess.Initiate(serviceScopeFactory, classProfile, classId);


        return classId;
    }
}

class AsyncClassGenerationProcess
{
    private readonly Guid classId;
    private readonly ClassProfile classProfile;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly PowerGenerator powerGenerator;

    public AsyncClassGenerationProcess(Guid classId, ClassProfile classProfile, IServiceScopeFactory serviceScopeFactory)
    {
        this.classId = classId;
        this.classProfile = classProfile;
        this.serviceScopeFactory = serviceScopeFactory;
        this.powerGenerator = new PowerGenerator(new Random().Next);
    }

    public async Task Run()
    {
        var result = ImmutableList<Generator.PowerProfile>.Empty;
        while (powerGenerator.RemainingPowers(result).FirstOrDefault() is (int level and > 0, Rules.PowerFrequency usage))
        {
            var newPower = powerGenerator.AddSinglePowerProfile(result, level: level, usage: usage, classProfile: classProfile);
            result = await AddAsync(newPower).ConfigureAwait(false);
        }

        await FinishAsync().ConfigureAwait(false);
    }

    private async Task<ImmutableList<Generator.PowerProfile>> AddAsync(Generator.PowerProfile powerProfile)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<GameStorage>();
        var current = await storage.LoadAsync<AsyncProcessed<GeneratedClassDetails>>(classId).ConfigureAwait(false);

        var next = current with
        {
            Original = current.Original with
            {
                Powers = current.Original.Powers.Items.Add(powerProfile),
            }
        };
        await storage.SaveAsync(classId, next).ConfigureAwait(false);
        return next.Original.Powers;
    }

    private async Task FinishAsync()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<GameStorage>();
        var current = await storage.LoadAsync<AsyncProcessed<GeneratedClassDetails>>(classId).ConfigureAwait(false);

        var next = current with
        {
            InProgress = false,
        };
        await storage.SaveAsync(classId, next).ConfigureAwait(false);
    }

    internal static async void Initiate(IServiceScopeFactory serviceScopeFactory, ClassProfile classProfile, Guid classId)
    {
        await Task.Run(async () =>
        {
            var process = new AsyncClassGenerationProcess(classId, classProfile, serviceScopeFactory);
            await process.Run().ConfigureAwait(false);

        }).ConfigureAwait(false);
    }
}
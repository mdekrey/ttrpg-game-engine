
using GameEngine.Generator;
using GameEngine.Web.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace GameEngine.Web.AsyncServices;
public class AsyncClassGenerator
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IBlobStorage<AsyncProcessed<GeneratedClassDetails>> gameStorage;
    private readonly ILogger<PowerGenerator> powerGeneratorLogger;

    public AsyncClassGenerator(IServiceScopeFactory serviceScopeFactory, Storage.IBlobStorage<AsyncProcessed<GeneratedClassDetails>> gameStorage, ILogger<PowerGenerator> powerGeneratorLogger)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.gameStorage = gameStorage;
        this.powerGeneratorLogger = powerGeneratorLogger;
    }

    internal async Task<Guid> BeginGeneratingNewClass(ClassProfile classProfile, string name)
    {
        var classId = Guid.NewGuid();
        var classDetails = new GeneratedClassDetails(name, classProfile, ImmutableList<NamedPowerProfile>.Empty);

        await gameStorage.SaveAsync(classId, new(classDetails, InProgress: true, CorrelationToken: Guid.NewGuid())).ConfigureAwait(false);

        AsyncClassGenerationProcess.Initiate(serviceScopeFactory, classId, powerGeneratorLogger);

        return classId;
    }

    internal async Task ResumeGeneratingNewClass(Guid classId)
    {
        if (await gameStorage.UpdateAsync(classId, t => t with { InProgress = true }) is StorageStatus<AsyncProcessed<GeneratedClassDetails>>.Success)
            AsyncClassGenerationProcess.Initiate(serviceScopeFactory, classId, powerGeneratorLogger);
    }
}

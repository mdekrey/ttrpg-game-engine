
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
    private readonly ITableStorage<ClassDetails> classStorage;

    public AsyncClassGenerator(IServiceScopeFactory serviceScopeFactory, ITableStorage<ClassDetails> classStorage)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.classStorage = classStorage;
    }

    internal async Task<Guid> BeginGeneratingNewClass(ClassProfile classProfile, string name, string description)
    {
        var classId = Guid.NewGuid();
        var classDetails = new ClassDetails(name, description, classProfile, ProgressState: ProgressState.InProgress);
        var key = ClassDetails.ToTableKey(classId);

        await classStorage.SaveAsync(key, classDetails).ConfigureAwait(false);

        AsyncClassGenerationProcess.Initiate(classId, serviceScopeFactory);

        return classId;
    }

    internal async Task ResumeGeneratingNewClass(Guid classId)
    {
        var key = ClassDetails.ToTableKey(classId);
        if (await classStorage.UpdateAsync(key, t => t with { ProgressState = ProgressState.InProgress }) is StorageStatus<ClassDetails>.Success)
            AsyncClassGenerationProcess.Initiate(classId, serviceScopeFactory);
    }
}

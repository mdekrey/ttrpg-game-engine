using GameEngine.Generator;
using GameEngine.Web.AsyncServices;
using GameEngine.Web.Storage;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Web.Api;

public class ClassController : ClassControllerBase
{
    private readonly AsyncClassGenerator asyncClassGenerator;
    private readonly ITableStorage<ClassDetails> classStorage;
    private readonly ITableStorage<PowerDetails> powerStorage;
    private readonly JsonSerializer serializer;

    public ClassController(AsyncClassGenerator asyncClassGenerator, ITableStorage<ClassDetails> classStorage, ITableStorage<PowerDetails> powerStorage, IOptions<GameEngine.Web.Storage.GameStorageOptions> options)
    {
        this.asyncClassGenerator = asyncClassGenerator;
        this.classStorage = classStorage;
        this.powerStorage = powerStorage;
        this.serializer = options.Value.CreateJsonSerializer();
    }

    protected override async Task<TypeSafeGeneratePowersResult> GeneratePowersTypeSafe(EditableClassDescriptor generateClassProfileBody)
    {
        if (!ModelState.IsValid) return TypeSafeGeneratePowersResult.BadRequest(ModelState.ToApiModelErrors());

        var classProfile = generateClassProfileBody.FromApi();

        var id = await asyncClassGenerator.BeginGeneratingNewClass(classProfile, generateClassProfileBody.Name, generateClassProfileBody.Description).ConfigureAwait(false);

        return TypeSafeGeneratePowersResult.Ok(new GeneratePowersResponse(id.ToString()));
    }

    protected override async Task<TypeSafeUpdateClassResult> UpdateClassTypeSafe(string id, EditableClassDescriptor updateClassBody)
    {
        if (!Guid.TryParse(id, out var classId))
            return TypeSafeUpdateClassResult.NotFound();
        if (!ModelState.IsValid) return TypeSafeUpdateClassResult.BadRequest(ModelState.ToApiModelErrors());
        var key = ClassDetails.ToTableKey(classId);
        if (await classStorage.LoadAsync(key) is not StorageStatus<ClassDetails>.Success { Value: var classProfile } || classProfile.ProgressState == AsyncServices.ProgressState.Deleted)
            return TypeSafeUpdateClassResult.NotFound();
        if (classProfile.ProgressState == AsyncServices.ProgressState.Locked)
            return TypeSafeUpdateClassResult.Conflict();

        if (await classStorage.UpdateAsync(key, cd => cd with
        {
            Name = updateClassBody.Name,
            Description = updateClassBody.Description,
            ClassProfile = updateClassBody.FromApi(),
        }) is not StorageStatus<ClassDetails>.Success)
            return TypeSafeUpdateClassResult.Conflict();

        return TypeSafeUpdateClassResult.NoContent();
    }

    protected override async Task<TypeSafeGetClassResult> GetClassTypeSafe(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return TypeSafeGetClassResult.NotFound();
        var status = await classStorage.LoadAsync(ClassDetails.ToTableKey(guid)).ConfigureAwait(false);
        var powers = await powerStorage.Query((key, power) => key.PartitionKey == id.ToString()).ToArrayAsync();

        return status is StorageStatus<ClassDetails>.Success { Value: var classDetails } && classDetails.ProgressState != AsyncServices.ProgressState.Deleted
            ? TypeSafeGetClassResult.Ok(new(
                Original: classDetails.ToApi(from v in powers
                                             orderby v.Value.Profile.PowerInfo.Level, v.Value.Profile.PowerInfo.Usage
                                             select v.Value),
                State: classDetails.ProgressState.ToApi()))
            : TypeSafeGetClassResult.NotFound();
    }

    protected override async Task<TypeSafeReplacePowerResult> ReplacePowerTypeSafe(string classStringId, string powerStringId)
    {
        if (!Guid.TryParse(classStringId, out var classId) || !Guid.TryParse(powerStringId, out var powerId))
            return TypeSafeReplacePowerResult.NotFound();

        var status = await classStorage.LoadAsync(ClassDetails.ToTableKey(classId));

        if (status is StorageStatus<ClassDetails>.Success { Value: ClassDetails v })
        {
            if (v.ProgressState == AsyncServices.ProgressState.Locked)
                return TypeSafeReplacePowerResult.Conflict();
            await powerStorage.DeleteAsync(PowerDetails.ToTableKey(classId, powerId));
            if (v.ProgressState == AsyncServices.ProgressState.Finished)
                await asyncClassGenerator.ResumeGeneratingNewClass(classId);
            return TypeSafeReplacePowerResult.Ok();
        }

        return TypeSafeReplacePowerResult.NotFound();
    }

    protected override async Task<TypeSafeReplacePowerWithResult> ReplacePowerWithTypeSafe(string classStringId, string powerStringId, ReplacePowerWithRequest replacePowerWithBody)
    {
        if (!Guid.TryParse(classStringId, out var classId) || !Guid.TryParse(powerStringId, out var powerId))
            return TypeSafeReplacePowerWithResult.NotFound();

        var key = PowerDetails.ToTableKey(classId, powerId);
        var status = await powerStorage.UpdateAsync(key, powerDetails =>
        {
            return powerDetails with
            {
                Profile = new ClassPowerProfile(PowerProfile: replacePowerWithBody.Profile.FromApi(serializer), PowerInfo: replacePowerWithBody.PowerInfo.FromApi().ToPowerInfo()),
                Flavor = replacePowerWithBody.FlavorText.FromApi(),
            };
        });

        return status is StorageStatus<PowerDetails>.Success
            ? TypeSafeReplacePowerWithResult.Ok()
            : TypeSafeReplacePowerWithResult.Conflict();
    }

    protected override async Task<TypeSafeSetPowerFlavorResult> SetPowerFlavorTypeSafe(string classStringId, string powerStringId, Dictionary<string, string> setPowerFlavorBody)
    {
        if (!Guid.TryParse(classStringId, out var classId) || !Guid.TryParse(powerStringId, out var powerId))
            return TypeSafeSetPowerFlavorResult.NotFound();

        var key = PowerDetails.ToTableKey(classId, powerId);
        var status = await powerStorage.UpdateAsync(key, powerDetails =>
        {
            return powerDetails with
            {
                Flavor = setPowerFlavorBody.FromApi(),
            };
        });

        return status is StorageStatus<PowerDetails>.Success
            ? TypeSafeSetPowerFlavorResult.Ok()
            : TypeSafeSetPowerFlavorResult.Conflict();
    }

    protected override async Task<TypeSafeLockClassResult> LockClassTypeSafe(string id)
    {
        if (!Guid.TryParse(id, out var classId))
            return TypeSafeLockClassResult.NotFound();
        var key = ClassDetails.ToTableKey(classId);
        if (await classStorage.LoadAsync(key) is not StorageStatus<ClassDetails>.Success { Value: var classProfile } || classProfile.ProgressState == AsyncServices.ProgressState.Deleted)
            return TypeSafeLockClassResult.NotFound();
        if (classProfile.ProgressState is AsyncServices.ProgressState.Locked)
            return TypeSafeLockClassResult.Conflict();

        if (await classStorage.UpdateAsync(key, cd => cd with
        {
            ProgressState = AsyncServices.ProgressState.Locked,
        }) is not StorageStatus<ClassDetails>.Success)
            return TypeSafeLockClassResult.Conflict();

        return TypeSafeLockClassResult.NoContent();
    }

    protected override async Task<TypeSafeDeleteClassResult> DeleteClassTypeSafe(string id)
    {
        if (!Guid.TryParse(id, out var classId))
            return TypeSafeDeleteClassResult.NotFound();
        var key = ClassDetails.ToTableKey(classId);
        if (await classStorage.LoadAsync(key) is not StorageStatus<ClassDetails>.Success { Value: var classProfile } || classProfile.ProgressState == AsyncServices.ProgressState.Deleted)
            return TypeSafeDeleteClassResult.NotFound();

        if (await classStorage.UpdateAsync(key, cd => cd with
        {
            ProgressState = AsyncServices.ProgressState.Deleted,
        }) is not StorageStatus<ClassDetails>.Success)
            return TypeSafeDeleteClassResult.NotFound();

        return TypeSafeDeleteClassResult.NoContent();
    }
}

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

    protected override async Task<CreateClassActionResult> CreateClass(EditableClassDescriptor createClassBody)
    {
        if (!ModelState.IsValid) return CreateClassActionResult.BadRequest(ModelState.ToApiModelErrors());

        var classProfile = createClassBody.FromApi();

        var classDetails = new ClassDetails(createClassBody.Name, createClassBody.Description, classProfile, ProgressState: AsyncServices.ProgressState.Finished);
        var classId = Guid.NewGuid();
        var key = ClassDetails.ToTableKey(classId);

        await classStorage.SaveAsync(key, classDetails).ConfigureAwait(false);

        return CreateClassActionResult.Ok(new CreateClassResponse(classId.ToString()));
    }

    protected override async Task<GeneratePowersActionResult> GeneratePowers(EditableClassDescriptor generateClassProfileBody)
    {
        if (!ModelState.IsValid) return GeneratePowersActionResult.BadRequest(ModelState.ToApiModelErrors());

        var classProfile = generateClassProfileBody.FromApi();

        var id = await asyncClassGenerator.BeginGeneratingNewClass(classProfile, generateClassProfileBody.Name, generateClassProfileBody.Description).ConfigureAwait(false);

        return GeneratePowersActionResult.Ok(new GeneratePowersResponse(id.ToString()));
    }

    protected override async Task<UpdateClassActionResult> UpdateClass(string id, EditableClassDescriptor updateClassBody)
    {
        if (!Guid.TryParse(id, out var classId))
            return UpdateClassActionResult.NotFound();
        if (!ModelState.IsValid) return UpdateClassActionResult.BadRequest(ModelState.ToApiModelErrors());
        var key = ClassDetails.ToTableKey(classId);
        if (await classStorage.LoadAsync(key) is not StorageStatus<ClassDetails>.Success { Value: var classProfile } || classProfile.ProgressState == AsyncServices.ProgressState.Deleted)
            return UpdateClassActionResult.NotFound();
        if (classProfile.ProgressState == AsyncServices.ProgressState.Locked)
            return UpdateClassActionResult.Conflict();

        if (await classStorage.UpdateAsync(key, cd => cd with
        {
            Name = updateClassBody.Name,
            Description = updateClassBody.Description,
            ClassProfile = updateClassBody.FromApi(),
        }) is not StorageStatus<ClassDetails>.Success)
            return UpdateClassActionResult.Conflict();

        return UpdateClassActionResult.NoContent();
    }

    protected override async Task<GetClassActionResult> GetClass(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return GetClassActionResult.NotFound();
        var status = await classStorage.LoadAsync(ClassDetails.ToTableKey(guid)).ConfigureAwait(false);
        var powers = await powerStorage.Query((key, power) => key.PartitionKey == id.ToString()).ToArrayAsync();

        return status is StorageStatus<ClassDetails>.Success { Value: var classDetails } && classDetails.ProgressState != AsyncServices.ProgressState.Deleted
            ? GetClassActionResult.Ok(new(
                Original: classDetails.ToApi(from v in powers
                                             orderby v.Value.Profile.PowerInfo.Level, v.Value.Profile.PowerInfo.Usage
                                             select v.Value),
                State: classDetails.ProgressState.ToApi()))
            : GetClassActionResult.NotFound();
    }

    protected override async Task<ReplacePowerActionResult> ReplacePower(string classStringId, string powerStringId)
    {
        if (!Guid.TryParse(classStringId, out var classId) || !Guid.TryParse(powerStringId, out var powerId))
            return ReplacePowerActionResult.NotFound();

        var status = await classStorage.LoadAsync(ClassDetails.ToTableKey(classId));

        if (status is StorageStatus<ClassDetails>.Success { Value: ClassDetails v })
        {
            if (v.ProgressState == AsyncServices.ProgressState.Locked)
                return ReplacePowerActionResult.Conflict();
            await powerStorage.DeleteAsync(PowerDetails.ToTableKey(classId, powerId));
            if (v.ProgressState == AsyncServices.ProgressState.Finished)
                await asyncClassGenerator.ResumeGeneratingNewClass(classId);
            return ReplacePowerActionResult.Ok();
        }

        return ReplacePowerActionResult.NotFound();
    }

    protected override async Task<ReplacePowerWithActionResult> ReplacePowerWith(string classStringId, string powerStringId, ReplacePowerWithRequest replacePowerWithBody)
    {
        if (!Guid.TryParse(classStringId, out var classId) || !Guid.TryParse(powerStringId, out var powerId))
            return ReplacePowerWithActionResult.NotFound();

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
            ? ReplacePowerWithActionResult.Ok()
            : ReplacePowerWithActionResult.Conflict();
    }

    protected override async Task<SetPowerFlavorActionResult> SetPowerFlavor(string classStringId, string powerStringId, Dictionary<string, string> setPowerFlavorBody)
    {
        if (!Guid.TryParse(classStringId, out var classId) || !Guid.TryParse(powerStringId, out var powerId))
            return SetPowerFlavorActionResult.NotFound();

        var key = PowerDetails.ToTableKey(classId, powerId);
        var status = await powerStorage.UpdateAsync(key, powerDetails =>
        {
            return powerDetails with
            {
                Flavor = setPowerFlavorBody.FromApi(),
            };
        });

        return status is StorageStatus<PowerDetails>.Success
            ? SetPowerFlavorActionResult.Ok()
            : SetPowerFlavorActionResult.Conflict();
    }

    protected override async Task<LockClassActionResult> LockClass(string id)
    {
        if (!Guid.TryParse(id, out var classId))
            return LockClassActionResult.NotFound();
        var key = ClassDetails.ToTableKey(classId);
        if (await classStorage.LoadAsync(key) is not StorageStatus<ClassDetails>.Success { Value: var classProfile } || classProfile.ProgressState == AsyncServices.ProgressState.Deleted)
            return LockClassActionResult.NotFound();
        if (classProfile.ProgressState is AsyncServices.ProgressState.Locked)
            return LockClassActionResult.Conflict();

        if (await classStorage.UpdateAsync(key, cd => cd with
        {
            ProgressState = AsyncServices.ProgressState.Locked,
        }) is not StorageStatus<ClassDetails>.Success)
            return LockClassActionResult.Conflict();

        return LockClassActionResult.NoContent();
    }

    protected override async Task<DeleteClassActionResult> DeleteClass(string id)
    {
        if (!Guid.TryParse(id, out var classId))
            return DeleteClassActionResult.NotFound();
        var key = ClassDetails.ToTableKey(classId);
        if (await classStorage.LoadAsync(key) is not StorageStatus<ClassDetails>.Success { Value: var classProfile } || classProfile.ProgressState == AsyncServices.ProgressState.Deleted)
            return DeleteClassActionResult.NotFound();

        if (await classStorage.UpdateAsync(key, cd => cd with
        {
            ProgressState = AsyncServices.ProgressState.Deleted,
        }) is not StorageStatus<ClassDetails>.Success)
            return DeleteClassActionResult.NotFound();

        return DeleteClassActionResult.NoContent();
    }
}

using GameEngine.Generator;
using GameEngine.Web.AsyncServices;
using GameEngine.Web.Storage;
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

    public ClassController(AsyncClassGenerator asyncClassGenerator, ITableStorage<ClassDetails> classStorage, ITableStorage<PowerDetails> powerStorage)
    {
        this.asyncClassGenerator = asyncClassGenerator;
        this.classStorage = classStorage;
        this.powerStorage = powerStorage;
    }

    protected override async Task<TypeSafeGeneratePowersResult> GeneratePowersTypeSafe(ClassProfile generateClassProfileBody)
    {
        if (!ModelState.IsValid) return TypeSafeGeneratePowersResult.BadRequest(ModelState.ToApiModelErrors());

        var classProfile = generateClassProfileBody.FromApi();

        var id = await asyncClassGenerator.BeginGeneratingNewClass(classProfile, generateClassProfileBody.Name).ConfigureAwait(false);

        return TypeSafeGeneratePowersResult.Ok(new GeneratePowersResponse(id.ToString()));
    }

    protected override async Task<TypeSafeGetClassResult> GetClassTypeSafe(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return TypeSafeGetClassResult.NotFound();
        var status = await classStorage.LoadAsync(ClassDetails.ToTableKey(guid)).ConfigureAwait(false);
        var powers = await powerStorage.Query((key, power) => key.PartitionKey == id.ToString()).ToArrayAsync();

        return status is StorageStatus<ClassDetails>.Success { Value: var classDetails }
            ? TypeSafeGetClassResult.Ok(new(Original: classDetails.ToApi(powers), InProgress: classDetails.InProgress))
            : TypeSafeGetClassResult.NotFound();
    }

    protected override async Task<TypeSafeReplacePowerResult> ReplacePowerTypeSafe(string classStringId, string powerStringId)
    {
        if (!Guid.TryParse(classStringId, out var classId) || !Guid.TryParse(powerStringId, out var powerId))
            return TypeSafeReplacePowerResult.NotFound();

        await powerStorage.DeleteAsync(PowerDetails.ToTableKey(classId, powerId));
        var status = await classStorage.LoadAsync(ClassDetails.ToTableKey(classId));

        if (status is StorageStatus<ClassDetails>.Success { Value: ClassDetails v })
        {
            if (!v.InProgress)
                await asyncClassGenerator.ResumeGeneratingNewClass(classId);
            return TypeSafeReplacePowerResult.Ok();
        }

        return TypeSafeReplacePowerResult.Conflict();
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
                Flavor = new Generator.Text.FlavorText(
                    Fields: setPowerFlavorBody.ToImmutableDictionary(f => f.Key, f => f.Value)
                ),
            };
        });

        return status is StorageStatus<PowerDetails>.Success
            ? TypeSafeSetPowerFlavorResult.Ok()
            : TypeSafeSetPowerFlavorResult.Conflict();
    }
}

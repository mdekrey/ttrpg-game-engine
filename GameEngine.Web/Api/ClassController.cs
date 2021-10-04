using GameEngine.Generator;
using GameEngine.Web.AsyncServices;
using GameEngine.Web.Storage;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Web.Api;
public class ClassController : ClassControllerBase
{
    private readonly AsyncClassGenerator asyncClassGenerator;
    private readonly GameStorage gameStorage;

    public ClassController(AsyncClassGenerator asyncClassGenerator, Storage.GameStorage gameStorage)
    {
        this.asyncClassGenerator = asyncClassGenerator;
        this.gameStorage = gameStorage;
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
        var status = await gameStorage.LoadAsync<AsyncProcessed<GeneratedClassDetails>>(guid).ConfigureAwait(false);

        return status is GameStorage.Status<AsyncProcessed<GeneratedClassDetails>>.Success { Value: var classDetails }
            ? TypeSafeGetClassResult.Ok(new(Original: classDetails.Original.ToApi(), InProgress: classDetails.InProgress))
            : TypeSafeGetClassResult.NotFound();
    }

    protected override async Task<TypeSafeReplacePowerResult> ReplacePowerTypeSafe(string classStringId, string powerStringId)
    {
        if (!Guid.TryParse(classStringId, out var classId) || !Guid.TryParse(powerStringId, out var powerId))
            return TypeSafeReplacePowerResult.NotFound();
        // TODO - retry
        try
        {
            var status = await gameStorage.UpdateAsync<GeneratedClassDetails>(classId, classDetails =>
            {
                return classDetails with
                {
                    Original = classDetails.Original with
                    {
                        Powers = classDetails.Original.Powers.Items.Where(power => power.Id != powerId).ToImmutableList()
                    }
                };
            });

            if (status is GameStorage.Status<AsyncProcessed<GeneratedClassDetails>>.Success { Value: AsyncProcessed<GeneratedClassDetails> v })
            {
                if (!v.InProgress)
                    await asyncClassGenerator.ResumeGeneratingNewClass(classId);
                return TypeSafeReplacePowerResult.Ok();
            }

            return TypeSafeReplacePowerResult.Conflict();
        }
        catch (InvalidOperationException)
        {
            return TypeSafeReplacePowerResult.NotFound();
        }
    }

    protected override async Task<TypeSafeSetPowerFlavorResult> SetPowerFlavorTypeSafe(string classStringId, string powerStringId, SetPowerFlavorRequest setPowerFlavorBody)
    {
        if (!Guid.TryParse(classStringId, out var classId) || !Guid.TryParse(powerStringId, out var powerId))
            return TypeSafeSetPowerFlavorResult.NotFound();
        // TODO - retry
        try
        {
            var status = await gameStorage.UpdateAsync<GeneratedClassDetails>(classId, classDetails =>
            {
                var index = classDetails.Original.Powers.Items.FindIndex(power => power.Id == powerId);
                if (index < 0)
                    throw new InvalidOperationException();

                return classDetails with
                {
                    Original = classDetails.Original with
                    {
                        Powers = classDetails.Original.Powers.Items.SetItem(index, classDetails.Original.Powers.Items[index] with
                        {
                            Name = setPowerFlavorBody.Name,
                            FlavorText = setPowerFlavorBody.FlavorText,
                        })
                    }
                };
            });

            return status is GameStorage.Status<AsyncProcessed<GeneratedClassDetails>>.Success
                ? TypeSafeSetPowerFlavorResult.Ok()
                : TypeSafeSetPowerFlavorResult.Conflict();
        }
        catch (InvalidOperationException)
        {
            return TypeSafeSetPowerFlavorResult.NotFound();
        }
    }
}

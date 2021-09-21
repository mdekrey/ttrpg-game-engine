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

        var id = await asyncClassGenerator.BeginGeneratingNewClass(classProfile).ConfigureAwait(false);

        return TypeSafeGeneratePowersResult.Ok(new GeneratePowersResponse(id.ToString()));
    }

    protected override async Task<TypeSafeGetClassResult> GetClassTypeSafe(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return TypeSafeGetClassResult.NotFound();
        var classDetails = await gameStorage.LoadAsync<AsyncProcessed<GeneratedClassDetails>>(guid).ConfigureAwait(false);

        return TypeSafeGetClassResult.Ok(new(Original: classDetails.Original.ToApi(), InProgress: classDetails.InProgress));
    }

    protected override async Task<TypeSafeSetPowerFlavorResult> SetPowerFlavorTypeSafe(string id, int index, SetPowerFlavorRequest setPowerFlavorBody)
    {
        if (!Guid.TryParse(id, out var guid) || index < 0)
            return TypeSafeSetPowerFlavorResult.NotFound();
        // TODO - retry and make sure correlation id matches
        var classDetails = await gameStorage.LoadAsync<AsyncProcessed<GeneratedClassDetails>>(guid).ConfigureAwait(false);

        if (index >= classDetails.Original.Powers.Items.Count)
            return TypeSafeSetPowerFlavorResult.NotFound();

        classDetails = classDetails with
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
        await gameStorage.SaveAsync(guid, classDetails).ConfigureAwait(false);
        return TypeSafeSetPowerFlavorResult.Ok();
    }
}

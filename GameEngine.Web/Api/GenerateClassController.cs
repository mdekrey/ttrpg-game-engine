using GameEngine.Generator;
using System.Linq;

namespace GameEngine.Web.Api;
public class GenerateClassController : GenerateClassControllerBase
{
    protected override Task<TypeSafeGeneratePowersResult> GeneratePowersTypeSafe(ClassProfile generateClassProfileBody)
    {
        if (!ModelState.IsValid) return Task.FromResult(TypeSafeGeneratePowersResult.BadRequest(ModelState.ToApiModelErrors()));

        var classProfile = generateClassProfileBody.FromApi();

        var target = new PowerGenerator(new Random().Next);

        var powerProfile = target.GenerateProfiles(classProfile);

        var textBlocks = powerProfile.Powers.Select(p => new Api.PowerTextProfile(Text: p.ToPowerTextBlock().ToApi(), Profile: p.ToApi())).ToArray();


        return Task.FromResult(TypeSafeGeneratePowersResult.Ok(new GenerateClassProfileResponse(textBlocks)));
    }
}

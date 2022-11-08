using System;
using System.Threading.Tasks;

namespace GameEngine.Web.FoundryApi;

public class FoundryActorController : FoundryActorControllerBase
{
    protected override Task<SubmitActorActionResult> SubmitActor(Actor submitActorBody)
    {
        if (!ModelState.IsValid)
            return Task.FromResult(SubmitActorActionResult.BadRequest());
        var id = Guid.NewGuid().ToString();
        if (!GameEngine.Web.Pages.CharacterSheetModel.Characters.TryAdd(id, submitActorBody))
            return Task.FromResult(SubmitActorActionResult.Conflict());
        return Task.FromResult(SubmitActorActionResult.Ok($"/character-sheet/{id}"));
    }
}

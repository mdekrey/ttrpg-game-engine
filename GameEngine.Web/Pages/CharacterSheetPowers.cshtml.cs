using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using PrincipleStudios.OpenApiCodegen.Json.Extensions;

namespace GameEngine.Web.Pages
{
    public class CharacterSheetPowersModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Id { get; set; }

        public IActionResult OnGet()
        {
            if (GetPowersData() == null)
                return Redirect($"/character-sheet/{Id}");

            return Page();
        }

        public Data? GetPowersData()
        {
            return (Id == null || !CharacterSheetModel.Characters.TryGetValue(Id, out var actor)) ? null
                : !actor.Powers.TryGet(out var powers) ? null 
                : new Data(Id, powers);
        }

        public record Data(
            [property: JsonPropertyName("id")] string Id,
            [property: JsonPropertyName("powers")] IEnumerable<FoundryApi.Power> Actor);
    }
}

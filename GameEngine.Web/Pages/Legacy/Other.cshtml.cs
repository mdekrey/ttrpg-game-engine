using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameEngine.Web.Pages.Legacy
{
    public class OtherModel : PageModel
    {
        public string Id => RouteData.Values["id"]!.ToString()!;

        public void OnGet()
        {
        }
    }
}

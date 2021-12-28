using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameEngine.Web.Pages.Class;

public class ViewModel : PageModel
{
    public string ClassId => RouteData.Values["id"]!.ToString()!;

    public void OnGet()
    {
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GameEngine.Web.Pages;
public class ClassSurveyModel : PageModel
{
    private readonly ILogger<ClassSurveyModel> _logger;

    public ClassSurveyModel(ILogger<ClassSurveyModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}

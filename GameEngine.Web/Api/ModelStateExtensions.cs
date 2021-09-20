using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Web.Api;

public static class ModelStateExtensions
{
    public static Dictionary<string, IEnumerable<string>> ToApiModelErrors(this ModelStateDictionary modelState)
    {
        return modelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>());
    }
}
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace GameEngine.Web.Pages.ReactServices;
public static class ReactFrontendExtensions
{
    public static async Task<IHtmlContent> React(this IHtmlHelper htmlHelper, string entrypoint, object? context = null)
    {
        if (htmlHelper is null)
        {
            throw new ArgumentNullException(nameof(htmlHelper));
        }
        if (string.IsNullOrEmpty(entrypoint))
        {
            throw new ArgumentException($"'{nameof(entrypoint)}' cannot be null or empty.", nameof(entrypoint));
        }
        return await htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<ReactFrontendService>().React(htmlHelper, entrypoint, context).ConfigureAwait(false);
    }

    public static void AddReactLibrary(this IHtmlHelper htmlHelper, string entrypoint)
    {
        if (htmlHelper is null)
        {
            throw new ArgumentNullException(nameof(htmlHelper));
        }
        if (string.IsNullOrEmpty(entrypoint))
        {
            throw new ArgumentException($"'{nameof(entrypoint)}' cannot be null or empty.", nameof(entrypoint));
        }

        htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<ReactFrontendService>().AddReactLibrary(htmlHelper, entrypoint);
    }
    public static IHtmlContent RenderHeadScripts(this IHtmlHelper htmlHelper)
    {
        if (htmlHelper is null)
        {
            throw new ArgumentNullException(nameof(htmlHelper));
        }
        return ReactFrontendService.RenderHeadScripts(htmlHelper);
    }

    public static IHtmlContent RenderBodyScripts(this IHtmlHelper htmlHelper)
    {
        if (htmlHelper is null)
        {
            throw new ArgumentNullException(nameof(htmlHelper));
        }
        return ReactFrontendService.RenderBodyScripts(htmlHelper);
    }
}

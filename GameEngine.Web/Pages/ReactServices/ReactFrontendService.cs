using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;

namespace GameEngine.Web.Pages.ReactServices;
public class ReactFrontendService
{
    private readonly IWebHostEnvironment env;
    private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
    {
        ContractResolver = new IgnorePageModelPropertiesContractResolver(new DefaultContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy(),
        })
    };

#if DEBUG
    // Do not cache for dev experience only
    private JObject GetManifest() => LoadManifest();
#else
        private readonly Lazy<JObject> manifest;
        private JObject GetManifest() => manifest.Value;
#endif
    private JObject LoadManifest()
    {
        using var contentStream = env.WebRootFileProvider.GetFileInfo("react-frontend/build/asset-manifest.json").CreateReadStream();
        using var textReader = new System.IO.StreamReader(contentStream);
        using var reader = new Newtonsoft.Json.JsonTextReader(textReader);
        var serializer = new Newtonsoft.Json.JsonSerializer();
        return serializer.Deserialize<JObject>(reader)!;
    }


    public ReactFrontendService(IWebHostEnvironment env)
    {
        this.env = env;
#if !DEBUG
            this.manifest = new Lazy<JObject>(LoadManifest);
#endif
    }


    public async Task<IHtmlContent> React(IHtmlHelper htmlHelper, string entrypoint, object? data = null)
    {
        if (htmlHelper is null)
        {
            throw new ArgumentNullException(nameof(htmlHelper));
        }
        if (string.IsNullOrEmpty(entrypoint))
        {
            throw new ArgumentException($"'{nameof(entrypoint)}' cannot be null or empty.", nameof(entrypoint));
        }

        entrypoint = Regex.Replace(entrypoint, "[^a-zA-Z0-9_]+", "_");
        htmlHelper.AddReactLibrary(entrypoint);

        var reactScripts = GetReactScripts(htmlHelper);
        var id = Guid.NewGuid();

        var context = htmlHelper.ViewContext.HttpContext;
        var props = new Dictionary<string, object?>
            {
                { "data", data }
            };
        foreach (var autoProperty in context.RequestServices.GetServices<IReactAutomaticProperty>())
        {
            props.Add(autoProperty.Key, await autoProperty.GetValue(context).ConfigureAwait(false));
        }
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(props, settings: serializerSettings);

        reactScripts.Add(new ReactScript(Content: $"react_{entrypoint}.default(document.getElementById('{id}'), {json})"));

        return htmlHelper.Raw($@"<div id=""{id}""></div>");
    }

    public void AddReactLibrary(IHtmlHelper htmlHelper, string entrypoint)
    {
        if (htmlHelper is null)
        {
            throw new ArgumentNullException(nameof(htmlHelper));
        }
        if (string.IsNullOrEmpty(entrypoint))
        {
            throw new ArgumentException($"'{nameof(entrypoint)}' cannot be null or empty.", nameof(entrypoint));
        }

        var scripts = GetManifest()["entrypoints"]![entrypoint]?.ToObject<string[]>();
        if (scripts == null)
            throw new InvalidOperationException($"No such React Frontend entrypoint '{entrypoint}'");
        var reactScripts = GetReactScripts(htmlHelper);
        foreach (var script in scripts)
        {
            if (!reactScripts.Any(s => s.Src == script))
                reactScripts.Add(new ReactScript(Src: script));
        }
    }

    public static IHtmlContent RenderHeadScripts(IHtmlHelper htmlHelper)
    {
        if (htmlHelper is null)
        {
            throw new ArgumentNullException(nameof(htmlHelper));
        }

        var reactScripts = GetReactScripts(htmlHelper);
        return htmlHelper.Raw(string.Join("", from script in reactScripts
                                              where script is { Src: string }
                                              select script.ToTag()));
    }

    public static IHtmlContent RenderBodyScripts(IHtmlHelper htmlHelper)
    {
        if (htmlHelper is null)
        {
            throw new ArgumentNullException(nameof(htmlHelper));
        }

        var reactScripts = GetReactScripts(htmlHelper);
        return htmlHelper.Raw(string.Join("", from script in reactScripts
                                              where script is { Content: string }
                                              select script.ToTag()));
    }

    private static List<ReactScript> GetReactScripts(IHtmlHelper htmlHelper)
    {
        const string key = nameof(ReactScript);
        var bag = htmlHelper.ViewContext.HttpContext.Items;
        return (bag[key] as List<ReactScript>) ?? (List<ReactScript>)(bag[key] = new List<ReactScript>());
    }

    record ReactScript(string? Src = null, string? Content = null)
    {
        public string ToTag()
        {
            return this switch
            {
                { Src: string src } when src.EndsWith(".js") => $@"<script src=""{src}""></script>",
                { Src: string src } when src.EndsWith(".css") => $@"<link rel=""stylesheet"" href=""{src}"" />",
                _ => $@"<script type=""text/javascript"">{this.Content}</script>"
            };
        }
    }
}

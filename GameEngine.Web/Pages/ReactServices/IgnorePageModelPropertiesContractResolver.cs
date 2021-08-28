using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Serialization;

namespace GameEngine.Web.Pages.ReactServices;
internal class IgnorePageModelPropertiesContractResolver : IContractResolver
{
    public IContractResolver Original { get; }

    public IgnorePageModelPropertiesContractResolver(IContractResolver original)
    {
        this.Original = original ?? throw new ArgumentNullException(nameof(original));
    }

    public JsonContract ResolveContract(Type type)
    {
        var result = Original.ResolveContract(type);
        if (result is JsonObjectContract objectContract)
        {
            foreach (var property in objectContract.Properties)
            {
                HidePageModelProperty(property);
            }
        }
        return result;
    }

    private static void HidePageModelProperty(JsonProperty property)
    {
        if (property.DeclaringType?.IsAssignableFrom(typeof(PageModel)) ?? true)
        {
            property.ShouldSerialize = i => false;
            property.Ignored = true;
        }
    }
}

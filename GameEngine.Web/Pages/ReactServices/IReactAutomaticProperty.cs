using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GameEngine.Web.Pages.ReactServices;

public interface IReactAutomaticProperty
{
    string Key { get; }
    Task<object?> GetValue(HttpContext context);
}
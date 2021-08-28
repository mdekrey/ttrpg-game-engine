namespace GameEngine.Web.Pages.ReactServices;

public interface IReactAutomaticProperty
{
    string Key { get; }
    Task<object?> GetValue(HttpContext context);
}
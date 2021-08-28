[assembly: Microsoft.AspNetCore.Hosting.HostingStartup(typeof(GameEngine.Web.Pages.ReactServices.ReactHostingStartup))]
namespace GameEngine.Web.Pages.ReactServices;

public class ReactHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ConfigureServices((context, services) => {
            services.AddSingleton<ReactFrontendService>();
        });
    }
}

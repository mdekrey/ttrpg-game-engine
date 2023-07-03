using GameEngine.Web.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

const string MyAllowSpecificOrigins = "Foundry Origins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddOpenApiDndClasses<LegacyController>();
builder.Services.AddTransient<GameEngine.Web.Legacy.LegacyData>();

builder.Services.AddSqlite<GameEngine.Web.RulesDatabase.RulesDbContext>("Data Source=4e.db;Cache=Shared;Mode=ReadOnly;");

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:30000",
                                              "https://drinksdew.forge-vtt.com")
                                                  .AllowAnyHeader()
                                                  .AllowAnyMethod();
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider()
    {
        Mappings = { { ".yaml", "application/x-yaml" } }
    },
});

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapRazorPages();
        endpoints.MapFallback((context) =>
        {
            context.Response.Redirect("/");
            return System.Threading.Tasks.Task.CompletedTask;
        });
    });

app.Run();

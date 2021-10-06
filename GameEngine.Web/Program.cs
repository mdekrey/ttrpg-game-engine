using GameEngine.Generator.Serialization;
using GameEngine.Web.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddOpenApiDndClasses<ClassController>();
builder.Services.AddTransient<GameEngine.Web.AsyncServices.AsyncClassGenerator>();
builder.Services.Configure<GameEngine.Web.Storage.GameStorageOptions>(options =>
{
    options.ApplyJsonSerializerSettings = serializer =>
    {
        foreach (var converter in ProfileSerialization.GetJsonConverters())
            serializer.Converters.Add(converter);
        return serializer;
    };
});
builder.Services.AddSingleton<GameEngine.Web.Storage.GameStorage>();

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

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404)
    {
        context.Response.Redirect("/");
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

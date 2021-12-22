using Azure.Storage.Blobs;
using GameEngine.Generator.Serialization;
using GameEngine.Web.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddOpenApiDndClasses<PowerController, ClassController>();
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

if (builder.Configuration["BlobStorage"] is string blobStorageConfiguration)
{

    builder.Services.AddTransient(sp =>
    {
        var blobContainerClient = new BlobContainerClient(blobStorageConfiguration, "sample-container");
        blobContainerClient.CreateIfNotExists();
        return blobContainerClient;
    });

    builder.Services.AddSingleton<GameEngine.Web.Storage.IGameStorage, GameEngine.Web.Storage.GameBlobStorage>();
}
else
{
    builder.Services.AddSingleton<GameEngine.Web.Storage.IGameStorage, GameEngine.Web.Storage.GameInMemoryStorage>();
}

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
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider()
    {
        Mappings = { { ".yaml", "application/x-yaml" } }
    },
});

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

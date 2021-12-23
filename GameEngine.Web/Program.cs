using Azure.Storage.Blobs;
using GameEngine.Generator.Serialization;
using GameEngine.Web.Api;
using GameEngine.Web.AsyncServices;
using GameEngine.Web.Storage;
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

AddBlobStorage<AsyncProcessed<GeneratedClassDetails>>("sample-container");

void AddBlobStorage<T>(string containerName)
{
    if (builder.Configuration["BlobStorage"] is string connectionString)
    {
        var storageFactory = ActivatorUtilities.CreateFactory(typeof(AzureBlobStorage<T>), new System.Type[] { typeof(BlobContainerClient), });
        builder.Services.AddSingleton<IBlobStorage<T>>(sp => (IBlobStorage<T>)storageFactory(sp, new[] { CreateBlobContainerClient(connectionString, containerName) }));
    }
    else
    {
        builder.Services.AddSingleton<GameEngine.Web.Storage.IBlobStorage<T>, GameEngine.Web.Storage.InMemoryBlobStorage<T>>();
    }
}

BlobContainerClient CreateBlobContainerClient(string storageConfiguration, string containerName)
{
    var blobContainerClient = new BlobContainerClient(storageConfiguration, containerName);
    blobContainerClient.CreateIfNotExists();
    return blobContainerClient;
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

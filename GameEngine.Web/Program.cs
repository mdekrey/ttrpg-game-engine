using Azure.Data.Tables;
using Azure.Storage.Blobs;
using GameEngine.Generator.Serialization;
using GameEngine.Web.Api;
using GameEngine.Web.AsyncServices;
using GameEngine.Web.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

const string MyAllowSpecificOrigins = "Foundry Origins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddOpenApiDndClasses<PowerController, ClassController, PowerGenerationController, LegacyController>();
builder.Services.AddTransient<GameEngine.Web.AsyncServices.AsyncClassGenerator>();
builder.Services.AddTransient<GameEngine.Web.Legacy.LegacyData>();
builder.Services.Configure<GameEngine.Web.Storage.GameStorageOptions>(options =>
{
    options.ApplyJsonSerializerSettings = serializer =>
    {
        foreach (var converter in ProfileSerialization.GetJsonConverters())
            serializer.Converters.Add(converter);
        return serializer;
    };
});

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

AddTableStorage<ClassDetails, ClassDetails.ClassDetailsTableEntity>("classes", ClassDetails.FromTableEntity);
AddTableStorage<PowerDetails, PowerDetails.PowerDetailsTableEntity>("powers", PowerDetails.FromTableEntity);

//void AddBlobStorage<T>(string containerName)
//{
//    if (builder.Configuration["BlobStorage"] is string connectionString)
//    {
//        var storageFactory = ActivatorUtilities.CreateFactory(typeof(AzureBlobStorage<T>), new System.Type[] { typeof(BlobContainerClient), });
//        builder.Services.AddSingleton<IBlobStorage<T>>(sp => (IBlobStorage<T>)storageFactory(sp, new[] { CreateBlobContainerClient(connectionString, containerName) }));
//    }
//    else
//    {
//        builder.Services.AddSingleton<GameEngine.Web.Storage.IBlobStorage<T>, GameEngine.Web.Storage.InMemoryBlobStorage<T>>();
//    }
//}

//BlobContainerClient CreateBlobContainerClient(string storageConfiguration, string containerName)
//{
//    var blobContainerClient = new BlobContainerClient(storageConfiguration, containerName);
//    blobContainerClient.CreateIfNotExists();
//    return blobContainerClient;
//}

void AddTableStorage<T, TTableEntity>(string containerName, AzureTableStorage<T, TTableEntity>.EntityFactory entityFactory)
    where T : class, IStorable<TTableEntity, TableKey>
    where TTableEntity : class, ITableEntity, new()
{
    if (builder.Configuration["BlobStorage"] is string connectionString)
    {
        var storageFactory = ActivatorUtilities.CreateFactory(typeof(AzureTableStorage<T, TTableEntity>), new System.Type[] { typeof(TableClient), typeof(AzureTableStorage<T, TTableEntity>.EntityFactory) });
        builder.Services.AddSingleton<ITableStorage<T>>(sp => (ITableStorage<T>)storageFactory(sp, new object[] { CreateTableClient(connectionString, containerName), entityFactory }));
    }
    else
    {
        builder.Services.AddSingleton<GameEngine.Web.Storage.ITableStorage<T>, GameEngine.Web.Storage.InMemoryTableStorage<T>>();
    }
}
TableClient CreateTableClient(string storageConfiguration, string tableName)
{
    var tableContainerClient = new TableClient(storageConfiguration, tableName);
    tableContainerClient.CreateIfNotExists();
    return tableContainerClient;
}

var app = builder.Build();
GameSerialization.JsonSerializer = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<GameStorageOptions>>().Value.CreateJsonSerializer();

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

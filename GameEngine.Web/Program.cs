using GameEngine.Generator.Serialization;
using GameEngine.Web.Api;

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

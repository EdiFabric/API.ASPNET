using EdiFabric.Api.ASPNET;
using EdiFabric.Native.X12;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ILocalModelsService, LocalModelsService>();

var app = builder.Build();

var serial = app.Configuration["ApiKey"];
if (string.IsNullOrEmpty(serial))
    throw new Exception("No ApiKey configuration in appsettings.json.");

var libraryPath = app.Configuration["LibraryPath"];
EdiFabricX12.Load(string.IsNullOrWhiteSpace(libraryPath) ? null : libraryPath);
EdiFabricX12.SetSerial(serial);

var localModels = app.Services.GetRequiredService<ILocalModelsService>();
var mapPath = Path.Combine(app.Environment.ContentRootPath, "map", "map.json");
if (File.Exists(mapPath))
    localModels.Load(serial, mapPath);
else
    localModels.LoadOnline(serial);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    });
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

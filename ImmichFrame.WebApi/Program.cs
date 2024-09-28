using ImmichFrame.Core.Exceptions;
using ImmichFrame.Core.Helpers;
using ImmichFrame.Core.Interfaces;
using ImmichFrame.Core.Logic;
using ImmichFrame.WebApi.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "Settings.json");

Console.WriteLine(settingsPath);

ServerSettings? serverSettings = null;
ClientSettings? clientSettings = null;

if (File.Exists(settingsPath))
{
    var json = File.ReadAllText(settingsPath);
    JsonDocument doc;
    try
    {
        doc = JsonDocument.Parse(json);
    }
    catch (Exception ex)
    {
        throw new SettingsNotValidException($"Problem with parsing the settings: {ex.Message}", ex);
    }

    serverSettings = JsonSerializer.Deserialize<ServerSettings>(doc);
    clientSettings = JsonSerializer.Deserialize<ClientSettings>(doc);
}

builder.Services.AddSingleton(srv =>
{
    if (serverSettings == null)
        serverSettings = new ServerSettings();

    return new ImmichFrameLogic(serverSettings);
});

builder.Services.AddSingleton<IClientSettings>(srv =>
{
    if (clientSettings == null)
        clientSettings = new ClientSettings();

    return clientSettings;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

if (app.Environment.IsDevelopment())
{
    var root = Directory.GetCurrentDirectory();
    var dotenv = Path.Combine(root, "..", "docker", ".env");

    dotenv = Path.GetFullPath(dotenv);
    DotEnv.Load(dotenv);
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

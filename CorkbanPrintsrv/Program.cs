using System.Text;
using CorkbanPrintsrv.Auth;
using CorkbanPrintsrv.Configuration;
using CorkbanPrintsrv.DTOs;
using CorkbanPrintsrv.Infrastructure;
using CorkbanPrintsrv.Services;
using CorkbanPrintsrv.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets.json", true, true);
builder.Configuration.AddEnvironmentVariables();

// config
builder.Services.Configure<PrinterConfiguration>(builder.Configuration.GetSection(PrinterConfiguration.SectionName));
builder.Services.Configure<QueueConfiguration>(builder.Configuration.GetSection(QueueConfiguration.SectionName));

// initialize the sqlite provider immediately
var queueConfig = builder.Configuration.GetSection(QueueConfiguration.SectionName).Get<QueueConfiguration>();
if (string.IsNullOrWhiteSpace(queueConfig?.FilePath)) throw new ArgumentNullException(nameof(queueConfig.FilePath));
var sqliteProvider = new SqliteProvider(queueConfig);
await sqliteProvider.InitializeAsync();
builder.Services.AddSingleton<ISqliteProvider>(sqliteProvider);

// Add other services to the container.
builder.Services.AddSingleton<IPrinterProvider, PrinterProvider>();
builder.Services.AddScoped<IPrinterService, PrinterService>();
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddAuthentication(ApiKeyAuthenticationSchemeOptions.DefaultScheme)
    .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationSchemeOptions.DefaultScheme, _ => { });
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/print-text",
    [Authorize] async ([FromBody] PrintTextRequest request, IPrinterService printerService) =>
    {
        await printerService.PrintTextAsync(request.Text);
        return Results.Ok();
    }).WithName("PrintText");

app.MapPost("/print-image",
    [Authorize] async ([FromBody] PrintImageRequest request, IPrinterService printerService) =>
    {
        await printerService.PrintImageAsync(request.ImageData);
        return Results.Ok();
    }).WithName("PrintImage");

app.MapGet("/test-db", async (ISqliteProvider sqlite) =>
{
    var testcmd = PrinterCommandBuilder.New().CenterAlign().PrintLine("This is a test, yo").PrintLine("Second line")
        .FeedLines(2).PartialCut().Build();
    Console.WriteLine(string.Join(", ", testcmd));
    
    var stored = await sqlite.CreateItem(testcmd);
    Console.WriteLine(string.Join(", ", stored.Data!));

    try
    {
        var queried = await sqlite.GetItem(stored.Id);
        Console.WriteLine(string.Join(", ", queried.Data!));
    }
    catch (KeyNotFoundException e)
    {
        Console.WriteLine(e.Message);
    }
}).WithName("TestDbInsertion");

app.Run();
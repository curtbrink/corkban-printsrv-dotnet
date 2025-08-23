using CorkbanPrintsrv.Configuration;
using CorkbanPrintsrv.DTOs;
using CorkbanPrintsrv.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// config
builder.Services.Configure<PrinterConfiguration>(builder.Configuration.GetSection(PrinterConfiguration.SectionName));

// Add services to the container.
builder.Services.AddSingleton<IPrinterProvider, PrinterProvider>();
builder.Services.AddSingleton<IPrinterService, PrinterService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/print-text", async ([FromBody] PrintTextRequest request, IPrinterService printerService) =>
{
    await printerService.PrintTextAsync(request.Text);
}).WithName("PrintText");

app.MapPost("/print-image", async ([FromBody] PrintImageRequest request, IPrinterService printerService) =>
{
    await printerService.PrintImageAsync(request.ImageData);
}).WithName("PrintImage");

app.MapGet("/test-image", async (IPrinterService printerService) =>
{
    await printerService.PrintTestImageAsync();
}).WithName("TestImage");
app.Run();

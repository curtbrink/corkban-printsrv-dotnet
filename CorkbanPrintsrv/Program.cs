using CorkbanPrintsrv.Configuration;
using CorkbanPrintsrv.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// config
builder.Services.Configure<PrinterConfiguration>(builder.Configuration.GetSection(PrinterConfiguration.SectionName));

// Add services to the container.
builder.Services.AddSingleton<IPrinterProvider, PrinterProvider>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/test", async (IPrinterProvider printerProvider) =>
{
    await printerProvider.TestPrinter();
}).WithName("GetTest");

app.Run();

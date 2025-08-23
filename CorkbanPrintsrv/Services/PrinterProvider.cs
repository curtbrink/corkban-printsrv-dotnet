using CorkbanPrintsrv.Configuration;
using CorkbanPrintsrv.Utils;
using ESCPOS_NET;
using Microsoft.Extensions.Options;

namespace CorkbanPrintsrv.Services;

public interface IPrinterProvider
{
    Task TestPrinter();
}

public class PrinterProvider : IPrinterProvider
{
    private readonly ImmediateNetworkPrinter _printer;
    
    public PrinterProvider(IOptions<PrinterConfiguration> options)
    {
        var config = options.Value;
        ArgumentException.ThrowIfNullOrWhiteSpace(config.Hostname);
        ArgumentException.ThrowIfNullOrWhiteSpace(config.SecretKey);

        _printer = new ImmediateNetworkPrinter(new ImmediateNetworkPrinterSettings
            { ConnectionString = $"{config.Hostname}:{config.Port}", PrinterName = "CorkbanPrinter" });
    }

    public async Task TestPrinter()
    {
        var command = PrinterCommandBuilder.New()
            .CenterAlign()
            .PrintLine("TEST TEST TEST")
            .FeedLines(2)
            .PrintLine("UWU UWU UWU UWU")
            .Append(GetEndingCommands())
            .Build();

        await _printer.WriteAsync(command);
    }

    private static byte[] GetEndingCommands()
    {
        return PrinterCommandBuilder.New()
            .FeedLines(2)
            .PartialCut()
            .Build();
    }
}
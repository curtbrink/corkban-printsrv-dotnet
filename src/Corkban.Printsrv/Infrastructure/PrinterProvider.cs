using CorkbanPrintsrv.Configuration;
using CorkbanPrintsrv.Utils;
using ESCPOS_NET;
using Microsoft.Extensions.Options;

namespace CorkbanPrintsrv.Infrastructure;

public interface IPrinterProvider
{
    Task PrintAsync(byte[] payload);
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

    public async Task PrintAsync(byte[] payload)
    {
        var wrappedCommand = PrinterCommandBuilder.New()
            .Initialize()
            .Append(payload)
            .Append(GetEndingCommands())
            .Build();

        await _printer.WriteAsync(wrappedCommand);
    }

    private static byte[] GetEndingCommands()
    {
        return PrinterCommandBuilder.New()
            .FeedLines(2)
            .PartialCut()
            .Build();
    }
}
using CorkbanPrintsrv.Utils;

namespace CorkbanPrintsrv.Services;

public interface IPrinterService
{
    Task PrintTextAsync(string text);

    Task PrintImageAsync(string base64Image);

    Task PrintTestImageAsync();
}

public class PrinterService(IPrinterProvider printerProvider) : IPrinterService
{
    public async Task PrintTextAsync(string text)
    {
        var printTextCommand = PrinterCommandBuilder.New()
            .CenterAlign()
            .PrintLine(text)
            .Build();
        await printerProvider.PrintAsync(printTextCommand);
    }

    public async Task PrintImageAsync(string base64Image)
    {
        var imageBytes = Convert.FromBase64String(base64Image);
        var printImageCommand = PrinterCommandBuilder.New()
            .CenterAlign()
            .PrintImage(imageBytes, false)
            .Build();

        await printerProvider.PrintAsync(printImageCommand);
    }

    public async Task PrintTestImageAsync()
    {
        var imageBytes = await File.ReadAllBytesAsync("testimage.png");
        var cmd = PrinterCommandBuilder.New()
            .CenterAlign()
            .PrintImage(imageBytes, true)
            .Build();
        await printerProvider.PrintAsync(cmd);
    }
}
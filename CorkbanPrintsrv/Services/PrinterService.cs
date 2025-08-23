using CorkbanPrintsrv.Utils;

namespace CorkbanPrintsrv.Services;

public interface IPrinterService
{
    Task PrintTextAsync(string text);
    Task PrintImageAsync(string base64Image);
}

public class PrinterService(IPrinterProvider printerProvider, IImageService imageService) : IPrinterService
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
        var image = imageService.ConvertImageToMonoBitmap(base64Image);
        var printImageCommand = PrinterCommandBuilder.New()
            .CenterAlign()
            .PrintImage(image, true, true)
            .Build();

        await printerProvider.PrintAsync(printImageCommand);
    }
}
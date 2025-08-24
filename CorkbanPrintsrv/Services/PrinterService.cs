using CorkbanPrintsrv.Infrastructure;
using CorkbanPrintsrv.Utils;

namespace CorkbanPrintsrv.Services;

public interface IPrinterService
{
    Task PrintTextAsync(string text);
    Task PrintImageAsync(string base64Image);
}

public class PrinterService(IPrinterProvider printerProvider, IImageService imageService, ISqliteProvider sqliteProvider) : IPrinterService
{
    public async Task PrintTextAsync(string text)
    {
        var printTextCommand = PrinterCommandBuilder.New()
            .CenterAlign()
            .PrintLine(text)
            .Build();
        await ExecutePrintJob(printTextCommand);
    }

    public async Task PrintImageAsync(string base64Image)
    {
        var image = imageService.ConvertImageToMonoBitmap(base64Image);
        var printImageCommand = PrinterCommandBuilder.New()
            .CenterAlign()
            .PrintImage(image, true, true)
            .Build();

        await ExecutePrintJob(printImageCommand);
    }

    private async Task ExecutePrintJob(byte[] payload)
    {
        var printJob = await sqliteProvider.CreateItem(payload);

        try
        {
            await printerProvider.PrintAsync(payload);
            await sqliteProvider.CompleteItem(printJob.Id);
        }
        catch (Exception ex)
        {
            await sqliteProvider.UpdateItemMessage(printJob.Id, ex.Message);
        }
    }
}
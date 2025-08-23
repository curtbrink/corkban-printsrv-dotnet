using System.Buffers;
using ImageMagick;

namespace CorkbanPrintsrv.Services;

public interface IImageService
{
    byte[] ConvertImageToMonoBitmap(string base64Image);
}

public class ImageService : IImageService
{
    public byte[] ConvertImageToMonoBitmap(string base64Image)
    {
        var stream = new ReadOnlySequence<byte>(Convert.FromBase64String(base64Image));

        using var imageIn = new MagickImage(stream);
        imageIn.Format = MagickFormat.Bmp;
        imageIn.Depth = 1;
        imageIn.Resize(576, 0);
        imageIn.ColorType = ColorType.Bilevel;

        var output = imageIn.ToByteArray();

        return output;
    }
}
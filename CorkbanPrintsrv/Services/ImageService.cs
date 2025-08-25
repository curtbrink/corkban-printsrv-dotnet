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
        imageIn.Resize(576, 0);

        using var imageOut = new MagickImage(new MagickColor("#FFFFFF"), imageIn.Width, imageIn.Height);
        imageOut.Composite(imageIn, Gravity.Center, CompositeOperator.Over);
        
        imageOut.Format = MagickFormat.Bmp;
        imageOut.Depth = 1;
        imageOut.ColorType = ColorType.Bilevel;

        var output = imageOut.ToByteArray();

        return output;
    }
}
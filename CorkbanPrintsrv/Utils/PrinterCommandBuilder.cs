using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;

namespace CorkbanPrintsrv.Utils;

public class PrinterCommandBuilder
{
    private readonly List<byte[]> _commands;
    private readonly EPSON _epson;

    public static PrinterCommandBuilder New()
    {
        return new PrinterCommandBuilder();
    }
    
    public static PrinterCommandBuilder New(byte[] startingCommands)
    {
        return new PrinterCommandBuilder([startingCommands]);
    }

    private PrinterCommandBuilder() : this(new EPSON(), []) {}

    private PrinterCommandBuilder(List<byte[]> startingCommands) : this(new EPSON(), startingCommands) {}

    private PrinterCommandBuilder(EPSON epson, List<byte[]> commands)
    {
        _epson = epson;
        _commands = commands;
    }
    
    public PrinterCommandBuilder Append(byte[] bytes)
    {
        _commands.Add(bytes);
        return this;
    }

    public byte[] Build()
    {
        return ByteSplicer.Combine(_commands.ToArray());
    }
    
    /*
     * Everything below this line is builder methods for commands that the EPSON class provides.
     */

    public PrinterCommandBuilder PrintLine(string line)
    {
        return Append(_epson.PrintLine(line));
    }

    public PrinterCommandBuilder Print(string line)
    {
        return Append(_epson.Print(line));
    }

    public PrinterCommandBuilder FeedLines(int lineCount)
    {
        return Append(_epson.FeedLines(lineCount));
    }

    public PrinterCommandBuilder FeedLinesReverse(int lineCount)
    {
        return Append(_epson.FeedLinesReverse(lineCount));
    }

    public PrinterCommandBuilder FeedDots(int dotCount)
    {
        return Append(_epson.FeedDots(dotCount));
    }

    public PrinterCommandBuilder ResetLineSpacing()
    {
        return Append(_epson.ResetLineSpacing());
    }

    public PrinterCommandBuilder SetLineSpacingInDots(int dots)
    {
        return Append(_epson.SetLineSpacingInDots(dots));
    }

    public PrinterCommandBuilder Initialize()
    {
        return Append(_epson.Initialize());
    }

    public PrinterCommandBuilder Enable()
    {
        return Append(_epson.Enable());
    }

    public PrinterCommandBuilder Disable()
    {
        return Append(_epson.Disable());
    }

    public PrinterCommandBuilder CashDrawerOpenPin2(int pulseOnTimeMs = 120, int pulseOffTimeMs = 240)
    {
        return Append(_epson.CashDrawerOpenPin2(pulseOnTimeMs, pulseOffTimeMs));
    }

    public PrinterCommandBuilder CashDrawerOpenPin5(int pulseOnTimeMs = 120, int pulseOffTimeMs = 240)
    {
        return Append(_epson.CashDrawerOpenPin5(pulseOnTimeMs, pulseOffTimeMs));
    }

    public PrinterCommandBuilder SetStyles(PrintStyle style)
    {
        return Append(_epson.SetStyles(style));
    }

    public PrinterCommandBuilder LeftAlign()
    {
        return Append(_epson.LeftAlign());
    }

    public PrinterCommandBuilder RightAlign()
    {
        return Append(_epson.RightAlign());
    }

    public PrinterCommandBuilder CenterAlign()
    {
        return Append(_epson.CenterAlign());
    }

    public PrinterCommandBuilder ReverseMode(bool activate)
    {
        return Append(_epson.ReverseMode(activate));
    }

    public PrinterCommandBuilder RightCharacterSpacing(int spaceCount)
    {
        return Append(_epson.RightCharacterSpacing(spaceCount));
    }

    public PrinterCommandBuilder UpsideDownMode(bool activate)
    {
        return Append(_epson.UpsideDownMode(activate));
    }

    public PrinterCommandBuilder CodePage(CodePage codePage)
    {
        return Append(_epson.CodePage(codePage));
    }

    public PrinterCommandBuilder Color(Color color)
    {
        return Append(_epson.Color(color));
    }

    public PrinterCommandBuilder FullCut()
    {
        return Append(_epson.FullCut());
    }

    public PrinterCommandBuilder PartialCut()
    {
        return Append(_epson.PartialCut());
    }

    public PrinterCommandBuilder FullCutAfterFeed(int lineCount)
    {
        return Append(_epson.FullCutAfterFeed(lineCount));
    }

    public PrinterCommandBuilder PartialCutAfterFeed(int lineCount)
    {
        return Append(_epson.PartialCutAfterFeed(lineCount));
    }

    public PrinterCommandBuilder SetImageDensity(bool isHiDpi)
    {
        return Append(_epson.SetImageDensity(isHiDpi));
    }

    public PrinterCommandBuilder BufferImage(byte[] image, int maxWidth, bool isLegacy = false, int color = 1)
    {
        return Append(_epson.BufferImage(image, maxWidth, isLegacy, color));
    }

    public PrinterCommandBuilder WriteImageFromBuffer()
    {
        return Append(_epson.WriteImageFromBuffer());
    }

    public PrinterCommandBuilder PrintImage(byte[] image, bool isHiDpi, bool isLegacy = false, int maxWidth = -1, int color = 1)
    {
        return Append(_epson.PrintImage(image, isHiDpi, isLegacy, maxWidth, color));
    }

    public PrinterCommandBuilder EnableAutomaticStatusBack()
    {
        return Append(_epson.EnableAutomaticStatusBack());
    }

    public PrinterCommandBuilder EnableAutomaticInkStatusBack()
    {
        return Append(_epson.EnableAutomaticInkStatusBack());
    }

    public PrinterCommandBuilder DisableAutomaticStatusBack()
    {
        return Append(_epson.DisableAutomaticStatusBack());
    }

    public PrinterCommandBuilder DisableAutomaticInkStatusBack()
    {
        return Append(_epson.DisableAutomaticInkStatusBack());
    }

    public PrinterCommandBuilder RequestOnlineStatus()
    {
        return Append(_epson.RequestOnlineStatus());
    }

    public PrinterCommandBuilder RequestPaperStatus()
    {
        return Append(_epson.RequestPaperStatus());
    }

    public PrinterCommandBuilder RequestDrawerStatus()
    {
        return Append(_epson.RequestDrawerStatus());
    }

    public PrinterCommandBuilder RequestInkStatus()
    {
        return Append(_epson.RequestInkStatus());
    }

    public PrinterCommandBuilder PrintBarcode(BarcodeType type, string barcode, BarcodeCode code = BarcodeCode.CODE_B)
    {
        return Append(_epson.PrintBarcode(type, barcode, code));
    }

    public PrinterCommandBuilder PrintQrCode(string data, TwoDimensionCodeType type = TwoDimensionCodeType.QRCODE_MODEL2, Size2DCode size = Size2DCode.NORMAL,
        CorrectionLevel2DCode correction = CorrectionLevel2DCode.PERCENT_7)
    {
        return Append(_epson.PrintQRCode(data, type, size, correction));
    }

    public PrinterCommandBuilder Print2DCode(TwoDimensionCodeType type, string data, Size2DCode size = Size2DCode.NORMAL,
        CorrectionLevel2DCode correction = CorrectionLevel2DCode.PERCENT_7)
    {
        return Append(_epson.Print2DCode(type, data, size, correction));
    }

    public PrinterCommandBuilder SetBarcodeHeightInDots(int height)
    {
        return Append(_epson.SetBarcodeHeightInDots(height));
    }

    public PrinterCommandBuilder SetBarWidth(BarWidth width)
    {
        return Append(_epson.SetBarWidth(width));
    }

    public PrinterCommandBuilder SetBarLabelPosition(BarLabelPrintPosition position)
    {
        return Append(_epson.SetBarLabelPosition(position));
    }

    public PrinterCommandBuilder SetBarLabelFontB(bool fontB)
    {
        return Append(_epson.SetBarLabelFontB(fontB));
    }
}
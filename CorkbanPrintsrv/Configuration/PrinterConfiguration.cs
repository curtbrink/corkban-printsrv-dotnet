namespace CorkbanPrintsrv.Configuration;

public class PrinterConfiguration
{
    public const string SectionName = "Printer";

    public required string Hostname { get; init; }

    public required int Port { get; init; }

    public required string SecretKey { get; init; }
}
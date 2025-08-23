namespace CorkbanPrintsrv.Configuration;

public class PrinterConfiguration
{
    public const string SectionName = "Printer";
    
    public required string Hostname { get; set; }
    
    public required int Port { get; set; }
    
    public required string SecretKey { get; set; }
}
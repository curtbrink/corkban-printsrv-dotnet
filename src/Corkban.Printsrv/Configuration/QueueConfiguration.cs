namespace CorkbanPrintsrv.Configuration;

public class QueueConfiguration
{
    public const string SectionName = "Queue";

    public required string FilePath { get; init; }
}
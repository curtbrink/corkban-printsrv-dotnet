namespace Corkban.Printsrv.Configuration;

public class QueueConfiguration
{
    public const string SectionName = "Queue";

    public required string FilePath { get; init; }
}
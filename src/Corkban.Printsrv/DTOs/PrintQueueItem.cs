namespace CorkbanPrintsrv.DTOs;

public class PrintQueueItem
{
    public required string Id { get; set; }

    public required DateTime CreatedTimestamp { get; set; }

    public DateTime? CompletedTimestamp { get; set; }

    public string? Status { get; set; }

    public byte[]? Data { get; set; }
}
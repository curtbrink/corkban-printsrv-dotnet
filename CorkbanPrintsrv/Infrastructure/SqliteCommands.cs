namespace CorkbanPrintsrv.Infrastructure;

public static class SqliteCommands
{
    public const string CreateQueueTable =
        """
        CREATE TABLE IF NOT EXISTS print_queue (
           id TEXT PRIMARY KEY,
           created_timestamp TEXT NOT NULL,
           completed_timestamp TEXT,
           data BLOB
        );
        """;

    public const string CreateQueueItem =
        """
        INSERT INTO print_queue (id, created_timestamp, completed_timestamp, data)
        VALUES ($id, $createdTimestamp, $completedTimestamp, $data);
        """;

    public const string QueryItemById =
        """
        SELECT *
        FROM print_queue
        WHERE id = $id;
        """;
}
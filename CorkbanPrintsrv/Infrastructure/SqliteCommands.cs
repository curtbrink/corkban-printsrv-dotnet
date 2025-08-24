namespace CorkbanPrintsrv.Infrastructure;

public static class SqliteCommands
{
    /*
     * print_queue table commands
     */
    public const string CreateQueueTable =
        """
        CREATE TABLE IF NOT EXISTS print_queue (
           id TEXT PRIMARY KEY,
           created_timestamp TEXT NOT NULL,
           completed_timestamp TEXT,
           status TEXT,
           data BLOB
        );
        """;

    public const string CreateQueueItem =
        """
        INSERT INTO print_queue (id, created_timestamp, completed_timestamp, status, data)
        VALUES ($id, $createdTimestamp, $completedTimestamp, $status, $data);
        """;

    public const string QueryItemById =
        """
        SELECT *
        FROM print_queue
        WHERE id = $id;
        """;

    public const string CompleteItemById =
        """
        UPDATE print_queue
        SET completed_timestamp = $completedTimestamp
        WHERE id = $id;
        """;

    public const string UpdateMessageById =
        """
        UPDATE print_queue
        SET status = $status
        WHERE id = $id;
        """;

    // public const string QueryIncompleteItemsBetweenTimes =
    //     """
    //     SELECT *
    //     FROM print_queue
    //     WHERE completed_timestamp IS NULL
    //     AND created_timestamp 
    //     """;
    
    /*
     * retry_job_info table commands
     */
    
    public const string CreateRetryJobInfoTable =
        """
        CREATE TABLE IF NOT EXISTS retry_job_info (
           id TEXT PRIMARY KEY,
           last_run TEXT
        );
        """;

    public const string QueryRetryJobInfoLastRun =
        """
        SELECT *
        FROM retry_job_info
        WHERE id = $id;
        """;

    public const string UpdateRetryJobInfoLastRun =
        """
        INSERT INTO retry_job_info
        VALUES ($id, $lastRun)
        ON CONFLICT(id) DO UPDATE SET last_run = excluded.last_run;
        """;
}
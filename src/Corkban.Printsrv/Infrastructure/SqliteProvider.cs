using CorkbanPrintsrv.Configuration;
using CorkbanPrintsrv.DTOs;
using CorkbanPrintsrv.Utils;
using Microsoft.Data.Sqlite;

namespace CorkbanPrintsrv.Infrastructure;

public interface ISqliteProvider
{
    Task<PrintQueueItem> CreateItem(byte[] data);

    Task<PrintQueueItem> GetItem(string itemId);

    Task CompleteItem(string itemId);

    Task UpdateItemMessage(string itemId, string statusMessage);

    Task<List<PrintQueueItem>> QueryIncompleteItemsBetween(DateTime startDate, DateTime endDate);

    Task<DateTime?> GetRetryJobLastRun(string jobId);

    Task UpdateRetryJobLastRun(string jobId, DateTime lastRun);
}

public class SqliteProvider(QueueConfiguration queueConfig) : ISqliteProvider
{
    private const int ChunkSize = 1024;
    private readonly string _connectionString = $"DataSource={queueConfig.FilePath}";
    private readonly AsyncLock _lock = new();

    public async Task<PrintQueueItem> CreateItem(byte[] data)
    {
        var item = new PrintQueueItem
        {
            Id = Guid.NewGuid().ToString(),
            CreatedTimestamp = DateTime.UtcNow,
            Data = data
        };

        var command = new SqliteCommand(SqliteCommands.CreateQueueItem);
        command.Parameters.AddWithValue("$id", item.Id);
        command.Parameters.AddWithValue("$createdTimestamp", item.CreatedTimestamp);
        command.Parameters.AddWithValue("$completedTimestamp", DBNull.Value);
        command.Parameters.AddWithValue("$status", DBNull.Value);
        command.Parameters.AddWithValue("$data", item.Data);

        using (await _lock.LockAsync())
        {
            await ExecuteNonQueryAsync(command);
        }

        return item;
    }

    public async Task<PrintQueueItem> GetItem(string itemId)
    {
        var command = new SqliteCommand(SqliteCommands.QueryItemById);
        command.Parameters.AddWithValue("$id", itemId);

        List<PrintQueueItem> results;
        using (await _lock.LockAsync())
        {
            results = await ExecuteQueueItemQueryAsync(command);
        }

        return results.Count > 0 ? results[0] : throw new KeyNotFoundException($"Item with id {itemId} not found");
    }

    public async Task CompleteItem(string itemId)
    {
        var command = new SqliteCommand(SqliteCommands.CompleteItemById);
        command.Parameters.AddWithValue("$id", itemId);
        command.Parameters.AddWithValue("$completedTimestamp", DateTime.UtcNow);

        await ExecuteNonQueryAsync(command);
    }

    public async Task UpdateItemMessage(string itemId, string statusMessage)
    {
        var command = new SqliteCommand(SqliteCommands.UpdateMessageById);
        command.Parameters.AddWithValue("$id", itemId);
        command.Parameters.AddWithValue("$status", statusMessage);

        await ExecuteNonQueryAsync(command);
    }

    public async Task<List<PrintQueueItem>> QueryIncompleteItemsBetween(DateTime startDate, DateTime endDate)
    {
        var command = new SqliteCommand(SqliteCommands.QueryIncompleteItemsBetweenDates);
        command.Parameters.AddWithValue("$startDate", startDate);
        command.Parameters.AddWithValue("$endDate", endDate);

        return await ExecuteQueueItemQueryAsync(command);
    }

    public async Task<DateTime?> GetRetryJobLastRun(string jobId)
    {
        var command = new SqliteCommand(SqliteCommands.QueryRetryJobInfoLastRun);
        command.Parameters.AddWithValue("$id", jobId);

        return await ExecuteRetryQueryAsync(command);
    }

    public async Task UpdateRetryJobLastRun(string jobId, DateTime lastRun)
    {
        var command = new SqliteCommand(SqliteCommands.UpdateRetryJobInfoLastRun);
        command.Parameters.AddWithValue("$id", jobId);
        command.Parameters.AddWithValue("$lastRun", lastRun);

        await ExecuteNonQueryAsync(command);
    }

    public async Task InitializeAsync()
    {
        if (!File.Exists(queueConfig.FilePath))
        {
            File.Create(queueConfig.FilePath);
        }
        var queueCommand = new SqliteCommand(SqliteCommands.CreateQueueTable);
        var retryCommand = new SqliteCommand(SqliteCommands.CreateRetryJobInfoTable);
        using (await _lock.LockAsync())
        {
            await ExecuteNonQueryAsync(queueCommand);
            await ExecuteNonQueryAsync(retryCommand);
        }
    }

    private async Task<SqliteConnection> OpenConnectionAsync()
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private async Task ExecuteNonQueryAsync(SqliteCommand command)
    {
        var conn = await OpenConnectionAsync();
        command.Connection = conn;

        await command.ExecuteNonQueryAsync();

        await conn.DisposeAsync();
        await command.DisposeAsync();
    }

    private async Task<List<PrintQueueItem>> ExecuteQueueItemQueryAsync(SqliteCommand command)
    {
        var conn = await OpenConnectionAsync();
        command.Connection = conn;

        var reader = await command.ExecuteReaderAsync();

        var items = await ParseQueueItemResultsAsync(reader);

        await conn.DisposeAsync();
        await command.DisposeAsync();
        await reader.DisposeAsync();

        return items;
    }

    private async Task<DateTime?> ExecuteRetryQueryAsync(SqliteCommand command)
    {
        var conn = await OpenConnectionAsync();
        command.Connection = conn;

        var reader = await command.ExecuteReaderAsync();

        DateTime? result = null;
        if (await reader.ReadAsync())
            // row exists, grab last run time
            result = reader.GetDateTime(1);

        await conn.DisposeAsync();
        await command.DisposeAsync();
        await reader.DisposeAsync();

        return result;
    }

    private static async Task<List<PrintQueueItem>> ParseQueueItemResultsAsync(SqliteDataReader reader)
    {
        var items = new List<PrintQueueItem>();

        while (await reader.ReadAsync())
            items.Add(new PrintQueueItem
            {
                Id = reader.GetString(0),
                CreatedTimestamp = reader.GetDateTime(1),
                CompletedTimestamp = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                Status = reader.IsDBNull(3) ? null : reader.GetString(3),
                Data = GetBytes(reader, 4)
            });

        return items;
    }

    private static byte[]? GetBytes(SqliteDataReader reader, int columnPosition)
    {
        if (reader.IsDBNull(columnPosition)) return null;

        var buffer = new byte[ChunkSize];
        long bytesRead;
        long byteOffset = 0;

        using var stream = new MemoryStream();
        while ((bytesRead = reader.GetBytes(columnPosition, byteOffset, buffer, 0, buffer.Length)) > 0)
        {
            stream.Write(buffer, 0, (int)bytesRead);
            byteOffset += bytesRead;
        }

        return stream.ToArray();
    }
}
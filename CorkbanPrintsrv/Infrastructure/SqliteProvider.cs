using CorkbanPrintsrv.Configuration;
using CorkbanPrintsrv.DTOs;
using CorkbanPrintsrv.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace CorkbanPrintsrv.Infrastructure;

public interface ISqliteProvider
{
    Task<PrintQueueItem> CreateItem(byte[] data);

    Task<PrintQueueItem> GetItem(string itemId);

    Task CompleteItem(string itemId);
    // Task<List<PrintQueueItem>> GetIncompleteItems();
}

public class SqliteProvider(QueueConfiguration queueConfig) : ISqliteProvider
{
    private readonly string _connectionString = $"DataSource={queueConfig.FilePath}";
    private readonly AsyncLock _lock = new();

    private const int ChunkSize = 1024;

    public async Task InitializeQueueAsync()
    {
        var command = new SqliteCommand(SqliteCommands.CreateQueueTable);
        using (await _lock.LockAsync())
        {
            await ExecuteNonQueryAsync(command);
        }
    }

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
            results = await ExecuteQueryAsync(command);
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

    private async Task<List<PrintQueueItem>> ExecuteQueryAsync(SqliteCommand command)
    {
        var conn = await OpenConnectionAsync();
        command.Connection = conn;
        
        var reader = await command.ExecuteReaderAsync();
        
        var items = await ParseResultsAsync(reader);

        await conn.DisposeAsync();
        await command.DisposeAsync();
        await reader.DisposeAsync();

        return items;
    }

    private static async Task<List<PrintQueueItem>> ParseResultsAsync(SqliteDataReader reader)
    {
        var items = new List<PrintQueueItem>();

        while (await reader.ReadAsync())
        {
            items.Add(new PrintQueueItem
            {
                Id = reader.GetString(0),
                CreatedTimestamp = reader.GetDateTime(1),
                CompletedTimestamp = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                Data = GetBytes(reader, 3)
            });
        }

        return items;
    }

    private static byte[]? GetBytes(SqliteDataReader reader, int columnPosition)
    {
        if (reader.IsDBNull(columnPosition))
        {
            return null;
        }

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
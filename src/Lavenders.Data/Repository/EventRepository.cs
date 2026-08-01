using System.Globalization;
using Microsoft.Data.Sqlite;
using Lavenders.Data.Database;
using Lavenders.Core.Models;

namespace Lavenders.Data.Repository;

public class EventRepository : IEventRepository
{
    private readonly string _connectionString;
    private bool _schemaEnsured;

    public EventRepository() : this(SQLiteConnector.GetConnectionString())
    {
    }

    public EventRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A database connection string is required.", nameof(connectionString));

        _connectionString = connectionString;
    }

    private async Task EnsureSchemaAsync(SqliteConnection connection)
    {
        if (_schemaEnsured) return;

        using var createCommand = connection.CreateCommand();
        createCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS Events (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                StartDateTime TEXT NOT NULL,
                EndDateTime TEXT NOT NULL,
                Description TEXT
            );
            CREATE INDEX IF NOT EXISTS IX_Events_StartEnd ON Events(StartDateTime, EndDateTime);";
        await createCommand.ExecuteNonQueryAsync();

        _schemaEnsured = true;
    }

    public async Task<IReadOnlyList<Event>> GetEventsAsync(DateTime start, DateTime end)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await EnsureSchemaAsync(connection);

        using var command = connection.CreateCommand();
        // Fixed condition to correctly capture overlapping multi-day events
        command.CommandText = @"
            SELECT Id, Title, StartDateTime, EndDateTime, Description
            FROM Events
            WHERE StartDateTime < @end AND EndDateTime >= @start
            ORDER BY StartDateTime;";

        command.Parameters.AddWithValue("@start", start.ToString("O"));
        command.Parameters.AddWithValue("@end", end.ToString("O"));

        var events = new List<Event>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            events.Add(new Event
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1) ?? string.Empty,
                StartDateTime = DateTime.ParseExact(reader.GetString(2), "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                EndDateTime = DateTime.ParseExact(reader.GetString(3), "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
            });
        }
        return events;
    }

    public async Task<int> AddAsync(Event @event)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await EnsureSchemaAsync(connection);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Events (Title, StartDateTime, EndDateTime, Description)
            VALUES (@title, @start, @end, @desc);
            SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("@title", @event.Title);
        command.Parameters.AddWithValue("@start", @event.StartDateTime.ToString("O"));
        command.Parameters.AddWithValue("@end", @event.EndDateTime.ToString("O"));
        command.Parameters.AddWithValue("@desc", @event.Description ?? (object)DBNull.Value);

        var id = await command.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    public async Task UpdateAsync(Event @event)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await EnsureSchemaAsync(connection);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Events
            SET Title = @title,
                StartDateTime = @start,
                EndDateTime = @end,
                Description = @desc
            WHERE Id = @id;";

        command.Parameters.AddWithValue("@id", @event.Id);
        command.Parameters.AddWithValue("@title", @event.Title);
        command.Parameters.AddWithValue("@start", @event.StartDateTime.ToString("O"));
        command.Parameters.AddWithValue("@end", @event.EndDateTime.ToString("O"));
        command.Parameters.AddWithValue("@desc", @event.Description ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await EnsureSchemaAsync(connection);

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Events WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();
    }
}

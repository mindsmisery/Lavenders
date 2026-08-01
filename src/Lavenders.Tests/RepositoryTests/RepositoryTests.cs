using Lavenders.Core.Models;
using Lavenders.Data.Repository;
using Microsoft.Data.Sqlite;

namespace Lavenders.Tests.RepositoryTests;

public sealed class EventRepositoryTests : IDisposable
{
    private readonly SqliteConnection _keepAliveConnection;
    private readonly EventRepository _repository;

    public EventRepositoryTests()
    {
        var databaseName = $"lavenders-tests-{Guid.NewGuid():N}";
        var connectionString = $"Data Source={databaseName};Mode=Memory;Cache=Shared";

        // A shared in-memory SQLite database exists only while this connection is open.
        _keepAliveConnection = new SqliteConnection(connectionString);
        _keepAliveConnection.Open();
        _repository = new EventRepository(connectionString);
    }

    [Fact]
    public async Task AddAsync_CreatesEvent()
    {
        var item = CreateEvent("Uusi tapahtuma");

        item.Id = await _repository.AddAsync(item);
        var stored = await _repository.GetEventsAsync(
            item.StartDateTime.AddMinutes(-1), item.EndDateTime.AddMinutes(1));

        var result = Assert.Single(stored);
        Assert.True(item.Id > 0);
        Assert.Equal(item.Title, result.Title);
        Assert.Equal(item.Description, result.Description);
    }

    [Fact]
    public async Task UpdateAsync_EditsExistingEvent()
    {
        var item = CreateEvent("Alkuperäinen");
        item.Id = await _repository.AddAsync(item);
        item.Title = "Muokattu";
        item.Description = "Päivitetty kuvaus";

        await _repository.UpdateAsync(item);
        var stored = await _repository.GetEventsAsync(
            item.StartDateTime.AddMinutes(-1), item.EndDateTime.AddMinutes(1));

        var result = Assert.Single(stored);
        Assert.Equal("Muokattu", result.Title);
        Assert.Equal("Päivitetty kuvaus", result.Description);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEvent()
    {
        var item = CreateEvent("Poistettava");
        item.Id = await _repository.AddAsync(item);

        await _repository.DeleteAsync(item.Id);
        var stored = await _repository.GetEventsAsync(
            item.StartDateTime.AddMinutes(-1), item.EndDateTime.AddMinutes(1));

        Assert.Empty(stored);
    }

    private static Event CreateEvent(string title)
    {
        var start = new DateTime(2026, 8, 3, 9, 0, 0, DateTimeKind.Utc);
        return new Event
        {
            Title = title,
            StartDateTime = start,
            EndDateTime = start.AddHours(1),
            Description = "Testikuvaus"
        };
    }

    public void Dispose() => _keepAliveConnection.Dispose();
}

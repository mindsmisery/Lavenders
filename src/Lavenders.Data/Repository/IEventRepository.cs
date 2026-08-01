using Lavenders.Core.Models;
public interface IEventRepository
{
    Task<int> AddAsync(Event @event);
    Task UpdateAsync(Event @event);
    Task DeleteAsync(int id);

    /// Queries
    Task<IReadOnlyList<Event>> GetEventsAsync(DateTime start, DateTime end);
}
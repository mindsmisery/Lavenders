using Lavenders.Core.Models;

namespace Lavenders.UI.Services;

public interface ICalendarService
{
    Task<int> AddEventAsync(Event @event);
    Task UpdateEventAsync(Event @event);
    Task DeleteEventAsync(int id);
    Task<IReadOnlyList<Event>> GetEventsAsync(DateTime start, DateTime end);
    bool IsWeekend(DateTime localDate);
}

using Lavenders.Core.Extensions;
using Lavenders.Core.Models;
namespace Lavenders.UI.Services;

public class CalendarService : ICalendarService
{
    private readonly IEventRepository _repository;
    public CalendarService(IEventRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Unspecified)
        {
            value = DateTime.SpecifyKind(value, DateTimeKind.Local);
        }

        return value.ToUniversalTime();
    }

    public async Task<int> AddEventAsync(Event @event)
    {
        if (!EventValidator.Validate(@event, out var error))
            throw new InvalidOperationException(error);
        @event.StartDateTime = NormalizeToUtc(@event.StartDateTime);
        @event.EndDateTime = NormalizeToUtc(@event.EndDateTime);

        var id = await _repository.AddAsync(@event);
        @event.Id = id;
        return id;
    }

    public async Task UpdateEventAsync(Event @event)
    {
        if (!EventValidator.Validate(@event, out var error))
            throw new InvalidOperationException(error);

        @event.StartDateTime = NormalizeToUtc(@event.StartDateTime);
        @event.EndDateTime = NormalizeToUtc(@event.EndDateTime);

        await _repository.UpdateAsync(@event);
    }

    public async Task DeleteEventAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<IReadOnlyList<Event>> GetEventsAsync(DateTime start, DateTime end)
    {
        var utcStart = NormalizeToUtc(start);
        var utcEnd = NormalizeToUtc(end);

        var events = await _repository.GetEventsAsync(utcStart, utcEnd);

        return events
            .Select(evt => new Event
            {
                Id = evt.Id,
                Title = evt.Title,
                Description = evt.Description,
                StartDateTime = evt.StartDateTime.Kind == DateTimeKind.Utc ? evt.StartDateTime.ToLocalTime() : evt.StartDateTime,
                EndDateTime = evt.EndDateTime.Kind == DateTimeKind.Utc ? evt.EndDateTime.ToLocalTime() : evt.EndDateTime
            })
            .ToList();
    }

    public bool IsWeekend(DateTime localDate)
    {
        return localDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }
}

namespace Lavenders.UI.Services;

public class WeekNavigationService
{
    private DateTime _currentWeekStart;

    public WeekNavigationService() : this(DateTime.Now)
    {
    }

    public WeekNavigationService(DateTime referenceDate) =>
        _currentWeekStart = GetStartOfWeek(referenceDate);

    private static DateTime GetStartOfWeek(DateTime date)
    {
        var difference = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * difference).Date;
    }

    public DateTime GetRangeWeekStart() => _currentWeekStart;
    public DateTime GetRangeEndExclusive() => _currentWeekStart.AddDays(14);

    public void GoToPreviousWeek() => _currentWeekStart = _currentWeekStart.AddDays(-7);
    public void GoToNextWeek() => _currentWeekStart = _currentWeekStart.AddDays(7);
    public void GoToCurrentWeek() => _currentWeekStart = GetStartOfWeek(DateTime.Now);

    public List<DateTime> GetWeekDates()
    {
        var dates = new List<DateTime>();
        for (int i = 0; i < 14; i++)
        {
            dates.Add(_currentWeekStart.AddDays(i));
        }
        return dates;
    }
}

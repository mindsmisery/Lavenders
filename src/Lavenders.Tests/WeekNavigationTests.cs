using Lavenders.UI.Services;

namespace Lavenders.Tests;

public class WeekNavigationTests
{
    [Fact]
    public void Constructor_StartsOnMondayAndReturnsTwoWeeks()
    {
        var navigation = new WeekNavigationService(new DateTime(2026, 8, 6));

        var dates = navigation.GetWeekDates();

        Assert.Equal(new DateTime(2026, 8, 3), navigation.GetRangeWeekStart());
        Assert.Equal(new DateTime(2026, 8, 17), navigation.GetRangeEndExclusive());
        Assert.Equal(14, dates.Count);
        Assert.Equal(new DateTime(2026, 8, 3), dates[0]);
        Assert.Equal(new DateTime(2026, 8, 16), dates[^1]);
    }

    [Fact]
    public void RangeEnd_IsExclusiveAndIncludesTheSecondSunday()
    {
        var navigation = new WeekNavigationService(new DateTime(2026, 8, 6));
        var secondSundayAtNoon = new DateTime(2026, 8, 16, 12, 0, 0);

        Assert.True(secondSundayAtNoon < navigation.GetRangeEndExclusive());
    }

    [Fact]
    public void PreviousAndNextWeek_MoveBySevenDays()
    {
        var navigation = new WeekNavigationService(new DateTime(2026, 8, 6));

        navigation.GoToNextWeek();
        Assert.Equal(new DateTime(2026, 8, 10), navigation.GetRangeWeekStart());

        navigation.GoToPreviousWeek();
        Assert.Equal(new DateTime(2026, 8, 3), navigation.GetRangeWeekStart());
    }
}

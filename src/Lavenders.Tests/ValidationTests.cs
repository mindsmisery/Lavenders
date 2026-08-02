using Lavenders.Core.Extensions;
using Lavenders.Core.Models;
using Lavenders.UI.ViewModels;

namespace Lavenders.Tests;

public class ValidationTests
{
    [Fact]
    public void EventValidator_RejectsMissingTitle()
    {
        var item = ValidEvent();
        item.Title = "   ";

        var valid = EventValidator.Validate(item, out var error);

        Assert.False(valid);
        Assert.Equal("Title cannot be empty", error);
    }

    [Fact]
    public void EventValidator_RejectsEndBeforeStart()
    {
        var item = ValidEvent();
        item.EndDateTime = item.StartDateTime.AddMinutes(-1);

        var valid = EventValidator.Validate(item, out var error);

        Assert.False(valid);
        Assert.Equal("Start time must be before end time", error);
    }

    [Fact]
    public void EventEditViewModel_ShowsTitleErrorOnlyAfterSaveValidation()
    {
        var viewModel = new EventEditViewModel(new DateTime(2026, 8, 3));

        Assert.Empty(viewModel[nameof(EventEditViewModel.Title)]);
        Assert.False(viewModel.Validate());
        Assert.NotEmpty(viewModel[nameof(EventEditViewModel.Title)]);
    }

    [Fact]
    public void EventEditViewModel_CombinesSelectedDateAndTime()
    {
        var viewModel = new EventEditViewModel(new DateTime(2026, 8, 3))
        {
            Title = "Tapahtuma",
            SelectedTime = "14:35"
        };

        Assert.True(viewModel.Validate());
        Assert.Equal(new DateTime(2026, 8, 3, 14, 35, 0, DateTimeKind.Local), viewModel.StartDateTime);
    }

    [Fact]
    public void EventEditViewModel_UsesSelectedStartAndEndTimes()
    {
        var date = new DateTime(2026, 8, 3);
        var viewModel = new EventEditViewModel(date)
        {
            Title = "Meeting",
            SelectedClockTime = date.AddHours(14).AddMinutes(35),
            SelectedEndClockTime = date.AddHours(16)
        };

        Assert.True(viewModel.Validate());
        var item = viewModel.CreateEvent();
        Assert.Equal(date.AddHours(14).AddMinutes(35).ToUniversalTime(), item.StartDateTime);
        Assert.Equal(date.AddHours(16).ToUniversalTime(), item.EndDateTime);
    }

    [Fact]
    public void EventEditViewModel_RejectsEndTimeBeforeStartTime()
    {
        var date = new DateTime(2026, 8, 3);
        var viewModel = new EventEditViewModel(date)
        {
            Title = "Meeting",
            SelectedClockTime = date.AddHours(14),
            SelectedEndClockTime = date.AddHours(13)
        };

        Assert.False(viewModel.Validate());
    }

    private static Event ValidEvent()
    {
        var start = new DateTime(2026, 8, 3, 9, 0, 0, DateTimeKind.Utc);
        return new Event
        {
            Title = "Kelvollinen",
            StartDateTime = start,
            EndDateTime = start.AddHours(1)
        };
    }
}

using System.Globalization;
using Lavenders.Core.Models;
using Lavenders.UI.Models;
using Lavenders.UI.Services;
using Lavenders.UI.ViewModels;

namespace Lavenders.Tests;

public sealed class SettingsTests : IDisposable
{
    private readonly string _settingsPath = Path.Combine(
        Path.GetTempPath(), $"lavenders-settings-{Guid.NewGuid():N}.json");

    [Fact]
    public void SettingsService_PersistsLanguageAndWeekendPreferenceOutsideAppData()
    {
        var service = new SettingsService(_settingsPath);
        service.Update(false, "en-US", "LavenderDark");

        var reloaded = new SettingsService(_settingsPath);

        Assert.False(reloaded.Current.ShowWeekends);
        Assert.Equal("en-US", reloaded.Current.Language);
        Assert.Equal("LavenderDark", reloaded.Current.Theme);
    }

    [Fact]
    public async Task WeekViewModel_HidesWeekendDaysWhenSettingIsDisabled()
    {
        var settings = new MemorySettingsService(showWeekends: false, "en-US");
        var viewModel = new WeekViewModel(
            new EmptyCalendarService(),
            new WeekNavigationService(new DateTime(2026, 8, 6)),
            new EmptyDialogService(),
            settings,
            new TestLocalizationService("en-US"));

        await viewModel.InitializeViewCommand.ExecuteAsync(null);

        Assert.Equal(5, viewModel.DayColumnCount);
        Assert.Equal(5, viewModel.WeekOneDays.Count);
        Assert.Equal(5, viewModel.WeekTwoDays.Count);
        Assert.DoesNotContain(viewModel.WeekOneDays, day => day.IsWeekend);
    }

    [Fact]
    public void SettingsViewModel_AlwaysOffersBothLanguagesAndThemes()
    {
        var viewModel = new SettingsViewModel(
            new MemorySettingsService(true, "fi-FI") { Current = { Theme = "LavenderDark" } },
            new TestLocalizationService("fi-FI"),
            new TestThemeService());

        Assert.Equal(["fi-FI", "en-US"], viewModel.Languages.Select(option => option.Code));
        Assert.Equal(["LavenderLight", "LavenderDark"], viewModel.Themes.Select(option => option.Id));
    }

    public void Dispose()
    {
        if (File.Exists(_settingsPath)) File.Delete(_settingsPath);
    }

    private sealed class MemorySettingsService(bool showWeekends, string language) : ISettingsService
    {
        public AppSettings Current { get; } = new() { ShowWeekends = showWeekends, Language = language };
        public event EventHandler? SettingsChanged;
        public void Update(bool showWeekendsValue, string languageValue, string theme)
        {
            Current.ShowWeekends = showWeekendsValue;
            Current.Language = languageValue;
            Current.Theme = theme;
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class TestLocalizationService(string language) : ILocalizationService
    {
        public CultureInfo Culture { get; } = CultureInfo.GetCultureInfo(language);
        public string Get(string key) => key == "WeekPrefix" ? "week" : key;
        public void Apply(string selectedLanguage) { }
    }

    private sealed class TestThemeService : IThemeService
    {
        public void Apply(string theme) { }
    }

    private sealed class EmptyCalendarService : ICalendarService
    {
        public Task<int> AddEventAsync(Event item) => Task.FromResult(1);
        public Task UpdateEventAsync(Event item) => Task.CompletedTask;
        public Task DeleteEventAsync(int id) => Task.CompletedTask;
        public Task<IReadOnlyList<Event>> GetEventsAsync(DateTime start, DateTime end) =>
            Task.FromResult<IReadOnlyList<Event>>([]);
        public bool IsWeekend(DateTime localDate) => localDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }

    private sealed class EmptyDialogService : IDialogService
    {
        public Event? ShowEventEditDialog() => null;
        public Event? ShowEventEditDialog(DateTime selectedDate) => null;
        public (Event? Event, bool DeleteRequested) ShowEventEditDialog(Event existingEvent) => (null, false);
    }
}

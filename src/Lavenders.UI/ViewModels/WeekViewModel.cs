using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lavenders.UI.Services;

namespace Lavenders.UI.ViewModels;

public partial class WeekViewModel : ObservableObject
{
    private readonly WeekNavigationService _navigationService;
    private readonly ICalendarService _calendarService;
    private readonly IDialogService _dialogService;
    private readonly ISettingsService _settingsService;
    private readonly ILocalizationService _localization;

    private CancellationTokenSource? _statusCts;

    [ObservableProperty] private string _monthLabel = "";
    [ObservableProperty] private string _isoWeekLabel = "";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _statusTitle = "";
    [ObservableProperty] private bool _hasStatusMessage;
    [ObservableProperty] private bool _isStatusError;
    [ObservableProperty] private bool _isCurrentMonth;
    [ObservableProperty] private int _dayColumnCount = 7;
    private bool _isInitialized;

    public ObservableCollection<DayViewModel> WeekOneDays { get; } = new();
    public ObservableCollection<DayViewModel> WeekTwoDays { get; } = new();

    public WeekViewModel(
        ICalendarService calendarService,
        WeekNavigationService navigationService,
        IDialogService dialogService,
        ISettingsService settingsService,
        ILocalizationService localization)
    {
        _calendarService = calendarService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _settingsService = settingsService;
        _localization = localization;
        DayColumnCount = settingsService.Current.ShowWeekends ? 7 : 5;
        _settingsService.SettingsChanged += SettingsService_SettingsChanged;
    }

    private async void SettingsService_SettingsChanged(object? sender, EventArgs e)
    {
        DayColumnCount = _settingsService.Current.ShowWeekends ? 7 : 5;
        if (_isInitialized) await LoadCurrentWeekEvents();
    }

    private async Task SetTemporaryStatusAsync(
        string title,
        string message,
        int delayInMilliseconds = 3200,
        bool isError = false)
    {
        _statusCts?.Cancel();
        _statusCts = new CancellationTokenSource();
        var token = _statusCts.Token;

        StatusTitle = title;
        StatusMessage = message;
        IsStatusError = isError;
        HasStatusMessage = true;

        try
        {
            await Task.Delay(delayInMilliseconds, token);

            if (!token.IsCancellationRequested)
            {
                HasStatusMessage = false;
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    [RelayCommand]
    public async Task InitializeView()
    {
        if (_isInitialized) return;
        await LoadCurrentWeekEvents(throwOnError: true);
        _isInitialized = true;
    }

    private async Task LoadCurrentWeekEvents(bool throwOnError = false)
    {
        IsLoading = true;

        var allDates = _navigationService.GetWeekDates();
        var start = _navigationService.GetRangeWeekStart();
        var endExclusive = _navigationService.GetRangeEndExclusive();
        var displayEnd = allDates[^1];

        var culture = _localization.Culture;
        int displayMonth = start.Month;
        int displayYear = start.Year;

        IReadOnlyList<Core.Models.Event> events;
        try
        {
            events = (await _calendarService.GetEventsAsync(start, endExclusive)).ToList();
        }
        catch (Exception)
        {
            IsLoading = false;
            if (throwOnError) throw;

            _ = SetTemporaryStatusAsync(
                _localization.Get("LoadFailedTitle"),
                _localization.Get("LoadFailedMessage"),
                4500,
                true);
            return;
        }

        var eventsGroupedByDate = events
            .GroupBy(e => e.StartDateTime.ToLocalTime().Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var tempWeekOne = new List<DayViewModel>();
        var tempWeekTwo = new List<DayViewModel>();

        for (int i = 0; i < 7; i++)
        {
            var targetDate = allDates[i].Date;
            var dayVM = new DayViewModel
            {
                Date = targetDate,
                Header = targetDate.ToString("ddd", culture),
                IsCurrentMonth =
                    targetDate.Month == displayMonth &&
                    targetDate.Year == displayYear
            };

            if (eventsGroupedByDate.TryGetValue(targetDate, out var dayEvents))
            {
                foreach (var ev in dayEvents) dayVM.Events.Add(ev);
            }
            if (_settingsService.Current.ShowWeekends || !dayVM.IsWeekend)
                tempWeekOne.Add(dayVM);
        }

        for (int i = 7; i < 14; i++)
        {
            var targetDate = allDates[i].Date;
            var dayVM = new DayViewModel
            {
                Date = targetDate,
                Header = targetDate.ToString("ddd", culture),
                IsCurrentMonth =
                    targetDate.Month == displayMonth &&
                    targetDate.Year == displayYear
            };

            if (eventsGroupedByDate.TryGetValue(targetDate, out var dayEvents))
            {
                foreach (var ev in dayEvents) dayVM.Events.Add(ev);
            }
            if (_settingsService.Current.ShowWeekends || !dayVM.IsWeekend)
                tempWeekTwo.Add(dayVM);
        }

        void ApplyLoadedDays()
        {
            WeekOneDays.Clear();
            WeekTwoDays.Clear();

            foreach (var day in tempWeekOne) WeekOneDays.Add(day);
            foreach (var day in tempWeekTwo) WeekTwoDays.Add(day);

            if (start.Month == displayEnd.Month)
            {
                MonthLabel = start.ToString("MMMM yyyy", culture).ToLower(culture);
            }
            else
            {
                MonthLabel = $"{start.ToString("MMMM", culture)} – {displayEnd.ToString("MMMM yyyy", culture)}".ToLower(culture);
            }
            var secondWeekStart = start.AddDays(7);
            IsoWeekLabel = $"{_localization.Get("WeekPrefix")} {ISOWeek.GetWeekOfYear(start)} – {ISOWeek.GetWeekOfYear(secondWeekStart)}";
        }

        if (Application.Current?.Dispatcher is { } dispatcher)
            dispatcher.Invoke(ApplyLoadedDays);
        else
            ApplyLoadedDays();

        IsLoading = false;
    }

    [RelayCommand]
    private async Task PreviousWeek()
    {
        _navigationService.GoToPreviousWeek();
        await LoadCurrentWeekEvents();
    }

    [RelayCommand]
    private async Task NextWeek()
    {
        _navigationService.GoToNextWeek();
        await LoadCurrentWeekEvents();
    }

    [RelayCommand]
    private async Task Today()
    {
        _navigationService.GoToCurrentWeek();
        await LoadCurrentWeekEvents();
    }

    [RelayCommand]
    private async Task AddEvent()
    {
        var newEvent = _dialogService.ShowEventEditDialog();

        if (newEvent != null)
        {
            try
            {
                await _calendarService.AddEventAsync(newEvent);
                await LoadCurrentWeekEvents();
                _ = SetTemporaryStatusAsync(_localization.Get("EventAddedTitle"), _localization.Get("EventAddedMessage"));
            }
            catch (Exception)
            {
                _ = SetTemporaryStatusAsync(_localization.Get("SaveFailedTitle"), _localization.Get("SaveFailedMessage"), 4500, true);
            }
        }
    }

    [RelayCommand]
    private async Task AddEventForDate(DateTime date)
    {
        var newEvent = _dialogService.ShowEventEditDialog(date);
        if (newEvent == null) return;

        try
        {
            await _calendarService.AddEventAsync(newEvent);
            await LoadCurrentWeekEvents();
            _ = SetTemporaryStatusAsync(_localization.Get("EventAddedTitle"), _localization.Get("EventAddedMessage"));
        }
        catch (Exception)
        {
            _ = SetTemporaryStatusAsync(_localization.Get("SaveFailedTitle"), _localization.Get("SaveFailedMessage"), 4500, true);
        }
    }

    [RelayCommand]
    private async Task EditEvent(Core.Models.Event targetedEvent)
    {
        if (targetedEvent == null) return;

        var (updatedEvent, deleteRequested) = _dialogService.ShowEventEditDialog(targetedEvent);

        if (deleteRequested)
        {
            try
            {
                await _calendarService.DeleteEventAsync(targetedEvent.Id);
                await LoadCurrentWeekEvents();
                _ = SetTemporaryStatusAsync(_localization.Get("EventDeletedTitle"), _localization.Get("EventDeletedMessage"));
            }
            catch (Exception)
            {
                _ = SetTemporaryStatusAsync(_localization.Get("DeleteFailedTitle"), _localization.Get("DeleteFailedMessage"), 4500, true);
            }
            return;
        }

        if (updatedEvent != null)
        {
            try
            {
                await _calendarService.UpdateEventAsync(updatedEvent);
                await LoadCurrentWeekEvents();
                _ = SetTemporaryStatusAsync(_localization.Get("EventUpdatedTitle"), _localization.Get("EventUpdatedMessage"));
            }
            catch (Exception)
            {
                _ = SetTemporaryStatusAsync(_localization.Get("UpdateFailedTitle"), _localization.Get("UpdateFailedMessage"), 4500, true);
            }
        }
    }

}

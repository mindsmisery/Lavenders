using CommunityToolkit.Mvvm.ComponentModel;
using Lavenders.Core.Models;
using System.Collections.ObjectModel;

namespace Lavenders.UI.ViewModels;

public partial class DayViewModel : ObservableObject
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private bool _isCurrentMonth;

    public bool IsWeekend =>
        Date.DayOfWeek == DayOfWeek.Saturday ||
        Date.DayOfWeek == DayOfWeek.Sunday;

    public bool IsToday =>
        Date.Date == DateTime.Today;

    public ObservableCollection<Event> Events { get; } = new();

    partial void OnDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(IsWeekend));
        OnPropertyChanged(nameof(IsToday));
    }
}
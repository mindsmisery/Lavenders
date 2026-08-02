using CommunityToolkit.Mvvm.ComponentModel;

namespace Lavenders.Core.Models;

/// <summary>
/// Represents a calendar event stored in SQLite database.
/// All date-time values are stored in UTC,
/// but can be displayed and edited in local time.
/// </summary>

public class Event : ObservableObject
{
    private int _Id;
    private string _Title = string.Empty;
    private DateTime _StartDateTime;
    private DateTime _EndDateTime;
    private string _Description = string.Empty;

    public int Id
    {
        get => _Id;
        set => SetProperty(ref _Id, value);
    }

    public string Title
    {
        get => _Title;
        set => SetProperty(ref _Title, value);
    }

    /// <summary>
    /// Date-time in UTC.
    /// </summary>
    public DateTime StartDateTime
    {
        get => _StartDateTime;
        set
        {
            if (SetProperty(ref _StartDateTime, value))
                OnPropertyChanged(nameof(LocalStartDateTime));
        }
    }

    public DateTime LocalStartDateTime => StartDateTime.ToLocalTime();

    public DateTime EndDateTime
    {
        get => _EndDateTime;
        set
        {
            if (SetProperty(ref _EndDateTime, value))
                OnPropertyChanged(nameof(LocalEndDateTime));
        }
    }

    public DateTime LocalEndDateTime => EndDateTime.ToLocalTime();

    public string Description
    {
        get => _Description;
        set => SetProperty(ref _Description, value);
    }

}

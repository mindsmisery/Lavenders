using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lavenders.UI.Services;

namespace Lavenders.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly WeekViewModel _weekViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly IUpdateService _updateService;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty] private bool _isUpdateAvailable;
    [ObservableProperty] private bool _isUpdating;
    [ObservableProperty] private bool _hasUpdateError;
    [ObservableProperty] private string? _availableVersion;

    public MainWindowViewModel(
        WeekViewModel weekViewModel,
        SettingsViewModel settingsViewModel,
        IUpdateService updateService)
    {
        _weekViewModel = weekViewModel;
        _settingsViewModel = settingsViewModel;
        _updateService = updateService;

        // Set default view on startup
        CurrentView = _weekViewModel;
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        var version = await _updateService.CheckForUpdateAsync();
        if (version is null) return;

        AvailableVersion = version;
        IsUpdateAvailable = true;
    }

    [RelayCommand(CanExecute = nameof(CanInstallUpdate))]
    private async Task InstallUpdateAsync()
    {
        IsUpdating = true;
        HasUpdateError = false;
        InstallUpdateCommand.NotifyCanExecuteChanged();
        try
        {
            HasUpdateError = !await _updateService.DownloadAndApplyAsync();
        }
        finally
        {
            IsUpdating = false;
            InstallUpdateCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanInstallUpdate() => IsUpdateAvailable && !IsUpdating;

    [RelayCommand]
    private void Home()
    {
        CurrentView = _weekViewModel;
    }

    [RelayCommand]
    private void Settings()
    {
        CurrentView = _settingsViewModel;
    }
}

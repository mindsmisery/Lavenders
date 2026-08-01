using CommunityToolkit.Mvvm.ComponentModel;
using Lavenders.UI.Services;

namespace Lavenders.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly ILocalizationService _localization;
    private readonly IThemeService _themeService;
    private bool _isLoading;

    public IReadOnlyList<LanguageOption> Languages { get; } =
    [
        new("fi-FI", "Suomi"),
        new("en-US", "English")
    ];

    public IReadOnlyList<ThemeOption> Themes { get; } =
    [
        new("LavenderLight", "Lavender Light"),
        new("LavenderDark", "Lavender Dark")
    ];

    [ObservableProperty] private bool _showWeekends;
    [ObservableProperty] private LanguageOption? _selectedLanguage;
    [ObservableProperty] private ThemeOption? _selectedTheme;

    public SettingsViewModel(
        ISettingsService settings,
        ILocalizationService localization,
        IThemeService themeService)
    {
        _settings = settings;
        _localization = localization;
        _themeService = themeService;
        _isLoading = true;
        ShowWeekends = settings.Current.ShowWeekends;
        SelectedLanguage = Languages.First(option => option.Code == settings.Current.Language);
        SelectedTheme = Themes.First(option => option.Id == settings.Current.Theme);
        _isLoading = false;
    }

    partial void OnShowWeekendsChanged(bool value) => Save();

    partial void OnSelectedLanguageChanged(LanguageOption? value) => Save();

    partial void OnSelectedThemeChanged(ThemeOption? value) => Save();

    private void Save()
    {
        if (_isLoading || SelectedLanguage is null || SelectedTheme is null) return;
        _localization.Apply(SelectedLanguage.Code);
        _themeService.Apply(SelectedTheme.Id);
        _settings.Update(ShowWeekends, SelectedLanguage.Code, SelectedTheme.Id);
    }
}

public sealed record LanguageOption(string Code, string DisplayName);
public sealed record ThemeOption(string Id, string DisplayName);

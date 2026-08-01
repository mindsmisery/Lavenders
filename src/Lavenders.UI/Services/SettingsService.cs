using System.IO;
using System.Text.Json;
using Lavenders.UI.Models;

namespace Lavenders.UI.Services;

public sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _settingsPath;

    public AppSettings Current { get; private set; }
    public event EventHandler? SettingsChanged;

    public SettingsService() : this(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Lavenders",
        "settings.json"))
    {
    }

    public SettingsService(string settingsPath)
    {
        _settingsPath = settingsPath;
        Current = Load();
    }

    public void Update(bool showWeekends, string language, string theme)
    {
        Current.ShowWeekends = showWeekends;
        Current.Language = language is "en-US" ? "en-US" : "fi-FI";
        Current.Theme = theme is "LavenderDark" ? "LavenderDark" : "LavenderLight";

        try
        {
            var directory = Path.GetDirectoryName(_settingsPath)!;
            Directory.CreateDirectory(directory);
            var temporaryPath = _settingsPath + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(Current, JsonOptions));
            File.Move(temporaryPath, _settingsPath, true);
        }
        catch
        {
            // Keep the active in-memory preference even if persistence is unavailable.
        }
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return new AppSettings();
            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath))
                           ?? new AppSettings();
            settings.Language = settings.Language is "en-US" ? "en-US" : "fi-FI";
            settings.Theme = settings.Theme is "LavenderDark" ? "LavenderDark" : "LavenderLight";
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }
}

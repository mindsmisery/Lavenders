using Lavenders.UI.Models;

namespace Lavenders.UI.Services;

public interface ISettingsService
{
    AppSettings Current { get; }
    event EventHandler? SettingsChanged;
    void Update(bool showWeekends, string language, string theme);
}

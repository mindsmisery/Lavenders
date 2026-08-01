using System.Globalization;

namespace Lavenders.UI.Services;

public interface ILocalizationService
{
    CultureInfo Culture { get; }
    string Get(string key);
    void Apply(string language);
}

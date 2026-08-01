using System.Globalization;
using System.Windows;

namespace Lavenders.UI.Services;

public sealed class LocalizationService : ILocalizationService
{
    public CultureInfo Culture { get; private set; } = CultureInfo.GetCultureInfo("fi-FI");

    public string Get(string key) =>
        Application.Current?.TryFindResource(key)?.ToString() ?? key;

    public void Apply(string language)
    {
        var normalizedLanguage = language is "en-US" ? "en-US" : "fi-FI";
        Culture = CultureInfo.GetCultureInfo(normalizedLanguage);
        CultureInfo.DefaultThreadCurrentCulture = Culture;
        CultureInfo.DefaultThreadCurrentUICulture = Culture;
        Thread.CurrentThread.CurrentCulture = Culture;
        Thread.CurrentThread.CurrentUICulture = Culture;

        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var current = dictionaries.FirstOrDefault(dictionary =>
            dictionary.Source?.OriginalString.Contains("Strings.", StringComparison.OrdinalIgnoreCase) == true);
        if (current is not null) dictionaries.Remove(current);

        dictionaries.Add(new ResourceDictionary
        {
            Source = new Uri($"Resources/Strings.{normalizedLanguage}.xaml", UriKind.Relative)
        });
    }
}

using System.Windows;

namespace Lavenders.UI.Views;

public partial class StartupErrorDialog : Window
{
    public string LogMessage { get; }

    public StartupErrorDialog(string logPath)
    {
        var unavailable = Application.Current.TryFindResource("LogUnavailable")?.ToString()
                          ?? "The error log could not be saved.";
        var savedPrefix = Application.Current.TryFindResource("LogSavedPrefix")?.ToString()
                          ?? "Technical details were saved to:";
        LogMessage = string.IsNullOrWhiteSpace(logPath)
            ? unavailable
            : $"{savedPrefix} {logPath}";
        InitializeComponent();
        DataContext = this;
    }

    private void Retry_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void Close_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

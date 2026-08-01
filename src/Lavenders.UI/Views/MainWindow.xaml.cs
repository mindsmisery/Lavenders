using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Lavenders.UI.ViewModels;
using Lavenders.UI.Services;

namespace Lavenders.UI;

public partial class MainWindow : Window
{
    private readonly ISettingsService _settings;

    public MainWindow(MainWindowViewModel viewModel, ISettingsService settings)
    {
        InitializeComponent();
        DataContext = viewModel;
        _settings = settings;
        _settings.SettingsChanged += Settings_SettingsChanged;
        SourceInitialized += MainWindow_SourceInitialized;
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        var windowHandle = new WindowInteropHelper(this).Handle;
        var dark = _settings.Current.Theme == "LavenderDark";
        var lightMode = dark ? 1 : 0;
        var captionColor = dark ? 0x00302125 : 0x00FFFFFF;
        var textColor = dark ? 0x00F9EFF2 : 0x00473539;

        DwmSetWindowAttribute(windowHandle, DwmwaUseImmersiveDarkMode, ref lightMode, sizeof(int));
        DwmSetWindowAttribute(windowHandle, DwmwaCaptionColor, ref captionColor, sizeof(int));
        DwmSetWindowAttribute(windowHandle, DwmwaTextColor, ref textColor, sizeof(int));
    }

    private void Settings_SettingsChanged(object? sender, EventArgs e) => MainWindow_SourceInitialized(this, EventArgs.Empty);

    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaCaptionColor = 35;
    private const int DwmwaTextColor = 36;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        ref int attributeValue,
        int attributeSize);
}

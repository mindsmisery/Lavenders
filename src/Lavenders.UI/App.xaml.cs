using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Lavenders.Data.Repository;
using Lavenders.UI.Services;
using Lavenders.UI.ViewModels;
using Lavenders.UI.Views;
using System.Globalization;
using System.Threading;
using Velopack;

namespace Lavenders.UI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    [STAThread]
    public static void Main()
    {
        VelopackApp.Build().Run();
        var application = new App();
        application.InitializeComponent();
        application.Run();
    }

    public App()
    {
        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // 1. Data and Infrastructure Services (Transient ensures thread isolation)
        services.AddTransient<IEventRepository, EventRepository>();
        services.AddTransient<ICalendarService, CalendarService>();
        services.AddSingleton<WeekNavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IUpdateService, UpdateService>();

        // 2. ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<WeekViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // 3. UI Windows
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        var settings = Services.GetRequiredService<ISettingsService>();
        Services.GetRequiredService<ILocalizationService>().Apply(settings.Current.Language);
        Services.GetRequiredService<IThemeService>().Apply(settings.Current.Theme);

        base.OnStartup(e);

        while (true)
        {
            SplashWindow? splash = null;
            try
            {
                splash = new SplashWindow();
                splash.Show();

                var minimumDisplay = Task.Delay(850);
                var mainWindow = Services.GetRequiredService<MainWindow>();
                var mainViewModel = (MainWindowViewModel)mainWindow.DataContext;
                if (mainViewModel.CurrentView is WeekViewModel weekViewModel)
                    await weekViewModel.InitializeViewCommand.ExecuteAsync(null);
                await minimumDisplay;

                MainWindow = mainWindow;
                mainWindow.Show();
                splash.Close();
                mainViewModel.CheckForUpdatesCommand.Execute(null);
                return;
            }
            catch (Exception exception)
            {
                splash?.Close();
                var logPath = StartupLogger.Write(exception);
                var retry = new StartupErrorDialog(logPath).ShowDialog() == true;
                if (!retry)
                {
                    Shutdown(-1);
                    return;
                }
            }
        }
    }
}

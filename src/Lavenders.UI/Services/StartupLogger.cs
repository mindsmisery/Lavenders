using System.Text;
using System.IO;
using System.Reflection;

namespace Lavenders.UI.Services;

internal static class StartupLogger
{
    public static string Write(Exception exception)
    {
        try
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Lavenders",
                "Logs");
            Directory.CreateDirectory(logDirectory);

            var logPath = Path.Combine(logDirectory, "startup.log");
            var version = typeof(StartupLogger).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "unknown";
            var entry = $"[{DateTimeOffset.Now:O}] Lavenders {version} startup failure{Environment.NewLine}" +
                        $"{exception}{Environment.NewLine}{Environment.NewLine}";
            File.AppendAllText(logPath, entry, Encoding.UTF8);
            return logPath;
        }
        catch
        {
            return string.Empty;
        }
    }
}

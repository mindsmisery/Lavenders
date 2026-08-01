namespace Lavenders.Data.Database;

public static class SQLiteConnector
{
    private static string GetDbPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lavenders", "events.db");

    public static string GetConnectionString()
    {
        var dbPath = GetDbPath();
        var directory = Path.GetDirectoryName(dbPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory); // Thread-safe folder initialization

        return $"Data Source={dbPath};Mode=ReadWriteCreate";
    }
}

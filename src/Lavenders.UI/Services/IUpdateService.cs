namespace Lavenders.UI.Services;

public interface IUpdateService
{
    Task<string?> CheckForUpdateAsync();
    Task<bool> DownloadAndApplyAsync();
}

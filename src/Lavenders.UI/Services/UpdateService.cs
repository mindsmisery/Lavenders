using Velopack;
using Velopack.Sources;

namespace Lavenders.UI.Services;

public sealed class UpdateService : IUpdateService
{
    private const string RepositoryUrl = "https://github.com/mindsmisery/Lavenders";
    private readonly UpdateManager _manager = new(new GithubSource(RepositoryUrl, null, false));
    private UpdateInfo? _pendingUpdate;

    public async Task<string?> CheckForUpdateAsync()
    {
        if (!_manager.IsInstalled) return null;

        try
        {
            _pendingUpdate = await _manager.CheckForUpdatesAsync();
            return _pendingUpdate?.TargetFullRelease.Version.ToString();
        }
        catch
        {
            // Lavenders is offline-first. A failed update check must not affect startup.
            return null;
        }
    }

    public async Task<bool> DownloadAndApplyAsync()
    {
        if (_pendingUpdate is null) return false;

        try
        {
            await _manager.DownloadUpdatesAsync(_pendingUpdate);
            _manager.ApplyUpdatesAndRestart(_pendingUpdate);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

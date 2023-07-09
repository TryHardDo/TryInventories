using Octokit;

namespace TryInventories.Updater;

public class VersionChecker
{
    private readonly GitHubClient _gitHubClient;
    private readonly ILogger<VersionChecker> _logger;
    private Timer? _timer;

    public VersionChecker(ILogger<VersionChecker> logger)
    {
        _gitHubClient = new GitHubClient(new ProductHeaderValue("TryInventories"));
        _logger = logger;
    }

    public async void CheckVersionAsync(bool first = false)
    {
        var releases = await _gitHubClient.Repository.Release.GetAll("TryHardDo", "TryInventories");

        if (releases.Count <= 0) return;

        var latestRelease = releases[0];
        var currentVersion = TryInventories.Version;
        var latestVersion = new Version(latestRelease.TagName);

        if (currentVersion < latestVersion)
        {
            _logger.LogWarning(
                "A new {keyword} is available! Current: {current} => Latest: {latest} | Download the new version from here: {uri}",
                latestRelease.Prerelease ? "Pre-release" : "Release", currentVersion,
                latestVersion, latestRelease.Url);
        }
        else
        {
            if (first)
                _logger.LogInformation(
                    "You are running the latest version of the software! Current: {current} | Latest: {latest}",
                    currentVersion, latestVersion);
        }
    }

    public void StartScheduledVersionChecker()
    {
        var first = true;
        _timer = new Timer(_ =>
        {
            CheckVersionAsync(first);
            if (first) first = false;
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }

    public void StopScheduledVersionChecker()
    {
        if (_timer == null) return;

        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        _timer.Dispose();
    }
}
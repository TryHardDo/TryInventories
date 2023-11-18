using Octokit;

namespace TryInventories.Services;

public class VersionChecker : IHostedService
{
    private readonly GitHubClient _gitHubClient;
    private readonly ILogger<VersionChecker> _logger;
    private Timer? _timer;

    public VersionChecker(ILogger<VersionChecker> logger)
    {
        _gitHubClient = new GitHubClient(new ProductHeaderValue("TryInventories"));
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartScheduledVersionChecker();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        StopScheduledVersionChecker();
        return Task.CompletedTask;
    }

    private async void CheckVersionAsync(bool first = false)
    {
        try
        {
            var releases = await _gitHubClient.Repository.Release.GetAll("TryHardDo", "TryInventories");

            if (releases.Count <= 0) return;

            var latestRelease = releases[0];
            var currentVersion = TryInventories.Version;
            var latestVersion = new Version(latestRelease.TagName);

            if (currentVersion < latestVersion)
            {
                _logger.LogWarning(
                    "A new {keyword} is available! Current: {current} => Latest: {latest} | Pull the new version if you are running it inside Docker or download the new builds from here: {uri}",
                    latestRelease.Prerelease ? "Pre-release" : "Release", currentVersion,
                    latestVersion, latestRelease.HtmlUrl);
            }
            else
            {
                if (first)
                    _logger.LogInformation(
                        "You are running the latest version of the software! Current: {current} | Latest: {latest}",
                        currentVersion, latestVersion);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Version check failed due to an error. You can ignore this message if you are running the latest release!");
        }
    }

    private void StartScheduledVersionChecker()
    {
        var first = true;
        _timer = new Timer(_ =>
        {
            CheckVersionAsync(first);
            if (first) first = false;
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }

    private void StopScheduledVersionChecker()
    {
        if (_timer == null) return;

        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        _timer.Dispose();
    }
}
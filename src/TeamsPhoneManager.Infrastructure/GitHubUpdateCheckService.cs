using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using teams_phonemanager.Models;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services;

/// <summary>
/// Checks GitHub Releases for a newer version. Anonymous, single request,
/// fail-silent: any network/parse problem simply reports "no update".
/// </summary>
public sealed class GitHubUpdateCheckService : IUpdateCheckService
{
    private const string LatestReleaseApi =
        "https://api.github.com/repos/realgarit/teams-phonemanager/releases/latest";

    private readonly string _currentVersionText;

    public GitHubUpdateCheckService()
        : this(ConstantsService.Application.Version)
    {
    }

    public GitHubUpdateCheckService(string currentVersionText)
    {
        _currentVersionText = currentVersionText;
    }

    public async Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        if (!AppVersion.TryParse(_currentVersionText, out var current))
        {
            return null;
        }

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            http.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("teams-phonemanager", current.ToString()));
            http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var response = await http.GetAsync(LatestReleaseApi, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

            var tag = doc.RootElement.TryGetProperty("tag_name", out var tagElement)
                ? tagElement.GetString()
                : null;
            var url = doc.RootElement.TryGetProperty("html_url", out var urlElement)
                ? urlElement.GetString()
                : null;

            if (tag is null || url is null || !AppVersion.TryParse(tag, out var latest))
            {
                return null;
            }

            return latest.IsNewerThan(current) ? new UpdateInfo(latest.ToString(), url) : null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return null;
        }
    }
}

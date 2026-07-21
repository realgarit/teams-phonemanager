using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using PhoneDesk.Models;
using PhoneDesk.Services.Interfaces;

namespace PhoneDesk.Services;

/// <summary>
/// Checks GitHub Releases for a newer version. Anonymous, single request,
/// fail-silent: any network/parse problem simply reports "no update".
/// </summary>
public sealed class GitHubUpdateCheckService : IUpdateCheckService
{
    private const string LatestReleaseApi =
        "https://api.github.com/repos/realgarit/phonedesk/releases/latest";
    private const string WindowsInstallerName = "phonedesk-win-x64-setup.exe";

    private readonly string _currentVersionText;
    private readonly HttpClient _httpClient;

    public GitHubUpdateCheckService()
        : this(ConstantsService.Application.Version, CreateHttpClient())
    {
    }

    public GitHubUpdateCheckService(string currentVersionText)
        : this(currentVersionText, CreateHttpClient())
    {
    }

    public GitHubUpdateCheckService(string currentVersionText, HttpClient httpClient)
    {
        _currentVersionText = currentVersionText;
        _httpClient = httpClient;
    }

    public async Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        if (!AppVersion.TryParse(_currentVersionText, out var current))
        {
            return null;
        }

        try
        {
            using var response = await _httpClient.GetAsync(LatestReleaseApi, cancellationToken).ConfigureAwait(false);
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

            if (!latest.IsNewerThan(current))
            {
                return null;
            }

            var installer = TryGetWindowsInstaller(doc.RootElement);
            return new UpdateInfo(latest.ToString(), url, installer);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return null;
        }
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(new ProductHeaderValue("phonedesk")));
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return httpClient;
    }

    private static UpdateAsset? TryGetWindowsInstaller(JsonElement release)
    {
        if (!release.TryGetProperty("assets", out var assets) ||
            assets.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var asset in assets.EnumerateArray())
        {
            var name = asset.TryGetProperty("name", out var nameElement)
                ? nameElement.GetString()
                : null;

            if (!string.Equals(name, WindowsInstallerName, StringComparison.Ordinal))
            {
                continue;
            }

            var downloadUrl = asset.TryGetProperty("browser_download_url", out var urlElement)
                ? urlElement.GetString()
                : null;
            var digest = asset.TryGetProperty("digest", out var digestElement)
                ? digestElement.GetString()
                : null;

            if (downloadUrl is null ||
                digest is null ||
                !digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var sha256 = digest["sha256:".Length..];
            return sha256.Length == 64 && sha256.All(Uri.IsHexDigit)
                ? new UpdateAsset(WindowsInstallerName, downloadUrl, sha256)
                : null;
        }

        return null;
    }
}

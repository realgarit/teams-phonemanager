using System.Net;
using System.Security.Cryptography;
using System.Text;
using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Tests;

public sealed class UpdateServicesTests
{
    [Fact]
    public void DefaultServices_UseValidHttpHeaders()
    {
        _ = new GitHubUpdateCheckService();
        _ = new GitHubUpdateInstallerService();
    }

    [Fact]
    public async Task CheckForUpdateAsync_ReleaseHasDigest_ReturnsVerifiedInstallerMetadata()
    {
        const string digest = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var json = $$"""
            {
              "tag_name": "v2.0.0",
              "html_url": "https://github.com/realgarit/teams-phonemanager/releases/tag/v2.0.0",
              "assets": [
                {
                  "name": "teams-phonemanager-win-x64-setup.exe",
                  "browser_download_url": "https://github.com/realgarit/teams-phonemanager/releases/download/v2.0.0/teams-phonemanager-win-x64-setup.exe",
                  "digest": "sha256:{{digest}}"
                }
              ]
            }
            """;
        using var httpClient = new HttpClient(new StubHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            }));
        var service = new GitHubUpdateCheckService("1.0.0", httpClient);

        var update = await service.CheckForUpdateAsync();

        Assert.NotNull(update);
        Assert.NotNull(update.WindowsInstaller);
        Assert.Equal(digest, update.WindowsInstaller.Sha256);
    }

    [Fact]
    public async Task DownloadInstallerAsync_ValidDigest_ReturnsVerifiedFile()
    {
        var content = Encoding.UTF8.GetBytes("verified installer content");
        var digest = Convert.ToHexString(SHA256.HashData(content));
        using var httpClient = new HttpClient(new StubHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            }));
        var service = new GitHubUpdateInstallerService(httpClient);
        var asset = new UpdateAsset(
            "teams-phonemanager-win-x64-setup.exe",
            "https://github.com/realgarit/teams-phonemanager/releases/download/v2.0.0/teams-phonemanager-win-x64-setup.exe",
            digest);

        var path = await service.DownloadInstallerAsync(asset);

        Assert.Equal(content, await File.ReadAllBytesAsync(path));
        Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
    }

    [Fact]
    public async Task DownloadInstallerAsync_DigestMismatch_RejectsInstaller()
    {
        var content = Encoding.UTF8.GetBytes("tampered installer content");
        using var httpClient = new HttpClient(new StubHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            }));
        var service = new GitHubUpdateInstallerService(httpClient);
        var asset = new UpdateAsset(
            "teams-phonemanager-win-x64-setup.exe",
            "https://github.com/realgarit/teams-phonemanager/releases/download/v2.0.0/teams-phonemanager-win-x64-setup.exe",
            new string('a', 64));

        var exception = await Assert.ThrowsAsync<UpdateInstallationException>(
            () => service.DownloadInstallerAsync(asset));

        Assert.Contains("integrity check", exception.Message);
    }

    private sealed class StubHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }
}

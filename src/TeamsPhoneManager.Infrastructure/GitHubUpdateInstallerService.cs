using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services;

/// <summary>
/// Downloads the Windows installer from GitHub, verifies its release digest, and launches it.
/// </summary>
public sealed class GitHubUpdateInstallerService : IUpdateInstallerService
{
    private const string WindowsInstallerName = "teams-phonemanager-win-x64-setup.exe";
    private const long MaximumInstallerBytes = 512L * 1024 * 1024;
    private readonly HttpClient _httpClient;

    public GitHubUpdateInstallerService()
        : this(CreateHttpClient())
    {
    }

    public GitHubUpdateInstallerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        CleanupPreviousDownloads();
    }

    public bool IsSupported => OperatingSystem.IsWindows();

    public async Task<string> DownloadInstallerAsync(
        UpdateAsset asset,
        IProgress<UpdateDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ValidateAsset(asset);

        string? downloadDirectory = null;

        try
        {
            downloadDirectory = Path.Combine(
                Path.GetTempPath(),
                "TeamsPhoneManager",
                Guid.NewGuid().ToString("N"));
            var installerPath = Path.Combine(downloadDirectory, WindowsInstallerName);
            Directory.CreateDirectory(downloadDirectory);

            using var response = await _httpClient.GetAsync(
                asset.DownloadUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            if (totalBytes > MaximumInstallerBytes)
            {
                throw new UpdateInstallationException("The update installer is unexpectedly large.");
            }

            await using var source = await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            await using var destination = new FileStream(
                installerPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            var buffer = new byte[81920];
            long bytesReceived = 0;
            progress?.Report(new UpdateDownloadProgress(0, totalBytes));

            while (true)
            {
                var bytesRead = await source
                    .ReadAsync(buffer, cancellationToken)
                    .ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    break;
                }

                bytesReceived += bytesRead;
                if (bytesReceived > MaximumInstallerBytes)
                {
                    throw new UpdateInstallationException("The update installer is unexpectedly large.");
                }

                hash.AppendData(buffer.AsSpan(0, bytesRead));
                await destination
                    .WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken)
                    .ConfigureAwait(false);
                progress?.Report(new UpdateDownloadProgress(bytesReceived, totalBytes));
            }

            await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            var actualSha256 = Convert.ToHexString(hash.GetHashAndReset());
            if (!string.Equals(actualSha256, asset.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new UpdateInstallationException(
                    "The downloaded installer failed its integrity check and was deleted.");
            }

            return installerPath;
        }
        catch (OperationCanceledException)
        {
            DeleteDownloadDirectory(downloadDirectory);
            throw;
        }
        catch (UpdateInstallationException)
        {
            DeleteDownloadDirectory(downloadDirectory);
            throw;
        }
        catch (HttpRequestException ex)
        {
            DeleteDownloadDirectory(downloadDirectory);
            throw new UpdateInstallationException("The update installer could not be downloaded.", ex);
        }
        catch (IOException ex)
        {
            DeleteDownloadDirectory(downloadDirectory);
            throw new UpdateInstallationException("The update installer could not be saved.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            DeleteDownloadDirectory(downloadDirectory);
            throw new UpdateInstallationException("The update installer could not be saved.", ex);
        }
    }

    public void LaunchInstaller(string installerPath)
    {
        if (!IsSupported)
        {
            throw new UpdateInstallationException(
                "Automatic installation is currently available on Windows only.");
        }

        if (!File.Exists(installerPath) ||
            !string.Equals(Path.GetFileName(installerPath), WindowsInstallerName, StringComparison.Ordinal))
        {
            throw new UpdateInstallationException("The verified update installer could not be found.");
        }

        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments = "/SILENT /NORESTART /CLOSEAPPLICATIONS /RESTARTAPP=1",
                WorkingDirectory = Path.GetDirectoryName(installerPath),
                UseShellExecute = true
            });

            if (process is null)
            {
                throw new UpdateInstallationException("The update installer could not be started.");
            }
        }
        catch (Win32Exception ex)
        {
            throw new UpdateInstallationException("The update installer could not be started.", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new UpdateInstallationException("The update installer could not be started.", ex);
        }
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(new ProductHeaderValue("teams-phonemanager-updater")));
        return httpClient;
    }

    private static void CleanupPreviousDownloads()
    {
        try
        {
            var rootDirectory = Path.Combine(Path.GetTempPath(), "TeamsPhoneManager");
            if (!Directory.Exists(rootDirectory))
            {
                return;
            }

            foreach (var downloadDirectory in Directory.GetDirectories(rootDirectory))
            {
                try
                {
                    Directory.Delete(downloadDirectory, recursive: true);
                }
                catch (IOException ex)
                {
                    Trace.TraceWarning(
                        "Could not remove previous update download '{0}': {1}",
                        downloadDirectory,
                        ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Trace.TraceWarning(
                        "Could not remove previous update download '{0}': {1}",
                        downloadDirectory,
                        ex.Message);
                }
            }
        }
        catch (IOException ex)
        {
            Trace.TraceWarning("Could not inspect previous update downloads: {0}", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Trace.TraceWarning("Could not inspect previous update downloads: {0}", ex.Message);
        }
    }

    private static void ValidateAsset(UpdateAsset asset)
    {
        if (!string.Equals(asset.Name, WindowsInstallerName, StringComparison.Ordinal) ||
            !Uri.TryCreate(asset.DownloadUrl, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase) ||
            asset.Sha256.Length != 64 ||
            !asset.Sha256.All(Uri.IsHexDigit))
        {
            throw new UpdateInstallationException("The release does not contain a valid Windows installer.");
        }
    }

    private static void DeleteDownloadDirectory(string? directory)
    {
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            return;
        }

        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch (IOException ex)
        {
            Trace.TraceWarning(
                "Could not remove rejected update download '{0}': {1}",
                directory,
                ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Trace.TraceWarning(
                "Could not remove rejected update download '{0}': {1}",
                directory,
                ex.Message);
        }
    }
}

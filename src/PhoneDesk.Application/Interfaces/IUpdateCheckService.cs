namespace PhoneDesk.Services.Interfaces;

/// <summary>
/// Port for checking whether a newer application release is available.
/// </summary>
public interface IUpdateCheckService
{
    /// <summary>
    /// Returns info about a newer release, or null when up to date or when the
    /// check cannot be performed (offline, rate-limited). Never throws.
    /// </summary>
    Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default);
}

/// <summary>A newer release available for download.</summary>
public sealed record UpdateInfo(
    string LatestVersion,
    string ReleaseUrl,
    UpdateAsset? WindowsInstaller = null);

/// <summary>A release asset whose integrity can be verified before execution.</summary>
public sealed record UpdateAsset(
    string Name,
    string DownloadUrl,
    string Sha256);

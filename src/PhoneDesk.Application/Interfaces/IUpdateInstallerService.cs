namespace PhoneDesk.Services.Interfaces;

/// <summary>
/// Downloads, verifies, and launches the native Windows update installer.
/// </summary>
public interface IUpdateInstallerService
{
    bool IsSupported { get; }

    Task<string> DownloadInstallerAsync(
        UpdateAsset asset,
        IProgress<UpdateDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);

    void LaunchInstaller(string installerPath);
}

public sealed record UpdateDownloadProgress(long BytesReceived, long? TotalBytes)
{
    public int Percentage => TotalBytes is > 0
        ? (int)Math.Clamp(BytesReceived * 100 / TotalBytes.Value, 0, 100)
        : 0;
}

public sealed class UpdateInstallationException : Exception
{
    public UpdateInstallationException(string message)
        : base(message)
    {
    }

    public UpdateInstallationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

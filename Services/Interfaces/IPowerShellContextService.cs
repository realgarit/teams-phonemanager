namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Service for managing PowerShell context and executing commands.
/// </summary>
public interface IPowerShellContextService : IDisposable
{
    Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
    bool IsConnected(string service);
    Task<string> GetConnectionStatusAsync(CancellationToken cancellationToken = default);
}

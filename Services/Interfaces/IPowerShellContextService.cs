namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Service for managing PowerShell context and executing commands.
/// </summary>
public interface IPowerShellContextService : IDisposable
{
    Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
    Task<string> ExecuteCommandAsync(string command, Dictionary<string, string>? environmentVariables, CancellationToken cancellationToken = default);
    bool IsConnected(string service);
    Task<bool> IsConnectedAsync(string service, CancellationToken cancellationToken = default);
    Task<string> GetConnectionStatusAsync(CancellationToken cancellationToken = default);
}

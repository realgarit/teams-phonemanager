namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Service for managing PowerShell context and executing commands.
/// </summary>
public interface IPowerShellContextService : IDisposable
{
    Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
    Task<string> ExecuteCommandAsync(string command, Dictionary<string, string>? environmentVariables, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a command and returns both the flattened text output and the structured error records
    /// captured from the PowerShell error stream. The text output is byte-identical to what
    /// <see cref="ExecuteCommandAsync(string, Dictionary{string, string}?, CancellationToken)"/> returns;
    /// this overload additionally surfaces the raw <c>ErrorRecord</c> details for typed result mapping.
    /// </summary>
    Task<PowerShellExecutionResult> ExecuteCommandWithDetailsAsync(string command, Dictionary<string, string>? environmentVariables, CancellationToken cancellationToken = default);

    Task<bool> IsConnectedAsync(string service, CancellationToken cancellationToken = default);
    Task<string> GetConnectionStatusAsync(CancellationToken cancellationToken = default);
}

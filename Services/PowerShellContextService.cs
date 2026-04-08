using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    public class PowerShellContextService : IPowerShellContextService
    {
        private readonly ILoggingService _loggingService;
        private readonly Runspace _runspace;
        private readonly PowerShell _powerShell;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _disposed = false;

        public PowerShellContextService(ILoggingService loggingService)
        {
            _loggingService = loggingService;

            // Use local variables during construction to ensure proper cleanup on failure
            Runspace? localRunspace = null;
            PowerShell? localPowerShell = null;
            
            try
            {
                // Set execution policy via InitialSessionState (configuration-level, not a runtime command).
                // This is required because Import-Module loads .psm1 files which are subject to execution policy.
                // Setting it here instead of calling Set-ExecutionPolicy at runtime avoids triggering
                // AV behavioral heuristics (Kaspersky flags runtime Set-ExecutionPolicy Bypass as MITRE T1059.001).
                var initialState = InitialSessionState.CreateDefault();
                initialState.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Bypass;
                localRunspace = RunspaceFactory.CreateRunspace(initialState);
                localRunspace.Open();

                localPowerShell = PowerShell.Create();
                localPowerShell.Runspace = localRunspace;

                // Assign to fields only after successful initialization
                _runspace = localRunspace;
                _powerShell = localPowerShell;

                InitializeRunspacePreferences();
                _loggingService.Log("PowerShell context service initialized with persistent runspace", LogLevel.Info);
            }
            catch
            {
                // Clean up locally created resources if initialization fails
                localPowerShell?.Dispose();
                localRunspace?.Dispose();
                throw;
            }
        }

        private void InitializeRunspacePreferences()
        {
            try
            {
                // Note: Set-ExecutionPolicy is intentionally NOT called here.
                // When hosting PowerShell via System.Management.Automation with our own Runspace,
                // execution policy does not apply to AddScript() calls - only to .ps1 file execution.
                // Calling Set-ExecutionPolicy Bypass is unnecessary and triggers AV heuristics
                // (MITRE ATT&CK T1059.001).

                _powerShell.Commands.Clear();
                _powerShell.AddCommand("Set-Variable")
                    .AddParameter("Name", "InformationPreference")
                    .AddParameter("Value", "Continue")
                    .Invoke();

                // WAM bypass flags are already set at process level in Program.cs Main().
                // No need to set them again in the runspace.
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error initializing PowerShell preferences: {ex.Message}", LogLevel.Warning);
            }
        }

        public async Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default)
        {
            return await ExecuteCommandAsync(command, null, cancellationToken);
        }

        public async Task<string> ExecuteCommandAsync(string command, Dictionary<string, string>? environmentVariables, CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PowerShellContextService));

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                // Set environment variables for the PowerShell script to consume via $env:VAR_NAME.
                // These are set at process level because runspace session variables don't map to $env: scope.
                // Cleared in the finally block immediately after execution to minimize exposure window.
                var varsToClear = new List<string>();
                if (environmentVariables != null)
                {
                    foreach (var kvp in environmentVariables)
                    {
                        Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
                        varsToClear.Add(kvp.Key);
                    }
                }

                try
                {
                    _powerShell.Commands.Clear();
                    _powerShell.Streams.ClearStreams();

                    var fullCommand = $@"
# Ensure we're in the right context
$ErrorActionPreference = 'Continue'
{command}
";

                    _powerShell.AddScript(fullCommand);

                    var result = await Task.Run(() =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return _powerShell.Invoke();
                    }, cancellationToken);

                    var output = new StringBuilder();

                    foreach (var item in _powerShell.Streams.Information)
                    {
                        output.AppendLine(item.ToString());
                    }

                    // Capture Warning stream (device code from Connect-MgGraph -UseDeviceCode is output here)
                    foreach (var item in _powerShell.Streams.Warning)
                    {
                        _loggingService.Log($"{item}", LogLevel.Warning);
                        output.AppendLine($"WARNING: {item}");
                    }

                    foreach (var item in result)
                    {
                        output.AppendLine(item.ToString());
                    }

                    if (_powerShell.HadErrors)
                    {
                        foreach (var error in _powerShell.Streams.Error)
                        {
                            _loggingService.Log($"PowerShell error: {error}", LogLevel.Error);
                            output.AppendLine($"ERROR: {error}");
                        }
                    }

                    return output.ToString();
                }
                finally
                {
                    // SECURITY: Clear sensitive environment variables immediately after execution
                    foreach (var varName in varsToClear)
                    {
                        try
                        {
                            Environment.SetEnvironmentVariable(varName, null);
                        }
                        catch
                        {
                            // Ignore errors when clearing variables
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error executing PowerShell command: {ex.Message}", LogLevel.Error);
                return $"ERROR: {ex.Message}";
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> IsConnectedAsync(string service, CancellationToken cancellationToken = default)
        {
            // Use timeout to prevent indefinite blocking
            if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(ConstantsService.PowerShell.ConnectionCheckTimeoutSeconds), cancellationToken))
            {
                _loggingService.Log("IsConnectedAsync timed out waiting for semaphore", LogLevel.Warning);
                return false;
            }
            
            try
            {
                _powerShell.Commands.Clear();
                _powerShell.Streams.ClearStreams();

                switch (service.ToLowerInvariant())
                {
                    case "teams":
                        // Use command-based execution instead of script for better security tool compatibility
                        _powerShell.AddCommand("Get-CsTenant")
                            .AddParameter("ErrorAction", "SilentlyContinue");
                        break;
                    case "graph":
                        // Use command-based execution instead of script for better security tool compatibility
                        _powerShell.AddCommand("Get-MgContext")
                            .AddParameter("ErrorAction", "SilentlyContinue");
                        break;
                    default:
                        return false;
                }

                var result = await Task.Run(() => _powerShell.Invoke(), cancellationToken);
                return result != null && result.Count > 0;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error checking {service} connection: {ex.Message}", LogLevel.Warning);
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<string> GetConnectionStatusAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                _powerShell.Commands.Clear();
                _powerShell.Streams.ClearStreams();

                var statusCommand = @"
$status = @()
try {
    $teamsContext = Get-CsTenant -ErrorAction SilentlyContinue
    if ($teamsContext) {
        $status += ""Teams: Connected to $($teamsContext.DisplayName)""
    } else {
        $status += ""Teams: Not connected""
    }
} catch {
    $status += ""Teams: Error checking connection""
}

try {
    $graphContext = Get-MgContext -ErrorAction SilentlyContinue
    if ($graphContext) {
        $status += ""Graph: Connected as $($graphContext.Account)""
    } else {
        $status += ""Graph: Not connected""
    }
} catch {
    $status += ""Graph: Error checking connection""
}

$status -join ""`n""
";

                _powerShell.AddScript(statusCommand);
                var result = await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return _powerShell.Invoke();
                }, cancellationToken);

                var output = new StringBuilder();
                foreach (var item in result)
                {
                    output.AppendLine(item.ToString());
                }

                return output.ToString();
            }
            catch (Exception ex)
            {
                return $"Error checking connection status: {ex.Message}";
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore.Dispose();
                _powerShell?.Dispose();
                _runspace?.Dispose();
                _disposed = true;
                _loggingService.Log("PowerShell context service disposed", LogLevel.Info);
            }
        }
    }
}

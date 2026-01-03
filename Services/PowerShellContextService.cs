using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    public class PowerShellContextService : IPowerShellContextService
    {
        private readonly ILoggingService _loggingService;
        private readonly Runspace _runspace;
        private readonly PowerShell _powerShell;
        private bool _disposed = false;

        public PowerShellContextService(ILoggingService loggingService)
        {
            _loggingService = loggingService;

            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();

            _powerShell = PowerShell.Create();
            _powerShell.Runspace = _runspace;

            InitializeExecutionPolicy();
            _loggingService.Log("PowerShell context service initialized with persistent runspace", LogLevel.Info);
        }

        private void InitializeExecutionPolicy()
        {
            try
            {
                _powerShell.Commands.Clear();
                _powerShell.AddCommand("Set-ExecutionPolicy")
                    .AddParameter("ExecutionPolicy", ConstantsService.PowerShell.ExecutionPolicy)
                    .AddParameter("Scope", "Process")
                    .AddParameter("Force", true)
                    .Invoke();
                
                _powerShell.Commands.Clear();
                // Set InformationPreference using command-based approach instead of script
                _powerShell.AddCommand("Set-Variable")
                    .AddParameter("Name", "InformationPreference")
                    .AddParameter("Value", "Continue")
                    .Invoke();

                _powerShell.Commands.Clear();
                _powerShell.AddScript("$env:MSAL_DISABLE_WAM = 'true'; $env:AZURE_IDENTITY_DISABLE_WAM = 'true'");
                _powerShell.Invoke();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error initializing PowerShell preferences: {ex.Message}", LogLevel.Warning);
            }
        }

        public async Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PowerShellContextService));

            // Keep logs meaningful; avoid noisy debug spam
            
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
            catch (Exception ex)
            {
                _loggingService.Log($"Error executing PowerShell command: {ex.Message}", LogLevel.Error);
                return $"ERROR: {ex.Message}";
            }
        }

        public bool IsConnected(string service)
        {
            try
            {
                _powerShell.Commands.Clear();
                _powerShell.Streams.ClearStreams();
                
                switch (service.ToLower())
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
                
                var result = _powerShell.Invoke();
                return result != null && result.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetConnectionStatusAsync(CancellationToken cancellationToken = default)
        {
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
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _powerShell?.Dispose();
                _runspace?.Dispose();
                _disposed = true;
                _loggingService.Log("PowerShell context service disposed", LogLevel.Info);
            }
        }
    }
}

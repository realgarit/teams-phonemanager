using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace teams_phonemanager.Services
{
    public class PowerShellContextService : IDisposable
    {
        private static PowerShellContextService? _instance;
        private readonly Runspace _runspace;
        private readonly PowerShell _powerShell;
        private bool _disposed = false;

        private PowerShellContextService()
        {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
            
            _powerShell = PowerShell.Create();
            _powerShell.Runspace = _runspace;
            
            InitializeExecutionPolicy();
            LoggingService.Instance.Log("PowerShell context service initialized with persistent runspace", LogLevel.Info);
        }

        public static PowerShellContextService Instance
        {
            get
            {
                _instance ??= new PowerShellContextService();
                return _instance;
            }
        }

        private void InitializeExecutionPolicy()
        {
            try
            {
                _powerShell.Commands.Clear();
                _powerShell.AddCommand("Set-ExecutionPolicy")
                    .AddParameter("ExecutionPolicy", "Bypass")
                    .AddParameter("Scope", "Process")
                    .AddParameter("Force", true)
                    .Invoke();
                
                _powerShell.Commands.Clear();
                _powerShell.AddScript("$InformationPreference = 'Continue'").Invoke();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Log($"Error setting execution policy: {ex.Message}", LogLevel.Warning);
            }
        }

        public async Task<string> ExecuteCommandAsync(string command)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PowerShellContextService));

            LoggingService.Instance.Log("Executing PowerShell command in persistent context", LogLevel.Info);
            
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
                
                var result = await Task.Run(() => _powerShell.Invoke());
                
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
                        LoggingService.Instance.Log($"PowerShell error: {error}", LogLevel.Error);
                        output.AppendLine($"ERROR: {error}");
                    }
                }
                
                return output.ToString();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Log($"Error executing PowerShell command: {ex.Message}", LogLevel.Error);
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
                        _powerShell.AddScript("Get-CsTenant -ErrorAction SilentlyContinue");
                        break;
                    case "graph":
                        _powerShell.AddScript("Get-MgContext -ErrorAction SilentlyContinue");
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

        public async Task<string> GetConnectionStatusAsync()
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
                var result = await Task.Run(() => _powerShell.Invoke());
                
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
                LoggingService.Instance.Log("PowerShell context service disposed", LogLevel.Info);
            }
        }
    }
}

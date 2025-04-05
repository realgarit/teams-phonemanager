using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace teams_phonemanager.Services
{
    public class PowerShellService
    {
        private static PowerShellService? _instance;
        private PowerShell? _powershell;
        private Runspace? _runspace;

        private PowerShellService()
        {
            InitializePowerShell();
        }

        public static PowerShellService Instance
        {
            get
            {
                _instance ??= new PowerShellService();
                return _instance;
            }
        }

        private void InitializePowerShell()
        {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
            _powershell = PowerShell.Create();
            _powershell.Runspace = _runspace;
        }

        public async Task<string> ExecuteCommandAsync(string command)
        {
            try
            {
                _powershell?.AddScript(command);
                var result = await Task.Run(() => _powershell?.Invoke());
                
                if (result == null) return string.Empty;
                
                var output = string.Join("\n", result.Select(x => x.ToString()));
                _powershell?.Commands.Clear();
                return output;
            }
            catch (Exception ex)
            {
                return $"Error executing command: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _powershell?.Dispose();
            _runspace?.Dispose();
        }
    }
} 
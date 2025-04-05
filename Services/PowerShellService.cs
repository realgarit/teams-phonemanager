using System.Management.Automation;
using System.Management.Automation.Runspaces;
using teams_phonemanager.Services;

namespace teams_phonemanager.Services
{
    public class PowerShellService
    {
        private static PowerShellService? _instance;

        private PowerShellService()
        {
            LoggingService.Instance.Log("PowerShell service initialized", LogLevel.Info);
        }

        public static PowerShellService Instance
        {
            get
            {
                _instance ??= new PowerShellService();
                return _instance;
            }
        }

        public async Task<string> ExecuteCommandAsync(string command)
        {
            LoggingService.Instance.Log("Preparing to execute PowerShell command", LogLevel.Info);
            
            // Create a new runspace for each command to avoid state issues
            using var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            
            // Set execution policy to Bypass for this runspace
            using (var policyPowershell = PowerShell.Create())
            {
                policyPowershell.Runspace = runspace;
                policyPowershell.AddCommand("Set-ExecutionPolicy")
                    .AddParameter("ExecutionPolicy", "Bypass")
                    .AddParameter("Scope", "Process")
                    .AddParameter("Force", true)
                    .Invoke();
            }
            
            // Execute the actual command
            using var commandPowershell = PowerShell.Create();
            commandPowershell.Runspace = runspace;
            
            try
            {
                LoggingService.Instance.Log("Adding command to PowerShell instance", LogLevel.Info);
                
                // Enable information stream to capture Write-Host output
                commandPowershell.AddScript(@"
$InformationPreference = 'Continue'
" + command);
                
                LoggingService.Instance.Log("Starting command execution", LogLevel.Info);
                var result = await Task.Run(() => commandPowershell.Invoke());
                
                LoggingService.Instance.Log("Command execution completed", LogLevel.Info);
                
                var output = new System.Text.StringBuilder();
                
                // Collect output from the Information stream (Write-Host)
                foreach (var item in commandPowershell.Streams.Information)
                {
                    output.AppendLine(item.ToString());
                }
                
                // Collect output from the command results
                foreach (var item in result)
                {
                    output.AppendLine(item.ToString());
                }
                
                // Check for errors
                if (commandPowershell.HadErrors)
                {
                    var errors = commandPowershell.Streams.Error;
                    foreach (var error in errors)
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

        public void Dispose()
        {
            // Nothing to dispose of in the new implementation
            LoggingService.Instance.Log("PowerShell service disposed", LogLevel.Info);
        }
    }
} 
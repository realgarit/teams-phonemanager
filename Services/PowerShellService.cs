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
            return await PowerShellContextService.Instance.ExecuteCommandAsync(command);
        }

        public void Dispose()
        {
            LoggingService.Instance.Log("PowerShell service disposed", LogLevel.Info);
        }
    }
} 

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using teams_phonemanager.Services;
using System.Threading.Tasks;
using System;

namespace teams_phonemanager.ViewModels
{
    public partial class GetStartedViewModel : ViewModelBase
    {
        private readonly PowerShellService _powerShellService;
        private readonly LoggingService _loggingService;

        [ObservableProperty]
        private bool _modulesChecked;

        [ObservableProperty]
        private bool _teamsConnected;

        [ObservableProperty]
        private bool _graphConnected;

        [ObservableProperty]
        private bool _canProceed;

        public GetStartedViewModel()
        {
            _powerShellService = PowerShellService.Instance;
            _loggingService = LoggingService.Instance;
            _loggingService.Log("Get Started page loaded", LogLevel.Info);
        }

        [RelayCommand]
        private async Task CheckModulesAsync()
        {
            try
            {
                IsBusy = true;
                _loggingService.Log("Checking PowerShell modules...", LogLevel.Info);

                // Simple command to check if modules are available
                var command = @"
$ErrorActionPreference = 'Stop'
$output = @()

# Check MicrosoftTeams module
$teamsModule = Get-Module -ListAvailable -Name MicrosoftTeams
if ($teamsModule) {
    $output += 'MicrosoftTeams module is available: ' + $teamsModule.Version
} else {
    $output += 'MicrosoftTeams module is NOT available'
}

# Check Microsoft.Graph module
$graphModule = Get-Module -ListAvailable -Name Microsoft.Graph
if ($graphModule) {
    $output += 'Microsoft.Graph module is available: ' + $graphModule.Version
} else {
    $output += 'Microsoft.Graph module is NOT available'
}

$output | ForEach-Object { Write-Host $_ }
";

                var result = await _powerShellService.ExecuteCommandAsync(command);
                
                // Check if both modules are available
                ModulesChecked = result.Contains("MicrosoftTeams module is available") && 
                                result.Contains("Microsoft.Graph module is available");
                
                if (ModulesChecked)
                {
                    _loggingService.Log("PowerShell modules are available", LogLevel.Success);
                    // Log the module versions
                    foreach (var line in result.Split('\n'))
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            _loggingService.Log(line.Trim(), LogLevel.Info);
                        }
                    }
                }
                else
                {
                    var errorMessage = "One or more required modules are not available";
                    _loggingService.Log(errorMessage, LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error checking PowerShell modules: {ex.Message}", LogLevel.Error);
                ModulesChecked = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ConnectTeamsAsync()
        {
            try
            {
                if (!ModulesChecked)
                {
                    _loggingService.Log("Please check modules first", LogLevel.Warning);
                    return;
                }

                IsBusy = true;
                _loggingService.Log("Connecting to Microsoft Teams...", LogLevel.Info);

                var command = @"
try {
    Connect-MicrosoftTeams -ErrorAction Stop
    $connection = Get-CsTenant -ErrorAction Stop
    if ($connection) {
        Write-Host 'SUCCESS: Connected to Microsoft Teams'
    }
}
catch {
    Write-Error ""Failed to connect to Microsoft Teams: $_""
    exit 1
}";

                var result = await _powerShellService.ExecuteCommandAsync(command);
                TeamsConnected = result.Contains("SUCCESS:");
                
                if (TeamsConnected)
                {
                    _loggingService.Log("Connected to Microsoft Teams successfully", LogLevel.Success);
                }
                else
                {
                    _loggingService.Log($"Error connecting to Microsoft Teams: {result}", LogLevel.Error);
                }
                
                UpdateCanProceed();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error connecting to Microsoft Teams: {ex.Message}", LogLevel.Error);
                TeamsConnected = false;
                UpdateCanProceed();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ConnectGraphAsync()
        {
            try
            {
                if (!ModulesChecked)
                {
                    _loggingService.Log("Please check modules first", LogLevel.Warning);
                    return;
                }

                IsBusy = true;
                _loggingService.Log("Connecting to Microsoft Graph...", LogLevel.Info);

                var command = @"
try {
    Connect-MgGraph -Scopes User.ReadWrite.All, Organization.Read.All, Group.ReadWrite.All, Directory.ReadWrite.All -ErrorAction Stop
    $context = Get-MgContext -ErrorAction Stop
    if ($context) {
        Write-Host 'SUCCESS: Connected to Microsoft Graph'
        Write-Host ""Connected as: $($context.Account)""
    }
}
catch {
    Write-Error ""Failed to connect to Microsoft Graph: $_""
    exit 1
}";

                var result = await _powerShellService.ExecuteCommandAsync(command);
                GraphConnected = result.Contains("SUCCESS:");
                
                if (GraphConnected)
                {
                    _loggingService.Log($"Connected to Microsoft Graph successfully\n{result}", LogLevel.Success);
                }
                else
                {
                    _loggingService.Log($"Error connecting to Microsoft Graph: {result}", LogLevel.Error);
                }
                
                UpdateCanProceed();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error connecting to Microsoft Graph: {ex.Message}", LogLevel.Error);
                GraphConnected = false;
                UpdateCanProceed();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DisconnectTeamsAsync()
        {
            try
            {
                IsBusy = true;
                _loggingService.Log("Disconnecting from Microsoft Teams...", LogLevel.Info);

                var command = @"
try {
    Disconnect-MicrosoftTeams -ErrorAction Stop
    Write-Host 'SUCCESS: Disconnected from Microsoft Teams'
}
catch {
    Write-Error ""Failed to disconnect from Microsoft Teams: $_""
    exit 1
}";

                var result = await _powerShellService.ExecuteCommandAsync(command);
                TeamsConnected = false;
                _loggingService.Log("Disconnected from Microsoft Teams", LogLevel.Info);
                UpdateCanProceed();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error disconnecting from Microsoft Teams: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DisconnectGraphAsync()
        {
            try
            {
                IsBusy = true;
                _loggingService.Log("Disconnecting from Microsoft Graph...", LogLevel.Info);

                var command = @"
try {
    Disconnect-MgGraph -ErrorAction Stop
    Write-Host 'SUCCESS: Disconnected from Microsoft Graph'
}
catch {
    Write-Error ""Failed to disconnect from Microsoft Graph: $_""
    exit 1
}";

                var result = await _powerShellService.ExecuteCommandAsync(command);
                GraphConnected = false;
                _loggingService.Log("Disconnected from Microsoft Graph", LogLevel.Info);
                UpdateCanProceed();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error disconnecting from Microsoft Graph: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateCanProceed()
        {
            CanProceed = TeamsConnected && GraphConnected;
        }
    }
} 
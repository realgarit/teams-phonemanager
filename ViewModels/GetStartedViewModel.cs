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
        private readonly SessionManager _sessionManager;
        private readonly MainWindowViewModel _mainWindowViewModel;

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
            _sessionManager = SessionManager.Instance;
            _mainWindowViewModel = App.Current.MainWindow.DataContext as MainWindowViewModel;
            
            // Initialize state from session manager
            _modulesChecked = _sessionManager.ModulesChecked;
            _teamsConnected = _sessionManager.TeamsConnected;
            _graphConnected = _sessionManager.GraphConnected;
            UpdateCanProceed();
            
            _loggingService.Log("Get Started page loaded", LogLevel.Info);
        }

        [RelayCommand]
        private void NavigateTo(string page)
        {
            if (CanProceed)
            {
                _mainWindowViewModel?.NavigateToCommand.Execute(page);
                _loggingService.Log($"Navigating to {page} page", LogLevel.Info);
            }
        }

        [RelayCommand]
        private async Task CheckModulesAsync()
        {
            try
            {
                IsBusy = true;
                _loggingService.Log("Checking PowerShell modules...", LogLevel.Info);

                // Command to check and install modules if needed
                var command = @"
$ErrorActionPreference = 'Stop'
$output = @()

# Check and install MicrosoftTeams module
if (Get-Module -ListAvailable -Name MicrosoftTeams) {
    $teamsModule = Get-Module -ListAvailable -Name MicrosoftTeams
    $output += 'MicrosoftTeams module is available: ' + $teamsModule.Version
} else {
    $output += 'MicrosoftTeams module is NOT available, attempting to install...'
    try {
        Install-Module -Name MicrosoftTeams -Force -AllowClobber
        $output += 'MicrosoftTeams module installed successfully'
    } catch {
        $output += 'ERROR: Failed to install MicrosoftTeams module: ' + $_.Exception.Message
    }
}

# Check and install Microsoft.Graph module
if (Get-Module -ListAvailable -Name Microsoft.Graph) {
    $graphModule = Get-Module -ListAvailable -Name Microsoft.Graph
    $output += 'Microsoft.Graph module is available: ' + $graphModule.Version
} else {
    $output += 'Microsoft.Graph module is NOT available, attempting to install...'
    try {
        Install-Module -Name Microsoft.Graph -Force
        $output += 'Microsoft.Graph module installed successfully'
    } catch {
        $output += 'ERROR: Failed to install Microsoft.Graph module: ' + $_.Exception.Message
    }
}

$output | ForEach-Object { Write-Host $_ }
";

                var result = await _powerShellService.ExecuteCommandAsync(command);
                
                // Check if both modules are available or were successfully installed
                ModulesChecked = (result.Contains("MicrosoftTeams module is available") || result.Contains("MicrosoftTeams module installed successfully")) && 
                                (result.Contains("Microsoft.Graph module is available") || result.Contains("Microsoft.Graph module installed successfully"));
                
                // Update session manager
                _sessionManager.UpdateModulesChecked(ModulesChecked);
                
                if (ModulesChecked)
                {
                    _loggingService.Log("PowerShell modules are available", LogLevel.Success);
                    // Log the module versions and installation status
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
                    var errorMessage = "One or more required modules could not be installed";
                    _loggingService.Log(errorMessage, LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error checking PowerShell modules: {ex.Message}", LogLevel.Error);
                ModulesChecked = false;
                _sessionManager.UpdateModulesChecked(false);
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
        Write-Host ""Connected to tenant: $($connection.DisplayName) ($($connection.TenantId))""
    }
}
catch {
    Write-Error ""Failed to connect to Microsoft Teams: $_""
    exit 1
}";

                var result = await _powerShellService.ExecuteCommandAsync(command);
                TeamsConnected = result.Contains("SUCCESS:");
                
                // Extract tenant information if connected
                string? tenantId = null;
                string? tenantName = null;
                
                if (TeamsConnected)
                {
                    _loggingService.Log("Connected to Microsoft Teams successfully", LogLevel.Success);
                    
                    // Extract tenant information from the result
                    foreach (var line in result.Split('\n'))
                    {
                        if (line.Contains("Connected to tenant:"))
                        {
                            var parts = line.Split(':')[1].Trim().Split('(');
                            if (parts.Length >= 2)
                            {
                                tenantName = parts[0].Trim();
                                tenantId = parts[1].Trim(')').Trim();
                            }
                        }
                    }
                }
                else
                {
                    _loggingService.Log($"Error connecting to Microsoft Teams: {result}", LogLevel.Error);
                }
                
                // Update session manager
                _sessionManager.UpdateTeamsConnection(TeamsConnected);
                if (tenantId != null && tenantName != null)
                {
                    _sessionManager.UpdateTeamsTenantInfo(tenantId, tenantName);
                }
                
                UpdateCanProceed();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error connecting to Microsoft Teams: {ex.Message}", LogLevel.Error);
                TeamsConnected = false;
                _sessionManager.UpdateTeamsConnection(false);
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
    Connect-MgGraph -Scopes User.ReadWrite.All, Organization.Read.All, Group.ReadWrite.All, Directory.ReadWrite.All -ErrorAction Stop -NoWelcome
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
                
                // Extract account information if connected
                string? account = null;
                
                if (GraphConnected)
                {
                    _loggingService.Log($"Connected to Microsoft Graph successfully\n{result}", LogLevel.Success);
                    
                    // Extract account information from the result
                    foreach (var line in result.Split('\n'))
                    {
                        if (line.Contains("Connected as:"))
                        {
                            account = line.Split(':')[1].Trim();
                        }
                    }
                }
                else
                {
                    _loggingService.Log($"Error connecting to Microsoft Graph: {result}", LogLevel.Error);
                }
                
                // Update session manager
                _sessionManager.UpdateGraphConnection(GraphConnected, account);
                
                UpdateCanProceed();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error connecting to Microsoft Graph: {ex.Message}", LogLevel.Error);
                GraphConnected = false;
                _sessionManager.UpdateGraphConnection(false);
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
                _sessionManager.UpdateTeamsConnection(false);
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
                _sessionManager.UpdateGraphConnection(false);
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
            CanProceed = ModulesChecked && TeamsConnected && GraphConnected;
        }

        partial void OnModulesCheckedChanged(bool value) => UpdateCanProceed();
        partial void OnTeamsConnectedChanged(bool value) => UpdateCanProceed();
        partial void OnGraphConnectedChanged(bool value) => UpdateCanProceed();
    }
} 
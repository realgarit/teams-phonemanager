using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using teams_phonemanager.Services;
using System.Threading.Tasks;

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
            IsBusy = true;
            _loggingService.Log("Checking PowerShell modules...", LogLevel.Info);

            var command = @"
$ErrorActionPreference = 'Stop'
$error.Clear()
$output = @()

try {
    # First ensure NuGet provider and PowerShellGet are available
    if (-not (Get-PackageProvider -Name NuGet -ErrorAction SilentlyContinue)) {
        $output += 'Installing NuGet package provider...'
        Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force -Scope AllUsers | Out-Null
        $output += 'NuGet package provider installed successfully'
    }
    else {
        $output += 'NuGet package provider is already installed'
    }

    if (-not (Get-Module -ListAvailable -Name PowerShellGet)) {
        throw 'PowerShellGet module is not available. Please ensure PowerShell 5.1 or later is installed.'
    }
    else {
        $output += 'PowerShellGet module is available'
    }

    # Set PSGallery as trusted
    Set-PSRepository -Name PSGallery -InstallationPolicy Trusted
    $output += 'PSGallery set as trusted'

    # Check and install/import MicrosoftTeams module
    if (-not (Get-Module -ListAvailable -Name MicrosoftTeams)) {
        $output += 'Installing MicrosoftTeams module...'
        Install-Module -Name MicrosoftTeams -Force -AllowClobber -Scope AllUsers
        $output += 'MicrosoftTeams module installed successfully'
    }
    else {
        $output += 'MicrosoftTeams module is already installed'
    }
    
    Import-Module MicrosoftTeams -ErrorAction Stop
    $output += 'MicrosoftTeams module imported successfully'

    # Check and install/import Microsoft.Graph module
    if (-not (Get-Module -ListAvailable -Name Microsoft.Graph)) {
        $output += 'Installing Microsoft.Graph module...'
        Install-Module -Name Microsoft.Graph -Force -AllowClobber -Scope AllUsers
        $output += 'Microsoft.Graph module installed successfully'
    }
    else {
        $output += 'Microsoft.Graph module is already installed'
    }
    
    Import-Module Microsoft.Graph -ErrorAction Stop
    $output += 'Microsoft.Graph module imported successfully'

    $output += 'SUCCESS: All modules are installed and imported successfully'
    $output | ForEach-Object { Write-Host $_ }
}
catch {
    $errorDetails = @{
        Message = $_.Exception.Message
        ScriptStackTrace = $_.ScriptStackTrace
        FullError = $_
    } | ConvertTo-Json

    Write-Error ""ERROR: $($_.Exception.Message)`nStack Trace: $($_.ScriptStackTrace)""
    exit 1
}";

            var result = await _powerShellService.ExecuteCommandAsync(command);
            ModulesChecked = result.Contains("SUCCESS:");
            
            if (ModulesChecked)
            {
                _loggingService.Log("PowerShell modules installed and imported successfully", LogLevel.Success);
                // Log the detailed success information
                foreach (var line in result.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.Contains("SUCCESS:"))
                    {
                        _loggingService.Log(line.Trim(), LogLevel.Info);
                    }
                }
            }
            else
            {
                var errorMessage = result.Contains("ERROR:") 
                    ? result.Substring(result.IndexOf("ERROR:")).Trim() 
                    : "Unknown error occurred while checking PowerShell modules";
                _loggingService.Log(errorMessage, LogLevel.Error);
            }
            
            IsBusy = false;
        }

        [RelayCommand]
        private async Task ConnectTeamsAsync()
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
            IsBusy = false;
        }

        [RelayCommand]
        private async Task ConnectGraphAsync()
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
            IsBusy = false;
        }

        [RelayCommand]
        private async Task DisconnectTeamsAsync()
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
            IsBusy = false;
        }

        [RelayCommand]
        private async Task DisconnectGraphAsync()
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
            IsBusy = false;
        }

        private void UpdateCanProceed()
        {
            CanProceed = TeamsConnected && GraphConnected;
        }
    }
} 
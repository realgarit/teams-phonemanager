using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using teams_phonemanager.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace teams_phonemanager.ViewModels
{
    public partial class CallQueuesViewModel : ViewModelBase
    {
        private readonly PowerShellService _powerShellService;
        private readonly LoggingService _loggingService;
        private readonly SessionManager _sessionManager;
        private readonly MainWindowViewModel? _mainWindowViewModel;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to the Call Queues page. This page will help you create the call queue for your phone system.";

        [ObservableProperty]
        private bool _isQueueCreated;

        [ObservableProperty]
        private string _queueStatus = string.Empty;

        [ObservableProperty]
        private bool _showConfirmation;

        public CallQueuesViewModel()
        {
            _powerShellService = PowerShellService.Instance;
            _loggingService = LoggingService.Instance;
            _sessionManager = SessionManager.Instance;
            _mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

            _loggingService.Log("Call Queues page loaded", LogLevel.Info);
        }

        [RelayCommand]
        private void ShowVariablesConfirmation()
        {
            ShowConfirmation = true;
        }

        [RelayCommand]
        private void NavigateToVariables()
        {
            if (_mainWindowViewModel != null)
            {
                _mainWindowViewModel.NavigateTo("Variables");
                _loggingService.Log("Navigated to Variables page", LogLevel.Info);
            }
        }

        [RelayCommand]
        private async Task CreateCallQueueAsync()
        {
            string racqUPN = string.Empty;
            string racqDisplayName = string.Empty;
            string cqDisplayName = string.Empty;
            string csappcqid = string.Empty;
            string usagelocation = string.Empty;
            string languageId = string.Empty;
            string m365groupId = string.Empty;
            
            try
            {
                IsBusy = true;
                ShowConfirmation = false;

                var variables = _mainWindowViewModel?.Variables;
                if (variables == null)
                {
                    _loggingService.Log("Variables not found. Please set variables first.", LogLevel.Error);
                    QueueStatus = "Error: Variables not found. Please set variables first.";
                    return;
                }

                // Get variables from the VariablesViewModel
                racqUPN = variables.RacqUPN;
                racqDisplayName = variables.RacqDisplayName;
                cqDisplayName = variables.CqDisplayName;
                csappcqid = variables.CsAppCqId;
                usagelocation = variables.UsageLocation;
                languageId = variables.LanguageId;
                m365groupId = variables.M365Group;

                _loggingService.Log($"Creating Call Queue: {cqDisplayName}", LogLevel.Info);

                var command = $@"
# Create Resource Account for Main Call Queue
try {{
    $resourceAccount = New-CsOnlineApplicationInstance -UserPrincipalName ""{racqUPN}"" -ApplicationId ""{csappcqid}"" -DisplayName ""{racqDisplayName}""
    Write-Host ""Resource account created: $($resourceAccount.Identity)""
}} catch {{
    if ($_.Exception.Message -like ""*already exists*"") {{
        Write-Host ""Resource account already exists. Continuing with setup...""
    }} else {{
        throw $_
    }}
}}

# Wait for resource account creation
Write-Host ""Waiting for resource account creation to complete (2 minutes)..."" 
Start-Sleep -Seconds 120

# Get the resource account ID using Get-CsOnlineUser
$racqid = (Get-CsOnlineUser ""{racqUPN}"").Identity
if (-not $racqid) {{
    throw ""Failed to get resource account ID. Resource account may not have been created successfully.""
}}
Write-Host ""Resource account ID: $racqid""

# Assign Usage Location
Update-MgUser -UserId ""{racqUPN}"" -UsageLocation ""{usagelocation}""

# Get M365 Group ObjectId
$m365Group = Get-MgGroup -Filter ""displayName eq '{m365groupId}'""
if (-not $m365Group) {{
    throw ""Failed to find M365 group with name '{m365groupId}'""
}}
$m365GroupId = $m365Group.Id
Write-Host ""M365 Group ID: $m365GroupId""

# Check if call queue already exists
$existingQueue = Get-CsCallQueue -NameFilter ""{cqDisplayName}""
if ($existingQueue) {{
    Write-Host ""Call queue '{cqDisplayName}' already exists. Skipping creation.""
}} else {{
    # Create Main Call Queue
    $callQueue = New-CsCallQueue `
    -Name ""{cqDisplayName}"" `
    -RoutingMethod Attendant `
    -AllowOptOut $true `
    -ConferenceMode $true `
    -AgentAlertTime 30 `
    -LanguageId ""{languageId}"" `
    -DistributionLists @($m365GroupId) `
    -OverflowThreshold 15 `
    -OverflowAction Forward `
    -OverflowActionTarget $racqid `
    -TimeoutThreshold 30 `
    -TimeoutAction Forward `
    -TimeoutActionTarget $racqid `
    -UseDefaultMusicOnHold $true `
    -PresenceBasedRouting $false

    if (-not $callQueue) {{
        throw ""Failed to create call queue""
    }}
    Write-Host ""Call queue created successfully""
}}

# Wait for call queue creation
Write-Host ""Waiting for call queue creation to complete (2 minutes)...""
Start-Sleep -Seconds 120

# Assign resource account to Main Call Queue using the exact steps provided
try {{
    $cqapplicationInstanceId = (Get-CsOnlineUser ""{racqUPN}"").Identity
    $cqautoAttendantId = (Get-CsCallQueue -NameFilter ""{cqDisplayName}"").Identity
    
    if (-not $cqapplicationInstanceId -or -not $cqautoAttendantId) {{
        throw ""Failed to get resource account ID or call queue ID for association""
    }}
    
    New-CsOnlineApplicationInstanceAssociation -Identities @($cqapplicationInstanceId) -ConfigurationId $cqautoAttendantId -ConfigurationType CallQueue
    Write-Host ""Resource account associated with call queue successfully""
}} catch {{
    if ($_.Exception.Message -like ""*Cannot process argument transformation*"") {{
        Write-Host ""Warning: Could not associate resource account with call queue. This may be because the association already exists.""
    }} else {{
        throw $_
    }}
}}
";

                var result = await _powerShellService.ExecuteCommandAsync(command);
                if (!string.IsNullOrEmpty(result))
                {
                    QueueStatus = "Call Queue created successfully";
                    IsQueueCreated = true;
                    _loggingService.Log($"Call Queue {cqDisplayName} created successfully", LogLevel.Info);
                }
                else
                {
                    QueueStatus = "Error: No output from PowerShell command";
                    _loggingService.Log($"Error creating Call Queue {cqDisplayName}: No output from PowerShell command", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                QueueStatus = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in CreateCallQueueAsync for queue {cqDisplayName}: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
} 